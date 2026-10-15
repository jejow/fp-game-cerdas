using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

/// <summary>Shared server validation for human RPCs and local bot intentions.</summary>
public static class HarvesterActions
{
    public static bool IsAlive(EntityManager em, Entity character)
    {
        return em.Exists(character) && em.HasComponent<CharacterIsEnable>(character)
            && em.IsComponentEnabled<CharacterIsEnable>(character)
            && em.HasComponent<CurrentHealthComponent>(character)
            && em.GetComponentData<CurrentHealthComponent>(character).Value > 0;
    }

    public static bool OwnsCharacter(EntityManager em, Entity connection, Entity character)
    {
        return em.Exists(connection) && em.Exists(character) && em.HasComponent<NetworkId>(connection)
            && em.HasComponent<GhostOwner>(character)
            && em.GetComponentData<NetworkId>(connection).Value == em.GetComponentData<GhostOwner>(character).NetworkId;
    }

    public static unsafe bool IsOnSite(EntityManager em, Entity character)
    {
        if (!em.Exists(character) || !em.HasComponent<LocalTransform>(character) || !em.HasComponent<PhysicsCollider>(character)) return false;
        var characterCollider = em.GetComponentData<PhysicsCollider>(character).Value;
        if (!characterCollider.IsCreated) return false;
        var transform = em.GetComponentData<LocalTransform>(character);
        var input = new ColliderDistanceInput
        {
            Collider = (Unity.Physics.Collider*)characterCollider.GetUnsafePtr(),
            Transform = new RigidTransform(transform.Rotation, transform.Position),
            Scale = transform.Scale,
            MaxDistance = 0
        };
        using var physicsQuery = em.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        bool hasPhysics = !physicsQuery.IsEmptyIgnoreFilter;
        var physics = hasPhysics ? physicsQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld : default;
        using var query = em.CreateEntityQuery(typeof(BombSiteTriggerComponent), typeof(PhysicsCollider), typeof(LocalToWorld));
        using var sites = query.ToEntityArray(Allocator.Temp);
        foreach (var site in sites)
        {
            var collider = em.GetComponentData<PhysicsCollider>(site);
            if (!collider.Value.IsCreated) continue;
            var body = new RigidBody
            {
                Collider = collider.Value,
                Entity = site,
                WorldFromBody = Unity.Physics.Math.DecomposeRigidBodyTransform(em.GetComponentData<LocalToWorld>(site).Value),
                Scale = em.HasComponent<LocalTransform>(site) ? em.GetComponentData<LocalTransform>(site).Scale : 1f
            };
            if (hasPhysics)
            {
                int index = physics.GetRigidBodyIndex(site);
                if (index >= 0) body = physics.Bodies[index];
            }
            // A low site trigger overlaps the capsule even when the character's center is above it.
            if (body.CalculateDistance(input, out DistanceHit _)) return true;
        }
        return false;
    }

    public static bool CanPlant(EntityManager em, Entity character, Entity harvester, RoundPhase phase)
    {
        return phase == RoundPhase.ActionPhase && IsAlive(em, character) && em.HasComponent<CorpoTeamTag>(character)
            && em.Exists(harvester) && em.HasComponent<HarvesterPlanting>(harvester)
            && em.HasComponent<StuffDynamicData>(harvester)
            && em.GetComponentData<StuffDynamicData>(harvester).owner == character
            && !em.IsComponentEnabled<HarvesterPlanted>(harvester) && IsOnSite(em, character);
    }

    public static bool TryStartPlant(EntityManager em, Entity character, Entity harvester, NetworkTick tick, RoundPhase phase)
    {
        if (!CanPlant(em, character, harvester, phase)) return false;
        if (!em.IsComponentEnabled<HarvesterPlanting>(harvester))
        {
            em.SetComponentData(harvester, new HarvesterPlanting { PlantStartedTick = tick });
            em.SetComponentEnabled<HarvesterPlanting>(harvester, true);
        }
        var actions = em.GetComponentData<PlayerHarvesterActions>(character);
        actions.IsPlanting = true;
        em.SetComponentData(character, actions);
        return true;
    }

    public static void CancelPlant(EntityManager em, Entity harvester)
    {
        if (!em.Exists(harvester) || !em.HasComponent<HarvesterPlanting>(harvester)) return;
        em.SetComponentEnabled<HarvesterPlanting>(harvester, false);
        var owner = em.GetComponentData<StuffDynamicData>(harvester).owner;
        if (!em.Exists(owner) || !em.HasComponent<PlayerHarvesterActions>(owner)) return;
        var actions = em.GetComponentData<PlayerHarvesterActions>(owner);
        actions.IsPlanting = false;
        em.SetComponentData(owner, actions);
    }

    public static bool CanDefuse(EntityManager em, Entity character, Entity harvester, RoundPhase phase)
    {
        if (phase != RoundPhase.PostPlantPhase || !IsAlive(em, character) || !em.HasComponent<NatifTeamTag>(character)
            || !em.Exists(harvester) || !em.HasComponent<HarvesterDefusing>(harvester)
            || !em.IsComponentEnabled<HarvesterPlanted>(harvester)) return false;
        return math.distance(em.GetComponentData<LocalTransform>(character).Position,
            em.GetComponentData<LocalTransform>(harvester).Position) <= em.GetComponentData<HarvesterComponent>(harvester).defuseRange;
    }

    public static bool TryStartDefuse(EntityManager em, Entity character, Entity harvester, NetworkTick tick, RoundPhase phase)
    {
        if (!CanDefuse(em, character, harvester, phase)) return false;
        if (em.IsComponentEnabled<HarvesterDefusing>(harvester))
            return em.GetComponentData<HarvesterDefusing>(harvester).Defuser == character;
        em.SetComponentData(harvester, new HarvesterDefusing { Defuser = character, DefuseStartedTick = tick });
        em.SetComponentEnabled<HarvesterDefusing>(harvester, true);
        var actions = em.GetComponentData<PlayerHarvesterActions>(character);
        actions.IsDefusing = true;
        em.SetComponentData(character, actions);
        return true;
    }

    public static void CancelDefuse(EntityManager em, Entity harvester)
    {
        if (!em.Exists(harvester) || !em.HasComponent<HarvesterDefusing>(harvester)) return;
        var defuser = em.GetComponentData<HarvesterDefusing>(harvester).Defuser;
        em.SetComponentEnabled<HarvesterDefusing>(harvester, false);
        em.SetComponentData(harvester, default(HarvesterDefusing));
        if (!em.Exists(defuser) || !em.HasComponent<PlayerHarvesterActions>(defuser)) return;
        var actions = em.GetComponentData<PlayerHarvesterActions>(defuser);
        actions.IsDefusing = false;
        em.SetComponentData(defuser, actions);
    }
}
