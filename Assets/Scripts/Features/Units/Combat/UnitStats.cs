using UnityEngine;

[System.Serializable]
public struct UnitStats
{
    public float Attack;
    public float MaxHp;
    public float AttackPerSec;
    public float DetectRange;

    public UnitStats(float attack, float maxHp, float attackPerSec , float detectRange)
    {
        Attack = attack;
        MaxHp = maxHp;
        AttackPerSec = attackPerSec;
        DetectRange = detectRange;
    }
}