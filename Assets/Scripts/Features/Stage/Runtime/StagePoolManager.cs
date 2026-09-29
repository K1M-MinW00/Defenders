using System.Collections.Generic;
using UnityEngine;

public enum PoolCategory
{
    Unit,
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
    private readonly Dictionary<GameObject, PoolCategory> poolCategories = new();
    private readonly Dictionary<PoolCategory, HashSet<GameObjectPool>> poolsByCategory = new();

    public void Prewarm(GameObject prefab, int count, PoolCategory category)
    {
        if (prefab == null || count <= 0)
            return;

        if (TryGetOrCreatePool(prefab, category, out GameObjectPool pool))
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

        return TryGetOrCreatePool(prefab, category, out GameObjectPool pool)
            ? pool.Spawn(position, rotation, parent)
            : null;
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
        poolCategories.Clear();
        poolsByCategory.Clear();
    }

    public void DespawnAll(PoolCategory category)
    {
        if (!poolsByCategory.TryGetValue(category, out HashSet<GameObjectPool> categoryPools))
            return;

        foreach (GameObjectPool pool in categoryPools)
            pool.DespawnAll();
    }

    public void DespawnWaveObjects()
    {
        DespawnAll(PoolCategory.Monster);
        DespawnAll(PoolCategory.Projectile);
        DespawnAll(PoolCategory.Effect);
    }

    private bool TryGetOrCreatePool(GameObject prefab, PoolCategory category, out GameObjectPool pool)
    {
        if (pools.TryGetValue(prefab, out pool))
        {
            PoolCategory registeredCategory = poolCategories[prefab];
            if (registeredCategory == category)
                return true;

            Debug.LogError(
                $"[{nameof(StagePoolManager)}] Prefab '{prefab.name}' is already registered as " +
                $"{registeredCategory} and cannot also be used as {category}.",
                this);
            pool = null;
            return false;
        }

        Transform root = GetRoot(category);
        if (root == null)
        {
            Debug.LogWarning(
                $"[{nameof(StagePoolManager)}] Root for {category} is not assigned. Using StagePoolManager as fallback.",
                this);
            root = transform;
        }

        pool = new GameObjectPool(prefab, root);
        pools.Add(prefab, pool);
        poolCategories.Add(prefab, category);

        if (!poolsByCategory.TryGetValue(category, out HashSet<GameObjectPool> categoryPools))
        {
            categoryPools = new HashSet<GameObjectPool>();
            poolsByCategory.Add(category, categoryPools);
        }

        categoryPools.Add(pool);

        return true;
    }

    private Transform GetRoot(PoolCategory category)
    {
        return category switch
        {
            PoolCategory.Unit => transform,
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
