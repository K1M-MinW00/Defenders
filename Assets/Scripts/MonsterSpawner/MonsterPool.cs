using UnityEngine;
using System.Collections.Generic;

public class MonsterPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public string monsterId;
        public GameObject prefab;
        public int initialCount;
    }

    public List<PoolItem> poolItems;

    private Dictionary<string, Queue<GameObject>> pool = new();

    private void Awake()
    {
        foreach (var item in poolItems)
        {
            Queue<GameObject> queue = new();

            for (int i = 0; i < item.initialCount; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);

            }

            pool[item.monsterId] = queue;
        }
    }

    public GameObject Spawn(string monsterId, Vector3 position)
    {
        if (!pool.ContainsKey(monsterId))
        {
            Debug.LogError($"MonsterPool: No pool for {monsterId}");
            return null;
        }

        GameObject obj = pool[monsterId].Count > 0
            ? pool[monsterId].Dequeue()
            : Instantiate(GetPrefab(monsterId), transform);

        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void Despawn(string monsterId, GameObject obj)
    {
        obj.SetActive(false);
        pool[monsterId].Enqueue(obj);
    }

    private GameObject GetPrefab(string monsterId)
    {
        foreach (var item in poolItems)
        {
            if (item.monsterId == monsterId)
                return item.prefab;
        }
        return null;
    }
}
