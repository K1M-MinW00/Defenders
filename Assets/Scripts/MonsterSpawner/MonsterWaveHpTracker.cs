using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWaveHpTracker : MonoBehaviour
{
    public event Action<float, float> OnWaveHpChanged;

    private float totalMaxHp;
    private float remainingHp;

    private readonly Dictionary<MonsterHealth, float> lastKnownHp = new();

    public float TotalMaxHp => totalMaxHp;
    public float RemainingHp => remainingHp;

    public void PrepareWave(WaveData waveData)
    {
        totalMaxHp = CalculateWaveTotalMaxHp(waveData);
        remainingHp = totalMaxHp;
        lastKnownHp.Clear();

        OnWaveHpChanged?.Invoke(remainingHp, totalMaxHp);
    }

    public void RegisterSpawnedMonster(MonsterController monster)
    {
        if (monster == null || monster.Health == null)
            return;

        if (lastKnownHp.ContainsKey(monster.Health))
            return;

        float currentHp = monster.Health.CurrentHp;
        lastKnownHp[monster.Health] = currentHp;

        monster.OnDead += HandleMonsterDead;
        monster.Health.OnHpChanged += HandleMonsterHpChanged;
    }

    public void UnregisterMonster(MonsterController monster)
    {
        if (monster == null)
            return;

        monster.OnDead -= HandleMonsterDead;
        monster.Health.OnHpChanged -= HandleMonsterHpChanged;
        lastKnownHp.Remove(monster.Health);
    }

    public float GetHpRatio()
    {
        if (totalMaxHp <= 0f)
            return 0f;

        return RemainingHp / totalMaxHp;
    }
    private void HandleMonsterHpChanged(MonsterHealth monster, float damage)
    {
        if (monster == null)
            return;

        if (!lastKnownHp.TryGetValue(monster, out float previousHp))
            return;

        remainingHp = Mathf.Max(0f, remainingHp - damage);
        lastKnownHp[monster] = previousHp - damage;

        OnWaveHpChanged?.Invoke(remainingHp, totalMaxHp);
    }

    private void HandleMonsterDead(MonsterController monster)
    {
        if (monster == null)
            return;

        monster.OnDead -= HandleMonsterDead;
        monster.Health.OnHpChanged -= HandleMonsterHpChanged;
        lastKnownHp.Remove(monster.Health);
    }

    private float CalculateWaveTotalMaxHp(WaveData waveData)
    {
        if (waveData == null)
            return 0f;

        float total = 0f;

        foreach (SubWaveData subWave in waveData.subWaves)
        {
            if (subWave == null)
                continue;

            foreach (MonsterGroup group in subWave.spawnGroups)
            {
                if (group == null || string.IsNullOrEmpty(group.data.monsterId))
                    continue;

                total += group.data.Stats.maxHp * group.count;
            }
        }

        return total;
    }
}