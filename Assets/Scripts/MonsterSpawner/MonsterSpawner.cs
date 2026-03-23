using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MonsterSpawner : MonoBehaviour
{
    private ObjectPool pool;
    private UnitRoster unitRoster;
    [SerializeField] private MonsterWaveHpTracker waveHpTracker;
    public MonsterWaveHpTracker WaveHpTracker => waveHpTracker;
    [SerializeField] private Transform spawnPoint;

    private readonly List<MonsterController> aliveMonsters = new();
    public IReadOnlyList<MonsterController> MonsterLists => aliveMonsters;
    public int AliveCount => aliveMonsters.Count;
    
    private WaveData currentWave;
    private int plannedMonsterCount => currentWave != null ? currentWave.TotalMonsterCount : 0;
    private int deadMonsterCount;
    public int RemainingCount => Mathf.Max(0, plannedMonsterCount - deadMonsterCount);
    
    public event Action OnAllMonstersSpawned;
    public event Action<int> OnAliveCountChanged;


    public void Init(ObjectPool pool, UnitRoster unitRoster)
    {
        this.pool = pool;
        this.unitRoster = unitRoster;
    }

    public void StartWave(WaveData waveData)
    {
        currentWave  = waveData;
        StopAllCoroutines();

        deadMonsterCount = 0;
        StartCoroutine(SpawnWaveRoutine(waveData));
    }

    private IEnumerator SpawnWaveRoutine(WaveData waveData)
    {
        aliveMonsters.Clear();
        OnAliveCountChanged?.Invoke(RemainingCount);

        foreach (var subWave in waveData.subWaves)
        {
            yield return StartCoroutine(SpawnSubWave(subWave));
            yield return new WaitForSeconds(subWave.delayAfter);
        }

        OnAllMonstersSpawned?.Invoke();
    }

    private IEnumerator SpawnSubWave(SubWaveData subWave)
    {
        foreach (var group in subWave.spawnGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnMonster(group.data, spawnPoint.position);
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(subWave.delayAfter);
        }
    }


    private MonsterController SpawnMonster(MonsterDataSO data , Vector3 spawnPos)
    {
        string monsterId = data.monsterId;
        GameObject go = pool.Spawn(monsterId, spawnPos);
        MonsterController monster = go.GetComponent<MonsterController>();

        monster.Initialize(unitRoster, data);
        monster.SetPoolKey(monsterId);
        monster.OnDead += HandleMonsterDead;

        waveHpTracker?.RegisterSpawnedMonster(monster);
        aliveMonsters.Add(monster);
        return monster;
    }

    private void HandleMonsterDead(MonsterController monster)
    {
        monster.OnDead -= HandleMonsterDead;
        waveHpTracker?.UnregisterMonster(monster);
        aliveMonsters.Remove(monster);

        deadMonsterCount++;

        OnAliveCountChanged?.Invoke(RemainingCount);
        pool.Despawn(monster.PoolKey, monster.gameObject);
    }

    public MonsterController FindClosestAlive(Vector3 from)
    {
        MonsterController best = null;
        float bestD = float.PositiveInfinity;

        for (int i = aliveMonsters.Count - 1; i >= 0; i--)
        {
            MonsterController m = aliveMonsters[i];

            if (m == null || m.Health.IsDead)
                continue;

            float d = (m.transform.position - from).sqrMagnitude;

            if (d < bestD)
            {
                bestD = d;
                best = m;
            }
        }

        if (best == null)
            return null;

        return best;
    }
}
