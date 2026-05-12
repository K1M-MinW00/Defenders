using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWaveHpTracker : MonoBehaviour
{
    public event Action<float, float> OnWaveHpChanged;

    private float totalCurrentHp;
    private float totalMaxHp;

    private readonly Dictionary<MonsterHealth, float> lastKnownHp = new();

    public float CurrentHp => totalCurrentHp;
    public float MaxHp => totalMaxHp;

    public void PrepareWave(WaveData waveData)
    {
        totalMaxHp = CalculateWaveTotalMaxHp(waveData);
        totalCurrentHp = totalMaxHp;
        lastKnownHp.Clear();

        OnWaveHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
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

        return totalCurrentHp / totalMaxHp;
    }
    private void HandleMonsterHpChanged(MonsterHealth monster, float damage)
    {
        if (monster == null)
            return;

        if (!lastKnownHp.TryGetValue(monster, out float previousHp))
            return;

        totalCurrentHp = Mathf.Max(0f, totalCurrentHp - damage);
        lastKnownHp[monster] = previousHp - damage;

        OnWaveHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
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

            foreach (MonsterSpawnEntry entry in subWave.spawnEntries)
            {
                if (entry == null || entry.data == null)
                    continue;

                total += entry.data.Stats.maxHp * entry.count;
            }
        }

        return total;
    }
}