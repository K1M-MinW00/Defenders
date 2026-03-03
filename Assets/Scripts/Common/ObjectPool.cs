using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Dictionary<string, Queue<GameObject>> pool = new();
    private Dictionary<string, GameObject> prefabCache = new();

    public void Prewarm(string key, GameObject prefab, int count)
    {
        if (!pool.ContainsKey(key))
            pool[key] = new Queue<GameObject>();

        if (!prefabCache.ContainsKey(key))
            prefabCache[key] = prefab;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool[key].Enqueue(obj);
        }
    }

    public GameObject Spawn(string key, Vector3 position)
    {
        if (!pool.ContainsKey(key))
        {
            Debug.LogError($"Pool not found: {key}");
            return null;
        }

        GameObject obj;

        if (pool[key].Count > 0)
        {
            obj = pool[key].Dequeue();
        }
        else
        {
            obj = Instantiate(prefabCache[key], transform);
        }

        obj.transform.position = position;
        obj.SetActive(true);

        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnSpawn();

        return obj;
    }

    public GameObject Spawn(string key, Vector3 position, Transform parent)
    {
        if (!pool.ContainsKey(key))
        {
            Debug.LogError($"Pool not found: {key}");
            return null;
        }

        GameObject obj;

        if (pool[key].Count > 0)
        {
            obj = pool[key].Dequeue();
        }
        else
        {
            obj = Instantiate(prefabCache[key], transform);
        }

        obj.transform.SetParent(parent, true);
        obj.transform.position = position;
        obj.SetActive(true);

        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnSpawn();

        return obj;
    }

    public void Despawn(string key, GameObject obj)
    {
        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnDespawn();

        obj.SetActive(false);
        pool[key].Enqueue(obj);
    }
}
