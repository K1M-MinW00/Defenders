using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform inactiveRoot;
    private readonly Queue<Poolable> inactiveObjects = new();
    private readonly HashSet<Poolable> activeObjects = new();

    public int InactiveCount => inactiveObjects.Count;
    public int ActiveCount => activeObjects.Count;

    public GameObjectPool(GameObject prefab, Transform inactiveRoot)
    {
        this.prefab = prefab;
        this.inactiveRoot = inactiveRoot;
    }

    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Poolable poolable = CreateNew();
            inactiveObjects.Enqueue(poolable);
        }
    }

    public Poolable Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        Poolable poolable = inactiveObjects.Count > 0 ? inactiveObjects.Dequeue() : CreateNew();

        Transform targetParent = parent != null ? parent : inactiveRoot;

        poolable.transform.SetParent(targetParent);
        poolable.transform.SetPositionAndRotation(position, rotation);
        poolable.gameObject.SetActive(true);

        activeObjects.Add(poolable);
        poolable.MarkSpawned();

        return poolable;
    }

    public void Despawn(Poolable poolable)
    {
        if (poolable == null)
            return;

        if (!activeObjects.Remove(poolable))
            return;

        poolable.MarkDespawned();

        poolable.transform.SetParent(inactiveRoot, false);
        poolable.gameObject.SetActive(false);

        inactiveObjects.Enqueue(poolable);
    }

    public void Clear()
    {
        foreach (Poolable poolable in activeObjects)
        {
            if (poolable != null)
                Object.Destroy(poolable.gameObject);
        }

        activeObjects.Clear();

        while (inactiveObjects.Count > 0)
        {
            Poolable poolable = inactiveObjects.Dequeue();

            if (poolable != null)
                Object.Destroy(poolable.gameObject);
        }
    }

    private Poolable CreateNew()
    {
        GameObject obj = Object.Instantiate(prefab, inactiveRoot);
        obj.SetActive(false);

        if (!obj.TryGetComponent(out Poolable poolable))
            poolable = obj.AddComponent<Poolable>();

        poolable.SetOwner(this);
        return poolable;
    }
}