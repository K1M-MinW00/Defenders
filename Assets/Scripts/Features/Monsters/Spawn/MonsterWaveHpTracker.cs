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

    public void PrepareWave(
        WaveData waveData,
        Func<MonsterSpawnEntry, float> maxHpResolver = null)
    {
        UnsubscribeAll();
        totalMaxHp = CalculateWaveTotalMaxHp(waveData, maxHpResolver);
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

    public void ClearWave(bool notify = true)
    {
        UnsubscribeAll();
        lastKnownHp.Clear();
        totalCurrentHp = 0f;
        totalMaxHp = 0f;

        if (notify)
            OnWaveHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
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

        float currentHp = Mathf.Clamp(monster.CurrentHp, 0f, monster.MaxHp);
        float actualHpLoss = Mathf.Max(0f, previousHp - currentHp);
        totalCurrentHp = Mathf.Clamp(totalCurrentHp - actualHpLoss, 0f, totalMaxHp);
        lastKnownHp[monster] = currentHp;

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

    private float CalculateWaveTotalMaxHp(
        WaveData waveData,
        Func<MonsterSpawnEntry, float> maxHpResolver)
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

                float maxHp = maxHpResolver != null
                    ? maxHpResolver(entry)
                    : entry.data.BaseMaxHp;
                total += Mathf.Max(0f, maxHp) * Mathf.Max(0, entry.count);
            }
        }

        return total;
    }

    private void UnsubscribeAll()
    {
        foreach (MonsterHealth health in lastKnownHp.Keys)
        {
            if (health == null)
                continue;

            MonsterController monster = health.GetComponent<MonsterController>();
            if (monster != null)
                monster.OnDead -= HandleMonsterDead;

            health.OnHpChanged -= HandleMonsterHpChanged;
        }
    }

    private void OnDisable()
    {
        ClearWave(notify: false);
    }
}
