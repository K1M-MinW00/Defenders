using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private StagePoolManager poolManager;
    private UnitRoster unitRoster;
    private MonsterWaveHpTracker waveHpTracker;
    private DamageUIService damageUIService;

    private WaveData currentWave;
    private readonly List<MonsterController> aliveMonsters = new();
    private int deadMonsterCount;
    private Coroutine spawnRoutine;

    public MonsterWaveHpTracker WaveHpTracker => waveHpTracker;
    public int AliveCount => aliveMonsters.Count;

    private int plannedMonsterCount => currentWave != null ? currentWave.TotalMonsterCount : 0;
    public int RemainingCount => Mathf.Max(0, plannedMonsterCount - deadMonsterCount);

    public event Action OnAllMonstersSpawned;
    public event Action<int> OnAliveCountChanged;

    public void Init(StagePoolManager poolManager, UnitRoster unitRoster, MonsterWaveHpTracker waveHpTracker, DamageUIService damageUIService)
    {
        this.poolManager = poolManager;
        this.unitRoster = unitRoster;
        this.waveHpTracker = waveHpTracker; 
        this.damageUIService = damageUIService;
    }

    public void StartWave(WaveData waveData)
    {
        currentWave = waveData;
        StopAllCoroutines();

        deadMonsterCount = 0;
        spawnRoutine = StartCoroutine(SpawnWaveRoutine(waveData));
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
    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private MonsterController SpawnMonster(MonsterDataSO data, Vector3 spawnPos)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogError("Spawn Monster failed. MonsterDataSO or prefab is null.");
            return null;
        }

        MonsterController monster = poolManager.Spawn(data.prefab.GetComponent<MonsterController>(), spawnPos, Quaternion.identity, PoolCategory.Monster);

        if (monster == null)
        {
            Debug.LogError("Spawn Monster failed. MonsterController Not found.");
            return null;
        }

        monster.Initialize(unitRoster, data, poolManager);
        monster.Health.OnHpChanged += HandleMonsterDamaged;
        monster.OnDead += HandleMonsterDead;

        waveHpTracker?.RegisterSpawnedMonster(monster);
        aliveMonsters.Add(monster);

        return monster;
    }

    private void HandleMonsterDead(MonsterController monster)
    {
        monster.Health.OnHpChanged -= HandleMonsterDamaged;
        monster.OnDead -= HandleMonsterDead;

        waveHpTracker?.UnregisterMonster(monster);
        aliveMonsters.Remove(monster);

        deadMonsterCount++;

        OnAliveCountChanged?.Invoke(RemainingCount);
    }

    private void HandleMonsterDamaged(MonsterHealth health, float damage)
    {
        Vector3 worldPos = health.transform.position;
        damageUIService.Show(worldPos, damage);
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
