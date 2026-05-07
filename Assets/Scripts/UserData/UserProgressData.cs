using Firebase.Firestore;

[FirestoreData]
public class UserProgressData
{
    [FirestoreProperty] public int CurrentSector { get; set; } = 1;
    [FirestoreProperty] public int CurrentStage { get; set; } = 1;
    [FirestoreProperty] public int BestWaveCleared { get; set; } = 0;
}
