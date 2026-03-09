using Firebase.Firestore;

[FirestoreData]
public class UserProgressData
{
    [FirestoreProperty] public int SectorNum { get; set; } = 1;
    [FirestoreProperty] public int StageNum { get; set; } = 1;
}
