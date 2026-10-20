using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameNetwork.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine.UIElements;

/// <summary>Repeatable local validation, also callable by a local command file in an open editor.</summary>
[InitializeOnLoad]
public static class OfflineMatchValidation
{
    private const string Pending = "Suna.OfflineValidation.Pending";
    private const string Started = "Suna.OfflineValidation.Started";
    private const string RuntimeProbe = "Suna.OfflineValidation.RuntimeProbe";
    private static bool running;
    private static CancellationTokenSource smokeCancellation;
    private static double pollAt;
    private static double diagnosticAt;
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> errors = new List<string>();
    private static readonly Dictionary<string, int> runtimeWarnings = new Dictionary<string, int>();
    private static readonly string[] runtimeWarningKeys = { "Server Tick Batching has occurred", "Voice Starvation", "Ignoring invalid", "creates a query during OnUpdate", "creates a Lookup object", "has not been consumed or destroyed", "JobTempAlloc", "D3D11: failed" };
    private static string Folder => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs/OfflineVerification");

    static OfflineMatchValidation()
    {
        EditorApplication.update += Poll;
        EditorApplication.playModeStateChanged += OnPlayState;
        Application.logMessageReceived += OnLog;
    }

    private static void OnLog(string condition, string stack, LogType type)
    {
        if (running && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
            errors.Add($"{condition} [scene={SceneManager.GetActiveScene().name}; loading={GameNetwork.SessionData.Instance.LoadingStatusText}; time={Time.realtimeSinceStartup:F2}]");
        if (running && (type == LogType.Warning || type == LogType.Error))
        {
            foreach (var key in runtimeWarningKeys)
                if (condition.Contains(key)) runtimeWarnings[key] = runtimeWarnings.TryGetValue(key, out var count) ? count + 1 : 1;
        }
    }

    private static void Poll()
    {
        if (EditorApplication.timeSinceStartup < pollAt || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        pollAt = EditorApplication.timeSinceStartup + 0.5;
        if (running) EditorApplication.QueuePlayerLoopUpdate();
        if (running && EditorApplication.timeSinceStartup >= diagnosticAt)
        {
            diagnosticAt = EditorApplication.timeSinceStartup + 10;
            WriteState();
        }
        if (SessionState.GetBool(Pending, false) && EditorApplication.isPlaying && !running && !SessionState.GetBool(Started, false))
        {
            SessionState.SetBool(Pending, false);
            SessionState.SetBool(Started, true);
            running = true;
            if (SessionState.GetBool(RuntimeProbe, false)) _ = RunRuntimeProbeAsync();
            else _ = RunSmokeAsync();
            return;
        }
        var request = Path.Combine(Folder, "request.txt");
        if (!File.Exists(request)) return;
        var command = File.ReadAllText(request).Trim();
        File.Delete(request);
        if (command == "inspect") WriteResult($"READY scene={SceneManager.GetActiveScene().name}; playing={EditorApplication.isPlaying}; dirty={HasDirtyScenes()}");
        else if (command == "validate")
        {
            try { RunEntityChecks(); WriteResult("PASS " + string.Join("; ", checks)); }
            catch (Exception exception) { WriteResult("FAIL " + exception); }
        }
        else if (command == "refresh") AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        else if (command == "diagnose") WriteState();
        else if (command == "lights") WriteLightCounts();
        else if (command == "build") BuildValidationPlayer();
        else if (command == "smoke") { SessionState.SetBool(RuntimeProbe, false); BeginSmoke(); }
        else if (command == "runtime") { SessionState.SetBool(RuntimeProbe, true); BeginSmoke(); }
        else if (command == "focus") FocusGameView();
        else if (command == "quit")
        {
            if (EditorApplication.isPlaying || HasDirtyScenes()) WriteResult("BLOCKED editor quit requires stopped Play Mode and saved scenes.");
            else EditorApplication.Exit(0);
        }
        else if (command == "stop")
        {
            SessionState.SetBool(Pending, false);
            if (running)
            {
                smokeCancellation?.Cancel();
                OfflineMatch.CancelLoading();
            }
            else
            {
                SessionState.SetBool(Started, false);
                EditorApplication.isPlaying = false;
            }
        }
    }

    private static bool HasDirtyScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) return true;
        return false;
    }

    private static void WriteLightCounts()
    {
        var lines = new List<string>();
        foreach (var path in new[]
        {
            "Assets/Game/Scenes/MainMenu.unity",
            "Assets/Game/Scenes/Gameplay.unity",
            "Assets/Game/Scenes/SubScenes/GameplayEntities.unity"
        })
        {
            var scene = EditorSceneManager.OpenPreviewScene(path);
            try
            {
                var lights = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true)).ToArray();
                lines.Add($"{path}: total={lights.Length}; active={lights.Count(light => light.enabled && light.gameObject.activeInHierarchy)}; runtime={lights.Count(light => light.enabled && light.gameObject.activeInHierarchy && light.lightmapBakeType != LightmapBakeType.Baked)}");
                foreach (var light in lights)
                    lines.Add($"  {light.name}; type={light.type}; active={light.enabled && light.gameObject.activeInHierarchy}; mode={light.lightmapBakeType}");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
        Directory.CreateDirectory(Folder);
        File.WriteAllLines(Path.Combine(Folder, "light-counts.txt"), lines);
        WriteResult("LIGHTS " + string.Join("; ", lines.Where(line => !line.StartsWith("  "))));
    }

    private static void CheckRenderingLightBudget(string match)
    {
        var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
            .Where(light => light.enabled && light.gameObject.activeInHierarchy && light.lightmapBakeType != LightmapBakeType.Baked).ToArray();
        int limit = UnityEngine.Rendering.Universal.ShaderOptions.k_MaxVisibleLightCountDesktop;
        File.AppendAllText(Path.Combine(Folder, "live-light-counts.txt"), $"{match}: runtime lights={lights.Length}; desktop limit={limit}\n");
        Check(lights.Length <= limit, $"{match} lighting fits the desktop light budget ({lights.Length}/{limit})");
    }

