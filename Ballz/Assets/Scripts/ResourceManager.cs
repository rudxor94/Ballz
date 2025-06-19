using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public GameObject unusedObject;
    public int preloadCountPerPrefab = 1;
    public string preloadLabel = "Preload";

    private Dictionary<string, Queue<GameObject>> pool = new();
    private Dictionary<string, GameObject> prefabCache = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(PreloadByLabel(preloadLabel));
    }

    private IEnumerator<AsyncOperationHandle> PreloadByLabel(string label)
    {
        var locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
        yield return locationHandle;

        if (locationHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("[ResourceManager] Failed to load resource locations.");
            yield break;
        }

        foreach (var loc in locationHandle.Result)
        {
            string key = loc.PrimaryKey;

            var loadHandle = Addressables.LoadAssetAsync<GameObject>(key);
            yield return loadHandle;

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = loadHandle.Result;
                prefabCache[key] = prefab;
                pool[key] = new Queue<GameObject>();

                for (int i = 0; i < preloadCountPerPrefab; i++)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(unusedObject.transform);
                    pool[key].Enqueue(obj);
                }
            }
            else
            {
                Debug.LogError($"[ResourceManager] Failed to preload: {key}");
            }
        }
    }

    public GameObject GetResource(string key)
    {
        if (pool.TryGetValue(key, out var q) && q.Count > 0)
        {
            var obj = q.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        if (prefabCache.TryGetValue(key, out var prefab))
        {
            return Instantiate(prefab);
        }

        Debug.LogWarning($"[ResourceManager] Prefab not loaded for key: {key}");
        return null;
    }

    public void DestroyResource(string key, GameObject obj)
    {
        if (!pool.ContainsKey(key))
            pool[key] = new Queue<GameObject>();

        obj.SetActive(false);
        obj.transform.SetParent(unusedObject.transform);
        pool[key].Enqueue(obj);
    }
}