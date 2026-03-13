using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class UserRosterData
{
    [FirestoreProperty] public List<UserUnitData> OwnedUnits { get; set; } = new();
    [FirestoreProperty] public List<int> BattleSquadUnitCodes { get; set; } = new();

    public List<UnitCode> GetBattleSquadUnitCodes()
    {
        List<UnitCode> result = new();

        foreach (int value in BattleSquadUnitCodes)
            result.Add((UnitCode)value);

        return result;
    }
}