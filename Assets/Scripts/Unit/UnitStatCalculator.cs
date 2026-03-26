using System;
using UnityEngine;

public static class UnitStatCalculator
{
    public static UnitStats Calculate(UnitDataSO data, StageUnitRuntime runtime)
    {
        float attack = CalculateAttack(data, runtime);
        float maxHp = CalculateHp(data, runtime);
        float attackPerSec = CalculateAttackPerSec(data, runtime);
        float critChance = CalculateCritChance(data, runtime);
        float energyRecovery = CalculateEnergyRecovery(data, runtime);
        float detectRange = CalculateDetectRange(data, runtime);
        return new UnitStats(
            attack,
            maxHp,
            attackPerSec,
            detectRange,
            critChance,
            energyRecovery
        );
    }

    private static float CalculateDetectRange(UnitDataSO data, StageUnitRuntime runtime)
    {
        float value = data.BaseDetectRange;

        if (runtime.Star >= 2)
            value *= data.GetStarDetectRangeMultiplier(runtime.Star);

        return value;
    }

    private static float CalculateAttack(UnitDataSO data, StageUnitRuntime runtime)
    {
        float value = data.GetAttackByLevel(runtime.Level);

        if (runtime.Promotion >= 4)
            value *= 1.1f;

        value *= data.GetStarAttackMultiplier(runtime.Star);

        if (runtime.LimitBreak >= 4)
            value *= 1.05f;

        return value;
    }

    private static float CalculateHp(UnitDataSO data, StageUnitRuntime runtime)
    {
        float value = data.GetHpByLevel(runtime.Level);

        if (runtime.Promotion >= 4)
            value *= 1.1f;

        value *= data.GetStarHpMultiplier(runtime.Star);

        if (runtime.LimitBreak >= 2)
            value *= 1.05f;

        return value;
    }

    private static float CalculateAttackPerSec(UnitDataSO data, StageUnitRuntime runtime)
    {
        float value = data.BaseAttackPerSec;

        if (runtime.LimitBreak >= 3)
            value *= 1.05f;

        return value;
    }

    private static float CalculateCritChance(UnitDataSO data, StageUnitRuntime runtime)
    {
        float value = data.BaseCritChance;

        if (runtime.LimitBreak >= 1)
            value += 0.05f;

        return value;
    }

    private static float CalculateEnergyRecovery(UnitDataSO data, StageUnitRuntime runtime)
    {
        float value = data.BaseEnergyRecovery;

        if (runtime.LimitBreak >= 5)
            value *= 1.05f;

        return value;
    }
}