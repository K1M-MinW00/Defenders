using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BossSummonSettings
{
    public bool enabled;
    public List<MonsterDataSO> candidates = new();
    [Min(1)] public int count = 2;
    [Min(1)] public int maxAliveSummons = 4;
    [Min(0f)] public float initialDelay = 8f;
    [Min(0.01f)] public float cooldown = 15f;
    [Min(0.5f)] public float spawnRadius = 2.5f;
    [Min(0.01f)] public float hpMultiplier = 0.75f;
    [Min(0.01f)] public float attackMultiplier = 0.75f;

    public bool TryValidate(out string error)
    {
        if (!enabled)
        {
            error = string.Empty;
            return true;
        }

        if (candidates == null || candidates.Count == 0 || candidates.Exists(candidate => candidate == null))
            return Fail("Enabled boss summon requires valid monster candidates.", out error);
        if (candidates.Exists(candidate => candidate.Classification == MonsterClassification.Boss))
            return Fail("Boss summon candidates cannot be bosses.", out error);
        if (count <= 0 || maxAliveSummons <= 0 || count > maxAliveSummons)
            return Fail("Boss summon count must be positive and not exceed the alive limit.", out error);
        if (initialDelay < 0f || cooldown <= 0f || spawnRadius < 0.5f)
            return Fail("Boss summon timing and radius are invalid.", out error);
        if (hpMultiplier <= 0f || attackMultiplier <= 0f)
            return Fail("Boss summon stat multipliers must be positive.", out error);

        error = string.Empty;
        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
