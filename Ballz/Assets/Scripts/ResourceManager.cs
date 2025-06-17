using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public GameObject unusedObject;
    public GameObject brokenWallPrefab;
    public GameObject itemPrefab;
    public GameObject ballPrefab;
    public GameObject addBallPrefab;

    private Dictionary<string, Stack<GameObject>> unused = new Dictionary<string, Stack<GameObject>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Instance = this;
        unused.Add("BrokenWall", new Stack<GameObject>());
        unused.Add("Item", new Stack<GameObject>());
        unused.Add("AddBall", new Stack<GameObject>());
        unused.Add("Ball", new Stack<GameObject>());
    }

    public GameObject GetResource(string key)
    {
        if (unused[key].Count > 0)
        {
            return unused[key].Pop();
        }
        else
        {
            GameObject prefab = brokenWallPrefab;
            switch (key)
            {
                case "Item":
                    prefab = itemPrefab;
                    break;

                case "Ball":
                    prefab = ballPrefab;
                    break;

                case "AddBall":
                    prefab = addBallPrefab;
                    break;
            }

            return Instantiate(prefab);
        }
    }

    public void DestroyResource(string key, GameObject resource)
    {
        resource.transform.parent = unusedObject.transform;
        resource.SetActive(false);
        unused[key].Push(resource);
    }
}