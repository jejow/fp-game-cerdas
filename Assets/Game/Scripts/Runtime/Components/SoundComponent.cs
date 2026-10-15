
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public struct SoundRpc : IRpcCommand
{
    public FixedString32Bytes keyGroup;
    public FixedString32Bytes keyAction;
    public float3 pos;
    public TeamSideType side;
}

public struct SoundRegister : IBufferElementData
{
    public FixedString128Bytes key;
#if !UNITY_SERVER
    // Keep asset references unmanaged while entity scenes load in parallel.
    public UnityObjectRef<WwiseEventReference> eventReference;
#endif
}


[GhostComponent]
public struct SoundEmitter : IComponentData
{
    [GhostField] public FixedString32Bytes keyGroup;
    [GhostField] public float timer;
}

