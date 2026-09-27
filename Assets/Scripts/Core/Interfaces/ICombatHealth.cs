public interface ICombatHealth : IDamageable
{
    float CurrentHp { get; }
    float MaxHp { get; }
    bool IsDead { get; }

    DamageResult ApplyDamage(DamageRequest request);
}
