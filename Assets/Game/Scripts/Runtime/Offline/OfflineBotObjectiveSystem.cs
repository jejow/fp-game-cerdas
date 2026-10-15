using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(HarvesterSystemServer))]
[UpdateBefore(typeof(HarvesterPlantingSystemServer))]
[UpdateBefore(typeof(HarvesterDefuseSystemServer))]
public partial class OfflineBotObjectiveSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<RoundComponent>();
        RequireForUpdate<NetworkTime>();
    }

    protected override void OnUpdate()
    {
        if (!OfflineMatch.IsActive) return;
        var phase = SystemAPI.GetSingleton<RoundComponent>().currentPhase;
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        if (!tick.IsValid) return;
        foreach (var (attached, character) in SystemAPI.Query<RefRO<CharacterClientAttachedComponent>>()
            .WithAll<CharacterIsEnable>().WithEntityAccess())
        {
            var participant = attached.ValueRO.ClientEntity;
            if (!EntityManager.Exists(participant) || !EntityManager.HasComponent<OfflineBot>(participant)) continue;
            foreach (var (owner, harvester) in SystemAPI.Query<RefRO<StuffDynamicData>>()
                .WithAll<HarvesterComponent>().WithEntityAccess())
            {
                if (owner.ValueRO.owner == character)
                    HarvesterActions.TryStartPlant(EntityManager, character, harvester, tick, phase);
                else if (EntityManager.IsComponentEnabled<HarvesterPlanted>(harvester))
                    HarvesterActions.TryStartDefuse(EntityManager, character, harvester, tick, phase);
            }
        }
    }
}
