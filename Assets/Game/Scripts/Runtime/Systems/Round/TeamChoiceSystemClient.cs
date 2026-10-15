using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct TeamChoiceComponent : IComponentData
{
    public TeamSideType team;
}

 public struct MessageToServer : IRpcCommand
{
    public FixedString32Bytes word;
}


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct TeamChoiceSystemClient : ISystem
{
    static public void SendTeamChoice(EntityManager entityManager, TeamSideType team)
    {
        using var entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ClientComponent, GhostOwnerIsLocal>()
            .WithNone<TeamChoiceComponent>().Build(entityManager);
        using var entities = entityQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            entityManager.AddComponentData(entity, new TeamChoiceComponent { team = team });
        }
        
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var (client, choiceTeam, clientEntity) in
        SystemAPI.Query<ClientComponent, RefRO<TeamChoiceComponent>>()
        .WithAll<GhostOwnerIsLocal>()
        .WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<GhostOwner>(clientEntity))
            {
                Debug.LogWarning("No GhostOwner component found on the correct entity.");
                continue;
            }

            var ghostOwner = SystemAPI.GetComponent<GhostOwner>(clientEntity);
            var networkID = ghostOwner.NetworkId;  

            ChooseTeamRpc rpc = new ChooseTeamRpc
            {
                networkID = networkID,  
                team = choiceTeam.ValueRO.team
            };

            RpcUtils.SendClientToServerRpc(ref rpc);
            ecb.RemoveComponent<TeamChoiceComponent>(clientEntity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
