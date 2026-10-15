using GameNetwork.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct OfflineBot : IComponentData { public int Index; }

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(ServerSystem))]
[UpdateBefore(typeof(RespawnSystem))]
public partial class OfflineParticipantSystem : SystemBase
{
    protected override void OnCreate() => RequireForUpdate<ClientPrefabData>();

    protected override void OnUpdate()
    {
        if (!OfflineMatch.IsActive) return;
        TeamSideType humanTeam = TeamSideType.Neutre;
        foreach (var client in SystemAPI.Query<RefRO<ClientComponent>>().WithAll<GhostOwner>().WithNone<OfflineBot>())
        {
            humanTeam = client.ValueRO.team;
            break;
        }
        if (humanTeam == TeamSideType.Neutre) return;
        var enemyTeam = humanTeam == TeamSideType.Corpo ? TeamSideType.Natif : TeamSideType.Corpo;
        using var ecb = new EntityCommandBuffer(Allocator.Temp);
        int count = 0;
        foreach (var (bot, client, attached, entity) in SystemAPI
            .Query<RefRO<OfflineBot>, RefRW<ClientComponent>, RefRW<ClientCharacterAttached>>().WithEntityAccess())
        {
            count++;
            var botTeam = bot.ValueRO.Index < OfflineMatch.AllyBotCount ? humanTeam : enemyTeam;
            if (client.ValueRO.team == botTeam) continue;
            client.ValueRW.team = botTeam;
            PlayerHelpers.AssignLocalTeam(client.ValueRO.playerID.ToString(), botTeam);
            if (EntityManager.Exists(attached.ValueRO.Value)) ecb.DestroyEntity(attached.ValueRO.Value);
            attached.ValueRW.Value = Entity.Null;
            ecb.AddComponent<WaitForRespawnTag>(entity);
        }
        var prefabs = SystemAPI.GetSingleton<ClientPrefabData>();
        for (int i = count; i < OfflineMatch.BotCount; i++)
        {
            var botTeam = i < OfflineMatch.AllyBotCount ? humanTeam : enemyTeam;
            var participant = ecb.Instantiate(prefabs.Client);
            var data = new ClientComponent
            {
                networkID = -(i + 1), // Participant identity; never a transport connection ID.
                playerID = $"offline-bot-{i + 1}",
                name = $"Bot {i + 1}",
                team = botTeam
            };
            ecb.SetComponent(participant, new GhostOwner { NetworkId = 0 });
            ecb.SetComponent(participant, data);
            ecb.AddComponent(participant, new OfflineBot { Index = i });
            PlayerHelpers.AssignLocalTeam(data.playerID.ToString(), botTeam);
        }
        ecb.Playback(EntityManager);
    }
}