    [MenuItem("Tools/Offline/Validate objective rules")]
    public static void RunEntityChecks()
    {
        checks.Clear();
        using var world = new World("Offline rule validation");
        var em = world.EntityManager;
        var character = em.CreateEntity(typeof(CharacterIsEnable), typeof(CurrentHealthComponent), typeof(LocalTransform),
            typeof(PlayerHarvesterActions), typeof(CorpoTeamTag), typeof(PhysicsCollider));
        em.SetComponentData(character, LocalTransform.Identity);
        em.SetComponentData(character, new CurrentHealthComponent { Value = 100 });
        var characterCollider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry
        {
            Vertex0 = new float3(0, -0.5f, 0), Vertex1 = new float3(0, 0.5f, 0), Radius = 0.5f
        }, CollisionFilter.Default);
        em.SetComponentData(character, new PhysicsCollider { Value = characterCollider });
        var site = em.CreateEntity(typeof(BombSiteTriggerComponent), typeof(LocalToWorld), typeof(PhysicsCollider));
        var collider = Unity.Physics.BoxCollider.Create(new BoxGeometry
        {
            Center = float3.zero, Size = new float3(10), Orientation = quaternion.identity, BevelRadius = 0
        }, CollisionFilter.Default);
        em.SetComponentData(site, new LocalToWorld { Value = float4x4.identity });
        em.SetComponentData(site, new PhysicsCollider { Value = collider });
        var harvester = em.CreateEntity(typeof(HarvesterComponent), typeof(HarvesterPlanting), typeof(HarvesterDefusing),
            typeof(HarvesterPlanted), typeof(StuffDynamicData), typeof(LocalTransform));
        em.SetComponentData(harvester, new HarvesterComponent { defuseRange = 2 });
        em.SetComponentData(harvester, LocalTransform.Identity);
        em.SetComponentData(harvester, new StuffDynamicData { owner = character });
        em.SetComponentEnabled<HarvesterPlanted>(harvester, false);
        em.SetComponentEnabled<HarvesterPlanting>(harvester, false);
        em.SetComponentEnabled<HarvesterDefusing>(harvester, false);
        try
        {
            var tick = new NetworkTick(100);
            Check(!HarvesterActions.TryStartPlant(em, character, harvester, tick, RoundPhase.BuyPhase), "plant rejected in buy phase");
            Check(HarvesterActions.TryStartPlant(em, character, harvester, tick, RoundPhase.ActionPhase), "valid owner can plant on site");
            HarvesterActions.CancelPlant(em, harvester);
            Check(!em.GetComponentData<PlayerHarvesterActions>(character).IsPlanting, "plant cancellation clears character state");
            em.SetComponentData(character, LocalTransform.FromPosition(new float3(0, 5.025f, 0)));
            Check(HarvesterActions.CanPlant(em, character, harvester, RoundPhase.ActionPhase), "capsule overlapping a low site can plant with its center above the trigger");
            em.SetComponentData(character, LocalTransform.FromPosition(new float3(0, 6.1f, 0)));
            Check(!HarvesterActions.CanPlant(em, character, harvester, RoundPhase.ActionPhase), "plant rejected when the entire capsule is outside the site");
            em.SetComponentData(character, LocalTransform.Identity);
            em.RemoveComponent<CorpoTeamTag>(character);
            em.AddComponent<NatifTeamTag>(character);
            em.SetComponentEnabled<HarvesterPlanted>(harvester, true);
            Check(HarvesterActions.TryStartDefuse(em, character, harvester, tick, RoundPhase.PostPlantPhase), "native can defuse in range");
            em.SetComponentData(character, LocalTransform.FromPosition(new float3(10, 0, 0)));
            Check(!HarvesterActions.CanDefuse(em, character, harvester, RoundPhase.PostPlantPhase), "defuse rejected outside range");
            HarvesterActions.CancelDefuse(em, harvester);
            Check(!em.GetComponentData<PlayerHarvesterActions>(character).IsDefusing, "defuse cancellation clears state before dropping entity references");
            em.DestroyEntity(character);
            HarvesterActions.CancelDefuse(em, harvester);
            Check(true, "deleted defuser cleanup is safe");
        }
        finally { collider.Dispose(); characterCollider.Dispose(); }
        Debug.Log("[OfflineValidation] " + string.Join("; ", checks));
    }

    [MenuItem("Tools/Offline/Run offline match smoke test")]
    public static void BeginSmoke()
    {
        if (EditorApplication.isPlaying)
        {
            WriteResult("BLOCKED Stop Play Mode before running the offline smoke test.");
            return;
        }
        // Bake while gameplay worlds are absent, avoiding concurrent scene/texture copies.
        var cache = Path.Combine(Application.dataPath, "SceneDependencyCache");
        if (Directory.Exists(cache))
        {
            WriteResult("RUNNING preparing entity scene imports before Play Mode");
            var importErrors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception) importErrors.Add(message);
            };
            Application.logMessageReceived += capture;
            try
            {
                foreach (var path in Directory.GetFiles(cache, "*.sceneWithBuildSettings").OrderBy(path => path))
                    AssetDatabase.ImportAsset("Assets/SceneDependencyCache/" + Path.GetFileName(path), ImportAssetOptions.ForceSynchronousImport);
            }
            finally { Application.logMessageReceived -= capture; }
            if (importErrors.Count > 0)
            {
                WriteResult("FAIL entity scene import\n" + string.Join("\n", importErrors));
                return;
            }
        }
        SessionState.SetBool(Pending, true);
        SessionState.SetBool(Started, false);
        // Load the menu only in Play Mode, preserving even unsaved editor scenes.
        WriteResult("RUNNING importing gameplay and starting the offline match");
        EditorApplication.isPaused = false;
        FocusGameView();
        EditorApplication.isPlaying = true;
    }

    private static void FocusGameView()
    {
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if (type != null) EditorWindow.GetWindow(type).Focus();
    }

    private static void WriteState()
    {
        var lines = new List<string>
        {
            $"scene={SceneManager.GetActiveScene().name}; playing={EditorApplication.isPlaying}; paused={EditorApplication.isPaused}; loading={GameNetwork.SessionData.Instance.LoadingStatusText}; progress={GameNetwork.SessionData.Instance.LoadingProgress}"
        };
        foreach (var world in World.All)
        {
            if (!world.IsCreated) continue;
            lines.Add($"{world.Name}; time={world.Time.ElapsedTime}; loop={ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world)}; initialization={world.GetExistingSystemManaged<InitializationSystemGroup>()?.Enabled}; scenes={world.GetExistingSystemManaged<Unity.Scenes.SceneSystemGroup>()?.Enabled}");
        }
        Directory.CreateDirectory(Folder);
        File.WriteAllLines(Path.Combine(Folder, "editor-state.txt"), lines);
    }

    public static void BuildValidationPlayerBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("The batch build entry point requires Unity batch mode.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildValidationPlayer();
        var resultPath = Path.Combine(Folder, "build-result.txt");
        if (!File.Exists(resultPath) || !File.ReadAllText(resultPath).StartsWith("BUILD Succeeded"))
            throw new BuildFailedException(File.Exists(resultPath) ? File.ReadAllText(resultPath) : "The batch build did not produce a result.");
    }

    [MenuItem("Tools/Offline/Build Windows validation player")]
    public static void BuildValidationPlayer()
    {
        if (EditorApplication.isPlaying)
        {
            WriteResult("BLOCKED Stop Play Mode before building the validation player.");
            return;
        }
        if (HasDirtyScenes())
        {
            WriteResult("BUILD BLOCKED Save scene changes before building, or use a separate project copy for the batch build.");
            return;
        }
        WriteResult("BUILDING Windows offline validation player");
        var buildErrors = new List<string>();
        void CaptureBuildError(string condition, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                buildErrors.Add($"[{type}] {condition}\n{stack}");
        }
        Application.logMessageReceived += CaptureBuildError;
        try
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Builds/Offline", GameIdentity.ExecutableName),
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)StandaloneBuildSubtarget.Player,
                options = BuildOptions.None
            });
            var details = new List<string>
            {
                $"Finished: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Result: {report.summary.result}",
                $"Output: {report.summary.outputPath}",
                $"Size: {report.summary.totalSize} bytes",
                $"Duration: {report.summary.totalTime}",
                $"Errors: {report.summary.totalErrors}; warnings: {report.summary.totalWarnings}"
            };
            foreach (var step in report.steps)
                foreach (var message in step.messages)
                    if (message.type == LogType.Warning || message.type == LogType.Error || message.type == LogType.Exception || message.type == LogType.Assert)
                        details.Add($"[{message.type}] {step.name}: {message.content}");
            details.AddRange(buildErrors);
            Directory.CreateDirectory(Folder);
            File.WriteAllLines(Path.Combine(Folder, "build-details.txt"), details);
            var status = report.summary.totalErrors == 0 && buildErrors.Count == 0
                ? report.summary.result.ToString() : "FAIL " + report.summary.result;
            WriteResult($"BUILD {status}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}");
        }
        catch (Exception exception) { WriteResult("BUILD FAIL " + exception); }
        finally { Application.logMessageReceived -= CaptureBuildError; }
    }
    private static async Task RunRuntimeProbeAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        smokeCancellation = timeout;
        var token = timeout.Token;
        bool backgroundWasEnabled = Application.runInBackground;
        Application.runInBackground = true;
        checks.Clear();
        errors.Clear();
        runtimeWarnings.Clear();
        var report = new List<string>();
        List<RuntimeCounter> counters = null;
        try
        {
            if (SceneManager.GetActiveScene().name != "MainMenu") await LoadUtils.LoadSceneAsync("MainMenu", GameNetwork.SessionData.LoadingSteps.BackToMainMenu);
            await OfflineMatch.StartAsync(new OfflineMatchOptions(4, 5, BotDifficulty.Easy, TeamSideType.Corpo));
            await WaitUntilAsync(() => OfflineMatch.HasSpawnedTeams(ClientTransportHelper.ServerWorld.EntityManager), 60f, "both teams spawned", token);
            var server = ClientTransportHelper.ServerWorld;
            var em = server.EntityManager;
            using var roundQuery = em.CreateEntityQuery(typeof(RoundComponent));
            var round = roundQuery.GetSingleton<RoundComponent>();
            round.timer = 0;
            roundQuery.SetSingleton(round);
            double warmupUntil = Time.realtimeSinceStartupAsDouble + 5;
            while (Time.realtimeSinceStartupAsDouble < warmupUntil) await Awaitable.NextFrameAsync(token);
            var warningsAtStart = new Dictionary<string, int>(runtimeWarnings);
            int errorsAtStart = errors.Count;
            using var networkTimeQuery = em.CreateEntityQuery(typeof(NetworkTime));
            using var rateQuery = em.CreateEntityQuery(typeof(ClientServerTickRate));
            using var main = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Internal, "Main Thread", 1);
            using var render = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Internal, "Render Thread", 1);
            counters = CreateRuntimeCounters();
            var frameMs = new List<double>(4096);
            double mainTotal = 0, renderTotal = 0;
            int mainSamples = 0, renderSamples = 0, batchedFrames = 0;
            double wallStart = Time.realtimeSinceStartupAsDouble, lastFrame = wallStart;
            double simulationStart = server.Time.ElapsedTime;
            NetworkTick tickStart = networkTimeQuery.GetSingleton<NetworkTime>().ServerTick;
            while (Time.realtimeSinceStartupAsDouble - wallStart < 60)
            {
                await Awaitable.NextFrameAsync(token);
                double now = Time.realtimeSinceStartupAsDouble;
                frameMs.Add((now - lastFrame) * 1000);
                lastFrame = now;
                if (main.Valid && main.LastValue > 0) { mainTotal += main.LastValue / 1e6; mainSamples++; }
                if (render.Valid && render.LastValue > 0) { renderTotal += render.LastValue / 1e6; renderSamples++; }
                if (networkTimeQuery.GetSingleton<NetworkTime>().SimulationStepBatchSize > 1) batchedFrames++;
                foreach (var counter in counters) counter.Sample();
            }
            double wallElapsed = Time.realtimeSinceStartupAsDouble - wallStart;
            double simulationElapsed = server.Time.ElapsedTime - simulationStart;
            int ticksElapsed = networkTimeQuery.GetSingleton<NetworkTime>().ServerTick.TicksSince(tickStart);
            frameMs.Sort();
            report.Add($"5 vs 5 Easy; Unity Play Mode; graphics={SystemInfo.graphicsDeviceName}; job workers={Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount}; tick rate={rateQuery.GetSingleton<ClientServerTickRate>().SimulationTickRate}");
            report.Add($"wall seconds={wallElapsed:F2}; simulation seconds={simulationElapsed:F2}; frames={frameMs.Count}; average FPS={frameMs.Count / wallElapsed:F2}; median frame ms={frameMs[frameMs.Count / 2]:F2}; p95 frame ms={frameMs[(int)((frameMs.Count - 1) * 0.95)]:F2}; batched frames={batchedFrames}");
            report.Add($"actual server ticks={ticksElapsed}; tick seconds={ticksElapsed / (double)rateQuery.GetSingleton<ClientServerTickRate>().SimulationTickRate:F2}");
            report.Add($"main thread average ms={(mainSamples > 0 ? mainTotal / mainSamples : 0):F2}; samples={mainSamples}; render thread average ms={(renderSamples > 0 ? renderTotal / renderSamples : 0):F2}; samples={renderSamples}");
            foreach (var counter in counters.OrderByDescending(counter => counter.AverageMs))
            {
                report.Add($"marker {counter.Name}: average ms={counter.AverageMs:F3}; samples={counter.Samples}");
            }
            foreach (var key in runtimeWarnings.Keys)
                report.Add($"{key}: total={runtimeWarnings[key]}; measured gameplay={runtimeWarnings[key] - (warningsAtStart.TryGetValue(key, out var before) ? before : 0)}");
            report.Add($"errors: total={errors.Count}; measured gameplay={errors.Count - errorsAtStart}");
            report.AddRange(errors.Take(10));
            Directory.CreateDirectory(Folder);
            File.WriteAllLines(Path.Combine(Folder, "runtime-probe-result.txt"), report);
            WriteResult("RUNTIME PROBE COMPLETE\n" + string.Join("\n", report));
        }
        catch (Exception exception)
        {
            report.Add("FAIL " + exception);
            File.WriteAllText(Path.Combine(Folder, "runtime-probe-result.txt"), "FAIL\n" + exception + "\n" + string.Join("\n", errors.Take(10)));
            WriteResult("RUNTIME PROBE FAIL " + exception);
        }
        finally
        {
            if (counters != null) foreach (var counter in counters) counter.Recorder.Dispose();
            if (OfflineMatch.IsActive) await OfflineMatch.StopAsync();
            for (int i = 0; i < 5 && EditorApplication.isPlaying; i++) await Awaitable.NextFrameAsync();
            report.Add($"errors including world cleanup={errors.Count}");
            File.WriteAllLines(Path.Combine(Folder, "runtime-probe-result.txt"), report.Concat(errors.Take(10)));
            SessionState.SetBool(RuntimeProbe, false);
            SessionState.SetBool(Pending, false);
            SessionState.SetBool(Started, false);
            smokeCancellation = null;
            Application.runInBackground = backgroundWasEnabled;
            EditorApplication.isPlaying = false;
        }
    }

    private sealed class RuntimeCounter
    {
        public string Name;
        public Unity.Profiling.ProfilerRecorder Recorder;
        public double Total;
        public int Samples;
        public double AverageMs => Samples == 0 ? 0 : Total / Samples / 1e6;
        public void Sample()
        {
            if (!Recorder.Valid || Recorder.LastValue <= 0) return;
            Total += Recorder.LastValue;
            Samples++;
        }
    }

    private static List<RuntimeCounter> CreateRuntimeCounters()
    {
        var handles = new List<Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle>();
        Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetAvailable(handles);
        var counters = new List<RuntimeCounter>();
        foreach (var handle in handles)
        {
            var description = Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetDescription(handle);
            var name = description.Name;
            bool relevant = name.StartsWith("Suna.") ||
                name == "GPU Frame Time" || name == "UIR.DrawChain" || name == "JobHandle.Complete" ||
                name == "Gfx.WaitForPresentOnGfxThread" || name == "Gfx.WaitForGfxCommandsFromMainThread" ||
                name.Contains("PredictedSimulationSystemGroup") || name.Contains("PhysicsSimulationGroup") ||
                name.Contains("CharacterMovementSystem") || name.Contains("CommonCharacterColliderSystem") ||
                name.Contains("TriggerResolutionServerSystem") || name.Contains("StuffSystemClient") ||
                name.Contains("RoundSystemServer") || name.Contains("ApplyDamageSystem") ||
                name.StartsWith("UniversalRenderPipeline.RenderSingleCameraInternal:");
            if (relevant && description.UnitType.ToString() == "TimeNanoseconds")
                counters.Add(new RuntimeCounter { Name = name, Recorder = Unity.Profiling.ProfilerRecorder.StartNew(description.Category, name, 1) });
        }
        return counters;
    }

    private static async Task RunSmokeAsync()
    {
        var preferenceKeys = new[] { "Offline.AllyBots", "Offline.EnemyBots", "Offline.Difficulty", "Offline.PlayerTeam" };
        var savedPreferences = preferenceKeys.ToDictionary(key => key, key => (exists: PlayerPrefs.HasKey(key), value: PlayerPrefs.GetInt(key)));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        smokeCancellation = timeout;
        bool backgroundWasEnabled = Application.runInBackground;
        Application.runInBackground = true;
        var token = timeout.Token;
        checks.Clear();
        errors.Clear();
        runtimeWarnings.Clear();
        try
        {
            if (SceneManager.GetActiveScene().name != "MainMenu") await LoadUtils.LoadSceneAsync("MainMenu", GameNetwork.SessionData.LoadingSteps.BackToMainMenu);
            await Awaitable.NextFrameAsync(token);
            await Awaitable.NextFrameAsync(token);
            RunEntityChecks();
            await StartFromBotMenuAsync(new OfflineMatchOptions(0, 1, BotDifficulty.Normal, TeamSideType.Corpo), token, true);
            CheckLocalDrivers();
            Check(OfflineNavigation.IsReady, "map navigation baked and spawns connected");
            await SpawnTeamsAsync(TeamSideType.Corpo, token);
            CheckRenderingLightBudget("Corpo match");
            ScreenCapture.CaptureScreenshot(Path.Combine(Folder, "offline-gameplay.png"));
            var server = ClientTransportHelper.ServerWorld;
            var em = server.EntityManager;
            var human = FindCharacter(em, false);
            var bot = FindCharacter(em, true);
            Check(em.GetComponentData<GhostOwner>(bot).NetworkId == 0, "bot uses server ownership");
            Check(PlayerHelpers.GetCharacterTeam(bot, server) == TeamSideType.Natif, "bot joins the opposite team");
            using (var query = em.CreateEntityQuery(typeof(RoundComponent)))
            {
                var round = query.GetSingleton<RoundComponent>();
                Check(round.currentPhase == RoundPhase.BuyPhase && round.currentRound == 1, "first round starts in buy phase");
                round.timer = 0;
                query.SetSingleton(round);
            }
            var botStart = em.GetComponentData<LocalTransform>(bot).Position;
            float until = Time.realtimeSinceStartup + 8f;
            while (Time.realtimeSinceStartup < until) await Awaitable.NextFrameAsync(token);
            em.CompleteAllTrackedJobs();
            using (var query = em.CreateEntityQuery(typeof(RoundComponent)))
            {
                var round = query.GetSingleton<RoundComponent>();
                var input = em.GetComponentData<CharacterInput>(bot);
                File.WriteAllText(Path.Combine(Folder, "bot-state.txt"),
                    $"phase={round.currentPhase}; timer={round.timer}; round={round.currentRound}; start={botStart}; position={em.GetComponentData<LocalTransform>(bot).Position}; move={input.move}; enabled={em.IsComponentEnabled<CharacterIsEnable>(bot)}; "
                    + server.GetExistingSystemManaged<OfflineBotInputSystem>().Describe(bot));
            }
            Check(math.distance(botStart, em.GetComponentData<LocalTransform>(bot).Position) > 0.1f, "bot moves through the character controller");
            await CheckHumanCommandAsync(server, human, token);
            await CheckDamageAsync(em, human, bot, token);
            await CheckDamageAsync(em, bot, human, token);
            await CheckCombatAsync(server, human, bot, token);
            await CheckHumanPlantAndBotDefuseAsync(server, human, bot, token);
            await CheckDeathAndRespawnAsync(server, human, bot, token);
            Check(File.Exists(Path.Combine(Application.streamingAssetsPath, "Audio/GeneratedSoundBanks/Windows/Init.bnk")), "runtime audio Init bank available");
            await LoadUtils.QuitAsync();
            Check(!OfflineMatch.IsActive && ClientTransportHelper.ClientWorld == null && ClientTransportHelper.ServerWorld == null, "worlds cleaned up on exit");
            await LoadUtils.LoadSceneAsync("MainMenu", GameNetwork.SessionData.LoadingSteps.BackToMainMenu);
            await Awaitable.NextFrameAsync(token);
            await StartFromBotMenuAsync(new OfflineMatchOptions(0, 1, BotDifficulty.Normal, TeamSideType.Natif), token);
            await SpawnTeamsAsync(TeamSideType.Natif, token);
            CheckRenderingLightBudget("Natif match");
            Check(PlayerHelpers.GetCharacterTeam(FindCharacter(ClientTransportHelper.ServerWorld.EntityManager, true), ClientTransportHelper.ServerWorld)
                == TeamSideType.Corpo, "second match starts with the other team");
            await CheckBotPlantAsync(ClientTransportHelper.ServerWorld, token);
            await LoadUtils.QuitAsync();
            await CheckCustomTeamsAsync(new OfflineMatchOptions(4, 5, BotDifficulty.Hard, TeamSideType.Corpo), token);
            await CheckCustomTeamsAsync(new OfflineMatchOptions(2, 3, BotDifficulty.Easy, TeamSideType.Natif), token);
            Check(errors.Count == 0, "no runtime errors: " + string.Join(" | ", errors.Take(5)));
            WriteResult("PASS\n" + string.Join("\n", checks));
        }
        catch (Exception exception)
        {
            WriteResult("FAIL\n" + exception + "\nCompleted:\n" + string.Join("\n", checks) + "\nErrors:\n" + string.Join("\n", errors.Take(10)));
        }
        finally
        {
            try
            {
                if (OfflineMatch.IsActive) await OfflineMatch.StopAsync();
            }
            finally
            {
                SessionState.SetBool(Pending, false);
                SessionState.SetBool(Started, false);
                smokeCancellation = null;
                Application.runInBackground = backgroundWasEnabled;
                foreach (var entry in savedPreferences)
                {
                    if (entry.Value.exists) PlayerPrefs.SetInt(entry.Key, entry.Value.value);
                    else PlayerPrefs.DeleteKey(entry.Key);
                }
                PlayerPrefs.Save();
                EditorApplication.isPlaying = false;
            }
        }
    }

    private static async Task SpawnTeamsAsync(TeamSideType team, CancellationToken token)
    {
        var controller = UnityEngine.Object.FindFirstObjectByType<TeamChoiceController>();
        var button = controller.GetComponent<UIDocument>().rootVisualElement.Q<Button>(team == TeamSideType.Corpo ? "Corpo" : "Natif");
        using (var click = ClickEvent.GetPooled())
        {
            click.target = button;
            button.SendEvent(click);
        }
        while (!OfflineMatch.HasSpawnedTeams(ClientTransportHelper.ServerWorld.EntityManager)) await Awaitable.NextFrameAsync(token);
        // Allow loadout queues, body colliders, and ghost replication to settle.
        for (int i = 0; i < 90; i++) await Awaitable.NextFrameAsync(token);
        CheckRobotModels();
        Check(!controller.IsUIActive(), "team button closes the selection UI");
        Check(ClientTransportHelper.ClientWorld.GetExistingSystemManaged<CharacterInputSystem>().Enabled, "team button enables player controls");
        var em = ClientTransportHelper.ServerWorld.EntityManager;
        var corpo = FindCharacter(em, team == TeamSideType.Natif);
        var harvester = Harvester(em);
        await WaitUntilAsync(() => em.GetComponentData<StuffDynamicData>(harvester).owner == corpo, 5f, "corpo receives the harvester at round start", token);
        Check(true, "corpo receives the harvester at round start");
    }

    private static void ClickButton(VisualElement root, string name)
    {
        var button = root.Q<Button>(name);
        if (button == null) throw new InvalidOperationException("Missing menu button: " + name);
        using var submit = NavigationSubmitEvent.GetPooled();
        submit.target = button;
        button.SendEvent(submit);
    }

    private static void CheckRobotModels()
    {
        var em = ClientTransportHelper.ClientWorld.EntityManager;
        int visibleFirstPerson = 0, fullBodies = 0;
        bool robotMeshes = true, robotMaterials = true;
        void Inspect(GameObject model)
        {
            int bodyMeshes = 0;
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string mesh = AssetDatabase.GetAssetPath(renderer.sharedMesh);
                // Equipped weapons can have their own skinned mesh below WeaponSlot.
                if (!mesh.Contains("/Characters/")) continue;
                bodyMeshes++;
                robotMeshes &= mesh.Contains("/Characters/Corpo/") && !mesh.Contains("/Characters/Natif/");
                robotMaterials &= renderer.sharedMaterials.Length > 0 && renderer.sharedMaterials[0].shader.name == "Game/Robot Armor";
            }
            robotMeshes &= bodyMeshes > 0;
        }
        using (var query = em.CreateEntityQuery(typeof(FirstPersonCharacterModelReference)))
        using (var models = query.ToEntityArray(Allocator.Temp))
            foreach (var entity in models)
            {
                var model = em.GetComponentObject<FirstPersonCharacterModelReference>(entity).ModelGameObject;
                if (model == null) continue;
                if (model.activeInHierarchy) visibleFirstPerson++;
                Inspect(model);
            }
        using (var query = em.CreateEntityQuery(typeof(ThirdPersonCharacterModelReference)))
        using (var models = query.ToEntityArray(Allocator.Temp))
            foreach (var entity in models)
            {
                var model = em.GetComponentObject<ThirdPersonCharacterModelReference>(entity).ModelGameObject;
                if (model == null) continue;
                fullBodies++;
                Inspect(model);
            }
        Check(visibleFirstPerson == 1, "one local player has visible robot FPS arms");
        Check(fullBodies >= 2 && robotMeshes, "both teams instantiate robot bodies without human character meshes");
        Check(robotMaterials, "live FPS arms and bot bodies use their robot team materials");
    }

    private static async Task StartFromBotMenuAsync(OfflineMatchOptions options, CancellationToken token, bool checkMenu = false)
    {
        if (SceneManager.GetActiveScene().name != "MainMenu") await LoadUtils.LoadSceneAsync("MainMenu", GameNetwork.SessionData.LoadingSteps.BackToMainMenu);
        for (int i = 0; i < 3; i++) await Awaitable.NextFrameAsync(token);
        var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
        var root = menu.GetComponent<UIDocument>().rootVisualElement;
        ClickButton(root, "PlayButton");
        if (checkMenu)
        {
            Check(root.Q("BotSetup").style.display == DisplayStyle.Flex, "Play Offline opens custom bot setup");
            ClickButton(root, "FullTeamsPreset");
            Check(root.Q<DropdownField>("AllyBots").index == 4 && root.Q<DropdownField>("EnemyBots").index == 4, "5 vs 5 preset sets four allies and five enemies");
            ClickButton(root, "DuelPreset");
            Check(root.Q<DropdownField>("AllyBots").index == 0 && root.Q<DropdownField>("EnemyBots").index == 0, "1 vs 1 preset sets one enemy");
            root.Q<DropdownField>("BotDifficulty").index = 2;
            ClickButton(root, "BackFromBotSetup");
            Check(root.Q("BotSetup").style.display == DisplayStyle.None && !OfflineMatch.IsActive, "Back closes setup without starting a match");
            ClickButton(root, "PlayButton");
            Check(root.Q<DropdownField>("BotDifficulty").index == 2, "local setup preferences survive reopening the menu");
            Check(UnityEngine.Object.FindFirstObjectByType<OfflineMatchSettings>() != null, "menu prefab has valid offline settings component");
            if (!Application.isBatchMode)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(Folder, "custom-bot-setup.png"));
                for (int i = 0; i < 20; i++) await Awaitable.NextFrameAsync(token);
                File.WriteAllText(Path.Combine(Folder, "bot-menu-layout.txt"),
                    $"root={root.worldBound}; setup={root.Q("BotSetup").worldBound}; start={root.Q("StartOffline").worldBound}; back={root.Q("BackFromBotSetup").worldBound}");
            }
        }
        root.Q<DropdownField>("PlayerTeam").index = options.PlayerTeam == TeamSideType.Corpo ? 0 : 1;
        root.Q<DropdownField>("AllyBots").index = options.AllyBots;
        root.Q<DropdownField>("EnemyBots").index = options.EnemyBots - 1;
        root.Q<DropdownField>("BotDifficulty").index = (int)options.Difficulty;
        ClickButton(root, "StartOffline");
        await menu.StartingMatch;
        if (checkMenu)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(Folder, "robot-team-choice.png"));
            for (int i = 0; i < 3; i++) await Awaitable.NextFrameAsync(token);
        }
        Check(OfflineMatch.IsActive && OfflineMatch.Difficulty == options.Difficulty && OfflineMatch.BotCount == options.AllyBots + options.EnemyBots,
            $"start button applies {options.Difficulty} setup with {options.AllyBots} allies and {options.EnemyBots} enemies");
    }

    private static void CheckLocalDrivers()
    {
        foreach (var world in new[] { ClientTransportHelper.ServerWorld, ClientTransportHelper.ClientWorld })
        {
            using var query = world.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver));
            var store = query.GetSingleton<NetworkStreamDriver>().DriverStore;
            Check(store.DriversCount == 1 && store.GetDriverType(store.FirstDriver) == TransportType.IPC,
                world.Name + " has exactly one IPC driver and no socket driver");
        }
    }

    private static async Task CheckCustomTeamsAsync(OfflineMatchOptions options, CancellationToken token)
    {
        await StartFromBotMenuAsync(options, token);
        var server = ClientTransportHelper.ServerWorld;
        await WaitUntilAsync(() => OfflineMatch.HasSpawnedTeams(server.EntityManager), 30f, "custom teams spawn", token);
        for (int i = 0; i < 90; i++) await Awaitable.NextFrameAsync(token);
        var minimap = new Dictionary<int, MinimapTeamLinkSystem.MinimapTeamArgs>();
        void RecordMinimap(object sender, MinimapTeamLinkSystem.MinimapTeamArgs args) => minimap[args.PlayerId] = args;
        MinimapTeamLinkSystem.OnMinimapTeamLinkEvent += RecordMinimap;
        try { for (int i = 0; i < 3; i++) await Awaitable.NextFrameAsync(token); }
        finally { MinimapTeamLinkSystem.OnMinimapTeamLinkEvent -= RecordMinimap; }
        Check(minimap.Count == 4 && minimap.Keys.All(id => id >= 1 && id <= 4)
            && minimap.Values.All(pin => pin.TeamId == (int)options.PlayerTeam)
            && minimap.Values.Count(pin => pin.Alive) == options.AllyBots,
            "minimap shows ally bots in its four valid slots and hides unused pins");
        var em = server.EntityManager;
        {
        using var query = em.CreateEntityQuery(typeof(OfflineBot), typeof(ClientComponent), typeof(ClientCharacterAttached));
        using var participants = query.ToEntityArray(Allocator.Temp);
        int allies = 0, enemies = 0;
        var identities = new HashSet<int>();
        foreach (var entity in participants)
        {
            var client = em.GetComponentData<ClientComponent>(entity);
            if (client.team == options.PlayerTeam) allies++; else enemies++;
            identities.Add(client.networkID);
            var character = em.GetComponentData<ClientCharacterAttached>(entity).Value;
            Check(em.Exists(character) && PlayerHelpers.GetCharacterTeam(character, server) == client.team,
                $"custom bot {client.networkID} has a character on {client.team}");
        }
        Check(allies == options.AllyBots && enemies == options.EnemyBots && identities.Count == participants.Length,
            $"{options.Difficulty} match spawns {allies} ally bots, {enemies} enemy bots, and unique participant IDs");
        var human = FindCharacter(em, false);
        Check(PlayerHelpers.GetCharacterTeam(human, server) == options.PlayerTeam, "setup team choice is applied automatically");
        Check(!UnityEngine.Object.FindFirstObjectByType<TeamChoiceController>().IsUIActive()
            && ClientTransportHelper.ClientWorld.GetExistingSystemManaged<CharacterInputSystem>().Enabled,
            "custom match starts with team menu closed and player controls enabled");
        CheckLocalDrivers();
        }
        await LoadUtils.QuitAsync();
        Check(!OfflineMatch.IsActive && ClientTransportHelper.ClientWorld == null && ClientTransportHelper.ServerWorld == null,
            "custom match worlds cleaned up");
    }

    private static async Task CheckHumanCommandAsync(World server, Entity human, CancellationToken token)
    {
        var client = ClientTransportHelper.ClientWorld;
        var inputSystem = client.GetExistingSystemManaged<CharacterInputSystem>();
        using var query = client.EntityManager.CreateEntityQuery(typeof(CharacterTag), typeof(CharacterInput), typeof(GhostOwnerIsLocal));
        Entity local;
        using (var entities = query.ToEntityArray(Allocator.Temp))
        {
            if (entities.Length != 1) throw new Exception("Expected one locally owned character.");
            local = entities[0];
        }
        float3 start = server.EntityManager.GetComponentData<LocalTransform>(human).Position;
        inputSystem.Enabled = false;
        try
        {
            var input = client.EntityManager.GetComponentData<CharacterInput>(local);
            input.move = new float2(0, 1);
            client.EntityManager.SetComponentData(local, input);
            float until = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < until) await Awaitable.NextFrameAsync(token);
            Check(math.distance(start, server.EntityManager.GetComponentData<LocalTransform>(human).Position) > 0.1f,
                "human command input moves the character on the local server");
        }
        finally
        {
            var input = client.EntityManager.GetComponentData<CharacterInput>(local);
            input.move = float2.zero;
            client.EntityManager.SetComponentData(local, input);
            inputSystem.Enabled = true;
        }
    }

    private static RoundComponent ReadRound(EntityManager em)
    {
        using var query = em.CreateEntityQuery(typeof(RoundComponent));
        return query.GetSingleton<RoundComponent>();
    }

    private static void ExpirePhase(EntityManager em)
    {
        using var query = em.CreateEntityQuery(typeof(RoundComponent));
        var round = query.GetSingleton<RoundComponent>();
        round.timer = 0;
        query.SetSingleton(round);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, float seconds, string label, CancellationToken token)
    {
        float until = Time.realtimeSinceStartup + seconds * (Application.isBatchMode ? 4f : 1f);
        while (!condition())
        {
            if (Time.realtimeSinceStartup > until) throw new Exception("Timed out: " + label);
            await Awaitable.NextFrameAsync(token);
        }
    }

    private static void Teleport(EntityManager em, Entity character, float3 position)
    {
        em.CompleteAllTrackedJobs();
        var transform = em.GetComponentData<LocalTransform>(character);
        transform.Position = position;
        em.SetComponentData(character, transform);
        em.SetComponentData(character, default(PhysicsVelocity));
    }

    private static float3 SpawnPosition(EntityManager em, TeamSideType team)
    {
        using var query = em.CreateEntityQuery(typeof(TeamSpawnComponent), typeof(SpawnPointBufferComponent));
        using var entities = query.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
            if (em.GetComponentData<TeamSpawnComponent>(entity).team == team) return em.GetBuffer<SpawnPointBufferComponent>(entity)[0];
        throw new Exception("Team spawn is missing.");
    }

    private static Entity Gun(EntityManager em, Entity character)
    {
        var inventory = em.GetBuffer<CharacterStuffList>(character);
        for (int i = 0; i < 2; i++)
            if (em.Exists(inventory[i].entity) && em.HasComponent<RangedWeaponDynamicData>(inventory[i].entity)) return inventory[i].entity;
        throw new Exception("Character has no gun.");
    }

    private static async Task CheckCombatAsync(World server, Entity human, Entity bot, CancellationToken token)
    {
        var em = server.EntityManager;
        Teleport(em, bot, SpawnPosition(em, TeamSideType.Natif));
        for (int frame = 0; frame < 10; frame++) await Awaitable.NextFrameAsync(token);
        var position = em.GetComponentData<LocalTransform>(bot).Position;
        float yaw = em.GetComponentData<CharacterInput>(bot).Yaw;
        var targetPosition = FindVisibleCombatPosition(em, position, yaw);
        Check(true, "combat target has navigable ground and clear line of sight");
        Teleport(em, human, targetPosition);
        float health = em.GetComponentData<CurrentHealthComponent>(human).Value;
        var gun = Gun(em, bot);
        int ammo = em.GetComponentData<RangedWeaponDynamicData>(gun).currentAmmo;
        try
        {
            await WaitUntilAsync(() => em.GetComponentData<CurrentHealthComponent>(human).Value < health, 15f, "bot shoots and hits the visible human", token);
        }
        finally
        {
            File.WriteAllText(Path.Combine(Folder, "combat-state.txt"),
                $"human={em.GetComponentData<LocalTransform>(human).Position}; bot={em.GetComponentData<LocalTransform>(bot).Position}; health={em.GetComponentData<CurrentHealthComponent>(human).Value}; ammo={em.GetComponentData<RangedWeaponDynamicData>(gun).currentAmmo}; "
                + server.GetExistingSystemManaged<OfflineBotInputSystem>().Describe(bot));
        }
        Check(em.GetComponentData<RangedWeaponDynamicData>(gun).currentAmmo < ammo, "bot consumes ammunition through the shooting system");
        Check(em.GetComponentData<CurrentHealthComponent>(human).Value < health, "bot raycast hits the human body collider");
        Teleport(em, human, SpawnPosition(em, TeamSideType.Corpo));
        // Isolate the next scenario from damage still being resolved around the teleport.
        var restoredHealth = em.GetComponentData<CurrentHealthComponent>(human);
        restoredHealth.Value = 100;
        em.SetComponentData(human, restoredHealth);
        var weapon = em.GetComponentData<RangedWeaponDynamicData>(gun);
        weapon.currentAmmo = 0;
        weapon.remainingAmmo = 10;
        em.SetComponentData(gun, weapon);
        await WaitUntilAsync(() => em.GetComponentData<RangedWeaponDynamicData>(gun).state == RangedWeaponState.Reload, 4f, "bot requests reload", token);
        await WaitUntilAsync(() => em.GetComponentData<RangedWeaponDynamicData>(gun).currentAmmo > 0, 10f, "bot finishes reload", token);
        Check(true, "bot reloads an empty gun");
    }

    private static float3 FindVisibleCombatPosition(EntityManager em, float3 position, float yaw)
    {
        em.CompleteAllTrackedJobs();
        using var query = em.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        var physics = query.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var path = new NavMeshPath();
        if (!NavMesh.SamplePosition(position - new float3(0, 0.95f, 0), out var start, 1f, NavMesh.AllAreas))
            throw new Exception("Bot has no nearby navigable ground for combat.");
        foreach (float distance in new[] { 4f, 6f, 8f })
        foreach (float angle in new[] { 0f, 20f, -20f, 40f, -40f, 60f, -60f, 80f, -80f, 90f, -90f })
        {
            float heading = yaw + math.radians(angle);
            var goal = (float3)start.position + new float3(math.sin(heading), 0, math.cos(heading)) * distance;
            if (!NavMesh.SamplePosition(goal, out var end, 0.75f, NavMesh.AllAreas)
                || !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path)
                || path.status != NavMeshPathStatus.PathComplete) continue;
            var target = (float3)end.position + new float3(0, 0.95f, 0);
            var hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);
            bool clear = true;
            try
            {
                physics.CastRay(new RaycastInput { Start = position + new float3(0, 0.7f, 0), End = target + new float3(0, 0.6f, 0), Filter = CollisionFilter.Default }, ref hits);
                foreach (var hit in hits)
                {
                    if (em.HasComponent<CharacterTag>(hit.Entity) || em.HasComponent<CharacterColliderDataComponent>(hit.Entity)
                        || em.HasComponent<StuffDynamicData>(hit.Entity) || em.HasComponent<BombSiteTriggerComponent>(hit.Entity)
                        || em.HasComponent<SpawnBarrierComponent>(hit.Entity)) continue;
                    var collider = em.GetComponentData<PhysicsCollider>(hit.Entity);
                    if (collider.Value.Value.GetCollisionResponse(hit.ColliderKey) == CollisionResponsePolicy.Collide && hit.Fraction < 0.98f)
                    { clear = false; break; }
                }
            }
            finally { hits.Dispose(); }
            if (clear) return target;
        }
        throw new Exception("Could not find a visible combat placement; the test must not put the target behind a wall.");
    }

    private static Entity Harvester(EntityManager em)
    {
        using var query = em.CreateEntityQuery(typeof(HarvesterComponent));
        return query.GetSingletonEntity();
    }

    private static float3 SitePosition(EntityManager em)
    {
        using var query = em.CreateEntityQuery(typeof(BombSiteTriggerComponent), typeof(PhysicsCollider), typeof(LocalToWorld));
        using var sites = query.ToEntityArray(Allocator.Temp);
        foreach (var site in sites)
        {
            var bounds = em.GetComponentData<PhysicsCollider>(site).Value.Value.CalculateAabb();
            var center = math.transform(em.GetComponentData<LocalToWorld>(site).Value, bounds.Center);
            if (NavMesh.SamplePosition(center, out var sample, 20f, NavMesh.AllAreas)) return (float3)sample.position + new float3(0, 0.95f, 0);
        }
        throw new Exception("No bomb site has navigable ground.");
    }

    private static async Task CheckHumanPlantAndBotDefuseAsync(World server, Entity human, Entity bot, CancellationToken token)
    {
        var em = server.EntityManager;
        var harvester = Harvester(em);
        // Combat is tested separately; keep the planter alive for this objective scenario.
        var botInput = server.GetExistingSystemManaged<OfflineBotInputSystem>();
        bool inputWasEnabled = botInput.Enabled;
        botInput.Enabled = false;
        em.SetComponentData(bot, default(CharacterInput));
        try
        {
            Teleport(em, bot, SpawnPosition(em, TeamSideType.Natif));
            Teleport(em, human, SitePosition(em));
            Check(HarvesterActions.IsOnSite(em, human), "human stands inside the real bomb-site trigger");
            using (var query = em.CreateEntityQuery(typeof(NetworkTime)))
                Check(HarvesterActions.TryStartPlant(em, human, harvester, query.GetSingleton<NetworkTime>().ServerTick, ReadRound(em).currentPhase), "human can start a runtime plant");
            await WaitForPlantAsync(server, human, harvester, token);
        }
        finally { botInput.Enabled = inputWasEnabled; }
        Check(true, "four-second plant completes through the game systems");
        Teleport(em, human, SpawnPosition(em, TeamSideType.Corpo));
        Teleport(em, bot, em.GetComponentData<LocalTransform>(harvester).Position + new float3(0, 0.75f, 0));
        await WaitUntilAsync(() => em.GetComponentData<PlayerHarvesterActions>(bot).IsDefusing, 5f, "native bot starts defusing", token);
        await WaitUntilAsync(() => ReadRound(em).currentPhase == RoundPhase.PostRoundPhase, 15f, "native bot completes defusing", token);
        Check(!em.IsComponentEnabled<HarvesterPlanted>(harvester) && ReadRound(em).nativeScore == 1, "bot defuse awards the native round");
        ExpirePhase(em);
        await WaitUntilAsync(() => ReadRound(em).currentRound == 2 && ReadRound(em).currentPhase == RoundPhase.BuyPhase, 5f, "next round starts", token);
        for (int i = 0; i < 30; i++) await Awaitable.NextFrameAsync(token);
        Check(HarvesterActions.IsAlive(em, human) && HarvesterActions.IsAlive(em, bot), "both players respawn for the next round");
    }

    private static async Task CheckDeathAndRespawnAsync(World server, Entity human, Entity bot, CancellationToken token)
    {
        var em = server.EntityManager;
        ExpirePhase(em);
        await WaitUntilAsync(() => ReadRound(em).currentPhase == RoundPhase.ActionPhase, 5f, "second action phase starts", token);
        var command = em.CreateEntity(typeof(ApplyDamage));
        em.SetComponentData(command, new ApplyDamage { source = DamageSource.Weapon, playerSource = human, targetEntity = bot, damage = 1000 });
        await WaitUntilAsync(() => !em.IsComponentEnabled<CharacterIsEnable>(bot) && ReadRound(em).currentPhase == RoundPhase.PostRoundPhase, 5f, "bot death ends the round", token);
        Check(ReadRound(em).corporationScore == 1, "bot death awards the human round");
        ExpirePhase(em);
        await WaitUntilAsync(() => ReadRound(em).currentRound == 3 && HarvesterActions.IsAlive(em, bot), 5f, "dead bot respawns", token);
        for (int i = 0; i < 60; i++) await Awaitable.NextFrameAsync(token);
        Check(em.GetComponentData<CurrentHealthComponent>(bot).Value == 100 && em.GetComponentData<RangedWeaponDynamicData>(Gun(em, bot)).currentAmmo > 0, "dead bot respawns with health and a loaded gun");
    }

    private static async Task CheckBotPlantAsync(World server, CancellationToken token)
    {
        var em = server.EntityManager;
        var bot = FindCharacter(em, true);
        var harvester = Harvester(em);
        await WaitUntilAsync(() => em.GetComponentData<StuffDynamicData>(harvester).owner == bot, 5f, "corpo bot receives the harvester", token);
        ExpirePhase(em);
        await WaitUntilAsync(() => ReadRound(em).currentPhase == RoundPhase.ActionPhase, 5f, "corpo action phase starts", token);
        Teleport(em, bot, SitePosition(em));
        await WaitUntilAsync(() => em.GetComponentData<PlayerHarvesterActions>(bot).IsPlanting, 5f, "corpo bot starts planting", token);
        await WaitForPlantAsync(server, bot, harvester, token);
        Check(true, "corpo bot plants through the game systems");
        ExpirePhase(em);
        await WaitUntilAsync(() => ReadRound(em).currentPhase == RoundPhase.PostRoundPhase, 5f, "planted objective expires", token);
        Check(ReadRound(em).corporationScore == 1, "planted objective timeout awards the corpo bot round");
    }

    private static async Task WaitForPlantAsync(World server, Entity character, Entity harvester, CancellationToken token)
    {
        var em = server.EntityManager;
        var samples = new List<string>();
        using (var physicsQuery = em.CreateEntityQuery(typeof(PhysicsWorldSingleton)))
        {
            em.CompleteAllTrackedJobs();
            foreach (var body in physicsQuery.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld.Bodies)
                if (em.HasComponent<BombSiteTriggerComponent>(body.Entity))
                {
                    var bounds = body.CalculateAabb();
                    samples.Add($"physical site {body.Entity}: min={bounds.Min}; max={bounds.Max}; pose={body.WorldFromBody.pos}; scale={body.Scale}");
                }
        }
        // Batch editors may render below the simulation tick rate after a cold import.
        // Keep the four-second game rule; allow the test time to observe its completion.
        float deadline = Time.realtimeSinceStartup + (Application.isBatchMode ? 60f : 15f);
        float sampleAt = 0f;
        try
        {
            while (true)
            {
                if (Time.realtimeSinceStartup >= sampleAt)
                {
                    sampleAt = Time.realtimeSinceStartup + 1f;
                    samples.Add(DescribePlant(server, character, harvester));
                    File.WriteAllLines(Path.Combine(Folder, "objective-state.txt"), samples);
                }
                if (em.IsComponentEnabled<HarvesterPlanted>(harvester) && ReadRound(em).currentPhase == RoundPhase.PostPlantPhase) return;
                if (Time.realtimeSinceStartup > deadline) throw new Exception("Timed out: plant completes and enters post-plant phase. See objective-state.txt.");
                await Awaitable.NextFrameAsync(token);
            }
        }
        finally
        {
            samples.Add(DescribePlant(server, character, harvester));
            File.WriteAllLines(Path.Combine(Folder, "objective-state.txt"), samples);
        }
    }

    private static string DescribePlant(World server, Entity character, Entity harvester)
    {
        var em = server.EntityManager;
        using var timeQuery = em.CreateEntityQuery(typeof(NetworkTime));
        using var rateQuery = em.CreateEntityQuery(typeof(ClientServerTickRate));
        var tick = timeQuery.GetSingleton<NetworkTime>().ServerTick;
        var plantTick = em.GetComponentData<HarvesterPlanting>(harvester).PlantStartedTick;
        int age = tick.IsValid && plantTick.IsValid ? tick.TicksSince(plantTick) : -1;
        int tickRate = rateQuery.IsEmptyIgnoreFilter ? 60 : rateQuery.GetSingleton<ClientServerTickRate>().SimulationTickRate;
        return $"time={server.Time.ElapsedTime:F2}; phase={ReadRound(em).currentPhase}; position={em.GetComponentData<LocalTransform>(character).Position}; health={em.GetComponentData<CurrentHealthComponent>(character).Value}; alive={HarvesterActions.IsAlive(em, character)}; onSite={HarvesterActions.IsOnSite(em, character)}; owner={em.GetComponentData<StuffDynamicData>(harvester).owner}; planting={em.IsComponentEnabled<HarvesterPlanting>(harvester)}; planted={em.IsComponentEnabled<HarvesterPlanted>(harvester)}; plantAge={age}; tickRate={tickRate}; input={em.GetComponentData<CharacterInput>(character).move}";
    }

    private static Entity FindCharacter(EntityManager em, bool bot)
    {
        using var query = em.CreateEntityQuery(typeof(ClientCharacterAttached), typeof(GhostOwner));
        using var entities = query.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
            if (em.HasComponent<OfflineBot>(entity) == bot) return em.GetComponentData<ClientCharacterAttached>(entity).Value;
        throw new Exception("Expected participant character was not spawned.");
    }

    private static async Task CheckDamageAsync(EntityManager em, Entity source, Entity target, CancellationToken token)
    {
        float before = em.GetComponentData<CurrentHealthComponent>(target).Value;
        var command = em.CreateEntity(typeof(ApplyDamage));
        em.SetComponentData(command, new ApplyDamage { source = DamageSource.Weapon, playerSource = source, targetEntity = target, damage = 10 });
        for (int i = 0; i < 10; i++) await Awaitable.NextFrameAsync(token);
        Check(em.GetComponentData<CurrentHealthComponent>(target).Value < before, "damage pipeline works for " + (em.GetComponentData<GhostOwner>(target).NetworkId == 0 ? "bot" : "human"));
    }

    private static void OnPlayState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;
        SessionState.SetBool(Pending, false);
        SessionState.SetBool(Started, false);
        running = false;
    }

    private static void Check(bool value, string label)
    {
        if (!value) throw new Exception(label);
        checks.Add(label);
        Debug.Log("[OfflineValidation] CHECK " + label);
    }

    private static void WriteResult(string value)
    {
        Directory.CreateDirectory(Folder);
        File.WriteAllText(Path.Combine(Folder, "editor-result.txt"), value);
        if (value.StartsWith("PASS\n") || value.StartsWith("FAIL\n"))
            File.WriteAllText(Path.Combine(Folder, "smoke-result.txt"), value);
        if (value.StartsWith("BUILD ")) File.WriteAllText(Path.Combine(Folder, "build-result.txt"), value);
        Debug.Log("[OfflineValidation] " + value);
    }
}
