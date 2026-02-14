using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Economy/GameModeEconomyConfig")]
public class EconomyConfig : ScriptableObject
{
    [Header("Init")]
    public int initialGold = 15;

    [Header("Wave Reward")]
    public int normalReward = 14;
    public int eliteReward = 28;
    public int bossReward = 42;

    [Header("Bonus")]
    public int bonusPer10 = 1;        // 10 ´ç 1
    public int bonusCap = 5;          // ÃÖ´ë 5

    [Header("Shop")]
    public int summonUnit = 5;
    public int[] sellUnit = { 3, 6, 12, 24 };
    public int reRollUnit = 2;

    public int GetWaveReward(WaveType type)
    {
        return type switch
        {
            WaveType.Normal => normalReward,
            WaveType.Elite => eliteReward,
            WaveType.Boss => bossReward,
            _ => 0
        };
    }

    public int CalculateBonus(int currentGoldBeforeReward)
    {
        if (currentGoldBeforeReward < 10) return 0;
        int bonus = (currentGoldBeforeReward / 10) * bonusPer10;
        return Mathf.Min(bonus, bonusCap);
    }

    public int CalculateSellUnit(int star)
    {
        if (star >= sellUnit.Length)
            return -1;

        return sellUnit[star];
    }
}