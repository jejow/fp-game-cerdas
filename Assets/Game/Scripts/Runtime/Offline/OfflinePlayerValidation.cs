#if !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameNetwork;
using GameNetwork.Utils;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

/// <summary>Explicit command-line verification; inactive during normal game startup.</summary>
public static class OfflinePlayerValidation
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        var args = Environment.GetCommandLineArgs();
        if (!args.Contains("-suna-offline-check")) return;
        int index = Array.IndexOf(args, "-suna-offline-output");
        string folder = index >= 0 && index + 1 < args.Length
            ? args[index + 1] : Path.Combine(Application.persistentDataPath, "OfflineVerification");
        _ = RunAsync(folder);
    }

    private static async Task RunAsync(string folder)
    {
        var report = new List<string>();
        var errors = new List<string>();
        void Capture(string condition, string stack, LogType type)
        {
            if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && errors.Count < 100)
                errors.Add(condition);
        }
        Application.logMessageReceived += Capture;
        Application.runInBackground = true;
        Directory.CreateDirectory(folder);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        bool passed = false;
        try
        {
            await Awaitable.NextFrameAsync(timeout.Token);
            await Awaitable.NextFrameAsync(timeout.Token);
            await OfflineMatch.StartAsync(new OfflineMatchOptions(4, 5, BotDifficulty.Easy, TeamSideType.Corpo));
            var server = ClientTransportHelper.ServerWorld;
            while (!OfflineMatch.HasSpawnedTeams(server.EntityManager)) await Awaitable.NextFrameAsync(timeout.Token);
            using (var query = server.EntityManager.CreateEntityQuery(typeof(RoundComponent)))
            {
                var round = query.GetSingleton<RoundComponent>();
                round.timer = 0;
                query.SetSingleton(round);
            }
            double warmup = Time.realtimeSinceStartupAsDouble + 5;
            while (Time.realtimeSinceStartupAsDouble < warmup) await Awaitable.NextFrameAsync(timeout.Token);
            using var timeQuery = server.EntityManager.CreateEntityQuery(typeof(NetworkTime));
            using var rateQuery = server.EntityManager.CreateEntityQuery(typeof(ClientServerTickRate));
            NetworkTick tickStart = timeQuery.GetSingleton<NetworkTime>().ServerTick;
            double start = Time.realtimeSinceStartupAsDouble;
            int frames = 0, batches = 0;
            while (Time.realtimeSinceStartupAsDouble - start < 20)
            {
                await Awaitable.NextFrameAsync(timeout.Token);
                frames++;
                if (timeQuery.GetSingleton<NetworkTime>().SimulationStepBatchSize > 1) batches++;
            }
            double seconds = Time.realtimeSinceStartupAsDouble - start;
            int ticks = timeQuery.GetSingleton<NetworkTime>().ServerTick.TicksSince(tickStart);
            report.Add($"5 vs 5 Easy; resolution={Screen.width}x{Screen.height}; graphics={SystemInfo.graphicsDeviceName}");
            report.Add($"seconds={seconds:F2}; frames={frames}; average FPS={frames / seconds:F2}; batched frames={batches}");
            report.Add($"server ticks={ticks}; tick rate={rateQuery.GetSingleton<ClientServerTickRate>().SimulationTickRate}");
            using var botsQuery = server.EntityManager.CreateEntityQuery(typeof(OfflineBot));
            int actualBots = botsQuery.CalculateEntityCount();
            report.Add($"spawned bot count={actualBots}; Wwise initialized={AkUnitySoundEngine.IsInitialized()}");
            if (actualBots != 9) throw new Exception("The expected nine bots were not created.");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "runtime-fix-player-gameplay.png"));
            for (int i = 0; i < 10; i++) await Awaitable.NextFrameAsync(timeout.Token);
            if (errors.Count != 0) throw new Exception($"Gameplay logged {errors.Count} errors.");
            passed = true;
        }
        catch (Exception exception)
        {
            report.Add(exception.ToString());
        }
        finally
        {
            try { if (OfflineMatch.IsActive) await OfflineMatch.StopAsync(); }
            catch (Exception exception) { errors.Add(exception.ToString()); passed = false; }
            Application.logMessageReceived -= Capture;
            report.Add($"errors including cleanup={errors.Count}");
            report.AddRange(errors.Take(10));
            File.WriteAllLines(Path.Combine(folder, "runtime-fix-player-result.txt"), new[] { passed && errors.Count == 0 ? "PASS" : "FAIL" }.Concat(report));
            Debug.Log("[OfflinePlayerValidation] " + (passed && errors.Count == 0 ? "PASS" : "FAIL"));
            Application.Quit(passed && errors.Count == 0 ? 0 : 1);
        }
    }
}
#endif
