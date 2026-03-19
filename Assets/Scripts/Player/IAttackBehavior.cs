public interface IAttackBehavior
{
    bool IsAttacking { get; }
    bool CanAttack();

    bool TryAttack(MonsterController target);

    void CancelAttack();
}
