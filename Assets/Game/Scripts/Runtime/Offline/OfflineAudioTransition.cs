using System;
using UnityEngine;

/// <summary>Pauses audio throughout nested scene, entity-world and navigation transitions.</summary>
public sealed class OfflineAudioTransition : IDisposable
{
    private static int depth;
    private static bool suspended;
    private bool disposed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        depth = 0;
        suspended = false;
    }

    public OfflineAudioTransition()
    {
        depth++;
#if !UNITY_SERVER
        if (depth == 1 && AkUnitySoundEngine.IsInitialized())
        {
            AkUnitySoundEngine.StopAll();
            AkUnitySoundEngine.RenderAudio();
            suspended = AkUnitySoundEngine.Suspend(false, false) == AKRESULT.AK_Success;
        }
#endif
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        depth = Math.Max(0, depth - 1);
#if !UNITY_SERVER
        if (depth == 0 && suspended)
        {
            if (AkUnitySoundEngine.IsInitialized()) AkUnitySoundEngine.WakeupFromSuspend(100);
            suspended = false;
        }
#endif
    }
}
