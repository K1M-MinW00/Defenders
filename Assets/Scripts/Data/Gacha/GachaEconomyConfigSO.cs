using UnityEngine;

[CreateAssetMenu(fileName = "GachaEconomyConfig", menuName = "Game/Gacha/Economy Config")]
public sealed class GachaEconomyConfigSO : ScriptableObject
{
    [Header("Duplicate Unit Gem Reward")]
    [SerializeField, Min(0)] private int normalDuplicateGem = 30;
    [SerializeField, Min(0)] private int rareDuplicateGem = 100;
    [SerializeField, Min(0)] private int legendDuplicateGem = 300;

    public int GetDuplicateGemReward(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => normalDuplicateGem,
            Rarity.Rare => rareDuplicateGem,
            Rarity.Legend => legendDuplicateGem,
            _ => 0,
        };
    }

    public bool TryValidate(out string error)
    {
        if (normalDuplicateGem < 0 || rareDuplicateGem < 0 || legendDuplicateGem < 0)
        {
            error = "Duplicate unit rewards cannot be negative.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
