using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
#if !UNITY_SERVER
    public Dictionary<string, AK.Wwise.Event> bank = new();
    public AK.Wwise.RTPC volumeRTPC = null;
    private bool fallbackBanksLoaded;
    private float lastVolume = float.NaN;
    private Transform[] emitters;
#endif

    //GameObject go;
    int goId = 0;
    //protected override void Awake()
    //{
    //base.Awake();
    //go = Instantiate(new GameObject($"TempSoundEmitter"));
    //DontDestroyOnLoad(go);
    //}

    public void Play(string keyGroup, string keyAction, Vector3 pos)
    {
#if !UNITY_SERVER
        EnsureFallbackBanks();
        string key = keyGroup + keyAction;

        if (bank.TryGetValue(key, out AK.Wwise.Event sound))
        {
            Transform[] childrens = GetEmitters();
            if (childrens.Length == 1)
            {
                // The menu can be opened directly without the splash-screen prefab.
                for (int i = 0; i < 16; i++)
                {
                    var emitter = new GameObject("SoundEmitter" + i);
                    emitter.transform.SetParent(transform, false);
                }
                childrens = GetComponentsInChildren<Transform>();
                emitters = childrens;
            }

            goId++;
            if (goId >= childrens.Length)
            {
                goId = 1;
            }

            childrens[goId].transform.position = pos;
            sound.Post(childrens[goId].gameObject);
        }
        else
        {
            Debug.LogWarning($"Sound {key} not found in bank Dictionary");
        }
#endif
    }

    public void SetVolume(float volume)
    {
#if !UNITY_SERVER
        if (!AkUnitySoundEngine.IsInitialized()) return;
        EnsureFallbackBanks();
        if (Mathf.Approximately(volume, lastVolume)) return;
        lastVolume = volume;
        if (volumeRTPC == null)
        {
            AkUnitySoundEngine.SetRTPCValue("Volume", volume);
            return;
        }
        foreach (var transform in GetEmitters())
        {
            volumeRTPC.SetValue(transform.gameObject, volume);
        }
#endif
    }

#if !UNITY_SERVER
    private Transform[] GetEmitters()
    {
        if (emitters == null || emitters.Length == 0) emitters = GetComponentsInChildren<Transform>();
        return emitters;
    }

    private void EnsureFallbackBanks()
    {
        if (volumeRTPC != null || fallbackBanksLoaded || !AkUnitySoundEngine.IsInitialized()) return;
        AkBankManager.LoadBank("DefaultBank", false, false);
        AkBankManager.LoadBank("MainMenu", false, false);
        AkBankManager.LoadBank("VoiceLines", false, false);
        fallbackBanksLoaded = true;
    }
#endif
}
