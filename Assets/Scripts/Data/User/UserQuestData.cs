using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public sealed class UserQuestData
{
    [FirestoreProperty] public string DailyPeriodKey { get; set; }
    [FirestoreProperty] public string WeeklyPeriodKey { get; set; }
    [FirestoreProperty] public int DailyScore { get; set; }
    [FirestoreProperty] public int WeeklyScore { get; set; }
    [FirestoreProperty] public List<UserQuestProgressData> Progress { get; set; } = new();
    [FirestoreProperty] public List<int> ClaimedDailyRewards { get; set; } = new();
    [FirestoreProperty] public List<int> ClaimedWeeklyRewards { get; set; } = new();
}

[FirestoreData]
public sealed class UserQuestProgressData
{
    [FirestoreProperty] public string QuestId { get; set; }
    [FirestoreProperty] public int Progress { get; set; }
    [FirestoreProperty] public bool ScoreGranted { get; set; }
}
