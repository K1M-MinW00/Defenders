using UnityEngine;

public class PooledVfx : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifeTime = 0.4f;

    private Poolable poolable;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
    }

    public void Play(float duration = -1f)
    {
        CancelInvoke(nameof(Return));

        float finalDuration = duration > 0f ? duration : lifeTime;
        Invoke(nameof(Return), finalDuration);
    }

    private void Return()
    {
        poolable.ReturnToPool();
    }

    public void OnSpawn()
    {
        CancelInvoke(nameof(Return));
    }

    public void OnDespawn()
    {
        CancelInvoke(nameof(Return));
    }
}