using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

/// <summary>Rebuilds the robot presentation from existing, animated project assets.</summary>
public static class RobotPresentationSetup
{
    public const string RobotFolder = "Assets/Game/Prefabs/Character/Robots";
    public const string ArtFolder = "Assets/Game/Art/Branding";
    private const string CharacterPath = "Assets/Game/Prefabs/Character/Character.prefab";
    private const string PrefabsPath = "Assets/Game/Prefabs/PrefabsManager.prefab";
    private const string MenuControllerPath = "Assets/Game/Prefabs/Managers/MainMenuController.prefab";
    private const string OriginalMaterial = "Assets/Game/Art/Materials/PhysicalElements/Characters/Corpo/CorpoMaterial.mat";
    private const string PanelPath = "Assets/Game/UI/Branding/MenuPanelSettings.asset";
    private static readonly Color Cyan = new Color(0.12f, 0.68f, 0.88f);
    private static readonly Color Amber = new Color(0.95f, 0.48f, 0.12f);

    [MenuItem("Tools/Project/Apply robot presentation")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before rebuilding presentation assets.");
        foreach (string folder in new[] { RobotFolder, ArtFolder + "/Materials", ArtFolder + "/Meshes", ArtFolder + "/Portraits", ArtFolder + "/Animations" })
            Directory.CreateDirectory(folder);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material corpo = CreateArmor("CorpoArmor", Amber);
        Material natif = CreateArmor("NatifArmor", Cyan);
        GameObject corpoFps = CreateRobot("CorpoRobotFps", "Corpo/CharacterModelCorpoFps", corpo);
        GameObject corpoTps = CreateRobot("CorpoRobotTps", "Corpo/CharacterModelCorpoTps", corpo);
        GameObject natifFps = CreateRobot("NatifRobotFps", "Corpo/CharacterModelCorpoFps", natif);
        GameObject natifTps = CreateRobot("NatifRobotTps", "Corpo/CharacterModelCorpoTps", natif);
        UpdateCharacter(corpoFps, natifFps, corpoTps, natifTps);
        UpdateColliders();
        PanelSettings panel = PreparePanel();
        UpdateMenuDocument(panel);
        ConfigurePlayer();
        CreateMenu(panel, corpoTps);
        CapturePortrait(corpoTps, "CorpoRobot");
        CapturePortrait(natifTps, "NatifRobot");
        AssetDatabase.ImportAsset("Assets/Game/UI/TeamChoice/team-choice.uss", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtFolder + "/Portraits/CorpoRobot.png");
        int[] iconSizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone);
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, Enumerable.Repeat(icon, iconSizes.Length).ToArray());
        CreateOpening(panel);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("[RobotPresentation] STEEL PROTOCOL presentation applied.");
    }

    public static void ApplyBatch()
    {
        Apply();
        RobotPresentationValidation.Validate();
    }

    private static void Backup(string assetPath)
    {
        string project = Path.GetDirectoryName(Application.dataPath);
        string destination = Path.Combine(Path.GetDirectoryName(project), "_ProjectBackups/Suna-20261019-Rebranding/Before", assetPath);
        if (File.Exists(assetPath) && !File.Exists(destination))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(assetPath, destination);
        }
    }

    private static Material CreateArmor(string name, Color color)
    {
        Shader shader = Shader.Find("Game/Robot Armor");
        if (shader == null) throw new InvalidOperationException("Robot armor shader is missing.");
        var source = AssetDatabase.LoadAssetAtPath<Material>(OriginalMaterial);
        var material = LoadOrCreateMaterial(ArtFolder + "/Materials/" + name + ".mat", shader);
        foreach (string texture in new[] { "_BaseMap", "_BumpMap", "_MetallicGlossMap", "_EmissionMap" })
            material.SetTexture(texture, source.GetTexture(texture));
        material.SetColor("_TeamColor", color);
        material.SetColor("_EmissionColor", color * 2.4f);
        material.SetFloat("_Smoothness", 0.65f);
        material.SetFloat("_BumpScale", 1);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material LoadOrCreateMaterial(string path, Shader shader)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
            AssetDatabase.CreateAsset(material, path);
        }
        else material.shader = shader;
        return material;
    }

    private static GameObject CreateRobot(string name, string sourceName, Material armor)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Character/" + sourceName + ".prefab");
        var robot = (GameObject)PrefabUtility.InstantiatePrefab(source);
        try
        {
            PrefabUtility.UnpackPrefabInstance(robot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            robot.name = name;
            foreach (var renderer in robot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials.Length == 0) materials = new Material[1];
                materials[0] = armor;
                renderer.sharedMaterials = materials;
                renderer.updateWhenOffscreen = true;
            }
            return PrefabUtility.SaveAsPrefabAsset(robot, RobotFolder + "/" + name + ".prefab");
        }
        finally { Object.DestroyImmediate(robot); }
    }

    private static void UpdateCharacter(GameObject corpoFps, GameObject natifFps, GameObject corpoTps, GameObject natifTps)
    {
        Backup(CharacterPath);
        var root = PrefabUtility.LoadPrefabContents(CharacterPath);
        try
        {
            var first = root.GetComponent<FirstPersonCharacterModelAuthoring>();
            first.CorpoModelPrefab = corpoFps;
            first.NatifModelPrefab = natifFps;
            first.NatifAnimatorData = first.CorpoAnimatorData;
            var third = root.GetComponent<ThirdPersonCharacterModelAuthoring>();
            third.CorpoModelPrefab = corpoTps;
            third.NatifModelPrefab = natifTps;
            third.NatifAnimatorData = third.CorpoAnimatorData;
            third.NatifColliderBones = third.CorpoColliderBones;
            PrefabUtility.SaveAsPrefabAsset(root, CharacterPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void UpdateColliders()
    {
        Backup(PrefabsPath);
        var root = PrefabUtility.LoadPrefabContents(PrefabsPath);
        try
        {
            foreach (var converter in root.GetComponentsInChildren<NetcodePrefabsConverter>(true))
                foreach (var field in typeof(NetcodePrefabsConverter).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    if (field.Name.StartsWith("Natif", StringComparison.Ordinal))
                    {
                        var robotField = typeof(NetcodePrefabsConverter).GetField("Corpo" + field.Name.Substring(5));
                        if (robotField != null) field.SetValue(converter, robotField.GetValue(converter));
                    }
            PrefabUtility.SaveAsPrefabAsset(root, PrefabsPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static PanelSettings PreparePanel()
    {
        var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
        if (panel == null)
        {
            var original = AssetDatabase.LoadAssetAtPath<PanelSettings>(AssetDatabase.GUIDToAssetPath("c2023facb0ba74e4faf01a8cad90b871"));
            panel = Object.Instantiate(original);
            panel.name = "MenuPanelSettings";
            AssetDatabase.CreateAsset(panel, PanelPath);
        }
        panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panel.referenceResolution = new Vector2Int(1920, 1080);
        panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        panel.match = 0.5f;
        EditorUtility.SetDirty(panel);
        return panel;
    }

    private static void UpdateMenuDocument(PanelSettings panel)
    {
        Backup(MenuControllerPath);
        var root = PrefabUtility.LoadPrefabContents(MenuControllerPath);
        try
        {
            root.GetComponent<UIDocument>().panelSettings = panel;
            if (root.GetComponent<OfflineMatchSettings>() == null) root.AddComponent<OfflineMatchSettings>();
            PrefabUtility.SaveAsPrefabAsset(root, MenuControllerPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void ConfigurePlayer()
    {
        Backup("ProjectSettings/ProjectSettings.asset");
        PlayerSettings.productName = GameIdentity.Name;
        PlayerSettings.companyName = "Gim Cerdas";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.SplashScreen.logos = Array.Empty<PlayerSettings.SplashScreenLogo>();
    }

    private static void CreateMenu(PanelSettings panel, GameObject robotPrefab)
    {
        const string path = "Assets/Game/Scenes/MainMenu.unity";
        Backup(path);
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
        {
            bool manager = root.GetComponentInChildren<GameManager>(true) != null ||
                root.GetComponentInChildren<MainMenuController>(true) != null ||
                root.GetComponentInChildren<SoundManager>(true) != null ||
                root.name == "WwiseGlobal" || root.name.Contains("ConnectionManager");
            if (!manager) Object.DestroyImmediate(root);
        }
        foreach (var doc in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            doc.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
        RenderSettings.skybox = null;
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.16f, 0.22f, 0.29f);
        RenderSettings.ambientEquatorColor = new Color(0.09f, 0.13f, 0.18f);
        RenderSettings.ambientGroundColor = new Color(0.025f, 0.04f, 0.06f);
        RenderSettings.ambientIntensity = 1;

        var bay = new GameObject("RobotOperationsBay");
        Material floor = StageMaterial("BayFloor", new Color(0.045f, 0.068f, 0.095f), 0.55f);
        Material wall = StageMaterial("BayPanels", new Color(0.065f, 0.095f, 0.125f), 0.3f);
        Material glow = StageMaterial("CyanLight", new Color(0.08f, 0.65f, 0.78f), 0, true);
        Material amber = StageMaterial("AmberLight", new Color(0.95f, 0.43f, 0.12f), 0, true);
        Box("Floor", bay.transform, new Vector3(0, -0.14f, 1), new Vector3(22, 0.2f, 18), floor);
        Box("RearBulkhead", bay.transform, new Vector3(0, 2.3f, 3.2f), new Vector3(22, 5, 0.15f), floor);
        for (int i = -5; i <= 5; i++)
        {
            Box("WallPanel" + i, bay.transform, new Vector3(i * 1.15f, 1.7f, 3.05f), new Vector3(1.06f, 3.3f, 0.12f), wall);
            Box("WallLight" + i, bay.transform, new Vector3(i * 1.15f - 0.52f, 1.8f, 2.96f), new Vector3(0.018f, 2.9f, 0.025f), i % 3 == 0 ? amber : glow);
            Box("FloorSeam" + i, bay.transform, new Vector3(i * 1.15f, -0.032f, 0), new Vector3(0.014f, 0.008f, 10), wall);
        }
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = "RobotDock";
        platform.transform.SetParent(bay.transform);
        platform.transform.localPosition = new Vector3(1.28f, -0.035f, 0);
        platform.transform.localScale = new Vector3(2.3f, 0.045f, 2.3f);
        platform.GetComponent<Renderer>().sharedMaterial = wall;
        Object.DestroyImmediate(platform.GetComponent<Collider>());
        CreateDockRing(bay.transform, glow);

        var robot = (GameObject)PrefabUtility.InstantiatePrefab(robotPrefab, scene);
        PrefabUtility.UnpackPrefabInstance(robot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        robot.name = "MenuAssaultRobot";
        robot.transform.SetParent(bay.transform);
        robot.transform.localPosition = new Vector3(1.28f, 0.015f, 0);
        robot.transform.localRotation = Quaternion.Euler(0, 195, 0);
        foreach (var node in robot.GetComponentsInChildren<Transform>()) node.gameObject.layer = 0;
        foreach (var renderer in robot.GetComponentsInChildren<SkinnedMeshRenderer>()) renderer.sharedMaterials = new[] { renderer.sharedMaterials[0] };
        var animator = robot.GetComponentInChildren<Animator>();
        animator.runtimeAnimatorController = LoadIdleController();
        robot.AddComponent<MenuRobotDisplay>();
        var cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Main Camera Menu.prefab");
        var cameraRoot = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab, scene);
        PrefabUtility.UnpackPrefabInstance(cameraRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        cameraRoot.name = "OperationsCamera";
        Camera camera = cameraRoot.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.033f, 0.052f);
        camera.transform.position = new Vector3(0.35f, 1.5f, -5.0f);
        camera.transform.LookAt(new Vector3(0.2f, 1.05f, 0));
        cameraRoot.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing = false;
        camera.fieldOfView = 38;
        camera.farClipPlane = 40;
        if (cameraRoot.GetComponent<AudioListener>() == null) cameraRoot.AddComponent<AudioListener>();
        AddLight("KeyLight", LightType.Directional, new Vector3(0, 4, -2), new Vector3(32, -30, 0), new Color(0.74f, 0.84f, 1), 1.8f);
        AddLight("CyanRim", LightType.Point, new Vector3(2.4f, 1.9f, 1.2f), Vector3.zero, Cyan, 9);
        AddLight("AmberFill", LightType.Point, new Vector3(-0.2f, 2.5f, -1.5f), Vector3.zero, new Color(1, 0.62f, 0.3f), 4);
        AddLight("RobotFrontLight", LightType.Point, new Vector3(1.3f, 2.1f, -1.6f), Vector3.zero, new Color(0.86f, 0.94f, 1), 24, 3.5f);
        EditorSceneManager.SaveScene(scene, path);
    }

    private static Material StageMaterial(string name, Color color, float metallic, bool unlit = false)
    {
        var material = LoadOrCreateMaterial(ArtFolder + "/Materials/" + name + ".mat",
            Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
        material.SetColor("_BaseColor", color);
        if (!unlit) { material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", 0.45f); }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        box.transform.localPosition = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(box.GetComponent<Collider>());
    }

    private static void CreateDockRing(Transform parent, Material material)
    {
        const int segments = 96;
        var mesh = new Mesh { name = "RobotDockRing" };
        var vertices = new Vector3[segments * 2];
        var triangles = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            vertices[i * 2] = direction * 1.04f;
            vertices[i * 2 + 1] = direction * 1.065f;
            int next = (i + 1) % segments * 2, offset = i * 6;
            triangles[offset] = i * 2; triangles[offset + 1] = next; triangles[offset + 2] = i * 2 + 1;
            triangles[offset + 3] = i * 2 + 1; triangles[offset + 4] = next; triangles[offset + 5] = next + 1;
        }
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals();
        string path = ArtFolder + "/Meshes/RobotDockRing.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing == null) AssetDatabase.CreateAsset(mesh, path);
        else { EditorUtility.CopySerialized(mesh, existing); Object.DestroyImmediate(mesh); mesh = existing; }
        var ring = new GameObject("DockLightRing", typeof(MeshFilter), typeof(MeshRenderer));
        ring.transform.SetParent(parent);
        ring.transform.localPosition = new Vector3(1.28f, 0.013f, 0);
        ring.GetComponent<MeshFilter>().sharedMesh = mesh;
        ring.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Camera NewCamera(string name, Vector3 position, Vector3 target)
    {
        var camera = new GameObject(name, typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.033f, 0.052f);
        camera.transform.position = position;
        camera.transform.LookAt(target);
        return camera;
    }

    private static void AddLight(string name, LightType type, Vector3 position, Vector3 angles, Color color, float intensity, float range = 8)
    {
        var light = new GameObject(name, typeof(Light)).GetComponent<Light>();
        light.type = type; light.color = color; light.intensity = intensity;
        light.range = range; light.shadows = type == LightType.Directional ? LightShadows.Soft : LightShadows.None;
        light.transform.position = position; light.transform.rotation = Quaternion.Euler(angles);
    }

    private static RuntimeAnimatorController LoadIdleController()
    {
        const string path = ArtFolder + "/Animations/RobotMenuIdle.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var machine = controller.layers[0].stateMachine;
        var state = machine.states.Length == 0 ? machine.AddState("Robot idle") : machine.states[0].state;
        state.motion = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Game/Art/Animations/PhysicalElements/Characters/Corpo/Tps/Idle/ANIM_Corpo_TPS_Idle.fbx")
            .OfType<AnimationClip>().First(clip => !clip.name.StartsWith("__", StringComparison.Ordinal));
        machine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void CapturePortrait(GameObject prefab, string name)
    {
        var preview = new PreviewRenderUtility();
        try
        {
            var robot = Object.Instantiate(prefab);
            robot.transform.rotation = Quaternion.Euler(0, 180, 0);
            foreach (var renderer in robot.GetComponentsInChildren<SkinnedMeshRenderer>()) renderer.sharedMaterials = new[] { renderer.sharedMaterials[0] };
            var animator = robot.GetComponentInChildren<Animator>();
            animator.runtimeAnimatorController = LoadIdleController();
            animator.Update(0);
            animator.enabled = false;
            preview.AddSingleGO(robot);
            Transform head = CommonCharacterModelUtils.FindBoneByName(robot.transform, "J_HEAD");
            Vector3 target = head.position + new Vector3(0, -0.08f, 0);
            preview.camera.orthographic = true;
            preview.camera.orthographicSize = 0.4f;
            preview.camera.nearClipPlane = 0.01f;
            preview.camera.farClipPlane = 20;
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = Color.clear;
            preview.camera.transform.position = target + new Vector3(0, 0.04f, -3);
            preview.camera.transform.LookAt(target);
            preview.lights[0].intensity = 2.5f;
            preview.lights[0].transform.rotation = Quaternion.Euler(35, 145, 0);
            preview.lights[1].intensity = 1.6f;
            preview.lights[1].transform.rotation = Quaternion.Euler(20, 235, 0);
            preview.ambientColor = new Color(0.25f, 0.3f, 0.35f);
            preview.BeginStaticPreview(new Rect(0, 0, 512, 512));
            preview.Render(true);
            Texture2D portrait = preview.EndStaticPreview();
            string path = ArtFolder + "/Portraits/" + name + ".png";
            File.WriteAllBytes(path, portrait.EncodeToPNG());
            Object.DestroyImmediate(portrait);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 512;
            importer.SaveAndReimport();
        }
        finally { preview.Cleanup(); }
    }

    private static void CreateOpening(PanelSettings panel)
    {
        const string path = "Assets/Game/Scenes/SplashScreen.unity";
        Backup(path);
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        NewCamera("OpeningCamera", new Vector3(0, 0, -10), Vector3.zero);
        var root = new GameObject("SteelProtocolOpening", typeof(UIDocument), typeof(OpeningScreenController));
        var document = root.GetComponent<UIDocument>();
        // NewScene may unload the previous scene's panel object; reload the saved asset.
        document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
        document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Loading/OpeningScreen.uxml");
        EditorSceneManager.SaveScene(scene, path);
    }
}
