using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Gacha")]
public class GachaDataSO : ScriptableObject
{
    [Header("Banner")]
    [SerializeField] private RecruitType recruitType;
    [SerializeField] private string bannerName;
    [SerializeField] private Sprite bannerImage;

    [Header("Pickup Unit")]
    [SerializeField] private UnitDataSO pickupUnit;

    [Header("Rates")]
    [Range(0, 100)]
    [SerializeField] private float normalRate = 85f;

    [Range(0, 100)]
    [SerializeField] private float rareRate = 13f;

    [Range(0, 100)]
    [SerializeField] private float legendRate = 2f;

    [Header("Pity")]
    [SerializeField] private int legendPityCount = 50;

    [Header("Pools")]
    [SerializeField] private List<UnitDataSO> normalPool = new();
    [SerializeField] private List<UnitDataSO> rarePool = new();
    [SerializeField] private List<UnitDataSO> legendPool = new();

    [Header("Cost")]
    [SerializeField] private string ticketItemId;
    [SerializeField] private int gemCost = 300;

    public RecruitType RecruitType => recruitType;
    public string BannerName => bannerName;
    public Sprite BannerImage => bannerImage;
    public UnitDataSO PickupUnit => pickupUnit;
    public float NormalRate => normalRate;
    public float RareRate => rareRate;
    public float LegendRate => legendRate;
    public int LegendPityCount => legendPityCount;
    public IReadOnlyList<UnitDataSO> NormalPool => normalPool;
    public IReadOnlyList<UnitDataSO> RarePool => rarePool;
    public IReadOnlyList<UnitDataSO> LegendPool => legendPool;
    public string TicketItemId => ticketItemId;
    public int GemCost => gemCost;

    public IReadOnlyList<UnitDataSO> GetPool(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => normalPool,
            Rarity.Rare => rarePool,
            Rarity.Legend => legendPool,
            _ => null,
        };
    }

    public bool TryValidate(out string error)
    {
        if (string.IsNullOrWhiteSpace(ticketItemId))
            return Fail("Ticket item ID is empty.", out error);
        if (gemCost < 0)
            return Fail("Gem cost cannot be negative.", out error);
        if (legendPityCount <= 0)
            return Fail("Legend pity count must be positive.", out error);
        if (normalRate < 0f || rareRate < 0f || legendRate < 0f)
            return Fail("Recruit rates cannot be negative.", out error);
        if (Mathf.Abs(normalRate + rareRate + legendRate - 100f) > 0.01f)
            return Fail("Recruit rates must add up to 100%.", out error);

        if (!IsValidPool(legendPool, Rarity.Legend) ||
            (normalRate > 0f && !IsValidPool(normalPool, Rarity.Normal)) ||
            (rareRate > 0f && !IsValidPool(rarePool, Rarity.Rare)))
        {
            return Fail("A required pool is empty or contains an invalid unit rarity.", out error);
        }

        error = string.Empty;
        return true;
    }

    private static bool IsValidPool(IReadOnlyList<UnitDataSO> pool, Rarity expectedRarity)
    {
        if (pool == null || pool.Count == 0)
            return false;

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] == null || string.IsNullOrWhiteSpace(pool[i].unitId) || pool[i].rarity != expectedRarity)
                return false;
        }

        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
