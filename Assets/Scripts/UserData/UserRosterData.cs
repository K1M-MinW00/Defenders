using Firebase.Firestore;
using System.Collections.Generic;
using System.Linq;

[FirestoreData]
public class UserRosterData
{
    [FirestoreProperty] public List<UserUnitData> OwnedUnits { get; set; } = new();
    [FirestoreProperty] public List<string> SelectedUnitIds { get; set; } = new();
    [FirestoreProperty] public int Power { get; } = 0;

    public UserUnitData GetOwnedUnit(string unitId)
    {
        return OwnedUnits.FirstOrDefault(x => x.UnitId == unitId);
    }
}