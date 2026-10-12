using System;
using System.Collections.Generic;
using System.Linq;

public enum QuestEventType
{
    UnitReachedFourStar,
    UnitRecruited,
    UnitLevelUp,
    LabDeveloped,
    WaveCleared,
    UnitFused,
    FuelConsumed,
    Login,
}

public readonly struct QuestEvent
{
    public QuestEventType Type { get; }
    public int Amount { get; }

    public QuestEvent(QuestEventType type, int amount = 1)
    {
        Type = type;
        Amount = Math.Max(0, amount);
    }
}

public static class DailyQuestCatalog
{
    public static IReadOnlyList<QuestDefinition> All => GameConfig.DailyQuests.Quests;

    public static QuestDefinition FindById(this IReadOnlyList<QuestDefinition> definitions, string id) =>
        definitions?.FirstOrDefault(definition => definition.Id == id);
}
