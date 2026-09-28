public interface ICombatAttack
{
    bool CanAttack();
    bool TryAttack(ICombatTarget target);
    void CancelAttack();
}
