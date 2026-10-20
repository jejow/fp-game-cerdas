"""Regenerate the directory, file, and C# indexes from the current source tree."""
from pathlib import Path
from collections import defaultdict
from urllib.parse import quote
import os
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
DOCS = ROOT / "Docs"
EXCLUDED = {".git", "Library", "Temp", "Logs", "UserSettings", "Builds", "Build", "obj", ".vs", ".validation", "UIElementsSchema"}
GENERATED = {"Docs/FILE_INDEX.md", "Docs/DIRECTORY_INDEX.md", "Docs/CODE_REFERENCE.md"}
ROLES = {
    "Assets/Game/Scripts/Runtime/UI/Branding": "Identitas STEEL PROTOCOL yang dibagikan opening dan menu.",
    "Assets/Game/Scripts/Runtime/UI/Loading": "Opening robot, progres pemuatan menu, dan UI loading pertandingan.",
    "Assets/Game/Scripts/Runtime/Authoring": "Komponen authoring dan Baker ECS.",
    "Assets/Game/Scripts/Runtime/Components": "Definisi data, tag, dan buffer ECS.",
    "Assets/Game/Scripts/Runtime/Systems/Character": "Sistem gerak, input, model, collider, dan presentasi karakter.",
    "Assets/Game/Scripts/Runtime/Systems/Combat": "Sistem damage, kematian, granat, proyektil, dan recoil.",
    "Assets/Game/Scripts/Runtime/Systems/Round": "Sistem ronde, tim, peserta, spawn, dan trigger objektif.",
    "Assets/Game/Scripts/Runtime/Systems/Economy": "Sistem uang dan pembelian perlengkapan.",
    "Assets/Game/Scripts/Runtime/Systems/Presentation": "Sistem kamera, animasi, suara, dan pengaturan presentasi.",
    "Assets/Game/Scripts/Runtime/Systems/Simulation": "Sistem pengelolaan simulasi dan pesan internal.",
    "Assets/Game/Scripts/Runtime/Systems/Harvester": "Sistem plant, defuse, dan state objektif bom.",
    "Assets/Game/Scripts/Runtime/Systems/Stuff": "Sistem senjata, inventori, tembakan, reload, dan pergantian perlengkapan.",
    "Assets/Game/Scripts/Runtime/Systems/UI": "Penghubung state simulasi dengan HUD dan UI.",
    "Assets/Game/Scripts/Runtime/Systems": "Sistem pembaruan gameplay ECS.",
    "Assets/Game/Scripts/Runtime/Offline": "Pertandingan lokal, bot, navigasi, opsi, dan transisi audio.",
    "Assets/Game/Scripts/Runtime/Simulation": "World client/server lokal, bootstrap, smoothing, dan RPC internal.",
    "Assets/Game/Scripts/Runtime/Session": "Pengelolaan sesi dan progres pertandingan.",
    "Assets/Game/Scripts/Runtime/Loading": "Perpindahan scene dan urutan opening/loading.",
    "Assets/Game/Scripts/Runtime/Data": "Definisi C# ScriptableObject; instance dipisah di Game/Data.",
    "Assets/Game/Scripts/Runtime/Input": "Wrapper Input System yang dihasilkan otomatis.",
    "Assets/Game/Scripts/Runtime/UI": "Controller tampilan UI Toolkit.",
    "Assets/Game/Scripts/Runtime/Utilities": "Fungsi dan komponen pendukung yang digunakan bersama.",
    "Assets/Game/Scripts/Runtime/Enums": "Enumerasi untuk konfigurasi dan state game.",
    "Assets/Game/Scripts/Runtime/Interfaces": "Kontrak antarbagian kode.",
    "Assets/Game/Scripts/Runtime/Behaviours": "Komponen GameObject untuk kamera, decal, audio, dan presentasi.",
    "Assets/Game/Scripts/Runtime/Effects": "Penghubung efek visual senjata dan objektif.",
    "Assets/Game/Scripts/Runtime/Diagnostics": "Kode diagnostik lokal dari sumber yang dipertahankan.",
    "Assets/Game/Scripts/Runtime/Noise": "Assembly utilitas noise yang juga dipakai alat Editor.",
    "Assets/Game/Scripts/Editor/Offline": "Validasi pertandingan, batch runner, build, dan sinkronisasi bank audio.",
    "Assets/Game/Scripts/Editor/Project": "Validasi struktur/GUID; pembuatan aset presentasi robot dan pengujian menu.",
    "Assets/Game/Art/Branding/Animations": "Controller idle robot untuk menu dan potret UI.",
    "Assets/Game/Art/Branding/Materials": "Material armor dua tim, platform, panel, dan lampu ruang persiapan.",
    "Assets/Game/Art/Branding/Meshes": "Mesh cincin cahaya platform robot.",
    "Assets/Game/Art/Branding/Portraits": "Potret robot Corpo/Natif untuk pilihan tim, HUD, dan ikon produk.",
    "Assets/Game/Art/Branding": "Aset presentasi STEEL PROTOCOL yang dibuat dari sumber robot proyek.",
    "Assets/Game/Art/SplashScreen": "Aset opening asal yang dipertahankan; video/logo lama tidak dipakai scene pembuka.",
    "Assets/Game/Scripts/Editor/NoiseTools": "Alat Editor pembuat/penggabung noise, dengan assembly tersendiri.",
    "Assets/Game/Art": "Aset visual: model, tekstur, material, animasi, opening, dan VFX.",
    "Assets/Game/Data": "Instance data ScriptableObject untuk karakter, senjata, mod, UI, dan pengaturan.",
    "Assets/Game/Scenes/SubScenes": "Scene authoring entity untuk pertandingan.",
    "Assets/Game/Scenes/Gameplay": "Lightmap, lighting data, dan volume pendamping scene pertandingan.",
    "Assets/Game/Scenes": "Scene utama dan data presentasi pendamping.",
    "Assets/Game/Prefabs/Character/Robots": "Empat prefab robot FPS/TPS untuk kedua tim dalam rilis.",
    "Assets/Game/Prefabs": "Prefab gameplay dan presentasi yang dirujuk scene/sistem.",
    "Assets/Game/Rendering/Robots": "Shader URP untuk pewarnaan armor robot tanpa mengubah tekstur asal.",
    "Assets/Game/Rendering": "Shader, material, dan render target efek gameplay.",
    "Assets/Game/Input": "Asset binding Input System; wrapper C# berada di Scripts/Runtime/Input.",
    "Assets/Game/Physics": "Konfigurasi material fisika game.",
    "Assets/Game/UI/Branding": "Style identitas dan pengaturan skala panel menu/opening.",
    "Assets/Game/UI/Loading": "Layout dan style layar boot/loading pertama.",
    "Assets/Game/UI": "Layout UXML dan stylesheet USS; controller dipisahkan ke Scripts/Runtime/UI.",
    "Assets/Development": "Scene/prototipe yang dipertahankan dan tidak masuk daftar build utama.",
    "Assets/ThirdParty": "Aset pihak ketiga; struktur internal dan metadata vendor dipertahankan.",
    "Assets/Wwise": "Integrasi Wwise, plugin native, reference asset, dan alat audio Editor.",
    "Assets/StreamingAssets": "File yang disertakan untuk pemuatan lokal saat runtime, termasuk bank audio.",
    "Assets/Resources": "Aset yang dapat dimuat melalui mekanisme Resources Unity.",
    "Assets/Settings": "Pengaturan URP, kualitas rendering, dan volume global.",
    "Assets/AddressableAssetsData": "Konfigurasi Addressables, group, alamat aset, dan builder.",
    "Assets/Samples": "Sample package yang masih dibutuhkan atau dipertahankan bersama aset asal.",
    "Assets/TextMesh Pro": "Font, shader, resource, dan material TextMesh Pro.",
    "Assets/UI Toolkit": "Tema dan pengaturan UI Toolkit.",
    "Assets/Gizmos": "Ikon/gambar gizmo Editor.",
    "Assets/SceneDependencyCache": "Cache dependensi entity scene yang dibuat alat Unity; bukan kode game.",
    "Audio/WwiseProject": "Authoring Wwise, audio original, konfigurasi mixer, dan bank hasil generasi.",
    "Packages": "Manifest/lockfile package dan source package embedded.",
    "ProjectSettings": "Konfigurasi Unity proyek yang dibagikan.",
    "Docs/Validation/History": "Bukti pengujian historis, bukan hasil uji yang baru dijalankan.",
    "Docs/Validation": "Manifest identitas aset dan laporan pemeriksaan penataan.",
    "Docs": "Panduan proyek, arsitektur, indeks, dan hasil validasi.",
    "Tools/ThirdPartySources": "Arsip source SDK integrasi Wwise yang dipertahankan.",
    "Tools/Project": "Perintah pembaruan dokumentasi dan pemeriksaan proyek.",
    "Tools": "Alat pendukung maintenance di luar kompilasi Unity.",
    ".github": "Panduan kode repositori; workflow CI warisan diarsipkan di luar proyek.",
    ".vscode": "Pengaturan editor kode lokal yang dipertahankan.",
}
NAMES = {
    "GameIdentity": "Nama produk, tagline, executable, dan label identitas bersama.",
    "OpeningScreenController": "Pemuatan menu nyata, progres opening, dan input Skip.",
    "MenuRobotDisplay": "Gerakan idle ringan unit robot yang dipamerkan di menu.",
    "RobotPresentationSetup": "Membentuk prefab/material/potret robot serta scene opening dan menu.",
    "RobotPresentationValidation": "Memeriksa identitas, dependensi rilis, model robot, material, dan shader.",
    "PresentationSmokeBatch": "Uji Play Mode untuk opening, navigasi menu, dan layout dua resolusi.",
    "OfflineMatch": "Lifecycle pertandingan, world simulasi, scene gameplay, dan cleanup.",
    "OfflineMatchOptions": "Validasi, preset, dan penyimpanan preferensi pertandingan.",
    "OfflineMatchSettings": "Data konfigurasi pertandingan lokal.",
    "OfflineNavigation": "Pembuatan dan pencarian navigasi bot pada peta.",
    "OfflineParticipantSystem": "Identitas dan peserta pemain/bot dalam simulasi.",
    "OfflineBotInputSystem": "Keputusan/input gerak, bidik, tembak, dan reload bot.",
    "OfflineBotObjectiveSystem": "Perilaku bot terhadap objektif bom.",
    "OfflineAudioTransition": "Suspend/resume audio pada transisi yang berat.",
    "OfflinePlayerValidation": "Pemeriksaan standalone yang hanya aktif dengan argumen eksplisit.",
    "GameManager": "Meneruskan permintaan bermain dari menu ke OfflineMatch.",
    "SessionData": "State/progres loading yang digunakan binding UI.",
    "MainMenuController": "Interaksi menu dan konfigurasi custom bot match.",
    "HUDController": "Controller HUD selama pertandingan.",
    "DefaultInputSystem": "Wrapper input otomatis; edit asset inputactions, bukan file ini.",
    "OfflineAudioSetup": "Sinkronisasi bank Windows dari sumber Wwise sebelum build.",
    "OfflineMatchValidation": "Aturan objektif, smoke test pertandingan, dan build Windows.",
    "OfflineSmokeBatch": "Runner batch untuk suite pengujian offline.",
    "ProjectStructureValidation": "Validasi GUID, scene, pemisahan kode, dan bank audio.",
    "VideoPlayerSceneHandler": "Controller video historis; opening rilis memakai OpeningScreenController.",
}
KINDS = {".meta": "Metadata/GUID Unity", ".cs": "Kode C#", ".unity": "Scene Unity", ".prefab": "Prefab", ".asset": "Asset/data Unity", ".asmdef": "Batas assembly", ".uxml": "Layout UI", ".uss": "Style UI", ".inputactions": "Binding input", ".md": "Dokumentasi", ".json": "Data/konfigurasi JSON", ".xml": "Data/konfigurasi XML", ".fbx": "Model/animasi 3D", ".shader": "Shader", ".hlsl": "Kode shader", ".shadergraph": "Shader Graph", ".vfx": "Visual Effect Graph", ".wav": "Audio", ".wem": "Audio Wwise", ".bnk": "Bank Wwise", ".wproj": "Proyek Wwise", ".wwu": "Work unit Wwise", ".zip": "Arsip sumber", ".py": "Alat Python", ".ps1": "Alat PowerShell", ".dll": "Library/plugin", ".png": "Gambar/tekstur", ".jpg": "Gambar/tekstur", ".tga": "Gambar/tekstur", ".exr": "Tekstur/lightmap HDR", ".mp4": "Video", ".mat": "Material", ".anim": "Klip animasi", ".controller": "Animator controller", ".log": "Log validasi", ".txt": "Catatan/hasil validasi"}


