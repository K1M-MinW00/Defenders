public interface IPassiveSkill
{
    void Initialize(UnitController owner, UnitSkillController skillController);

    void OnBattleStart();
    void OnBattleEnd();

    void OnAttackStarted(MonsterController target);
    void OnAttackHit(MonsterController target, ref float damage);

    void OnBeforeTakeDamage(ref float damage);
    void OnAfterTakeDamage(float finalDamage);

    void OnActiveSkillStarted();
    void OnActiveSkillApplied();
    void OnActiveSkillEnded();
}