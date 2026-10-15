using Unity.Entities;
using UnityEngine;

class SoundAuthoring : MonoBehaviour
{
    public SoundGroupMapping soundMaping;
}

class SoundAuthoringBaker : Baker<SoundAuthoring>
{
    public override void Bake(SoundAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        var register = AddBuffer<SoundRegister>(entity);
        SoundUtils.AddSoundMappings(register, authoring.soundMaping.keyGroup, authoring.soundMaping.maping);
#if !UNITY_SERVER
        if (authoring.soundMaping.maping != null)
            foreach (var mapping in authoring.soundMaping.maping)
                if (mapping?.sound?.WwiseObjectReference != null) DependsOn(mapping.sound.WwiseObjectReference);
#endif

        AddComponent(entity, new SoundEmitter
        {
            keyGroup = authoring.soundMaping.keyGroup
        });
    }
}
