using UnityEngine;

public sealed class MonsterCombatController : MonoBehaviour
{
    private MonsterController owner;
    private ICombatAttack attackBehavior;

    public void Initialize(MonsterController owner)
    {
        this.owner = owner;
        attackBehavior = GetComponent<ICombatAttack>();
    }

    public bool TryAttack(UnitController target)
    {
        if (owner == null || target == null || owner.IsDead)
            return false;

        return attackBehavior?.TryAttack(target) ?? false;
    }

    public void CancelAttack()
    {
        attackBehavior?.CancelAttack();
    }

    public void ResetRuntime()
    {
        CancelAttack();
    }
}
