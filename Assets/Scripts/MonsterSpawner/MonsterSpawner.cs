using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MonsterSpawner : MonoBehaviour
{
    public ObjectPool pool;
    public Transform spawnPoint;

    private readonly List<MonsterController> aliveMonsters = new();
    public IReadOnlyList<MonsterController> MonsterLists => aliveMonsters;
    public int AliveCount => aliveMonsters.Count;

    public event Action OnAllMonstersSpawned;
    public event Action<int> OnAliveCountChanged;

    private bool isSpawning;
    public bool IsSpawning => isSpawning;

    public void Init(ObjectPool pool)
    {
        this.pool = pool;
    }

    public void StartWave(WaveData waveData)
    {
        StopAllCoroutines();
        StartCoroutine(SpawnWaveRoutine(waveData));
    }

    private IEnumerator SpawnWaveRoutine(WaveData waveData)
    {
        isSpawning = true;
        aliveMonsters.Clear();
        OnAliveCountChanged?.Invoke(aliveMonsters.Count);

        foreach (var subWave in waveData.subWaves)
        {
            yield return StartCoroutine(SpawnSubWave(subWave));
            yield return new WaitForSeconds(subWave.delayAfter);
        }

        isSpawning = false;
        OnAllMonstersSpawned?.Invoke();
    }

    private IEnumerator SpawnSubWave(SubWaveData subWave)
    {
        foreach (var group in subWave.spawnGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                GameObject obj = pool.Spawn(group.monsterId, spawnPoint.position);

                MonsterController monster = obj.GetComponent<MonsterController>();
                monster.SetPoolKey(group.monsterId);
                monster.OnDead += HandleMonsterDead;

                monster.OnSpawn();

                aliveMonsters.Add(monster);
                OnAliveCountChanged?.Invoke(aliveMonsters.Count);

            }

            yield return new WaitForSeconds(subWave.delayAfter);
        }
    }

    private void HandleMonsterDead(MonsterController monster)
    {
        monster.OnDead -= HandleMonsterDead;
        aliveMonsters.Remove(monster);
        OnAliveCountChanged?.Invoke(aliveMonsters.Count);

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
