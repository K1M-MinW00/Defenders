public class MeleeMonsterAttack : MonsterAttackBase
{
    protected override void ApplyHit(ICombatTarget target)
    {
        target.CombatHealth.ApplyDamage(
            new DamageRequest(owner.AtkDamage, owner, DamageOrigin.BasicAttack));
    }
}
