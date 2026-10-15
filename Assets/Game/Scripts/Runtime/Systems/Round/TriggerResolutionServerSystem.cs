using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct TriggerResolutionServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<SimulationSingleton>(out var simulationSingleton))
        {
            Debug.Log("No physics found");
            return;
        }

        if (simulationSingleton.Type == SimulationType.NoPhysics)
            return;
        state.Dependency = new SiteTriggerJob
            { Characters = SystemAPI.GetComponentLookup<CharacterComponent>(), Sites = SystemAPI.GetComponentLookup<BombSiteTriggerComponent>(true) }
            .Schedule(simulationSingleton, state.Dependency);
    }

    [BurstCompile]
    private struct SiteTriggerJob : ITriggerEventsJob
    {
        public ComponentLookup<CharacterComponent> Characters;
        [ReadOnly] public ComponentLookup<BombSiteTriggerComponent> Sites;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity character = Entity.Null;
            if (Characters.HasComponent(triggerEvent.EntityA) && Sites.HasComponent(triggerEvent.EntityB)) character = triggerEvent.EntityA;
            else if (Characters.HasComponent(triggerEvent.EntityB) && Sites.HasComponent(triggerEvent.EntityA)) character = triggerEvent.EntityB;
            if (character == Entity.Null) return;
            var data = Characters[character];
            data.isOnSite = true;
            Characters[character] = data;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}
