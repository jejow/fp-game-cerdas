using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

/// <summary>Bakes the same static DOTS colliders that the character controller collides with.</summary>
public static class OfflineNavigation
{
    private static readonly Unity.Profiling.ProfilerMarker PathMarker = new Unity.Profiling.ProfilerMarker("Suna.BotPath");
    public static bool IsReady { get; private set; }
    private static NavMeshData data;
    private static NavMeshDataInstance instance;

    public static async Task BuildAsync(World world, CancellationToken token)
    {
        Clear();
        await Awaitable.NextFrameAsync(token);
        world.EntityManager.CompleteAllTrackedJobs();
        using var physicsQuery = world.EntityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        var physics = physicsQuery.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        var meshes = new Dictionary<BlobAssetReference<Unity.Physics.Collider>, Mesh>();
        var sources = new List<NavMeshBuildSource>();
        var bounds = new Bounds();
        bool hasBounds = false;
        try
        {
            foreach (var body in physics.StaticBodies)
            {
                var entity = body.Entity;
                if (!world.EntityManager.Exists(entity)
                    || world.EntityManager.HasComponent<SpawnBarrierComponent>(entity)
                    || world.EntityManager.HasComponent<BombSiteTriggerComponent>(entity)
                    || world.EntityManager.HasComponent<CharacterTag>(entity)
                    || world.EntityManager.HasComponent<StuffDynamicData>(entity)) continue;
                var collider = new PhysicsCollider { Value = body.Collider };
                if (!collider.Value.IsCreated || (collider.Value.Value.Type != ColliderType.Compound
                    && collider.Value.Value.GetCollisionResponse() != CollisionResponsePolicy.Collide)) continue;
                var key = collider.Value;
                if (!meshes.TryGetValue(key, out var mesh))
                {
                    mesh = collider.ToMesh();
                    meshes.Add(key, mesh);
                }
                // Physics already bakes nonuniform scaling into the collider. LocalToWorld
                // would apply it a second time; use the actual rigid-body pose and scale.
                Matrix4x4 transform = float4x4.TRS(body.WorldFromBody.pos, body.WorldFromBody.rot, new float3(body.Scale));
                sources.Add(new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Mesh, sourceObject = mesh, transform = transform });
                var meshBounds = mesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = meshBounds.center + Vector3.Scale(meshBounds.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var point = transform.MultiplyPoint3x4(corner);
                    if (!hasBounds) { bounds = new Bounds(point, Vector3.zero); hasBounds = true; }
                    else bounds.Encapsulate(point);
                }
            }
            if (sources.Count == 0) throw new InvalidOperationException("No static map colliders were loaded for bot navigation.");
            var settings = NavMesh.GetSettingsCount() > 0 ? NavMesh.GetSettingsByIndex(0) : NavMesh.CreateSettings();
            using (var prefabs = world.EntityManager.CreateEntityQuery(typeof(ClientPrefabData)))
            {
                var character = prefabs.GetSingleton<ClientPrefabData>().Character;
                var movement = world.EntityManager.GetComponentData<CharacterComponent>(character);
                var shape = world.EntityManager.GetComponentData<PhysicsCollider>(character).Value.Value.CalculateAabb();
                var size = shape.Max - shape.Min;
                settings.agentRadius = math.max(size.x, size.z) * 0.5f;
                settings.agentHeight = size.y;
                settings.agentClimb = movement.maxStepHeight;
                settings.agentSlope = movement.maxSlopeAngle;
            }
            settings.overrideVoxelSize = true;
            settings.voxelSize = 0.15f;
            bounds.Expand(4f);
            data = new NavMeshData(settings.agentTypeID);
            var operation = NavMeshBuilder.UpdateNavMeshDataAsync(data, settings, sources, bounds);
            while (!operation.isDone) await Awaitable.NextFrameAsync(token);
            token.ThrowIfCancellationRequested();
            instance = NavMesh.AddNavMeshData(data);
            IsReady = instance.valid;
            if (!IsReady) throw new InvalidOperationException("Could not create bot navigation for the map.");
            Debug.Log($"[Offline] Navigation baked from {sources.Count} static map colliders.");
            ValidateSpawns(world);
        }
        catch
        {
            Clear();
            throw;
        }
        finally
        {
            foreach (var mesh in meshes.Values) UnityEngine.Object.Destroy(mesh);
        }
    }

    private static void ValidateSpawns(World world)
    {
        using var query = world.EntityManager.CreateEntityQuery(typeof(TeamSpawnComponent), typeof(SpawnPointBufferComponent));
        using var entities = query.ToEntityArray(Allocator.Temp);

        var teams = new Dictionary<TeamSideType, Vector3>();
        foreach (var entity in entities)
        {
            var team = world.EntityManager.GetComponentData<TeamSpawnComponent>(entity).team;
            var points = world.EntityManager.GetBuffer<SpawnPointBufferComponent>(entity);
            foreach (var point in points)
            {
                if (!NavMesh.SamplePosition((Vector3)point.Value, out var hit, 4f, NavMesh.AllAreas))
                    throw new InvalidOperationException($"The {team} spawn is outside bot navigation.");
                if (!teams.ContainsKey(team)) teams.Add(team, hit.position);
            }
        }
        if (!teams.TryGetValue(TeamSideType.Corpo, out var corpo) || !teams.TryGetValue(TeamSideType.Natif, out var natif))
            throw new InvalidOperationException("Both team spawns are required for an offline match.");
        var path = new NavMeshPath();
        bool found = NavMesh.CalculatePath(corpo, natif, NavMesh.AllAreas, path);
#if UNITY_EDITOR
        var triangulation = NavMesh.CalculateTriangulation();
        var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "Logs/OfflineVerification");
        System.IO.Directory.CreateDirectory(folder);
        System.IO.File.WriteAllText(System.IO.Path.Combine(folder, "navigation.json"), JsonUtility.ToJson(new NavigationDebugData
        {
            vertices = triangulation.vertices, indices = triangulation.indices,
            corpo = corpo, natif = natif, pathStatus = path.status.ToString(), corners = path.corners
        }));
