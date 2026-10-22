using UnityEngine;

public abstract class BossAbilityBehaviour : MonoBehaviour, IMonsterAbility
{
    [SerializeField, Min(0f)] private float initialDelay = 3f;
    [SerializeField, Min(0.01f)] private float cooldown = 8f;
    [SerializeField] private int priority;

    protected MonsterController Owner { get; private set; }
    protected float Elapsed { get; private set; }
    private float nextReadyTime;

    public int Priority => priority;
    public abstract bool IsComplete { get; }

    public virtual void Initialize(MonsterController owner)
    {
        Owner = owner;
        Elapsed = 0f;
        nextReadyTime = Time.time + initialDelay;
        ResetAbility();
    }

    protected void ConfigureTiming(float delay, float repeatCooldown)
    {
        initialDelay = Mathf.Max(0f, delay);
        cooldown = Mathf.Max(0.01f, repeatCooldown);
    }

    public virtual bool CanStart(MonsterController owner)
    {
        return isActiveAndEnabled && owner == Owner && Time.time >= nextReadyTime;
    }

    public void Begin(MonsterController owner)
    {
        Owner = owner;
        Elapsed = 0f;
        nextReadyTime = Time.time + cooldown;
        ResetAbility();
        OnBegin();
    }

    public void Tick(float deltaTime)
    {
        Elapsed += Mathf.Max(0f, deltaTime);
        OnTick(deltaTime);
    }

    public void Cancel()
    {
        OnCancel();
        ResetAbility();
    }

    public void End()
    {
        OnEnd();
        ResetAbility();
    }

    public virtual void ResetRuntime()
    {
        OnCancel();
        Owner = null;
        Elapsed = 0f;
        nextReadyTime = 0f;
        ResetAbility();
    }

    protected abstract void OnBegin();
    protected abstract void OnTick(float deltaTime);
    protected virtual void OnEnd() { }
    protected virtual void OnCancel() { }
    protected abstract void ResetAbility();
}
