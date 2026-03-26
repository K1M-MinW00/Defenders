using UnityEngine;

public abstract class PassiveSkillBase : MonoBehaviour
{
    protected UnitController owner;

    public virtual void Initialize(UnitController owner)
    {
        this.owner = owner;
    }

    public virtual void OnBattleStart() { }
    public virtual void OnAttackHit(IDamageable target) { }
    public virtual void OnTakeDamage(float damage) { }
    public virtual void OnHpChanged(float currentHp, float maxHp) { }
    public virtual void OnActiveSkillCast() { }
    public virtual void OnKillEnemy(MonsterController enemy) { }
    public virtual void Cleanup() { }
}