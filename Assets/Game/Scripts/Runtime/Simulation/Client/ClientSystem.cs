using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

public struct ClientMessageRpcCommand : IRpcCommand
{
    public FixedString64Bytes message;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ClientSystem : SystemBase
{
    protected override void OnCreate() => RequireForUpdate<NetworkId>();
    protected override void OnUpdate()
    {
        using var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, _, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ServerMessageRpcCommand>>().WithEntityAccess())
            ecb.DestroyEntity(entity);
        ecb.Playback(EntityManager);
    }
}
