using System.Collections.Generic;
using UnityEngine;

public class GachaService
{
    private UserGachaData GachaData => UserDataManager.Instance.UserData.Gacha;

    public int GetCurrentPity(RecruitType recruitType)
    {
        return recruitType switch
        {
            RecruitType.Normal => GachaData.NormalPity,
            RecruitType.Special => GachaData.SpecialPity,
            _ => 0
        };
    }

    public int GetRemainPity(GachaDataSO banner)
    {
        int currentPity = GetCurrentPity(banner.recruitType);

        return Mathf.Max(0, banner.legendPityCount - currentPity);
    }

    public List<GachaResult> Draw(GachaDataSO banner, int count)
    {
        List<GachaResult> results = new();

        for (int i = 0; i < count; i++)
        {
            results.Add(Draw(banner));
        }

        return results;
    }

    public GachaResult Draw(GachaDataSO banner)
    {
        bool isPity = IsLegendGuaranteed(banner);

        Rarity rarity = isPity ? Rarity.Legend : RollRarity(banner);

        UnitDataSO unit = RollUnit(banner, rarity);

        UpdatePity(banner.recruitType, rarity);

        return new GachaResult
        {
            Unit = unit,
            IsLegend = rarity == Rarity.Legend,
        };
    }

    private bool IsLegendGuaranteed(GachaDataSO banner)
    {
        int pity = GetCurrentPity(banner.recruitType);

        return pity >= banner.legendPityCount - 1;
    }

    private void UpdatePity(RecruitType recruitType, Rarity rarity)
    {
        if (rarity == Rarity.Legend)
        {
            SetPity(recruitType, 0);
        }
        else
        {
            int current = GetCurrentPity(recruitType);

            SetPity(recruitType, current + 1);
        }

        UserDataManager.Instance.MarkDirty();
    }

    private void SetPity(RecruitType recruitType, int value)
    {
        switch (recruitType)
        {
            case RecruitType.Normal:
                GachaData.NormalPity = value;
                break;

            case RecruitType.Special:
                GachaData.SpecialPity = value;
                break;
        }
    }

    private Rarity RollRarity(GachaDataSO banner)
    {
        float roll = Random.Range(0f, 100f);

        if (roll < banner.legendRate)
        {
            return Rarity.Legend;
        }

        roll -= banner.legendRate;

        if (roll < banner.rareRate)
        {
            return Rarity.Rare;
        }

        return Rarity.Normal;
    }

    private UnitDataSO RollUnit(GachaDataSO banner, Rarity rarity)
    {
        List<UnitDataSO> pool = GetPool(banner, rarity);

        if (pool == null || pool.Count == 0)
        {
            Debug.LogError($"Empty Pool : {rarity}");

            return null;
        }

        int index = Random.Range(0, pool.Count);

        return pool[index];
    }

    private List<UnitDataSO> GetPool(GachaDataSO banner, Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Normal:
                return banner.normalPool;

            case Rarity.Rare:
                return banner.rarePool;

            case Rarity.Legend:
                return banner.legendPool;

            default: return null;
        }
    }
}