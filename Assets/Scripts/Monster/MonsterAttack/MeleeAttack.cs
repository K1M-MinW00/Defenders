using UnityEngine;

public class MeleeAttack : MonoBehaviour, IMonsterAttack
{
    public void Execute(MonsterController ctx)
    {
        if (ctx == null)
            return;


        if (!ctx.IsTargetInAttackRange())
            return;

        ctx.PlayAttack();
        ctx.Target.TakeDamage(ctx.AtkDamage);
    }
}
