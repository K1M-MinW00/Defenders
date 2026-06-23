public interface IMonsterAttack
{
    bool CanAttack();
    bool TryAttack(UnitController unit);
}
