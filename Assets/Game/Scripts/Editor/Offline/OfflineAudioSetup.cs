using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Uses the checked-in banks; installing the Wwise authoring app is unnecessary.</summary>
[InitializeOnLoad]
public sealed class OfflineAudioSetup : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;
    static OfflineAudioSetup() => EditorApplication.delayCall += SyncWindowsBanks;

    [MenuItem("Tools/Offline/Sync existing audio banks")]
    public static void SyncWindowsBanks()
    {
        var root = Path.GetDirectoryName(Application.dataPath);
        var source = Path.Combine(root, "Audio", "WwiseProject", "GeneratedSoundBanks", "Windows");
        var target = Path.Combine(Application.streamingAssetsPath, "Audio", "GeneratedSoundBanks", "Windows");
        if (!Directory.Exists(source)) return;
        int copied = 0;
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = file.Substring(source.Length + 1);
            var destination = Path.Combine(target, relative);
            if (File.Exists(destination) && new FileInfo(destination).Length == new FileInfo(file).Length
                && File.GetLastWriteTimeUtc(destination) >= File.GetLastWriteTimeUtc(file)) continue;
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(file, destination, true);
            copied++;
        }
        if (copied > 0) Debug.Log($"[Offline] Synced {copied} existing Windows audio bank files.");
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.StandaloneWindows64 && report.summary.platform != BuildTarget.StandaloneWindows) return;
        SyncWindowsBanks();
        if (!File.Exists(Path.Combine(Application.streamingAssetsPath, "Audio/GeneratedSoundBanks/Windows/Init.bnk")))
            throw new BuildFailedException("The checked-in Windows Wwise Init bank is missing.");
    }
}
