public interface IUnitAttack
{
    bool IsAttacking { get; }
    bool CanAttack();

    bool TryAttack(MonsterController target);
    abstract void OnAttackHit();
    void OnAttackFinished();
    void CancelAttack();
}
