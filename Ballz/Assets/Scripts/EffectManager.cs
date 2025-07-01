using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> pool = new();
    private Dictionary<string, GameObject> prefabCache = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayEffect(string key, Vector3 position)
    {
        key = "Effect/" + key;
        if (pool.TryGetValue(key, out var q) && q.Count > 0)
        {
            GameObject obj = q.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
            StartCoroutine(ReturnWhenDone(key, obj));
            return;
        }

        if (prefabCache.TryGetValue(key, out var prefab))
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            StartCoroutine(ReturnWhenDone(key, obj));
            return;
        }

        // Load from Addressables if not cached
        Addressables.LoadAssetAsync<GameObject>(key).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = handle.Result;
                prefabCache[key] = prefab;

                GameObject obj = Instantiate(prefab, position, Quaternion.identity);
                StartCoroutine(ReturnWhenDone(key, obj));
            }
            else
            {
                Debug.LogError($"[EffectManager] Failed to load: {key}");
            }
        };
    }

    private IEnumerator ReturnWhenDone(string key, GameObject obj)
    {
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogWarning($"[EffectManager] {key} has no ParticleSystem.");
            yield break;
        }

        ps.Play();
        yield return new WaitWhile(() => ps.IsAlive(true));

        obj.SetActive(false);

        if (!pool.ContainsKey(key))
            pool[key] = new Queue<GameObject>();

        pool[key].Enqueue(obj);
    }
}