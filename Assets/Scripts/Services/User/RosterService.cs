using System.Collections.Generic;
using UnityEngine;

public class RosterService
{
    private UserRosterData Roster => UserDataManager.Instance.UserData.Roster;

    /// <summary>
    /// 유닛 지급
    /// 최초 획득 : OwnedUnits 추가
    /// 중복 획득 : DuplicateCount 증가
    /// 초과 중복 : 보상 지급
    /// </summary>
    public void GiveUnit(UnitDataSO unit)
    {
        if (unit == null)
            return;

        UserUnitData ownedUnit = FindOwnedUnit(unit.unitId);

        // 최초 획득
        if (ownedUnit == null)
        {
            Roster.OwnedUnits.Add(new UserUnitData { UnitId = unit.unitId, Level = 1 });
            UserDataManager.Instance.MarkDirty();

            return;
        }

        // 한계돌파 재료로 사용 가능
        if (CanReceiveDuplicate(ownedUnit))
        {
            ownedUnit.DuplicateCount++;
            UserDataManager.Instance.MarkDirty();

            return;
        }

        // 초과 중복 보상
        GiveDuplicateReward(unit);
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

    public bool CanReceiveDuplicate(UserUnitData unit)
    {
        return unit != null &&
            unit.LimitBreak + unit.DuplicateCount < UnitLimitBreakUseCase.MaxLimitBreak;
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

    /// <summary>
    /// 초과 중복 보상
    /// </summary>
    private void GiveDuplicateReward(UnitDataSO unit)
    {
        int gemReward = GetDuplicateReward(unit.rarity);

        UserDataManager.Instance.ResourceService.AddGem(gemReward);

        Debug.Log($"Duplicate Unit Reward : {unit.displayName} +{gemReward} Gem");

        UserDataManager.Instance.MarkDirty();
    }

    private int GetDuplicateReward(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => 30,
            Rarity.Rare => 100,
            Rarity.Legend => 300,
            _ => 0
        };
    }
}
