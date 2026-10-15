using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(HarvesterSystemServer))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct HarvesterPlantingSystemServer : ISystem
{
    private EntityQuery _connections;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<HarvesterComponent>().Build(ref state);
        state.RequireForUpdate(query);
        _connections = new EntityQueryBuilder(Allocator.Temp).WithAll<ClientComponent, NetworkStreamConnection, NetworkStreamInGame>().Build(ref state);
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        //Prepare the current tick since it's used in multiple branches
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        NetworkTick currentTick = networkTime.ServerTick;
        if (!currentTick.IsValid) { ecb.Dispose(); return; }
        int tickRate = SystemAPI.TryGetSingleton<ClientServerTickRate>(out var rate) && rate.SimulationTickRate > 0 ? rate.SimulationTickRate : 60;

        RoundPhase currentPhase;

        if (SystemAPI.TryGetSingleton<RoundComponent>(out var roundComponent))
        {
            currentPhase = roundComponent.currentPhase;
        }
        else
        {
            Debug.LogError("[Server] Couldn't find round component for harvester planting systems");
            currentPhase = RoundPhase.ActionPhase;
        }

        if (currentPhase is RoundPhase.ActionPhase)
        {
            foreach (var (harvesterRW, harvesterTransformRW, ownerRW, harvesterEntity) in
                        SystemAPI.Query<RefRW<HarvesterComponent>, RefRW<LocalTransform>, RefRW<StuffDynamicData>>()
                        .WithAll<HarvesterPlanting>()
                        .WithEntityAccess())
            {
                NetworkTick plantStartTick = SystemAPI.GetComponentRO<HarvesterPlanting>(harvesterEntity).ValueRO.PlantStartedTick;
                if (!HarvesterActions.CanPlant(state.EntityManager, ownerRW.ValueRO.owner, harvesterEntity, currentPhase))
                {
                    HarvesterActions.CancelPlant(state.EntityManager, harvesterEntity);
                    continue;
                }
                if (currentTick.TicksSince(plantStartTick) >= tickRate * 4)
                {
                    Debug.Log("[Harvester] Server planted harvester");

                    SystemAPI.SetComponentEnabled<HarvesterPlanting>(harvesterEntity, false);
                    ecb.SetComponentEnabled<HarvesterPlanted>(harvesterEntity, true);
                    Entity characterEntity = ownerRW.ValueRO.owner;

                    //StuffSlot switchToLocation = StuffSlot.MainWeapon;
                    //Entity targetWeaponEntity = Entity.Null;
                    //DynamicBuffer<CharacterStuffList> stuffList = SystemAPI.GetBuffer<CharacterStuffList>(characterEntity);
                    //if (StuffUtils.GetStuffInSlot(stuffList, StuffSlot.MainWeapon) == Entity.Null)
                    //{
                    //    if (StuffUtils.GetStuffInSlot(stuffList, StuffSlot.SecondaryWeapon) == Entity.Null)
                    //    {
                    //        switchToLocation = StuffSlot.Melee;
                    //    }
                    //    else
                    //    {
                    //        switchToLocation = StuffSlot.Melee;
                    //    }
                    //}
                    //targetWeaponEntity = StuffUtils.GetStuffInSlot(stuffList, switchToLocation);

                    //SystemAPI.GetComponentRW<CharacterStuffInfos>(characterEntity).ValueRW.StuffInHandSlot = StuffSlot.Melee;

                    PlantHarvester(ref state, harvesterEntity);

                    float3 plantPosition = SystemAPI.GetComponentRO<LocalTransform>(characterEntity).ValueRO.Position - new float3(0, 0.75f, 0);
                    harvesterTransformRW.ValueRW.Position = plantPosition;
                    SystemAPI.GetComponentRW<HarvesterPlanting>(harvesterEntity).ValueRW.PlantStartedTick = currentTick;

                    RpcHarvesterPlanted rpc = new RpcHarvesterPlanted
                    {
                        harvester = harvesterEntity,
                        plantedTick = currentTick,
                        harvesterOwner = characterEntity,
                        plantPosition = plantPosition
                    };

                    EntityQuery query = _connections;

                    foreach (var client in query.ToEntityArray(Allocator.Temp))
                    {
                        Entity rpcEntity = ecb.CreateEntity();
                        ecb.AddComponent(rpcEntity, rpc);
                        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest
                        {
                            TargetConnection = client
                        });
                    }
                    SystemAPI.GetComponentRW<PlayerHarvesterActions>(characterEntity).ValueRW.IsPlanting = false;



                    Debug.Log("[Server] Harvester planted");
                }
            }
        }

        //RPC HANDLING____________________________________________________________________________________
        //Plant Start
        foreach ((RefRO<ReceiveRpcCommandRequest> request, RpcHarvesterPlantStart rpc, Entity entity)
            in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RpcHarvesterPlantStart>().WithEntityAccess())
        {
            //Entity roundManagerEntity;
            ecb.DestroyEntity(entity);

            if (HarvesterActions.OwnsCharacter(state.EntityManager, request.ValueRO.SourceConnection, rpc.character))
                HarvesterActions.TryStartPlant(state.EntityManager, rpc.character, rpc.harvester, currentTick, currentPhase);
        }
        //Plant Stop
        foreach ((RefRO<ReceiveRpcCommandRequest> request, RpcHarvesterPlantStop rpc, Entity entity)
            in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RpcHarvesterPlantStop>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
            if (!SystemAPI.HasComponent<StuffDynamicData>(rpc.harvester)) continue;
            var owner = SystemAPI.GetComponent<StuffDynamicData>(rpc.harvester).owner;
            if (HarvesterActions.OwnsCharacter(state.EntityManager, request.ValueRO.SourceConnection, owner))
                HarvesterActions.CancelPlant(state.EntityManager, rpc.harvester);
        }

        if (currentPhase != RoundPhase.ActionPhase)
        {
            foreach (var (plant, harvester) in SystemAPI.Query<RefRO<HarvesterPlanting>>().WithEntityAccess())
                HarvesterActions.CancelPlant(state.EntityManager, harvester);
        }

        foreach (var characterRW in SystemAPI.Query<RefRW<CharacterComponent>>())
        {
            characterRW.ValueRW.isOnSite = false;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void PlantHarvester(ref SystemState state, Entity harvester)
    {
        ChangeHarvesterOwner(ref state, harvester, Entity.Null);
    }

    private void ChangeHarvesterOwner(ref SystemState state, Entity harvesterEntity, Entity newOwner)
    {
        Entity oldOwner = SystemAPI.GetComponent<StuffDynamicData>(harvesterEntity).owner;
        var database = SystemAPI.GetSingleton<GameResourcesDatabase>();

        if (oldOwner != Entity.Null)
        {
            DynamicBuffer<UnequipStuffQueue> unequipQueue = SystemAPI.GetSingletonBuffer<UnequipStuffQueue>();

            var linkedEntityGroup = SystemAPI.GetBuffer<LinkedEntityGroup>(oldOwner);
            var ownerStuffList = SystemAPI.GetBuffer<CharacterStuffList>(oldOwner);

            //Stuff
            var stuffGhostOwnerRW = SystemAPI.GetComponentRW<GhostOwner>(harvesterEntity);
            var stuffDynamicDataRW = SystemAPI.GetComponentRW<StuffDynamicData>(harvesterEntity);
            ref var stuffData = ref SystemAPI.GetComponentRO<StuffDatabaseAccess>(harvesterEntity).ValueRO.GetData(ref database);

            StuffUtils.Unequip(ref state, oldOwner, linkedEntityGroup, ownerStuffList,
                harvesterEntity, stuffGhostOwnerRW, ref stuffData);
            stuffDynamicDataRW.ValueRW.owner = Entity.Null;

            //StuffUtils.UnequipNextFrame(unequipQueue, oldOwner, harvesterEntity);
        }

        if (newOwner != Entity.Null)
        {
            DynamicBuffer<EquipStuffQueue> equipQueue = SystemAPI.GetSingletonBuffer<EquipStuffQueue>();

            StuffUtils.EquipNextFrame(equipQueue, newOwner, harvesterEntity, false);
        }
    }
}
