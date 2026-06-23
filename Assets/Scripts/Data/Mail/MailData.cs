using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class MailData
{
    [FirestoreProperty] public string MailId { get; set; }
    [FirestoreProperty] public string Title { get; set; }
    [FirestoreProperty] public string Description { get; set; }
    [FirestoreProperty] public Timestamp CreatedAt { get; set; }
    [FirestoreProperty] public Timestamp ExpireAt { get; set; }
    [FirestoreProperty] public bool Claimed { get; set; }
    [FirestoreProperty] public MailType MailType { get; set; }
    [FirestoreProperty] public List<RewardData> Rewards { get; set; }
}
