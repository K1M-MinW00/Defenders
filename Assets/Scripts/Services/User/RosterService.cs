using System.Collections.Generic;
using UnityEngine;

public class RosterService
{
    private UserRosterData Roster => UserDataManager.Instance.UserData.Roster;
    private const int MaxLimitBreak = 5;

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

        UserUnitData ownedUnit = Roster.GetOwnedUnit(unit.unitId);

        // 최초 획득
        if (ownedUnit == null)
        {
            Roster.OwnedUnits.Add(new UserUnitData { UnitId = unit.unitId, Level = 1 });
            UserDataManager.Instance.MarkDirty();

            return;
        }

        // 한계돌파 재료로 사용 가능
        if (ownedUnit.CanReceive)
        {
            ownedUnit.DuplicateCount++;
            UserDataManager.Instance.MarkDirty();

            return;
        }

        // 초과 중복 보상
        GiveDuplicateReward(unit);
    }

    /// <summary>
    /// 한계돌파 진행
    /// </summary>
    public bool TryLimitBreak(string unitId)
    {
        UserUnitData unit = Roster.GetOwnedUnit(unitId);

        if (unit == null)
            return false;

        if (unit.DuplicateCount <= 0)
            return false;

        if (unit.LimitBreak >= MaxLimitBreak)
            return false;

        unit.DuplicateCount--;
        unit.LimitBreak++;

        UserDataManager.Instance.MarkDirty();

        return true;
    }

    public bool TryPromotion(UnitDataSO unitData)
    {
        UserUnitData unit = Roster.GetOwnedUnit(unitData.unitId);

        if (unit == null)
            return false;

        if (unit.Promotion >= 4)
            return false;

        PromotionCost cost = unitData.promotionCost[unit.Promotion];

        InventoryService inventory = UserDataManager.Instance.InventoryService;

        if (inventory.GetItemCount(cost.MaterialId) < cost.Count)
            return false;

        inventory.RemoveStackItem(ItemCategory.Material, cost.MaterialId, cost.Count);

        unit.Promotion++;

        UserDataManager.Instance.MarkDirty();
        return true;
    }

    /// <summary>
    /// 유닛 보유 여부
    /// </summary>
    public bool HasUnit(string unitId)
    {
        return Roster.GetOwnedUnit(unitId) != null;
    }

    /// <summary>
    /// 유닛 데이터 조회
    /// </summary>
    public UserUnitData GetUnit(string unitId)
    {
        return Roster.GetOwnedUnit(unitId);
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