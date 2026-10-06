using System.Collections.Generic;

public static class LabBonusProvider
{
    public static float GetTotal(LabEffectType effectType)
    {
        UserDataRoot data = UserDataManager.Instance?.UserData;
        LabConfigSO config = GameConfig.Lab;
        if (data?.Lab?.AcquiredCardIds == null || config?.Cards == null)
            return 0f;

        HashSet<string> owned = new(data.Lab.AcquiredCardIds);
        float total = 0f;
        foreach (LabCardDataSO card in config.Cards)
            if (card != null && card.EffectType == effectType && owned.Contains(card.CardId))
                total += card.Value;
        return total;
    }

    public static float ApplyPercent(float value, LabEffectType type) => value * (1f + GetTotal(type) / 100f);
}
