using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Runs the existing offline integration suite in a single batch editor.</summary>
[InitializeOnLoad]
public static class OfflineSmokeBatch
{
    private const string Active = "Suna.SmokeBatch.Active";
    private const string StartedAt = "Suna.SmokeBatch.StartedAt";

    static OfflineSmokeBatch() => EditorApplication.update += ExitWhenFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void PrepareInput()
    {
        if (!Application.isBatchMode || !SessionState.GetBool(Active, false)) return;
        if (Keyboard.current == null) InputSystem.AddDevice<Keyboard>();
        if (Mouse.current == null) InputSystem.AddDevice<Mouse>();
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("This entry point requires Unity batch mode.");
        ProjectStructureValidation.Validate();
        RobotPresentationValidation.Validate();
        PresentationSmokeBatch.SetGameViewSize(1600, 900);
        SessionState.SetBool(Active, true);
        SessionState.SetString(StartedAt, DateTime.UtcNow.Ticks.ToString());
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        OfflineMatchValidation.BeginSmoke();
    }

    private static void ExitWhenFinished()
    {
        if (!Application.isBatchMode || !SessionState.GetBool(Active, false) ||
            EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!long.TryParse(SessionState.GetString(StartedAt, ""), out long ticks)) return;
        string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs/OfflineVerification/smoke-result.txt");
        if (!File.Exists(path) || File.GetLastWriteTimeUtc(path).Ticks < ticks) return;
        string result = File.ReadAllText(path).Replace("\r\n", "\n");
        if (!result.StartsWith("PASS\n", StringComparison.Ordinal) && !result.StartsWith("FAIL\n", StringComparison.Ordinal)) return;
        SessionState.SetBool(Active, false);
        EditorApplication.Exit(result.StartsWith("PASS\n", StringComparison.Ordinal) ? 0 : 1);
    }
}
