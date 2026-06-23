using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Gacha")]
public class GachaDataSO : ScriptableObject
{
    [Header("Banner")]
    public RecruitType recruitType;
    public string bannerName;
    public Sprite bannerImage;

    [Header("Pickup Unit")]
    public UnitDataSO pickupUnit;

    [Header("Rates")]
    [Range(0, 100)]
    public float normalRate = 85f;

    [Range(0, 100)]
    public float rareRate = 13f;

    [Range(0, 100)]
    public float legendRate = 2f;

    [Header("Pity")]
    public int legendPityCount = 50;

    [Header("Pools")]
    public List<UnitDataSO> normalPool = new();
    public List<UnitDataSO> rarePool = new();
    public List<UnitDataSO> legendPool = new();

    [Header("Cost")]
    public string ticketItemId;
    public int gemCost = 300;
}