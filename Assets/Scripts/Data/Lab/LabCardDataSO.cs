using UnityEngine;

[CreateAssetMenu(fileName = "LabCard", menuName = "Game/Lab/Card")]
public sealed class LabCardDataSO : ScriptableObject
{
    [Header("Identity")]
    public string CardId;
    public string DisplayName;
    [TextArea] public string Description;
    public LabCardRarity Rarity;
    public Sprite Icon;

    [Header("Effect")]
    public LabEffectType EffectType;
    public float Value;
    [Min(0f)] public float DrawWeight = 1f;

    public string GetFormattedValue()
    {
        return EffectType == LabEffectType.StartingMinerals || EffectType == LabEffectType.MaxUnitCount
            ? $"+{Value:0}"
            : $"+{Value:0.#}%";
    }

    public float GetEffectiveDrawWeight()
    {
        if (DrawWeight > 0f)
            return DrawWeight;

        return Rarity switch
        {
            LabCardRarity.Common => 70f,
            LabCardRarity.Rare => 25f,
            LabCardRarity.Legendary => 5f,
            _ => 1f,
        };
    }
}
