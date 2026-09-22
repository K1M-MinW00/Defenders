using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserLevelProgression", menuName = "Configs/User Level Progression")]
public sealed class UserLevelProgressionSO : ScriptableObject
{
    [Tooltip("Index 0 is the experience required to advance from level 1.")]
    [SerializeField] private List<int> requiredExpByLevel = new() { 100 };

    public int GetRequiredExp(int level)
    {
        if (level <= 0 || requiredExpByLevel == null || requiredExpByLevel.Count == 0)
            return 0;

        int index = level - 1;
        if (index < requiredExpByLevel.Count)
            return Mathf.Max(0, requiredExpByLevel[index]);

        int last = Mathf.Max(0, requiredExpByLevel[^1]);
        int growth = requiredExpByLevel.Count >= 2
            ? Mathf.Max(0, last - requiredExpByLevel[^2])
            : 0;

        return last + growth * (index - requiredExpByLevel.Count + 1);
    }

    public float GetNormalizedExp(int exp, int level)
    {
        int requiredExp = GetRequiredExp(level);
        return requiredExp > 0 ? Mathf.Clamp01((float)Mathf.Max(0, exp) / requiredExp) : 0f;
    }

    public bool TryValidate(out string error)
    {
        if (requiredExpByLevel == null || requiredExpByLevel.Count == 0)
        {
            error = "At least one level experience requirement is needed.";
            return false;
        }

        for (int i = 0; i < requiredExpByLevel.Count; i++)
        {
            if (requiredExpByLevel[i] <= 0)
            {
                error = $"Required experience must be positive at index {i}.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}
