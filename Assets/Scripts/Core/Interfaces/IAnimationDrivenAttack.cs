public interface IAnimationDrivenAttack : ICombatAttack
{
    AttackPhase Phase { get; }
    void OnAttackHit();
    void OnAttackFinished();
}