#endif
        if (!found || path.status != NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("The two team spawns are not connected by bot navigation.");
    }

#if UNITY_EDITOR
    [Serializable]
    private sealed class NavigationDebugData
    {
        public Vector3[] vertices;
        public int[] indices;
        public Vector3 corpo, natif;
        public string pathStatus;
        public Vector3[] corners;
    }
#endif

    public static bool GetNextPoint(Vector3 start, Vector3 destination, NavMeshPath path, out Vector3 next, Vector3[] cornerBuffer = null)
    {
        using var profiling = PathMarker.Auto();
        next = start;
        if (!IsReady || !NavMesh.SamplePosition(start, out var startHit, 3f, NavMesh.AllAreas)
            || !NavMesh.SamplePosition(destination, out var endHit, 4f, NavMesh.AllAreas)) return false;
        if (!NavMesh.CalculatePath(startHit.position, endHit.position, NavMesh.AllAreas, path)
            || path.status != NavMeshPathStatus.PathComplete) return false;
        var corners = cornerBuffer ?? path.corners;
        int count = cornerBuffer == null ? corners.Length : path.GetCornersNonAlloc(corners);
        if (count == 0) return false;
        next = corners[count - 1];
        for (int i = 1; i < count; i++)
        {
            var delta = corners[i] - start;
            delta.y = 0;
            if (delta.sqrMagnitude > 0.25f) { next = corners[i]; break; }
        }
        return true;
    }

    public static void Clear()
    {
        IsReady = false;
        if (data != null) NavMeshBuilder.Cancel(data);
        if (instance.valid) instance.Remove();
        if (data != null) UnityEngine.Object.Destroy(data);
        data = null;
        instance = default;
    }
}
