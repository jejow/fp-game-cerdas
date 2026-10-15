using GameNetwork.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct ServerMessageRpcCommand : IRpcCommand
{
    public FixedString64Bytes message;
}

[GhostComponent]
public struct ClientComponent : IComponentData
{
    [GhostField] public int networkID;
    [GhostField] public FixedString64Bytes playerID;
    [GhostField] public TeamSideType team;
    [GhostField] public FixedString64Bytes name;
}

/// <summary>Creates the one human participant in the local simulation.</summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ServerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<NetworkId>();
        RequireForUpdate<ClientPrefabData>();
    }

    protected override void OnUpdate()
    {
        if (!OfflineMatch.IsActive) return;
        using var ecb = new EntityCommandBuffer(Allocator.Temp);
        var prefab = SystemAPI.GetSingleton<ClientPrefabData>().Client;
        foreach (var (id, connection) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<ClientComponent>().WithEntityAccess())
        {
            var data = new ClientComponent
            {
                networkID = id.ValueRO.Value,
                playerID = OfflineMatch.LocalPlayerId,
                name = "Player",
                team = TeamSideType.Neutre
            };
            var participant = ecb.Instantiate(prefab);
            ecb.SetComponent(participant, new GhostOwner { NetworkId = data.networkID });
            ecb.SetComponent(participant, data);
            ecb.AppendToBuffer(connection, new LinkedEntityGroup { Value = participant });
            ecb.AddComponent(connection, data);
            PlayerHelpers.AssignLocalTeam(data.playerID.ToString(), data.team);
        }

        foreach (var (request, name, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<NameCommand>>().WithEntityAccess())
        {
            var connection = request.ValueRO.SourceConnection;
            if (EntityManager.Exists(connection) && EntityManager.HasComponent<ClientComponent>(connection))
            {
                var player = EntityManager.GetComponentData<ClientComponent>(connection);
                foreach (var (client, entity) in SystemAPI.Query<RefRO<ClientComponent>>().WithEntityAccess())
                {
                    if (client.ValueRO.networkID != player.networkID) continue;
                    var updated = client.ValueRO;
                    updated.name = name.ValueRO.name;
                    ecb.SetComponent(entity, updated);
                }
            }
            ecb.DestroyEntity(rpcEntity);
        }
        foreach (var (_, entity) in SystemAPI.Query<RefRO<ClientMessageRpcCommand>>().WithEntityAccess())
            ecb.DestroyEntity(entity);
        ecb.Playback(EntityManager);
    }
}
