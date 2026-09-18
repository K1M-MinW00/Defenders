using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Promotion Progression", menuName = "Game/Units/Promotion Progression")]
public sealed class PromotionProgressionSO : ScriptableObject
{
    [SerializeField] private List<PromotionStageData> stages = new();

    public IReadOnlyList<PromotionStageData> Stages => stages;

    public IEnumerable<PromotionStatBonus> GetUnlockedStatBonuses(int promotion)
    {
        if (stages == null)
            yield break;

        foreach (PromotionStageData stage in stages)
        {
            if (stage == null || stage.promotionLevel <= 0 || stage.promotionLevel > promotion ||
                stage.statBonuses == null)
            {
                continue;
            }

            foreach (PromotionStatBonus bonus in stage.statBonuses)
            {
                if (bonus != null)
                    yield return bonus;
            }
        }
    }
}

[Serializable]
public sealed class PromotionStageData
{
    [Min(1)] public int promotionLevel = 1;
    [TextArea] public string description;
    public List<PromotionStatBonus> statBonuses = new();
}

[Serializable]
public sealed class PromotionStatBonus
{
    public StatType statType;
    public float percentValue;
}

public static class PromotionProgressionDatabase
{
    private const string ResourcePath = "Database/PromotionProgression";
    private static PromotionProgressionSO progression;

    public static PromotionProgressionSO Get()
    {
        if (progression == null)
            progression = Resources.Load<PromotionProgressionSO>(ResourcePath);

        return progression;
    }
}
