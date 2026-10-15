using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using Unity.Profiling;

public enum OfflineBotState { Patrol, Search, Chase, Attack, Reload, Objective, Dead }

/// <summary>Writes ordinary character input after NetCode applies received commands and before fixed-step physics.</summary>
// NetCode's command and fixed-step groups both use OrderFirst; AI must share that ordering tier.
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(CopyCommandBufferToInputSystemGroup))]
[UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
public partial class OfflineBotInputSystem : SystemBase
{
    private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker("Suna.OfflineBotInput");
    private sealed class Brain
    {
        public OfflineBotState State;
        public Entity Target;
        public float3 LastSeen;
        public float SeenAt = -100f;
        public float ReactAt;
        public float Yaw, Pitch;
        public float RepathAt;
        public float ScanAt;
        public bool TargetVisible;
        public float3 NextPoint;
        public float3 LastPosition;
        public float StuckFor;
        public int Round = -1;
        public int PatrolIndex;
        public readonly NavMeshPath Path = new NavMeshPath();
        public readonly Vector3[] Corners = new Vector3[64];
        public int CornerCount, CornerIndex;
    }

    private readonly Dictionary<Entity, Brain> brains = new Dictionary<Entity, Brain>();
    private readonly List<Entity> stale = new List<Entity>();
    private readonly List<float3> siteGoals = new List<float3>();
    private EntityQuery sitesQuery;
    private NativeList<Unity.Physics.RaycastHit> sightHits;

#if UNITY_EDITOR
    public string Describe(Entity character) => brains.TryGetValue(character, out var brain)
        ? $"state={brain.State}; yaw={brain.Yaw}; next={brain.NextPoint}; stuck={brain.StuckFor}; target={brain.Target}"
        : "No AI tick has run for this character.";
#endif

    protected override void OnCreate()
    {
        RequireForUpdate<NetworkTime>();
        RequireForUpdate<PhysicsWorldSingleton>();
        RequireForUpdate<RoundComponent>();
        sitesQuery = GetEntityQuery(typeof(BombSiteTriggerComponent), typeof(LocalToWorld));
        sightHits = new NativeList<Unity.Physics.RaycastHit>(16, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        Dependency.Complete();
        if (sightHits.IsCreated) sightHits.Dispose();
        brains.Clear();
    }

    protected override void OnUpdate()
    {
        using var profiling = UpdateMarker.Auto();
        if (!OfflineMatch.IsActive || !OfflineMatch.NavigationReady) return;
        var time = SystemAPI.GetSingleton<NetworkTime>();
        if (!time.IsFirstTimeFullyPredictingTick) return;
        Dependency.Complete();
        CacheSiteGoals();
        var physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var round = SystemAPI.GetSingleton<RoundComponent>();
        float now = (float)SystemAPI.Time.ElapsedTime;
        float dt = SystemAPI.Time.DeltaTime;
        foreach (var (transform, input, attached, character) in SystemAPI
            .Query<RefRO<LocalTransform>, RefRW<CharacterInput>, RefRO<CharacterClientAttachedComponent>>()
            .WithAll<CharacterTag>().WithEntityAccess())
        {
            var participant = attached.ValueRO.ClientEntity;
            if (!EntityManager.Exists(participant) || !EntityManager.HasComponent<OfflineBot>(participant)) continue;
            if (!brains.TryGetValue(character, out var brain))
            {
                brain = new Brain { LastPosition = transform.ValueRO.Position };
                brains.Add(character, brain);
            }
            // Clear one-tick events; AI aim persists independently of the command buffer.
            input.ValueRW = new CharacterInput { Yaw = brain.Yaw, Pitch = brain.Pitch };
            if (!EntityManager.IsComponentEnabled<CharacterIsEnable>(character)
                || EntityManager.GetComponentData<CurrentHealthComponent>(character).Value <= 0)
            {
                brain.State = OfflineBotState.Dead;
                brain.Target = Entity.Null;
                brain.SeenAt = -100f;
                brain.TargetVisible = false;
                continue;
            }
            if (brain.Round != round.currentRound)
            {
                brain.Round = round.currentRound;
                brain.Target = Entity.Null;
                brain.SeenAt = -100f;
                brain.RepathAt = 0;
                brain.ScanAt = now + EntityManager.GetComponentData<OfflineBot>(participant).Index * 0.012f;
                brain.TargetVisible = false;
                brain.CornerCount = 0;
                brain.StuckFor = 0;
                brain.State = OfflineBotState.Patrol;
            }
            if (round.gameWon || round.currentPhase == RoundPhase.BuyPhase || round.currentPhase == RoundPhase.PostRoundPhase)
                continue;
            var actions = EntityManager.GetComponentData<PlayerHarvesterActions>(character);
            if (actions.IsPlanting || actions.IsDefusing)
            {
                brain.State = OfflineBotState.Objective;
                input.ValueRW.stuffLocation = (int)(actions.IsPlanting ? StuffSlot.Harvester : StuffSlot.Melee) + 1;
                continue;
            }

            var position = transform.ValueRO.Position;
            var team = EntityManager.GetComponentData<ClientComponent>(participant).team;
            int index = EntityManager.GetComponentData<OfflineBot>(participant).Index;
            float reaction = OfflineMatch.Difficulty == BotDifficulty.Easy ? 0.65f : OfflineMatch.Difficulty == BotDifficulty.Hard ? 0.18f : 0.35f;
            float aimSpeed = OfflineMatch.Difficulty == BotDifficulty.Easy ? 100f : OfflineMatch.Difficulty == BotDifficulty.Hard ? 240f : 160f;
            float error = OfflineMatch.Difficulty == BotDifficulty.Easy ? 3.5f : OfflineMatch.Difficulty == BotDifficulty.Hard ? 0.7f : 1.8f;
            Entity enemy = brain.TargetVisible ? brain.Target : Entity.Null;
            if (now >= brain.ScanAt)
            {
                enemy = FindVisibleEnemy(character, team, position, brain.Yaw, physics);
                brain.TargetVisible = enemy != Entity.Null;
                brain.ScanAt = now + 0.12f;
            }
            if (enemy != Entity.Null && (!EntityManager.Exists(enemy)
                || !EntityManager.IsComponentEnabled<CharacterIsEnable>(enemy)
                || EntityManager.GetComponentData<CurrentHealthComponent>(enemy).Value <= 0))
            {
                enemy = Entity.Null;
                brain.TargetVisible = false;
            }
            bool visible = enemy != Entity.Null;
            if (visible)
            {
                if (brain.Target != enemy || now - brain.SeenAt > 0.5f) brain.ReactAt = now + reaction;
                brain.Target = enemy;
                brain.LastSeen = EntityManager.GetComponentData<LocalTransform>(enemy).Position;
                brain.SeenAt = now;
            }

            float3 goal;
            bool pursue = brain.Target != Entity.Null && now - brain.SeenAt < 5f;
            if (pursue)
            {
                goal = brain.LastSeen;
                brain.State = visible ? OfflineBotState.Chase : OfflineBotState.Search;
            }
            else
            {
                brain.Target = Entity.Null;
                goal = GetObjectiveGoal(team, position, index + brain.PatrolIndex, out var objective);
                brain.State = objective ? OfflineBotState.Objective : OfflineBotState.Patrol;
                if (math.distance(position.xz, goal.xz) < 1f && !objective)
                {
                    brain.PatrolIndex++;
                    brain.RepathAt = 0;
                }
            }

            float3 look = pursue ? brain.LastSeen - position : brain.NextPoint - position;
            if (math.lengthsq(look.xz) > 0.001f)
            {
                float yaw = math.degrees(math.atan2(look.x, look.z));
                float pitch = visible ? -math.degrees(math.atan2(look.y, math.length(look.xz))) : 0f;
                float sway = math.sin(now * 2.3f + index * 3f) * error;
                brain.Yaw = math.radians(Mathf.MoveTowardsAngle(math.degrees(brain.Yaw), yaw + (visible ? sway : 0), aimSpeed * dt));
                brain.Pitch = math.radians(Mathf.MoveTowardsAngle(math.degrees(brain.Pitch), pitch + (visible ? sway * 0.4f : 0), aimSpeed * dt));
            }
            input.ValueRW.Yaw = brain.Yaw;
            input.ValueRW.Pitch = brain.Pitch;
            input.ValueRW.aimingCanceled.Set();
            input.ValueRW.walkCanceled.Set();
            input.ValueRW.attackCanceled.Set();
            input.ValueRW.stuffLocation = SelectGun(character, out var weapon);
            bool reloading = false;
            if (weapon != Entity.Null)
            {
                var ammo = EntityManager.GetComponentData<RangedWeaponDynamicData>(weapon);
                reloading = ammo.state == RangedWeaponState.Reload;
                if (ammo.currentAmmo == 0 && ammo.remainingAmmo > 0 && !reloading)
                {
                    input.ValueRW.reload.Set();
                    reloading = true;
                }
                if (reloading) brain.State = OfflineBotState.Reload;
            }
            float distance = math.distance(position, brain.LastSeen);
            float yawError = Mathf.Abs(Mathf.DeltaAngle(math.degrees(brain.Yaw), math.degrees(math.atan2(look.x, look.z))));
            // Pulsed fire also works for the existing semiautomatic default pistol.
            if (visible && now >= brain.ReactAt && yawError < 8f && distance < 35f && !reloading && weapon != Entity.Null
                && (now + index * 0.1f) % 0.55f < 0.22f)
            {
                input.ValueRW.attackCanceled = default;
                input.ValueRW.attackStarted.Set();
                input.ValueRW.aimingCanceled = default;
                input.ValueRW.aimingStarted.Set();
                brain.State = OfflineBotState.Attack;
            }
            if (!visible || distance > 12f) MoveAlongPath(position, goal, brain, ref input.ValueRW, now, dt);
            brain.LastPosition = position;
        }
        stale.Clear();
        foreach (var entry in brains) if (!EntityManager.Exists(entry.Key)) stale.Add(entry.Key);
        foreach (var entity in stale) brains.Remove(entity);
    }

    private Entity FindVisibleEnemy(Entity self, TeamSideType team, float3 position, float yaw, CollisionWorld physics)
    {
        Entity closest = Entity.Null;
        float best = 40f * 40f;
        float3 forward = new float3(math.sin(yaw), 0, math.cos(yaw));
        foreach (var (transform, attached, entity) in SystemAPI
            .Query<RefRO<LocalTransform>, RefRO<CharacterClientAttachedComponent>>()
            .WithAll<CharacterIsEnable, CharacterTag>().WithEntityAccess())
        {
            if (entity == self || !EntityManager.Exists(attached.ValueRO.ClientEntity)) continue;
            if (EntityManager.GetComponentData<ClientComponent>(attached.ValueRO.ClientEntity).team == team) continue;
            var delta = transform.ValueRO.Position - position;
            float sq = math.lengthsq(delta);
            if (sq >= best || math.dot(forward, math.normalizesafe(new float3(delta.x, 0, delta.z))) < -0.15f) continue;
            if (!HasLineOfSight(physics, position + new float3(0, 0.7f, 0), transform.ValueRO.Position + new float3(0, 0.6f, 0))) continue;
            closest = entity;
            best = sq;
        }
        return closest;
    }

    private bool HasLineOfSight(CollisionWorld physics, float3 start, float3 end)
    {
        sightHits.Clear();
        physics.CastRay(new RaycastInput { Start = start, End = end, Filter = CollisionFilter.Default }, ref sightHits);
        foreach (var hit in sightHits)
        {
            // Character/body-part colliders and trigger volumes do not occlude sight.
            if (EntityManager.HasComponent<CharacterTag>(hit.Entity)
                || EntityManager.HasComponent<CharacterColliderDataComponent>(hit.Entity)
                || EntityManager.HasComponent<StuffDynamicData>(hit.Entity)
                || EntityManager.HasComponent<BombSiteTriggerComponent>(hit.Entity)
                || EntityManager.HasComponent<SpawnBarrierComponent>(hit.Entity)) continue;
            if (EntityManager.HasComponent<PhysicsCollider>(hit.Entity))
            {
                var collider = EntityManager.GetComponentData<PhysicsCollider>(hit.Entity);
                if (collider.Value.Value.GetCollisionResponse(hit.ColliderKey) == CollisionResponsePolicy.Collide && hit.Fraction < 0.98f) return false;
            }
        }
        return true;
    }

    private void CacheSiteGoals()
    {
        if (siteGoals.Count == sitesQuery.CalculateEntityCount()) return;
        siteGoals.Clear();
        using var entities = sitesQuery.ToEntityArray(Allocator.Temp);
        foreach (var site in entities)
        {
            var matrix = EntityManager.GetComponentData<LocalToWorld>(site);
            if (EntityManager.HasComponent<PhysicsCollider>(site))
            {
                var collider = EntityManager.GetComponentData<PhysicsCollider>(site);
                siteGoals.Add(math.transform(matrix.Value, collider.Value.Value.CalculateAabb().Center));
            }
            else siteGoals.Add(matrix.Position);
        }
    }

    private float3 GetObjectiveGoal(TeamSideType team, float3 position, int index, out bool objective)
    {
        objective = true;
        if (team == TeamSideType.Natif)
        {
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<HarvesterComponent, HarvesterPlanted>())
                return transform.ValueRO.Position;
        }
        if (siteGoals.Count > 0) return siteGoals[index % siteGoals.Count];
        objective = false;
        foreach (var (spawn, entity) in SystemAPI.Query<RefRO<TeamSpawnComponent>>().WithEntityAccess())
        {
            if (spawn.ValueRO.team == team) continue;
            var points = EntityManager.GetBuffer<SpawnPointBufferComponent>(entity);
            if (points.Length > 0) return points[index % points.Length];
        }
        return position;
    }

    private int SelectGun(Entity character, out Entity gun)
    {
        var stuff = EntityManager.GetBuffer<CharacterStuffList>(character);
        for (int slot = 0; slot < 2 && slot < stuff.Length; slot++)
        {
            var entity = stuff[slot].entity;
            if (EntityManager.Exists(entity) && EntityManager.HasComponent<RangedWeaponDynamicData>(entity))
            {
                var ammo = EntityManager.GetComponentData<RangedWeaponDynamicData>(entity);
                if (ammo.currentAmmo <= 0 && ammo.remainingAmmo <= 0) continue;
                gun = entity;
                return slot + 1;
            }
        }
        gun = Entity.Null;
        return 0;
    }

    private void MoveAlongPath(float3 position, float3 goal, Brain brain, ref CharacterInput input, float now, float dt)
    {
        if (now >= brain.RepathAt)
        {
            brain.RepathAt = now + 0.45f;
            if (!OfflineNavigation.GetNextPoint(position, goal, brain.Path, out var next, brain.Corners))
            {
                brain.CornerCount = 0;
                return;
            }
            brain.CornerCount = brain.Path.GetCornersNonAlloc(brain.Corners);
            brain.CornerIndex = 1;
            brain.NextPoint = next;
        }
        if (brain.CornerCount == 0) return;
        while (brain.CornerIndex < brain.CornerCount - 1
            && math.distance(position.xz, ((float3)brain.Corners[brain.CornerIndex]).xz) < 0.5f)
            brain.CornerIndex++;
        brain.NextPoint = brain.Corners[math.min(brain.CornerIndex, brain.CornerCount - 1)];
        var direction = brain.NextPoint - position;
        direction.y = 0;
        if (math.lengthsq(direction) < 0.09f) return;
        var local = math.rotate(quaternion.RotateY(-brain.Yaw), math.normalizesafe(direction));
        input.move = new float2(local.x, local.z);
        brain.StuckFor = math.distance(position, brain.LastPosition) < 0.015f ? brain.StuckFor + dt : 0f;
        if (brain.StuckFor > 1f)
        {
            input.jump.Set();
            brain.RepathAt = 0;
            brain.StuckFor = 0;
        }
    }
}
