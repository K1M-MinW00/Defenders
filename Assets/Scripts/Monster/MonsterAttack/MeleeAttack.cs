using UnityEngine;

public class MeleeAttack : MonoBehaviour, IMonsterAttack
{
    public void Execute(MonsterController ctx)
    {
        if (ctx == null)
            return;

        var target = ctx.TargetUnit;

        if(target == null || !target.IsAlive)
                return;

        float range = ctx.AttackRange;
        float sqr = (target.transform.position - ctx.transform.position).sqrMagnitude;

        if (sqr > range * range)
            return;

        target.TakeDamage(ctx.AtkDamage);
    }
}
