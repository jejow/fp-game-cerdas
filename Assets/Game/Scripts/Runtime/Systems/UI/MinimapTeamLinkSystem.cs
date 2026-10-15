using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct MinimapTeamLinkSystem : ISystem
{
    public class MinimapTeamArgs : EventArgs
    {
        public int TeamId;
        public int PlayerId;
        public float3 Position;
        public float3 Forward;
        public bool Alive;
    }
    public static EventHandler<MinimapTeamArgs> OnMinimapTeamLinkEvent;

    public void OnUpdate(ref SystemState state)
    {
        var localTeam = TeamSideType.Neutre;
        foreach (var client in SystemAPI.Query<RefRO<ClientComponent>>().WithAll<GhostOwnerIsLocal>())
        {
            localTeam = client.ValueRO.team;
            break;
        }
        if (localTeam == TeamSideType.Neutre) return;
        int count = 1;

        foreach (var (localTransform, ghostOwner, entity) in SystemAPI
            .Query<RefRO<LocalTransform>, RefRO<GhostOwner>>()
            .WithAll<CharacterComponent>()
            .WithNone<GhostOwnerIsLocal>()
            .WithEntityAccess())
        {
            var team = PlayerHelpers.GetCharacterTeam(entity, state.EntityManager);
            if (team != localTeam || count > OfflineMatchOptions.MaxAllyBots) continue;
            OnMinimapTeamLinkEvent?.Invoke(this, new MinimapTeamArgs
            {
                TeamId = (int)team,
                PlayerId = count,
                Position = localTransform.ValueRO.Position,
                Forward = localTransform.ValueRO.Forward(),
                Alive = state.EntityManager.IsComponentEnabled(entity, typeof(CharacterIsEnable))
            });

            count++;
        }
        // Clear pins left by teammates whose character was removed or changed teams.
        while (count <= OfflineMatchOptions.MaxAllyBots)
            OnMinimapTeamLinkEvent?.Invoke(this, new MinimapTeamArgs { TeamId = (int)localTeam, PlayerId = count++, Alive = false });
    }
}
