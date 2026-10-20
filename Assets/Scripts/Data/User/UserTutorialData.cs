using Firebase.Firestore;

[FirestoreData]
public sealed class UserTutorialData
{
    public const string FirstBattleSequenceId = "first_battle";
    public const string InitialStepId = FirstBattleTutorialSequence.InitialStep;

    [FirestoreProperty] public int Version { get; set; } = FirstBattleTutorialSequence.Version;
    [FirestoreProperty] public string SequenceId { get; set; } = FirstBattleSequenceId;
    [FirestoreProperty] public string NextStepId { get; set; } = InitialStepId;
    [FirestoreProperty] public bool Completed { get; set; }
    [FirestoreProperty] public bool Skipped { get; set; }
    [FirestoreProperty] public bool LobbyUnitGuideCompleted { get; set; }
    [FirestoreProperty] public bool LaboratoryUnlockGuideCompleted { get; set; }

    public bool ShouldRun => !Completed && !Skipped;
}
