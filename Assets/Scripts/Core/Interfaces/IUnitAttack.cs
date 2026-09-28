public interface IUnitAttack : ICombatAttack
{
    bool IsAttacking { get; }
    void OnAttackHit();
    void OnAttackFinished();
}
