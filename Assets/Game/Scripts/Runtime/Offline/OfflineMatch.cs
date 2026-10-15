using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameNetwork;
using GameNetwork.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BotDifficulty { Easy, Normal, Hard }

/// <summary>Owns the local match across the menu/game scene transition.</summary>
public static class OfflineMatch
{
    public static bool IsActive { get; private set; }
    public static bool NavigationReady => OfflineNavigation.IsReady;
    public static int AllyBotCount { get; private set; }
    public static int EnemyBotCount { get; private set; } = 1;
    public static int BotCount => AllyBotCount + EnemyBotCount;
    public static TeamSideType PreferredTeam { get; private set; } = TeamSideType.Corpo;
    public static BotDifficulty Difficulty { get; private set; } = BotDifficulty.Normal;
    public const string LocalPlayerId = "offline-player";
    private static CancellationTokenSource lifetime;
    private static Task starting;
    private static INetworkStreamDriverConstructor previousDriver;
    private static bool quitting;
    private static int previousJobWorkerCount = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        lifetime?.Cancel();
        lifetime?.Dispose();
        lifetime = null;
        starting = null;
        quitting = false;
        previousJobWorkerCount = -1;
        Application.quitting -= OnQuitting;
        IsActive = false;
        PlayerHelpers.ClearTeams();
        CameraSystem.ResetState();
    }

    public static Task StartAsync(OfflineMatchOptions options)
    {
        if (starting != null && !starting.IsCompleted) return starting;
        if (IsActive) throw new InvalidOperationException("An offline match is already running.");
        starting = StartCoreAsync(options.Normalized());
        return starting;
    }

    private static async Task StartCoreAsync(OfflineMatchOptions options)
    {
        IsActive = true;
        AllyBotCount = options.AllyBots;
        EnemyBotCount = options.EnemyBots;
        Difficulty = options.Difficulty;
        PreferredTeam = options.PlayerTeam;
        lifetime = new CancellationTokenSource();
        Application.quitting += OnQuitting;
        using var loading = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        // A first editor import can spend several minutes baking the entity scene.
        loading.CancelAfter(TimeSpan.FromSeconds(Application.isEditor ? 600 : 120));
        var token = loading.Token;
        previousDriver = NetworkStreamReceiveSystem.DriverConstructor;
        using var audioTransition = new OfflineAudioTransition();
        try
        {
            previousJobWorkerCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
            int workers = Math.Min(4, Math.Max(1, SystemInfo.processorCount - 2));
            workers = Math.Min(workers, Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount);
            if (workers != previousJobWorkerCount) Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount = workers;
            SessionData.Instance.UpdateLoading(SessionData.LoadingSteps.StartLoading);
            PlayerHelpers.ClearTeams();
            CameraSystem.ResetState();
            ClientServerBootstrap.AutoConnectPort = 0;
            NetworkStreamReceiveSystem.DriverConstructor = new OfflineDriverConstructor();

            // The default world would also load subscenes and render a duplicate map.
            foreach (var world in SnapshotWorlds())
            {
                if (!world.IsCreated || world.Flags != WorldFlags.Game) continue;
                ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
                if (World.DefaultGameObjectInjectionWorld == world) World.DefaultGameObjectInjectionWorld = null;
                world.Dispose();
            }
            SessionData.Instance.UpdateLoading(SessionData.LoadingSteps.CreateWorld);
            ClientTransportHelper.ServerWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            ClientTransportHelper.ClientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            World.DefaultGameObjectInjectionWorld = ClientTransportHelper.ClientWorld;

            await LoadUtils.LoadSceneAsync("Gameplay", SessionData.LoadingSteps.LoadGameScene);
            token.ThrowIfCancellationRequested();
            await WaitForSceneAsync(ClientTransportHelper.ServerWorld, SessionData.LoadingSteps.LoadServer, token);
            await WaitForSceneAsync(ClientTransportHelper.ClientWorld, SessionData.LoadingSteps.LoadClient, token);

            // NetCode enables prespawned resource ghosts after a connection enters the game.
            // Waiting for that database before connecting would block both sides indefinitely.
            var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7979);
            using (var query = ClientTransportHelper.ServerWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)))
            {
                var driver = query.GetSingletonRW<NetworkStreamDriver>();
                if (!driver.ValueRW.Listen(endpoint)) throw new InvalidOperationException("Could not start the local IPC server.");
            }
            using (var query = ClientTransportHelper.ClientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)))
            {
                query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(ClientTransportHelper.ClientWorld.EntityManager, endpoint);
            }
            await WaitForSceneAsync(ClientTransportHelper.ServerWorld, SessionData.LoadingSteps.WorldReplication, token, true);
            await WaitForSceneAsync(ClientTransportHelper.ClientWorld, SessionData.LoadingSteps.WorldReplication, token, true);
            using (var roundQuery = ClientTransportHelper.ServerWorld.EntityManager.CreateEntityQuery(typeof(RoundComponent)))
            {
                var round = roundQuery.GetSingleton<RoundComponent>();
                round.roundSystemActive = true;
                round.gameWon = false;
                roundQuery.SetSingleton(round);
            }
            using (var spawnQuery = ClientTransportHelper.ServerWorld.EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<SpawnerSettingsTag>(), ComponentType.ReadOnly<AutoRespawnIsEnable>() },
                Options = EntityQueryOptions.IgnoreComponentEnabledState
            }))
            {
                using var spawners = spawnQuery.ToEntityArray(Allocator.Temp);
                foreach (var spawner in spawners)
                    ClientTransportHelper.ServerWorld.EntityManager.SetComponentEnabled<AutoRespawnIsEnable>(spawner, false);
            }
            SessionData.Instance.LoadingStatusText = "Preparing offline bot navigation...";
            await OfflineNavigation.BuildAsync(ClientTransportHelper.ServerWorld, token);

            await WaitForLocalPlayerAsync(token);
            TeamChoiceSystemClient.SendTeamChoice(ClientTransportHelper.ClientWorld.EntityManager, PreferredTeam);
            ClientTransportHelper.State = ClientConnectionState.Connected;
            SessionData.Instance.UpdateLoading(SessionData.LoadingSteps.LoadingDone);
            Debug.Log($"[Offline] Local IPC match ready: {PreferredTeam}, {AllyBotCount} ally bots, {EnemyBotCount} enemy bots, {Difficulty}.");
        }
        catch
        {
            await StopAsync();
            if (Application.isPlaying && !quitting)
                await LoadUtils.LoadSceneAsync("MainMenu", SessionData.LoadingSteps.BackToMainMenu);
            throw;
        }
    }

    private static async Task WaitForSceneAsync(World world, SessionData.LoadingSteps step, CancellationToken token, bool requireResources = false)
    {
        SessionData.Instance.UpdateLoading(step);
        using var query = world.EntityManager.CreateEntityQuery(typeof(SceneReference));
        float reportAt = 0f;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            // Temp arrays must be released in the same frame, before awaiting.
            bool ready;
            int total, loaded = 0;
            using (var scenes = query.ToEntityArray(Allocator.Temp))
            {
                total = scenes.Length;
                foreach (var scene in scenes)
                {
                    if (world.EntityManager.HasComponent<RequestSceneLoaded>(scene))
                    {
                        var state = SceneSystem.GetSceneStreamingState(world.Unmanaged, scene);
                        if (state == SceneSystem.SceneStreamingState.LoadedWithSectionErrors ||
                            state == SceneSystem.SceneStreamingState.FailedLoadingSceneHeader)
                            throw new InvalidOperationException($"{world.Name} failed to load entity scene {world.EntityManager.GetComponentData<SceneReference>(scene).SceneGUID}: {state}. See the preceding Unity Console error.");
                    }
                    if (SceneSystem.IsSceneLoaded(world.Unmanaged, scene)) loaded++;
                }
                ready = total > 0 && loaded == total;
            }
            using var prefabs = world.EntityManager.CreateEntityQuery(typeof(ClientPrefabData));
            using var resources = world.EntityManager.CreateEntityQuery(typeof(GameResourcesDatabase));
            if (ready && !prefabs.IsEmptyIgnoreFilter && (!requireResources || !resources.IsEmptyIgnoreFilter)) return;
            if (Time.realtimeSinceStartup >= reportAt)
            {
                reportAt = Time.realtimeSinceStartup + 10f;
                var details = new List<string>();
                using (var scenes = query.ToEntityArray(Allocator.Temp))
                    foreach (var scene in scenes)
                    {
                        var state = world.EntityManager.HasComponent<RequestSceneLoaded>(scene)
                            ? SceneSystem.GetSceneStreamingState(world.Unmanaged, scene).ToString() : "not requested";
                        details.Add($"{world.EntityManager.GetComponentData<SceneReference>(scene).SceneGUID}: {state}");
                    }
                Debug.Log($"[Offline] {world.Name}: scenes {loaded}/{total}, prefabs {prefabs.CalculateEntityCount()}, resource databases {resources.CalculateEntityCount()}; time={world.Time.ElapsedTime:F2}; loop={ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world)}; initialization={world.GetExistingSystemManaged<InitializationSystemGroup>()?.Enabled}; scene system={world.GetExistingSystemManaged<SceneSystemGroup>()?.Enabled}; {string.Join(", ", details)}.");
            }
            await Awaitable.NextFrameAsync(token);
        }
    }

    private static async Task WaitForLocalPlayerAsync(CancellationToken token)
    {
        var world = ClientTransportHelper.ClientWorld;
        using var query = world.EntityManager.CreateEntityQuery(typeof(ClientComponent), typeof(GhostOwnerIsLocal));
        while (query.IsEmpty)
        {
            token.ThrowIfCancellationRequested();
            await Awaitable.NextFrameAsync(token);
        }
    }

    public static bool HasSpawnedTeams(EntityManager em)
    {
        if (!NavigationReady) return false;
        bool corpo = false, natif = false;
        using var query = em.CreateEntityQuery(typeof(ClientComponent), typeof(ClientCharacterAttached), typeof(GhostOwner));
        using var entities = query.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            var character = em.GetComponentData<ClientCharacterAttached>(entity).Value;
            if (!em.Exists(character)) continue;
            var team = em.GetComponentData<ClientComponent>(entity).team;
            corpo |= team == TeamSideType.Corpo;
            natif |= team == TeamSideType.Natif;
        }
        return corpo && natif;
    }

    public static Task StopAsync()
    {
        using var audioTransition = new OfflineAudioTransition();
        lifetime?.Cancel();
        lifetime?.Dispose();
        lifetime = null;
        Application.quitting -= OnQuitting;
        foreach (var world in SnapshotWorlds())
        {
            if (!world.IsCreated || (!world.IsClient() && !world.IsServer())) continue;
            world.EntityManager.CompleteAllTrackedJobs();
            ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
            if (World.DefaultGameObjectInjectionWorld == world) World.DefaultGameObjectInjectionWorld = null;
            world.Dispose();
        }
        ClientTransportHelper.ClientWorld = null;
        ClientTransportHelper.ServerWorld = null;
        ClientTransportHelper.State = ClientConnectionState.NotConnected;
        NetworkStreamReceiveSystem.DriverConstructor = previousDriver;
        OfflineNavigation.Clear();
        CameraSystem.ResetState();
        PlayerHelpers.ClearTeams();
        IsActive = false;
        if (World.DefaultGameObjectInjectionWorld == null && Application.isPlaying && !quitting)
            DefaultWorldInitialization.Initialize("Default World", false);
        if (previousJobWorkerCount >= 0)
        {
            Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount = previousJobWorkerCount;
            previousJobWorkerCount = -1;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return Task.CompletedTask;
    }

    public static void CancelLoading() => lifetime?.Cancel();

    private static void OnQuitting()
    {
        quitting = true;
        CancelLoading();
    }

    private static List<World> SnapshotWorlds()
    {
        var worlds = new List<World>(World.All.Count);
        for (int i = 0; i < World.All.Count; i++)
        {
            var world = World.All[i];
            if (world.IsCreated) worlds.Add(world);
        }
        return worlds;
    }
}

public sealed class OfflineDriverConstructor : INetworkStreamDriverConstructor
{
    public void CreateClientDriver(World world, ref NetworkDriverStore store, NetDebug debug)
    {
        using var settings = DefaultDriverBuilder.GetNetworkClientSettings();
        DefaultDriverBuilder.RegisterClientIpcDriver(world, ref store, debug, settings);
    }

    public void CreateServerDriver(World world, ref NetworkDriverStore store, NetDebug debug)
    {
        using var settings = DefaultDriverBuilder.GetNetworkServerSettings();
        DefaultDriverBuilder.RegisterServerIpcDriver(world, ref store, debug, settings);
    }
}
