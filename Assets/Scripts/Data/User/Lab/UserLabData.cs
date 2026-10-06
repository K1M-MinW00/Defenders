using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public sealed class UserLabData
{
    [FirestoreProperty] public List<string> AcquiredCardIds { get; set; } = new();
}
