using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LabConfig", menuName = "Game/Lab/Config")]
public sealed class LabConfigSO : ScriptableObject
{
    [Min(0)] public int BaseDevelopmentCost = 100;
    [Min(1f)] public float CostGrowth = 1.5f;
    [Min(1)] public int ChoiceCount = 4;
    public List<LabCardDataSO> Cards = new();

    public int GetCost(int acquiredCount)
    {
        double cost = BaseDevelopmentCost * System.Math.Pow(CostGrowth, Mathf.Max(0, acquiredCount));
        return cost >= int.MaxValue ? int.MaxValue : Mathf.CeilToInt((float)cost);
    }

    public bool TryValidate(out string error)
    {
        if (BaseDevelopmentCost < 0 || CostGrowth < 1f || ChoiceCount < 1)
        {
            error = "Lab economy values are invalid.";
            return false;
        }

        HashSet<string> ids = new();
        foreach (LabCardDataSO card in Cards)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.CardId) || !ids.Add(card.CardId) || card.DrawWeight < 0f)
            {
                error = "Lab cards must be non-null and have unique IDs and non-negative weights.";
                return false;
            }
        }

        error = null;
        return true;
    }
}
