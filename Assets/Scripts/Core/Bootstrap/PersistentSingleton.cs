using UnityEngine;

public abstract class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        T current = (T)this;
        if (Instance != null && Instance != current)
        {
            Destroy(gameObject);
            return;
        }

        Instance = current;
        DontDestroyOnLoad(gameObject);
        OnSingletonAwake();
    }

    protected virtual void OnDestroy()
    {
        if (Instance != (T)this)
            return;

        OnSingletonDestroyed();
        Instance = null;
    }

    protected virtual void OnSingletonAwake()
    {
    }

    protected virtual void OnSingletonDestroyed()
    {
    }
}
