using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;
    private Dictionary<string, AudioClip> clipCache = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySound(string name)
    {
        name = "Sound/" + name;
        if (clipCache.TryGetValue(name, out var clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Addressables.LoadAssetAsync<AudioClip>(name).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var loadedClip = handle.Result;
                    clipCache[name] = loadedClip;
                    audioSource.PlayOneShot(loadedClip);
                }
                else
                {
                    Debug.LogWarning($"[AudioManager] Failed to load sound: {name}");
                }
            };
        }
    }
}