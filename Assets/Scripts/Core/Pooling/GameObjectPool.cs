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
        Poolable poolable = TakeInactiveOrCreate();

        Transform targetParent = parent != null ? parent : inactiveRoot;

        poolable.transform.SetParent(targetParent);
        poolable.transform.SetPositionAndRotation(position, rotation);
        poolable.gameObject.SetActive(true);

        activeObjects.Add(poolable);
        poolable.MarkSpawned();

        return poolable;
    }

    public bool Despawn(Poolable poolable)
    {
        if (poolable == null)
            return false;

        if (!poolable.IsOwnedBy(this))
        {
            Debug.LogError($"[{nameof(GameObjectPool)}] An object was returned to a pool that does not own it: {poolable.name}");
            return false;
        }

        if (!activeObjects.Remove(poolable))
            return false;

        poolable.MarkDespawned();

        poolable.transform.SetParent(inactiveRoot, false);
        poolable.gameObject.SetActive(false);

        inactiveObjects.Enqueue(poolable);
        return true;
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

    public void DespawnAll()
    {
        if (activeObjects.Count == 0)
            return;

        var snapshot = new List<Poolable>(activeObjects);
        foreach (Poolable poolable in snapshot)
        {
            if (poolable != null)
                Despawn(poolable);
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

    private Poolable TakeInactiveOrCreate()
    {
        while (inactiveObjects.Count > 0)
        {
            Poolable poolable = inactiveObjects.Dequeue();
            if (poolable != null)
                return poolable;
        }

        return CreateNew();
    }
}
