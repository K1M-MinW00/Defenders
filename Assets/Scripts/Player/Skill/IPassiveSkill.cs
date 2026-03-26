public interface IPassiveSkill
{
    void Initialize(UnitController owner);
    void OnBattleStart();
    void OnAttackHit(IDamageable target);
    void OnTakeDamage(float damage);
    void OnHpChanged(float currentHp, float maxHp);
    void OnActiveSkillCast();
    void OnKillEnemy(MonsterController enemy);
    void Dispose();
}