def role(path):
    for prefix in sorted(ROLES, key=len, reverse=True):
        if path == prefix or path.startswith(prefix + "/"):
            return ROLES[prefix]
    return "File root atau data pendukung proyek; lihat jenis file dan nama sumber."


def link(relative, label=None):
    label = (label or relative).replace("[", r"\[").replace("]", r"\]").replace("|", r"\|")
    return f"[{label}]({quote('../' + relative, safe='/')})"


def main():
    folders = defaultdict(list)
    files = []
    # Match the portable repository when Git is available, including untracked
    # source files while leaving local/generated assets out of published links.
    publishable = None
    visible_dirs = None
    try:
        result = subprocess.run(["git", "ls-files", "--cached", "--others", "--exclude-standard", "-z"],
                                cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=True)
        publishable = {p.decode("utf-8") for p in result.stdout.split(b"\0") if p}
        visible_dirs = {"."}
        for path in publishable:
            visible_dirs.update(parent.as_posix() for parent in Path(path).parents)
    except (OSError, subprocess.CalledProcessError):
        pass
    for here, dirs, names in os.walk(ROOT):
        rel_dir = Path(here).relative_to(ROOT).as_posix()
        dirs[:] = sorted(d for d in dirs if (rel_dir != "." or d not in EXCLUDED)
                         and d != "__pycache__" and not d.startswith((".cache", ".backup"))
                         and (visible_dirs is None or (Path(here) / d).relative_to(ROOT).as_posix() in visible_dirs))
        if rel_dir == ".":
            names = [n for n in names if not n.endswith((".csproj", ".sln", ".slnx"))]
        folders[rel_dir]
        for name in sorted(names):
            p = Path(here) / name
            rel = p.relative_to(ROOT).as_posix()
            if rel in GENERATED or (publishable is not None and rel not in publishable):
                continue
            folders[rel_dir].append((rel, p.stat().st_size))
            files.append(rel)
    # Include delivery instructions without indexing regenerable player binaries.
    build_readme = ROOT / "Builds/README.md"
    if build_readme.exists():
        folders["Builds"].append(("Builds/README.md", build_readme.stat().st_size))
        files.append("Builds/README.md")
    index = ["# Indeks seluruh file sumber", "", "Dibuat oleh `python Tools/Project/update_docs.py`. Termasuk `.meta`; tidak mencakup cache Unity, Git, build biner, atau tiga indeks otomatis ini.", "", f"Snapshot: **{len(files)} file**, **{len(folders)} direktori**.", ""]
    directories = ["# Indeks direktori", "", "Jumlah file/ukuran pada tabel adalah isi langsung; subdirektori memiliki baris sendiri. Cache dan build otomatis dikecualikan.", "", "| Direktori | File langsung | KiB langsung | Fungsi |", "| --- | ---: | ---: | --- |"]
    for directory, entries in sorted(folders.items()):
        explanation = role(directory)
        directories.append(f"| `{directory}` | {len(entries)} | {sum(n for _, n in entries) / 1024:.1f} | {explanation} |")
        index += [f"## `{directory}`", "", explanation, "", "| File | Jenis | KiB |", "| --- | --- | ---: |"]
        for relative, size in entries:
            index.append(f"| {link(relative, Path(relative).name)} | {KINDS.get(Path(relative).suffix.lower(), 'Data/aset pendukung')} | {size / 1024:.1f} |")
        if not entries:
            index.append("| — | Tidak ada file langsung | — |")
        index.append("")
    code = ["# Referensi kode proyek", "", "Mencakup kode game, alat Editor milik proyek, dan script sandbox. Kode vendor/package tercantum dalam indeks file dan tidak digabung dengan kode game.", "", "Nama tipe dan namespace lama dipertahankan. Tipe pada tabel adalah ringkasan deklarasi; buka file untuk API lengkap.", ""]
    modules = defaultdict(list)
    for relative in files:
        if relative.endswith(".cs") and relative.startswith(("Assets/Game/Scripts/", "Assets/Development/")):
            modules[str(Path(relative).parent).replace("\\", "/")].append(relative)
    total = 0
    for module, scripts in sorted(modules.items()):
        code += [f"## `{module}`", "", role(module), "", "| File | Peran | Tipe yang dideklarasikan |", "| --- | --- | --- |"]
        for relative in scripts:
            p = ROOT / relative
            text = p.read_text(encoding="utf-8-sig", errors="replace")
            text = re.sub(r"/\*.*?\*/|//[^\n]*", "", text, flags=re.S)
            types = list(dict.fromkeys(re.findall(r"\b(?:class|struct|enum|interface)\s+(\w+)", text)))
            names = ", ".join(f"`{t}`" for t in types[:8]) or "Atribut/utility tanpa tipe bernama."
            if len(types) > 8:
                names += f"; +{len(types)-8} tipe lain"
            code.append(f"| {link(relative, p.name)} | {NAMES.get(p.stem, role(module))} | {names} |")
            total += 1
        code.append("")
    code.insert(4, f"Total: **{total} file C#** dalam **{len(modules)} kelompok direktori**.")
    DOCS.mkdir(exist_ok=True)
    for name, lines in [("FILE_INDEX.md", index), ("DIRECTORY_INDEX.md", directories), ("CODE_REFERENCE.md", code)]:
        (DOCS / name).write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Documented {len(files)} source files, {len(folders)} directories, and {total} game/editor C# files.")


if __name__ == "__main__":
    main()
