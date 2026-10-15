using GameNetwork.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static Unity.NetCode.ClientServerBootstrap;

public static class PlayerHelpers
{
    public struct AliveCounts
    {
        public int natifPlayersAlive;
        public int corpoPlayersAlive;
    }

    public struct GlobalTeamCount
    {
        public int natifPlayersCount;
        public int corpoPlayersCount;
        public int neutralPlayersCount;
    }

    public struct TeamList
    {
        public List<string> natifPlayersId;
        public List<string> corpoPlayersid;
        public List<string> neutralPlayersId;
    }

    private static TeamList _teams = new TeamList
    {
        natifPlayersId = new List<string>(),
        corpoPlayersid = new List<string>(),
        neutralPlayersId = new List<string>()
    };

    private static int CountPlayersAliveForTeam(TeamSideType team, World world)
    {
        int aliveCount = 0;

        ComponentType teamTag;
        switch (team)
        {
            case TeamSideType.Corpo:
                teamTag = ComponentType.ReadOnly<CorpoTeamTag>();
                break;
            case TeamSideType.Natif:
                teamTag = ComponentType.ReadOnly<NatifTeamTag>();
                break;
            default:
                Debug.LogWarning($"Team {team} does not have a defined tag component.");
                return 0;
        }

        EntityManager entityManager = world.EntityManager;

        using EntityQuery characterQuery = entityManager.CreateEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
            ComponentType.ReadOnly<CharacterClientAttachedComponent>(),
            ComponentType.ReadOnly<CharacterIsEnable>(),
            teamTag
            },
            Options = EntityQueryOptions.IgnoreComponentEnabledState // Allows checking enable state manually.
        });

        NativeArray<Entity> characterEntities = characterQuery.ToEntityArray(Allocator.Temp);
        //Debug.Log($"[CountPlayersAlive] Filtered entity count: {characterEntities.Length} for team {team}.");

        for (int i = 0; i < characterEntities.Length; i++)
        {
            Entity characterEntity = characterEntities[i];
            if (!entityManager.IsComponentEnabled<CharacterIsEnable>(characterEntity))
            {
                continue;
            }
            aliveCount++;

            //Debug.Log($"[CountPlayersAlive] Counting entity {characterEntity} for team {team}.");
        }

        characterEntities.Dispose();
        return aliveCount;
    }

    public static TeamSideType AssignLocalTeam(string playerId, TeamSideType team)
    {
        _teams.corpoPlayersid.Remove(playerId);
        _teams.natifPlayersId.Remove(playerId);
        _teams.neutralPlayersId.Remove(playerId);
        switch (team)
        {
            case TeamSideType.Corpo: _teams.corpoPlayersid.Add(playerId); break;
            case TeamSideType.Natif: _teams.natifPlayersId.Add(playerId); break;
            default: _teams.neutralPlayersId.Add(playerId); break;
        }
        return team;
    }

    public static TeamSideType GetCharacterTeam(Entity character, World world)
    {
        if (world == null || !world.IsCreated) return TeamSideType.Neutre;
        return GetCharacterTeam(character, world.EntityManager);
    }

    public static TeamSideType GetCharacterTeam(Entity character, EntityManager em)
    {
        if (!em.Exists(character)) return TeamSideType.Neutre;
        if (em.HasComponent<ClientComponent>(character)) return em.GetComponentData<ClientComponent>(character).team;
        if (!em.HasComponent<CharacterClientAttachedComponent>(character)) return TeamSideType.Neutre;
        var participant = em.GetComponentData<CharacterClientAttachedComponent>(character).ClientEntity;
        return em.Exists(participant) && em.HasComponent<ClientComponent>(participant)
            ? em.GetComponentData<ClientComponent>(participant).team : TeamSideType.Neutre;
    }

    public static void RemovePlayer(string playerId)
    {
        var corpo = _teams.corpoPlayersid.FirstOrDefault(id => id == playerId);
        if (corpo != null)
        {
            Debug.Log($"Player removed from CORPO ID {playerId} ");
            _teams.corpoPlayersid.Remove(corpo);
            return;
        }

        var natif = _teams.natifPlayersId.FirstOrDefault(id => id == playerId);
        if (natif != null)
        {
            Debug.Log($"Player removed from NATIF ID {playerId} ");
            _teams.natifPlayersId.Remove(natif);
            return;
        }

        var neutral = _teams.neutralPlayersId.FirstOrDefault(id => id == playerId);
        if (neutral != null)
        {
            Debug.Log($"Player removed from NEUTRAL ID {playerId} ");
            _teams.neutralPlayersId.Remove(neutral);
            return;
        }
        Debug.LogWarning($"[PlayerHelpers] Tried to remove {playerId} but they weren’t in any team list");
    }


    private static int GetPlayersAlive(TeamSideType team, World world)
    {
        return CountPlayersAliveForTeam(team, world);
    }

    public static AliveCounts GetCurrentAliveCounts(World world)
    {
        AliveCounts counts;
        counts.natifPlayersAlive = GetPlayersAlive(TeamSideType.Natif, world);
        counts.corpoPlayersAlive = GetPlayersAlive(TeamSideType.Corpo, world);
        return counts;
    }


    public static GlobalTeamCount GetCurrentTeamCounts()
    {
        return new GlobalTeamCount
        {
            corpoPlayersCount = _teams.corpoPlayersid.Count,
            natifPlayersCount = _teams.natifPlayersId.Count,
            neutralPlayersCount = _teams.neutralPlayersId.Count,
        };
    }

    public static TeamList GetTeamList()
    {
        return _teams;
    }

    public static List<int> GetClientPlayerIdsByTeam(TeamSideType teamSide)
    {
        var world = ClientWorld;
        if (world == null || !world.IsCreated)
            return new List<int>();

        var em = world.EntityManager;

        using var query = em.CreateEntityQuery(
            ComponentType.ReadOnly<GhostOwner>(),
            ComponentType.ReadOnly<ClientComponent>()
        );

        using (var owners = query.ToComponentDataArray<GhostOwner>(Allocator.TempJob))
        using (var clients = query.ToComponentDataArray<ClientComponent>(Allocator.TempJob))
        {
            var result = new List<int>(owners.Length);
            for (int i = 0; i < owners.Length; i++)
            {
                if (clients[i].team == teamSide)
                    result.Add(owners[i].NetworkId);
            }
            return result;
        }
    }

    public static List<ClientComponent> GetClientPlayersByTeam(TeamSideType teamSide)
    {
        var world = ClientWorld;
        if (world == null || !world.IsCreated)
            return new List<ClientComponent>();

        var em = world.EntityManager;
        using var query = em.CreateEntityQuery(
            ComponentType.ReadOnly<GhostOwner>(),
            ComponentType.ReadOnly<ClientComponent>()
        );

        using (var clients = query.ToComponentDataArray<ClientComponent>(Allocator.TempJob))
        {
            var result = new List<ClientComponent>(clients.Length);
            for (int i = 0; i < clients.Length; i++)
            {
                if (clients[i].team == teamSide)
                    result.Add(clients[i]);
            }
            return result;
        }
    }



    public static IReadOnlyList<string> GetPlayersByTeamOnServer(TeamSideType teamSide)
    {
        switch (teamSide)
        {
            case TeamSideType.Corpo:
                return _teams.corpoPlayersid;
            case TeamSideType.Natif:
                return _teams.natifPlayersId;
            default:
                return Array.Empty<string>();
        }
    }

    public static IReadOnlyList<string> GetPlayersByTeamOnServer(string teamName)
    {
        TeamSideType teamSide = teamName == "Corpo" ? TeamSideType.Corpo : TeamSideType.Natif;
        switch (teamSide)
        {
            case TeamSideType.Corpo:
                return _teams.corpoPlayersid;
            case TeamSideType.Natif:
                return _teams.natifPlayersId;
            default:
                return Array.Empty<string>();
        }
    }

    public static void ClearTeamOnServer(string teamName)
    {
        TeamSideType teamSide = teamName == "Corpo" ? TeamSideType.Corpo : TeamSideType.Natif;
        switch (teamSide)
        {
            case TeamSideType.Corpo:
                _teams.corpoPlayersid.Clear();
                break;
            case TeamSideType.Natif:
                _teams.natifPlayersId.Clear();
                break;
            default:

                break;
        }
    }

    public static void ClearTeamOnServer(TeamSideType teamSide)
    {
        switch (teamSide)
        {
            case TeamSideType.Corpo:
                _teams.corpoPlayersid.Clear();
                break;
            case TeamSideType.Natif:
                _teams.natifPlayersId.Clear();
                break;
            default:

                break;
        }
    }

    public static void ClearTeams()
    {
        _teams.corpoPlayersid.Clear();
        _teams.natifPlayersId.Clear();
        _teams.neutralPlayersId.Clear();
    }

    public static TeamSideType GetPlayerInTeamOnServer(int networkId)
    {
        var world = ClientTransportHelper.ServerWorld;
        if (world == null || !world.IsCreated) return TeamSideType.Neutre;
        using var query = world.EntityManager.CreateEntityQuery(typeof(ClientComponent), typeof(GhostOwner));
        using var entities = query.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            var client = world.EntityManager.GetComponentData<ClientComponent>(entity);
            if (client.networkID == networkId) return client.team;
        }
        return TeamSideType.Neutre;
    }

    public static TeamSideType GetPlayerInTeam(int networkId)
    {
        var world = ClientWorld;
        if (world == null || !world.IsCreated)
            return TeamSideType.Neutre;

        var em = world.EntityManager;

        using var query = em.CreateEntityQuery(
            ComponentType.ReadOnly<GhostOwner>(),
            ComponentType.ReadOnly<ClientComponent>()
        );

        using (var owners = query.ToComponentDataArray<GhostOwner>(Allocator.TempJob))
        using (var clients = query.ToComponentDataArray<ClientComponent>(Allocator.TempJob))
        {
            for (int i = 0; i < owners.Length; i++)
            {
                if (owners[i].NetworkId == networkId)
                    return clients[i].team;
            }
        }

        return TeamSideType.Neutre;
    }

    public static void SetPlayerName(int networkId, string newName, EntityCommandBuffer ecb)
    {
        var world = ClientWorld;
        if (world == null || !world.IsCreated)
            return;
        Debug.Log($"[name] networkId {networkId}, newName {newName}");
        var em = world.EntityManager;

        using var query = em.CreateEntityQuery(
            ComponentType.ReadOnly<GhostOwner>(),
            ComponentType.ReadOnly<ClientComponent>()
        );

        using (var entities = query.ToEntityArray(Allocator.TempJob))
        using (var owners = query.ToComponentDataArray<GhostOwner>(Allocator.TempJob))
        {
            for (int i = 0; i < owners.Length; i++)
            {
                if (owners[i].NetworkId == networkId)
                {
                    var client = em.GetComponentData<ClientComponent>(entities[i]);
                    client.name = newName;
                    ecb.SetComponent(entities[i], client);
                    Debug.Log($"[name] new client.name is {client.name}");
                }
            }
        }
    }

    public static ClientComponent GetPlayer(int networkId)
    {
        var world = ClientWorld;
        if (world == null || !world.IsCreated)
            return default;

        var em = world.EntityManager;

        using var query = em.CreateEntityQuery(
            ComponentType.ReadOnly<GhostOwner>(),
            ComponentType.ReadOnly<ClientComponent>()
        );

        using (var owners = query.ToComponentDataArray<GhostOwner>(Allocator.TempJob))
        using (var clients = query.ToComponentDataArray<ClientComponent>(Allocator.TempJob))
        {
            for (int i = 0; i < owners.Length; i++)
            {
                if (owners[i].NetworkId == networkId)
                    return clients[i];
            }
        }

        return default;
    }
}
