using System.Collections.Generic;
using UnityEngine;

public enum PoolCategory
{
    Monster,
    Projectile,
    Effect,
    UI
}

public class StagePoolManager : MonoBehaviour
{
    [Header("Pool Roots")]
    [SerializeField] private Transform monsterRoot;
    [SerializeField] private Transform projectileRoot;
    [SerializeField] private Transform effectRoot;
    [SerializeField] private Transform uiRoot;

    private readonly Dictionary<GameObject, GameObjectPool> pools = new();

    public void Prewarm(GameObject prefab, int count, PoolCategory category)
    {
        if (prefab == null || count <= 0)
            return;

        GameObjectPool pool = GetOrCreatePool(prefab, category);
        pool.Prewarm(count);
    }

    public T Spawn<T>(T prefab,Vector3 position,Quaternion rotation,PoolCategory category,Transform parent = null) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogError("Spawn failed. Prefab is null.");
            return null;
        }

        Poolable poolable = Spawn(prefab.gameObject,position,rotation,category,parent);

        return poolable != null ? poolable.GetComponent<T>() : null;
    }

    public Poolable Spawn(GameObject prefab,Vector3 position,Quaternion rotation,PoolCategory category,Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("Spawn failed. Prefab is null.");
            return null;
        }

        GameObjectPool pool = GetOrCreatePool(prefab, category);
        return pool.Spawn(position, rotation, parent);
    }

    public void Despawn(Poolable poolable)
    {
        if (poolable == null)
            return;

        poolable.ReturnToPool();
    }

    public void ClearAll()
    {
        foreach (GameObjectPool pool in pools.Values)
            pool.Clear();

        pools.Clear();
    }

    private GameObjectPool GetOrCreatePool(GameObject prefab, PoolCategory category)
    {
        if (pools.TryGetValue(prefab, out GameObjectPool pool))
            return pool;

        Transform root = GetRoot(category);
        pool = new GameObjectPool(prefab, root);
        pools.Add(prefab, pool);

        return pool;
    }

    private Transform GetRoot(PoolCategory category)
    {
        return category switch
        {
            PoolCategory.Monster => monsterRoot,
            PoolCategory.Projectile => projectileRoot,
            PoolCategory.Effect => effectRoot,
            PoolCategory.UI => uiRoot,
            _ => transform
        };
    }

    private void OnDestroy()
    {
        ClearAll();
    }
    public int GetInactiveCount(GameObject prefab)
    {
        if (prefab == null)
            return 0;

        return pools.TryGetValue(prefab, out GameObjectPool pool) ? pool.InactiveCount : 0;
    }
}