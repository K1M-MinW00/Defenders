using System;
using System.Collections.Generic;
using System.Linq;

public static class QuestProgressService
{
    public static bool RefreshPeriods(UserQuestData data, DateTimeOffset utcNow)
    {
        if (data == null) return false;
        bool changed = false;
        string daily = QuestPeriodPolicy.GetDailyKey(utcNow);
        string weekly = QuestPeriodPolicy.GetWeeklyKey(utcNow);

        if (data.WeeklyPeriodKey != weekly)
        {
            data.WeeklyPeriodKey = weekly;
            data.WeeklyScore = 0;
            data.ClaimedWeeklyRewards.Clear();
            changed = true;
        }

        if (data.DailyPeriodKey != daily)
        {
            data.DailyPeriodKey = daily;
            data.DailyScore = 0;
            data.Progress.Clear();
            data.ClaimedDailyRewards.Clear();
            changed = true;
        }

        return changed;
    }

    public static bool Apply(UserQuestData data, QuestEvent questEvent, DateTimeOffset utcNow)
    {
        if (data == null || questEvent.Amount <= 0) return false;
        bool changed = RefreshPeriods(data, utcNow);

        foreach (QuestDefinition definition in DailyQuestCatalog.All.Where(q => q.EventType == questEvent.Type))
        {
            UserQuestProgressData progress = data.Progress.FirstOrDefault(p => p.QuestId == definition.Id);
            if (progress == null)
            {
                progress = new UserQuestProgressData { QuestId = definition.Id };
                data.Progress.Add(progress);
            }

            if (progress.Progress < definition.Target)
            {
                progress.Progress = Math.Min(definition.Target, checked(progress.Progress + questEvent.Amount));
                changed = true;
            }

        }

        return changed;
    }

    public static int GetProgress(UserQuestData data, string questId) =>
        data?.Progress?.FirstOrDefault(p => p.QuestId == questId)?.Progress ?? 0;

    public static bool IsScoreClaimed(UserQuestData data, string questId) =>
        data?.Progress?.FirstOrDefault(p => p.QuestId == questId)?.ScoreGranted == true;

    public static QuestScoreClaimFailure ClaimScore(UserQuestData data, string questId, DateTimeOffset utcNow)
    {
        QuestDefinition definition = DailyQuestCatalog.All.FindById(questId);
        if (data == null || definition == null)
            return QuestScoreClaimFailure.InvalidQuest;

        RefreshPeriods(data, utcNow);
        UserQuestProgressData progress = data.Progress.FirstOrDefault(item => item.QuestId == questId);
        if (progress == null || progress.Progress < definition.Target)
            return QuestScoreClaimFailure.NotCompleted;
        if (progress.ScoreGranted)
            return QuestScoreClaimFailure.AlreadyClaimed;

        progress.ScoreGranted = true;
        data.DailyScore = checked(data.DailyScore + definition.Score);
        data.WeeklyScore = checked(data.WeeklyScore + definition.Score);
        return QuestScoreClaimFailure.None;
    }

    public static bool Revert(UserQuestData data, QuestEvent questEvent, DateTimeOffset utcNow)
    {
        if (data == null || questEvent.Amount <= 0) return false;
        bool changed = RefreshPeriods(data, utcNow);
        foreach (QuestDefinition definition in DailyQuestCatalog.All.Where(q => q.EventType == questEvent.Type))
        {
            UserQuestProgressData progress = data.Progress.FirstOrDefault(p => p.QuestId == definition.Id);
            if (progress == null || progress.Progress <= 0) continue;
            progress.Progress = Math.Max(0, progress.Progress - questEvent.Amount);
            if (progress.ScoreGranted && progress.Progress < definition.Target)
            {
                progress.ScoreGranted = false;
                data.DailyScore = Math.Max(0, data.DailyScore - definition.Score);
                data.WeeklyScore = Math.Max(0, data.WeeklyScore - definition.Score);
            }
            changed = true;
        }
        return changed;
    }
}
