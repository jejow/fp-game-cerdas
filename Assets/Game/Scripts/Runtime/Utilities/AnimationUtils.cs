using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

public class AnimationUtils
{
    // Weapon controllers expose different parameters (for example, only the harvester has IsPlant).
    private static readonly Dictionary<RuntimeAnimatorController, Dictionary<int, AnimatorControllerParameterType>> parameterCache = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetParameterCache() => parameterCache.Clear();

    private static bool HasParameter(Animator animator, string name, AnimatorControllerParameterType type, out int hash)
    {
        hash = Animator.StringToHash(name);
        var controller = animator.runtimeAnimatorController;
        if (controller == null) return false;
        if (!parameterCache.TryGetValue(controller, out var parameters))
        {
            parameters = new Dictionary<int, AnimatorControllerParameterType>();
            foreach (var parameter in animator.parameters) parameters[parameter.nameHash] = parameter.type;
            parameterCache.Add(controller, parameters);
        }
        return parameters.TryGetValue(hash, out var actualType) && actualType == type;
    }

    // Only used in Animation System
    public static void UpdateFloatParameter(in Animator animator, DynamicBuffer<AnimationFloatBufferElement> floatBuffer)
    {
        foreach (var floatElement in floatBuffer)
        {
            if (HasParameter(animator, floatElement.Parameter.ToString(), AnimatorControllerParameterType.Float, out var hash))
                animator.SetFloat(hash, floatElement.Value);
        }
    }

    // Only used in Animation System
    public static void UpdateIntParameter(in Animator animator, DynamicBuffer<AnimationIntBufferElement> intBuffer)
    {
        foreach (var intElement in intBuffer)
        {
            if (HasParameter(animator, intElement.Parameter.ToString(), AnimatorControllerParameterType.Int, out var hash))
                animator.SetInteger(hash, intElement.Value);
        }
    }

    // Only used in Animation System
    public static void UpdateBoolParameter(in Animator animator, DynamicBuffer<AnimationBoolBufferElement> boolBuffer)
    {
        foreach (var boolElement in boolBuffer)
        {
            if (HasParameter(animator, boolElement.Parameter.ToString(), AnimatorControllerParameterType.Bool, out var hash))
                animator.SetBool(hash, boolElement.Value);
        }
    }

    // Only used in Animation System
    public static void UpdateTriggerParameter(in Animator animator, DynamicBuffer<AnimationTriggerBufferElement> triggerBuffer)
    {
        foreach (var triggerElement in triggerBuffer)
        {
            if (HasParameter(animator, triggerElement.Parameter.ToString(), AnimatorControllerParameterType.Trigger, out var hash))
                animator.SetTrigger(hash);
        }
    }

    // Only used in Animation System
    public static void SetAnimator(in Animator animator, in Entity entity, in EntityCommandBuffer ecb, in EntityManager entityManager)
    {
        if (entityManager.HasComponent<AnimatorReference>(entity))
        {
            AnimatorReference animatorRef = entityManager.GetComponentObject<AnimatorReference>(entity);

            if (animatorRef.Animator != animator)
            {
                animatorRef.Animator = animator;
            }
        }
    }

    // To be used to set a parameter outside of a job
    public static void AddFloatCommand(in FixedString32Bytes name, in float value, in Entity entity, in EntityCommandBuffer ecb)
    {
        ecb.AppendToBuffer(entity, new AnimationFloatBufferElement
        {
            Parameter = name,
            Value = value,
        });
    }

    // To be used to set a parameter in a job
    public static void AddFloatCommandJob(in FixedString32Bytes name, in float value, in Entity entity, 
        in EntityCommandBuffer.ParallelWriter ecb, in int sortKey)
    {
        ecb.AppendToBuffer(sortKey, entity, new AnimationFloatBufferElement
        {
            Parameter = name,
            Value = value,
        });
    }

    // To be used to set a parameter outside of a job
    public static void AddIntCommand(in FixedString32Bytes name, in int value, in Entity entity, 
        in EntityCommandBuffer ecb)
    {
        ecb.AppendToBuffer(entity, new AnimationIntBufferElement
        {
            Parameter = name,
            Value = value,
        });
    }

    // To be used to set a parameter in a job
    public static void AddIntCommandJob(in FixedString32Bytes name, in int value, in Entity entity, 
        in EntityCommandBuffer.ParallelWriter ecb, in int sortKey)
    {
        ecb.AppendToBuffer(sortKey, entity, new AnimationIntBufferElement
        {
            Parameter = name,
            Value = value,
        });
    }

    // To be used to set a parameter outside of a job
    public static void AddBoolCommand(in FixedString32Bytes name, in bool value, in Entity entity, 
        in EntityCommandBuffer ecb)
    {
        ecb.AppendToBuffer(entity, new AnimationBoolBufferElement
        {
            Parameter = name,
            Value = value,
        });
    }

    // To be used to set a parameter in a job
    public static void AddBoolCommandJob(in FixedString32Bytes name, in bool value, in Entity entity, 
        in EntityCommandBuffer.ParallelWriter ecb, in int sortKey)
    {
        ecb.AppendToBuffer(sortKey, entity, new AnimationBoolBufferElement
        {
            Parameter = name,
            Value = value,
        });
    }

    // To be used to set a parameter outside of a job
    public static void AddTriggerCommand(in FixedString32Bytes name, in Entity entity, 
        in EntityCommandBuffer ecb)
    {
        ecb.AppendToBuffer(entity, new AnimationTriggerBufferElement
        {
            Parameter = name,
        });
    }

    // To be used to set a parameter in a job
    public static void AddTriggerCommandJob(in FixedString32Bytes name, in Entity entity, 
        in EntityCommandBuffer.ParallelWriter ecb, in int sortKey)
    {
        ecb.AppendToBuffer(sortKey, entity, new AnimationTriggerBufferElement
        {
            Parameter = name,
        });
    }
}
