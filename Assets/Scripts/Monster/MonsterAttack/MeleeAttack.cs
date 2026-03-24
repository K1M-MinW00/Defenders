using UnityEngine;

public class MeleeAttack : MonoBehaviour, IMonsterAttack
{
    protected MonsterController owner;
    
    protected virtual void Awake()
    {
        if(owner == null)
            owner = GetComponent<MonsterController>();
    }

    public bool CanAttack()
    {
        if (owner == null || owner.Health.IsDead)
            return false;

        if (!owner.IsTargetInAttackRange())
            return false;

        return true;
    }

    public bool TryAttack(UnitRuntime target)
    {
        if (target == null || target.IsDead)
            return false;

        if (!CanAttack())
            return false;

        owner.PlayAttack();

        target.TakeDamage(owner.AtkDamage);

        return true;
    }
}
