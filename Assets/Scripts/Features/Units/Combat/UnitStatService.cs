using UnityEngine;

public class UnitStatService : MonoBehaviour
{
    private UnitController owner;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
    }

    public void BuildInitialStats(StageUnitInitData initData)
    {
        UnitDataSO data = initData.UnitData;
        int level = initData.UserData.Level;

        UnitStats origin = data.GetOriginStats(level);
        owner.Runtime.SetOriginStats(origin);

        Recalculate(StatRefreshPolicy.FullHeal);
    }

    public void Recalculate(StatRefreshPolicy statRefreshPolicy)
    {
        UnitStats stageBaseStats = owner.UnitData.ApplyStar(owner.Runtime.OriginStats, owner.Runtime.Star);
        UnitStats finalStats = CalculateFinalStats(stageBaseStats);

        owner.Runtime.SetRuntimeBaseStats(stageBaseStats);
        owner.Runtime.SetFinalStats(finalStats);
        
        owner.Health.ApplyStatRefresh(finalStats.MaxHp);
        owner.Targeting.ApplyRange(owner.Runtime.FinalStats.DetectRange);
    }

    private UnitStats CalculateFinalStats(UnitStats stageBaseStats)
    {
        UnitStats result = stageBaseStats;

        result.Attack = CalculateBuffedValue(StatType.Attack, stageBaseStats.Attack);
        result.MaxHp = CalculateBuffedValue(StatType.MaxHp, stageBaseStats.MaxHp);
        result.AttackPerSec = CalculateBuffedValue(StatType.AttackPerSec, stageBaseStats.AttackPerSec);
        result.DetectRange = CalculateBuffedValue(StatType.DetectRange, stageBaseStats.DetectRange);

        return result;
    }

    private float CalculateBuffedValue(StatType statType, float baseValue)
    {
        float additive = owner.BuffController.GetAdditive(statType);
        float multiplier = owner.BuffController.GetMultiplier(statType);
        return (baseValue + additive) * multiplier;
    }
}

public enum StatRefreshPolicy
{
    FullHeal,
    KeepRatio,
}