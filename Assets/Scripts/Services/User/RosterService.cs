using System.Collections.Generic;
using UnityEngine;

public sealed class RosterService
{
    private readonly UserDataRoot userData;
    private UserRosterData Roster => userData.Roster;

    public RosterService(UserDataRoot userData)
    {
        this.userData = userData;
    }

    /// <summary>
    /// 유닛 보유 여부
    /// </summary>
    public bool HasUnit(string unitId)
    {
        return FindOwnedUnit(unitId) != null;
    }

    /// <summary>
    /// 유닛 데이터 조회
    /// </summary>
    public UserUnitData GetUnit(string unitId)
    {
        return FindOwnedUnit(unitId);
    }

    private UserUnitData FindOwnedUnit(string unitId)
    {
        if (string.IsNullOrEmpty(unitId) || Roster?.OwnedUnits == null)
            return null;

        return Roster.OwnedUnits.Find(x => x != null && x.UnitId == unitId);
    }

    /// <summary>
    /// 전체 보유 유닛
    /// </summary>
    public IReadOnlyList<UserUnitData> GetOwnedUnits()
    {
        return Roster.OwnedUnits;
    }

}
