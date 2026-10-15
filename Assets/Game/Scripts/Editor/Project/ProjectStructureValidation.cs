using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks the release scene paths and asset identities after folder maintenance.</summary>
public static class ProjectStructureValidation
{
    [Serializable] private sealed class AssetIdentity { public string guid; public string path; }
    [Serializable] private sealed class IdentityManifest { public AssetIdentity[] assets; }
    [Serializable] private sealed class ValidationResult
    {
        public string utc;
        public string unityVersion;
        public string status;
        public int assetIdentitiesChecked;
        public int scenesChecked;
        public int missingScripts;
        public int scriptReferencesChecked;
        public string sceneInspection = "Serialized scene script GUIDs; no scene instantiation.";
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    private static readonly string[] BuildScenes =
    {
        "Assets/Game/Scenes/SplashScreen.unity",
        "Assets/Game/Scenes/MainMenu.unity",
        "Assets/Game/Scenes/Gameplay.unity"
    };

    [MenuItem("Tools/Project/Validate project structure")]
    public static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Stop Play Mode and wait for compilation before validating.");

        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        var result = new ValidationResult { utc = DateTime.UtcNow.ToString("o"), unityVersion = Application.unityVersion };
        string manifestPath = Path.Combine(projectRoot, "Docs/Validation/expected-asset-guids.json");
        if (!File.Exists(manifestPath)) result.errors.Add("The asset identity manifest is missing.");
        else
        {
            var manifest = JsonUtility.FromJson<IdentityManifest>(File.ReadAllText(manifestPath));
            foreach (var asset in manifest.assets)
            {
                string actual = AssetDatabase.GUIDToAssetPath(asset.guid);
                if (actual != asset.path)
                    result.errors.Add($"Asset identity mismatch: {asset.guid}: expected {asset.path}, actual {actual}");
                result.assetIdentitiesChecked++;
            }
        }

        string[] enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (!enabledScenes.SequenceEqual(BuildScenes))
            result.errors.Add("Enabled build scenes must be SplashScreen, MainMenu, Gameplay in that order.");

        foreach (string path in BuildScenes.Concat(new[] { "Assets/Game/Scenes/SubScenes/GameplayEntities.unity" }))
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                result.errors.Add("Scene is missing: " + path);
                continue;
            }
            // Inspect serialized references without invoking scene components or loading
            // lightmaps/subscene previews into memory before a gameplay smoke test.
            foreach (Match script in Regex.Matches(File.ReadAllText(Path.Combine(projectRoot, path)),
                @"m_Script:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]{32}))?"))
            {
                result.scriptReferencesChecked++;
                if (script.Groups[1].Value == "0" ||
                    (script.Groups[2].Success && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(script.Groups[2].Value))))
                {
                    result.missingScripts++;
                    result.errors.Add($"Missing script reference in {path}: {script.Value}");
                }
            }
            result.scenesChecked++;
        }

        foreach (string path in AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/Game/Scripts/Runtime/", StringComparison.Ordinal)))
        {
            if (Directory.Exists(path)) continue;
            if (!path.EndsWith(".cs", StringComparison.Ordinal) && !path.EndsWith(".asmdef", StringComparison.Ordinal) && !path.EndsWith(".md", StringComparison.Ordinal))
                result.errors.Add("Non-code asset found in Runtime: " + path);
        }

        string bank = Path.Combine(Application.streamingAssetsPath, "Audio/GeneratedSoundBanks/Windows/Init.bnk");
        if (!File.Exists(bank)) result.errors.Add("The Windows Wwise Init sound bank is missing.");
        if (!File.Exists(Path.Combine(projectRoot, "Audio/WwiseProject/CSLike 0.0_WwiseProject.wproj")))
            result.errors.Add("The Wwise authoring project is missing.");
        var audioSettings = XDocument.Load(Path.Combine(Application.dataPath, "WwiseSettings.xml"));
        string configuredAudioPath = audioSettings.Root.Element("WwiseProjectPath")?.Value;
        if (string.IsNullOrEmpty(configuredAudioPath) || !File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, configuredAudioPath))))
            result.errors.Add("The Wwise authoring project path in WwiseSettings.xml is invalid.");
        else
        {
            string authoring = Path.GetFullPath(Path.Combine(Application.dataPath, configuredAudioPath));
            var project = XDocument.Load(authoring);
            var rootOutput = project.Root.Element("ProjectInfo")?.Element("Project")?.Element("PropertyList")?
                .Elements("Property").FirstOrDefault(property => (string)property.Attribute("Name") == "SoundBankHeaderFilePath");
            string relativeOutput = (string)rootOutput?.Attribute("Value");
            if (string.IsNullOrEmpty(relativeOutput) || !File.Exists(Path.Combine(Path.GetDirectoryName(authoring), relativeOutput, "ProjectInfo.json")))
                result.errors.Add("The Wwise metadata root output path is invalid.");
        }

        result.status = result.errors.Count == 0 ? "PASS" : "FAIL";
        string output = Path.Combine(projectRoot, "Docs/Validation/structure-validation.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output, JsonUtility.ToJson(result, true));
        if (result.errors.Count > 0)
            throw new InvalidOperationException("Project structure validation failed: " + string.Join("; ", result.errors.Take(10)));
        Debug.Log($"[ProjectStructure] PASS: {result.assetIdentitiesChecked} asset identities; {result.scenesChecked} scenes; no missing scripts.");
    }

    public static void ValidateBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use Unity batch mode for this entry point.");
        Validate();
        OfflineMatchValidation.RunEntityChecks();
    }
}
