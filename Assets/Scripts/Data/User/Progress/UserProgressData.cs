using Firebase.Firestore;

[FirestoreData]
public class UserProgressData
{
    [FirestoreProperty] public int CurrentSector { get; set; } = 1;
    [FirestoreProperty] public int CurrentStage { get; set; } = 1;
    // 현재 스테이지에서 완료한 최고 웨이브 수다. 다음 도전의 시작 웨이브로 사용하지 않는다.
    [FirestoreProperty] public int BestWaveCleared { get; set; } = 0;
}
