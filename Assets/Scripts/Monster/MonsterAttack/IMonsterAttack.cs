public interface IMonsterAttack
{
    bool CanAttack();
    bool TryAttack(UnitRuntime unit);
}
