using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateAfter(typeof(HarvesterSystemServer))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct HarvesterDefuseSystemServer : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HarvesterComponent>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<RoundComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        using var ecb = new EntityCommandBuffer(Allocator.Temp);
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        var phase = SystemAPI.GetSingleton<RoundComponent>().currentPhase;
        if (!tick.IsValid) return;
        int tickRate = SystemAPI.TryGetSingleton<ClientServerTickRate>(out var rate) && rate.SimulationTickRate > 0
            ? rate.SimulationTickRate : 60;
        foreach (var (defusing, harvester) in SystemAPI.Query<RefRO<HarvesterDefusing>>().WithEntityAccess())
        {
            var character = defusing.ValueRO.Defuser;
            var start = defusing.ValueRO.DefuseStartedTick;
            if (!HarvesterActions.CanDefuse(state.EntityManager, character, harvester, phase))
            {
                HarvesterActions.CancelDefuse(state.EntityManager, harvester);
                continue;
            }
            if (tick.TicksSince(start) < 4 * tickRate) continue;
            SystemAPI.SetComponentEnabled<HarvesterPlanted>(harvester, false);
            HarvesterActions.CancelDefuse(state.EntityManager, harvester);
            var rpc = ecb.CreateEntity();
            ecb.AddComponent(rpc, new RpcHarvesterDefused { harvester = harvester });
            ecb.AddComponent(rpc, new SendRpcCommandRequest());
        }
        foreach (var (request, rpc, entity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcHarvesterDefuseStart>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
            if (!HarvesterActions.OwnsCharacter(state.EntityManager, request.SourceConnection, rpc.character)) continue;
            HarvesterActions.TryStartDefuse(state.EntityManager, rpc.character, rpc.harvester, tick, phase);
        }
        foreach (var (request, rpc, entity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcHarvesterDefuseStop>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
            if (!HarvesterActions.OwnsCharacter(state.EntityManager, request.SourceConnection, rpc.character)
                || !SystemAPI.HasComponent<HarvesterDefusing>(rpc.harvester)) continue;
            if (SystemAPI.GetComponent<HarvesterDefusing>(rpc.harvester).Defuser == rpc.character)
                HarvesterActions.CancelDefuse(state.EntityManager, rpc.harvester);
        }
        ecb.Playback(state.EntityManager);
    }
}
