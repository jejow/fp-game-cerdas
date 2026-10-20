using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

/// <summary>Exercises opening, navigation, and two widescreen layouts in Play Mode.</summary>
[InitializeOnLoad]
public static class PresentationSmokeBatch
{
    private const string Active = "SteelProtocol.PresentationSmoke.Active";
    private const string Finished = "SteelProtocol.PresentationSmoke.Finished";
    private const string Succeeded = "SteelProtocol.PresentationSmoke.Succeeded";
    private static readonly string Output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Docs/Validation");

    [Serializable] private sealed class Report
    {
        public string utc;
        public string status;
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
    }

    static PresentationSmokeBatch() => EditorApplication.playModeStateChanged += OnPlayState;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void PrepareInput()
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (Keyboard.current == null) InputSystem.AddDevice<Keyboard>();
        if (Mouse.current == null) InputSystem.AddDevice<Mouse>();
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use the batch entry point for this suite.");
        ProjectStructureValidation.Validate();
        RobotPresentationValidation.Validate();
        SetGameViewSize(1600, 900);
        SessionState.SetBool(Active, true);
        SessionState.SetBool(Finished, false);
        EditorSceneManager.OpenScene("Assets/Game/Scenes/SplashScreen.unity");
        var opening = UnityEngine.Object.FindFirstObjectByType<OpeningScreenController>();
        // A test-only hold allows a rendered opening frame to be captured before Skip.
        var serialized = new SerializedObject(opening);
        serialized.FindProperty("minimumDisplaySeconds").floatValue = 600;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorApplication.isPlaying = true;
    }

    public static void SetGameViewSize(int width, int height)
    {
        var assembly = typeof(EditorWindow).Assembly;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
        var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
        var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType, "Standalone") });
        int index = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var sizeType = assembly.GetType("UnityEditor.GameViewSize");
        var kindType = assembly.GetType("UnityEditor.GameViewSizeType");
        var size = Activator.CreateInstance(sizeType, new object[] { Enum.Parse(kindType, "FixedResolution"), width, height, "Robot presentation QA" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        var window = EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));
        window.GetType().GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, index);
        window.Focus();
    }

    private static void OnPlayState(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode) RunChecks();
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Finished, false))
        {
            SessionState.SetBool(Active, false);
            EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
            EditorApplication.Exit(SessionState.GetBool(Succeeded, false) ? 0 : 1);
        }
    }

    private static async void RunChecks()
    {
        var report = new Report { utc = DateTime.UtcNow.ToString("o") };
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(4));
        var token = cancellation.Token;
        void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException(description);
            report.checks.Add(description);
        }
        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message);
        }
        Application.logMessageReceived += Log;
        bool background = Application.runInBackground;
        Application.runInBackground = true;
        try
        {
            await Frames(3, token);
            var opening = UnityEngine.Object.FindFirstObjectByType<OpeningScreenController>();
            Check(opening != null, "Opening starts from the first build scene.");
            var openingRoot = opening.GetComponent<UIDocument>().rootVisualElement;
            Check(openingRoot.panel != null, "Opening UI is attached to a rendering panel.");
            await Until(() => openingRoot.Q<Label>(className: "game-title").worldBound.width > 0, token);
            await Frames(4, token);
            Check(openingRoot.Q<Label>(className: "game-title").text == GameIdentity.Name, "Opening displays STEEL PROTOCOL.");
            Check(UnityEngine.Object.FindFirstObjectByType<VideoPlayer>() == null, "Opening has no original logo video.");
            await Capture("opening-20261020.png", token);
            opening.Skip();
            await Until(() => SceneManager.GetActiveScene().name == "MainMenu", token);
            await Frames(10, token);
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            Check(menu != null, "Skip reaches the functional main menu.");
            var root = menu.GetComponent<UIDocument>().rootVisualElement;
            Check(root.Q<Label>(className: "game-title").text == GameIdentity.Name, "Main menu displays the chosen title.");
            Check(UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None)
                .All(renderer => !AssetDatabase.GetAssetPath(renderer.sharedMesh).Contains("/Characters/Natif/")), "Menu contains only robot character meshes.");
            Check(GameObject.Find("MenuAssaultRobot").GetComponentsInChildren<SkinnedMeshRenderer>()
                .All(renderer => renderer.sharedMaterials.Length == 1 && renderer.sharedMaterials[0].shader.name == "Game/Robot Armor"),
                "Menu robot renders its armor without the gameplay outline overlay.");
            Check(Fits(root, root.Q("PlayButton")), "Main menu controls fit at 1600 x 900.");
            await Capture("main-menu-20261020.png", token);
            Click(root, "PlayButton");
            await Frames(3, token);
            Check(root.Q("BotSetup").resolvedStyle.display == DisplayStyle.Flex, "Play Offline opens robot match setup.");
            Check(Fits(root, root.Q("StartOffline")) && Fits(root.Q("BotSetup"), root.Q("StartOffline")), "Start Match fits inside the setup panel.");
            await Capture("bot-setup-20261020.png", token);
            Click(root, "BackFromBotSetup");
            await Frames(2, token);
            Click(root, "SettingsButton");
            await Frames(3, token);
            Check(UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>() != null, "Settings opens from the new menu.");
            Check(Fits(root, root.Q("SaveButton")), "Settings Save control fits on screen.");
            Check(root.Q(className: "settings-card").resolvedStyle.backgroundColor.a > 0.95f, "Settings keeps its panel stylesheet after instantiation.");
            await Capture("settings-20261020.png", token);
            Click(root, "SaveButton");
            await Frames(2, token);
            Check(root.Q("SaveButton") == null, "Settings Save and Close returns to the menu.");
            Check(root.Q<Button>("CreditsButton") == null, "Main menu has no contributor page entry.");
            Check(!root.Query<Label>().ToList().Any(label => label.text.IndexOf("CONTRIBUTORS", StringComparison.OrdinalIgnoreCase) >= 0),
                "Menu UI contains no contributor list.");
            SetGameViewSize(1280, 720);
            await Frames(5, token);
            Check(Fits(root, root.Q("PlayButton")) && Fits(root, root.Q("QuitButton")), "Main menu controls fit at 1280 x 720.");
            await Capture("main-menu-720p-20261020.png", token);
            Click(root, "PlayButton");
            await Frames(4, token);
            Check(Fits(root, root.Q("StartOffline")) && Fits(root.Q("BotSetup"), root.Q("StartOffline")), "Robot match setup remains usable at 1280 x 720.");
            await Capture("bot-setup-720p-20261020.png", token);
            Click(root, "BackFromBotSetup");
            Check(report.errors.Count == 0, "Presentation produces no runtime errors.");
            RobotPresentationValidation.Validate();
            report.status = "PASS";
        }
        catch (Exception exception)
        {
            report.status = "FAIL";
            report.errors.Add(exception.ToString());
        }
        finally
        {
            Application.logMessageReceived -= Log;
            Application.runInBackground = background;
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output, "presentation-smoke.json"), JsonUtility.ToJson(report, true));
            SessionState.SetBool(Succeeded, report.status == "PASS");
            SessionState.SetBool(Finished, true);
            EditorApplication.isPlaying = false;
        }
    }

    private static bool Fits(VisualElement outer, VisualElement inner)
    {
        Rect a = outer.worldBound, b = inner.worldBound;
        return b.width > 0 && b.height > 0 && b.xMin >= a.xMin - 2 && b.yMin >= a.yMin - 2 && b.xMax <= a.xMax + 2 && b.yMax <= a.yMax + 2;
    }

    private static void Click(VisualElement root, string name)
    {
        var button = root.Q<Button>(name);
        if (button == null) throw new InvalidOperationException("Missing button: " + name);
        using var submit = NavigationSubmitEvent.GetPooled();
        submit.target = button;
        button.SendEvent(submit);
    }

    private static async Task Until(Func<bool> predicate, CancellationToken token)
    {
        while (!predicate()) await Awaitable.NextFrameAsync(token);
    }

    private static async Task Frames(int count, CancellationToken token)
    {
        for (int i = 0; i < count; i++) await Awaitable.NextFrameAsync(token);
    }

    private static async Task Capture(string name, CancellationToken token)
    {
        ScreenCapture.CaptureScreenshot(Path.Combine(Output, name));
        await Frames(3, token);
    }
}
