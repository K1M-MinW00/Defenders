using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MonsterSpawner : MonoBehaviour
{
    public MonsterSpawner Instance;

    // public MonsterPool monsterPool;
    public ObjectPool pool;

    public Transform spawnPoint;

    private List<Monster> aliveMonsters = new();
    public List<Monster> MonsterLists => aliveMonsters;

    public event Action OnWaveCompleted;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(ObjectPool pool)
    {
        this.pool = pool;
    }

    public void StartWave(WaveData waveData)
    {
        StartCoroutine(SpawnWaveRoutine(waveData));
    }

    private IEnumerator SpawnWaveRoutine(WaveData waveData)
    {
        aliveMonsters.Clear();

        foreach (var subWave in waveData.subWaves)
        {
            yield return StartCoroutine(SpawnSubWave(subWave));
            yield return new WaitForSeconds(subWave.delayAfter);
        }
        Debug.Log("Wave finished");

        yield return new WaitUntil(() => aliveMonsters.Count == 0);

        OnWaveCompleted?.Invoke();
    }

    private IEnumerator SpawnSubWave(SubWaveData subWave)
    {
        foreach (var group in subWave.spawnGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                GameObject obj = pool.Spawn(group.monsterId, spawnPoint.position);

                Monster monster = obj.GetComponent<Monster>();
                monster.SetPool(pool, group.monsterId);
                monster.OnDead += HandleMonsterDead;

                aliveMonsters.Add(monster);
            }
            yield return new WaitForSeconds(subWave.delayAfter);
        }
        yield return null;
    }

    private void HandleMonsterDead(Monster monster)
    {
        monster.OnDead -= HandleMonsterDead;
        aliveMonsters.Remove(monster);

        pool.Despawn(monster.PoolKey, monster.gameObject);
    }
}
