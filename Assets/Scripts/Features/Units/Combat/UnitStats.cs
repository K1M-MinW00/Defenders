[System.Serializable]
public struct UnitStats
{
    public float Attack;
    public float MaxHp;
    
    public float AttackPerSec;
    public float DetectRange;

    public float CritChance;
    public float CritDamage;

    public float EnergyRecovery;
    public float DamageReduction;

    public UnitStats(float attack, float maxHp, float attackPerSec, float detectRange, float critChance, float critDamage, float energyRecovery, float damageReduction = 0f)
    {
        Attack = attack;
        MaxHp = maxHp;
        AttackPerSec = attackPerSec;
        DetectRange = detectRange;
        CritChance = critChance;
        CritDamage = critDamage;
        EnergyRecovery = energyRecovery;
        DamageReduction = damageReduction;
    }
}
