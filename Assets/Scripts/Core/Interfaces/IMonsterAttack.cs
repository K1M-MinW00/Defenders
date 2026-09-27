public interface IMonsterAttack
{
    bool CanAttack();
    bool TryAttack(ICombatTarget target);
}
