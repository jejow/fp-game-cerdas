using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Checks that both teams, FPS arms, and shipped scenes use robot assets.</summary>
public static class RobotPresentationValidation
{
    [Serializable] private sealed class Report
    {
        public string utc;
        public string title;
        public string status;
        public int checks;
        public List<string> errors = new List<string>();
    }

    [MenuItem("Tools/Project/Validate robot presentation")]
    public static void Validate()
    {
        var report = new Report { utc = DateTime.UtcNow.ToString("o"), title = GameIdentity.Name };
        void Check(bool value, string error) { report.checks++; if (!value) report.errors.Add(error); }
        Check(PlayerSettings.productName == GameIdentity.Name, "The player product name differs from the menu title.");
        Check(!PlayerSettings.SplashScreen.show && !PlayerSettings.SplashScreen.showUnityLogo, "The original engine splash is still enabled.");
        Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Managers/MainMenuController.prefab")
            .GetComponent<OfflineMatchSettings>() != null, "The menu prefab is missing its offline match defaults.");
        var openingPanel = Regex.Match(File.ReadAllText("Assets/Game/Scenes/SplashScreen.unity"),
            @"m_PanelSettings: \{fileID: 11400000, guid: ([a-f0-9]{32})");
        Check(openingPanel.Success && AssetDatabase.LoadAssetAtPath<PanelSettings>(
            openingPanel.Success ? AssetDatabase.GUIDToAssetPath(openingPanel.Groups[1].Value) : "") != null,
            "The opening UI document has no saved rendering panel.");
        var character = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Character/Character.prefab");
        var first = character.GetComponent<FirstPersonCharacterModelAuthoring>();
        var third = character.GetComponent<ThirdPersonCharacterModelAuthoring>();
        Check(first.CorpoAnimatorData == first.NatifAnimatorData, "Both FPS rigs must use the robot animation set.");
        Check(third.CorpoAnimatorData == third.NatifAnimatorData, "Both full body rigs must use the robot animation set.");
        Check(third.CorpoColliderBones == third.NatifColliderBones, "The Natif collider bone mapping still uses the human rig.");
        foreach (var prefab in new[] { first.CorpoModelPrefab, first.NatifModelPrefab, third.CorpoModelPrefab, third.NatifModelPrefab })
        {
            Check(prefab != null && AssetDatabase.GetAssetPath(prefab).StartsWith(RobotPresentationSetup.RobotFolder + "/", StringComparison.Ordinal), "A character still references a legacy prefab.");
            if (prefab == null) continue;
            foreach (var renderer in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string mesh = AssetDatabase.GetAssetPath(renderer.sharedMesh);
                Check(mesh.Contains("/Corpo/") && !mesh.Contains("/Natif/"), "A character renderer is using a human mesh: " + mesh);
                Check(renderer.sharedMaterials.Length > 0 && renderer.sharedMaterials[0].shader.name == "Game/Robot Armor", "The robot team armor material is missing.");
            }
        }
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        string[] dependencies = AssetDatabase.GetDependencies(scenes, true);
        Check(!dependencies.Any(path => path.Contains("/UI/Credits/")), "A release scene still depends on a contributor page.");
        Check(!File.ReadAllText("Assets/Game/UI/MainMenu/MainMenu.uxml").Contains("CreditsButton"), "The main menu still exposes a contributor page.");
        Check(!Directory.Exists("Assets/Game/UI/Credits"), "Unused contributor UI assets remain in the project.");
        Check(!dependencies.Any(path => path.Contains("/Models/PhysicalElements/Characters/Natif/")), "A release scene still depends on a human character model.");
        Check(!dependencies.Any(path => path.EndsWith("/MainMenuCharacter.prefab", StringComparison.Ordinal)), "The menu still depends on its original human character.");
        Check(!dependencies.Any(path => path.EndsWith("/VideoLogo.mp4", StringComparison.Ordinal)), "The original logo opening is still a scene dependency.");
        Check(!dependencies.Any(path => path.EndsWith("/TITRE_Update.png", StringComparison.Ordinal)), "The original Suna title artwork remains in a release UI.");
        foreach (string name in new[] { "CorpoRobot", "NatifRobot" })
            Check(AssetDatabase.LoadAssetAtPath<Texture2D>(RobotPresentationSetup.ArtFolder + "/Portraits/" + name + ".png") != null, "Missing robot portrait: " + name);
        Shader shader = Shader.Find("Game/Robot Armor");
        Check(shader != null, "Robot armor shader is missing.");
        if (shader != null)
        {
            Check(shader.isSupported, "Robot armor shader is not supported by the active graphics device.");
            foreach (var message in ShaderUtil.GetShaderMessages(shader))
                Check(message.severity.ToString() != "Error", "Robot armor shader: " + message.message);
        }
        report.status = report.errors.Count == 0 ? "PASS" : "FAIL";
        string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Docs/Validation/presentation-validation.json");
        File.WriteAllText(output, JsonUtility.ToJson(report, true));
        if (report.errors.Count != 0) throw new InvalidOperationException(string.Join("\n", report.errors));
        Debug.Log($"[RobotPresentation] {report.checks} checks PASS.");
    }

    public static void ValidateBatch()
    {
        ProjectStructureValidation.Validate();
        Validate();
    }
}
