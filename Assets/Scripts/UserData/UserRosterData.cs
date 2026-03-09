using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class UserRosterData
{
    [FirestoreProperty] public List<UserUnitData> OwnedUnits { get; set; } = new();
}