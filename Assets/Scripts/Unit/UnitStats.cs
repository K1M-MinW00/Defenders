using UnityEngine;

[System.Serializable]
public struct UnitStats
{
    public float Attack;
    public float MaxHp;
    public float AttackPerSec;
    public float CritChance;
    public float EnergyRecovery;
    public float DetectRange;

    public UnitStats(float attack, float maxHp, float attackPerSec , float detectRange, float critChance = 0f, float energyRecovery = 0f)
    {
        Attack = attack;
        MaxHp = maxHp;
        AttackPerSec = attackPerSec;
        CritChance = critChance;
        EnergyRecovery = energyRecovery;
        DetectRange = detectRange;
    }
}