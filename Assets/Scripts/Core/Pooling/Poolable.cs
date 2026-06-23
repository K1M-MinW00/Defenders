using System;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    private GameObjectPool ownerPool;
    private bool isSpawned;

    public bool IsSpawned => isSpawned;

    public void SetOwner(GameObjectPool ownerPool)
    {
        this.ownerPool = ownerPool;
    }

    public void MarkSpawned()
    {
        isSpawned = true;

        if (TryGetComponent(out IPoolable poolable))
            poolable.OnSpawn();
    }

    public void MarkDespawned()
    {
        if (!isSpawned)
            return;

        isSpawned = false;

        if(TryGetComponent(out IPoolable poolable))
            poolable.OnDespawn();
    }

    public void ReturnToPool()
    {
        if(ownerPool == null)
        {
            Destroy(gameObject);
            return;
        }

        ownerPool.Despawn(this);
    }

}
