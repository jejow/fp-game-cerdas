"""Check the staged Unity source tree before committing or pushing."""
import argparse
import datetime
import json
from pathlib import Path, PurePosixPath
import posixpath
import re
import subprocess
import sys
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
POINTER = re.compile(rb"version https://git-lfs.github.com/spec/v1\noid sha256:([a-f0-9]{64})\nsize (\d+)\n")
SECRETS = {
    "private key": re.compile(rb"-----BEGIN (?:[A-Z0-9]+ )?PRIVATE KEY-----\s+[A-Za-z0-9+/=\r\n]{80,}"),
    "GitHub token": re.compile(rb"\bgh[pousr]_[A-Za-z0-9_]{30,}\b|\bgithub_pat_[A-Za-z0-9_]{60,}\b"),
    "API token": re.compile(rb"\bsk-(?:proj-|svcacct-)?[A-Za-z0-9_-]{40,}\b"),
    "AWS access key": re.compile(rb"\b(?:AKIA|ASIA)[A-Z0-9]{16}\b"),
}


def git(*args, data=None):
    result = subprocess.run(["git", *args], cwd=ROOT, input=data,
                            stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if result.returncode:
        raise RuntimeError(result.stderr.decode("utf-8", "replace").strip())
    return result.stdout


def blobs(oids):
    raw = git("cat-file", "--batch", data="".join(oid + "\n" for oid in oids).encode())
    result, offset = {}, 0
    for oid in oids:
        end = raw.index(b"\n", offset)
        actual, kind, size = raw[offset:end].split()
        if actual.decode() != oid or kind != b"blob":
            raise RuntimeError("Invalid staged object: " + oid)
        size = int(size)
        offset = end + 1
        result[oid] = raw[offset:offset + size]
        offset += size + 1
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--lfs-fsck", action="store_true", help="Also verify full LFS object hashes.")
    parser.add_argument("--report", type=Path, help="Write a JSON receipt; use an external path while checking the index.")
    args = parser.parse_args()
    errors, entries = [], {}
    for row in git("ls-files", "--stage", "-z").split(b"\0"):
        if not row:
            continue
        header, path = row.split(b"\t", 1)
        mode, oid, stage = header.decode().split()
        path = path.decode("utf-8")
        if stage != "0":
            errors.append("Unresolved merge: " + path)
        if mode not in {"100644", "100755"}:
            errors.append("Unsupported source entry: " + path)
        entries[path] = oid
    if not entries:
        raise RuntimeError("The Git index is empty.")
    bodies = blobs(sorted(set(entries.values())))
    contents = {path: bodies[oid] for path, oid in entries.items()}
    directories = {parent.as_posix() for path in entries for parent in PurePosixPath(path).parents}
    attrs_raw = git("check-attr", "--cached", "-z", "--stdin", "filter",
                    data=b"".join(path.encode("utf-8") + b"\0" for path in entries)).split(b"\0")
    filters = {attrs_raw[i].decode("utf-8"): attrs_raw[i + 2].decode()
               for i in range(0, len(attrs_raw) - 1, 3)}
    media_dir = None
    for line in git("lfs", "env").decode("utf-8", "replace").splitlines():
        if line.startswith("LocalMediaDir="):
            media_dir = Path(line.split("=", 1)[1])
    if media_dir is None:
        raise RuntimeError("Git LFS storage is unavailable.")
    lfs_objects, lfs_paths, meta_ids, case_paths = {}, 0, {}, {}
    missing_meta, orphan_meta, secret_hits = [], [], []
    largest_regular = 0
    private_roots = {"library", "temp", "logs", "usersettings", "obj", ".vs", ".idea", ".codex", "_projectbackups", "_local", ".validation"}
    for path, body in contents.items():
        parts = PurePosixPath(path).parts
        name = parts[-1]
        if parts[0].lower() in private_roots or (parts[0].lower() in {"build", "builds"} and path != "Builds/README.md"):
            errors.append("Local/generated path is staged: " + path)
        if name.endswith((".csproj", ".sln", ".slnx", ".log", ".pyc", ".pfx", ".p12", ".jks", ".keystore", ".ulf")):
            errors.append("Generated/private file is staged: " + path)
        if name == ".env" or (name.startswith(".env.") and not name.endswith(".example")):
            errors.append("Local environment file is staged: " + path)
        folded = path.casefold()
        if folded in case_paths and case_paths[folded] != path:
            errors.append("Case-colliding paths: " + path)
        case_paths[folded] = path
        pointer = POINTER.fullmatch(body)
        if filters.get(path) == "lfs":
            lfs_paths += 1
            if pointer is None:
                errors.append("Noncanonical LFS pointer: " + path)
            else:
                oid, size = pointer.group(1).decode(), int(pointer.group(2))
                lfs_objects[oid] = size
        else:
            if pointer:
                errors.append("LFS pointer without an LFS attribute: " + path)
            largest_regular = max(largest_regular, len(body))
            if len(body) >= 100 * 1024 * 1024:
                errors.append("Oversized regular Git blob: " + path)
            if b"\0" not in body[:4096]:
                for label, pattern in SECRETS.items():
                    if pattern.search(body):
                        secret_hits.append({"file": path, "type": label})
        if path.startswith("Assets/"):
            if path.endswith(".meta"):
                owner = path[:-5]
                if owner not in entries and owner not in directories:
                    orphan_meta.append(path)
                match = re.search(rb"(?m)^guid: ([a-f0-9]{32})\r?$", body)
                if not match:
                    errors.append("Missing Unity GUID: " + path)
                else:
                    guid = match.group(1).decode()
                    if guid in meta_ids:
                        errors.append("Duplicate Unity GUID: " + path)
                    meta_ids[guid] = owner
            elif not name.startswith(".") and not any(part.endswith(".bundle") for part in parts[:-1]):
                if path + ".meta" not in entries:
                    missing_meta.append(path)
    for oid, size in lfs_objects.items():
        obj = media_dir / oid[:2] / oid[2:4] / oid
        if not obj.is_file() or obj.stat().st_size != size:
            errors.append("Missing or incomplete local LFS object: " + oid)
    errors.extend("Missing Unity metadata: " + path for path in missing_meta)
    errors.extend("Orphan Unity metadata: " + path for path in orphan_meta)
    errors.extend("Possible " + hit["type"] + " in " + hit["file"] for hit in secret_hits)
    manifest = json.loads(contents["Docs/Validation/expected-asset-guids.json"].decode("utf-8-sig"))
    for asset in manifest["assets"]:
        if meta_ids.get(asset["guid"]) != asset["path"]:
            errors.append("Asset identity does not match the staged tree: " + asset["path"])
    links = 0
    for path, body in contents.items():
        if not path.endswith(".md") or not (path == "README.md" or path.startswith(("Docs/", "Builds/"))):
            continue
        for match in re.finditer(r"\]\(([^)]+)\)", body.decode("utf-8-sig", "replace")):
            url = match.group(1).strip().strip("<>")
            if re.match(r"^[A-Za-z][A-Za-z0-9+.-]*:", url) or url.startswith("#"):
                continue
            target = posixpath.normpath(posixpath.join(posixpath.dirname(path), unquote(url.split("#")[0])))
            links += 1
            if target not in entries and target not in directories:
                errors.append("Broken staged documentation link: " + path + " -> " + target)
    integrity = "Skipped because structural checks failed" if args.lfs_fsck and errors else "Not requested"
    if args.lfs_fsck and not errors:
        # --objects covers HEAD and the index. Pointer/attribute checks above
        # deliberately use the index, including repairs not committed yet.
        git("lfs", "fsck", "--objects")
        integrity = "PASS"
    report = {
        "utc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "status": "PASS" if not errors else "FAIL", "scope": "Staged index; no network access or push.",
        "sourceFiles": len(entries), "lfsPointerFiles": lfs_paths, "uniqueLfsObjectsPresent": len(lfs_objects),
        "uniqueLfsBytes": sum(lfs_objects.values()), "lfsObjectHashVerification": integrity,
        "largestRegularGitBlobBytes": largest_regular, "assetIdentitiesChecked": len(manifest["assets"]),
        "unityMetaFilesChecked": len(meta_ids), "missingMetaFiles": len(missing_meta),
        "orphanMetaFiles": len(orphan_meta), "documentationLinksChecked": links,
        "strongCredentialPatternMatches": len(secret_hits), "errors": errors,
    }
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if not errors else 1


if __name__ == "__main__":
    sys.exit(main())
