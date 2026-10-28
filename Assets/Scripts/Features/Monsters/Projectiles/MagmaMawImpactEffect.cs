using UnityEngine;

[RequireComponent(typeof(Poolable))]
public sealed class MagmaMawImpactEffect : MonoBehaviour, IPoolable
{
    [SerializeField, Min(0.01f)] private float duration = 0.7f;

    private Poolable poolable;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
    }

    public void OnSpawn()
    {
        CancelInvoke(nameof(Release));
        Invoke(nameof(Release), duration);
    }

    public void OnDespawn()
    {
        CancelInvoke(nameof(Release));
    }

    private void Release()
    {
        poolable?.ReturnToPool();
    }
}
