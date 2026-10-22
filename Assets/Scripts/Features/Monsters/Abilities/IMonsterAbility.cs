public interface IMonsterAbility
{
    bool CanStart(MonsterController owner);
    void Begin(MonsterController owner);
    void Tick(float deltaTime);
    bool IsComplete { get; }
    void End();
    void Cancel();
}
