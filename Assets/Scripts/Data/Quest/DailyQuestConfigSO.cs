using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class QuestDefinition
{
    [SerializeField] private string id;
    [SerializeField] private QuestEventType eventType;
    [SerializeField] private string displayName;
    [SerializeField, Min(1)] private int target = 1;
    [SerializeField, Min(1)] private int score = 1;

    public string Id => id;
    public QuestEventType EventType => eventType;
    public string DisplayName => displayName;
    public int Target => target;
    public int Score => score;

    public QuestDefinition(string id, QuestEventType eventType, string displayName, int target, int score)
    {
        this.id = id;
        this.eventType = eventType;
        this.displayName = displayName;
        this.target = target;
        this.score = score;
    }
}

[CreateAssetMenu(fileName = "DailyQuestConfig", menuName = "Game/Quest/Daily Quest Config")]
public sealed class DailyQuestConfigSO : ScriptableObject
{
    [SerializeField] private List<QuestDefinition> quests = new();
    public IReadOnlyList<QuestDefinition> Quests => quests;

    public bool TryValidate(out string error)
    {
        if (quests == null || quests.Count == 0)
        {
            error = "At least one daily quest is required.";
            return false;
        }
        if (quests.Any(quest => quest == null || string.IsNullOrWhiteSpace(quest.Id) ||
            string.IsNullOrWhiteSpace(quest.DisplayName) || quest.Target <= 0 || quest.Score <= 0))
        {
            error = "A daily quest contains invalid values.";
            return false;
        }
        if (quests.Select(quest => quest.Id).Distinct(StringComparer.Ordinal).Count() != quests.Count)
        {
            error = "Daily quest IDs must be unique.";
            return false;
        }
        error = string.Empty;
        return true;
    }

#if UNITY_EDITOR
    public void SetQuests(List<QuestDefinition> values) => quests = values ?? new();
#endif
}
