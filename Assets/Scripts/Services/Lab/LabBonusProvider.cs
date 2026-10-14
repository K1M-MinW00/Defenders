using System.Collections.Generic;

public static class LabBonusProvider
{
    public static LabBonusSnapshot CreateSnapshot(UserDataRoot data, LabConfigSO config)
    {
        Dictionary<LabEffectType, float> totals = new();
        if (data?.Lab?.AcquiredCardIds == null || config?.Cards == null)
            return new LabBonusSnapshot(totals);

        HashSet<string> owned = new(data.Lab.AcquiredCardIds);
        foreach (LabCardDataSO card in config.Cards)
        {
            if (card == null || !owned.Contains(card.CardId))
                continue;

            totals.TryGetValue(card.EffectType, out float current);
            totals[card.EffectType] = current + card.Value;
        }

        return new LabBonusSnapshot(totals);
    }

    public static float GetTotal(UserDataRoot data, LabEffectType effectType)
    {
        return CreateSnapshot(data, GameConfig.Lab).GetTotal(effectType);
    }
}
