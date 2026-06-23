using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private StagePoolManager poolManager;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private MonsterWaveHpTracker waveHpTracker;
    [SerializeField] private DamageUIService damageUIService;

    [SerializeField] private Transform[] spawnPoints;

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

    public void SetSpawnPoints(Transform[] spawnPoints)
    {
        this.spawnPoints = spawnPoints;
    }

    public void StartWave(WaveData waveData)
    {
        if (waveData == null)
        {
            Debug.LogError("StartWave failed. WaveData is null.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("StartWave failed. SpawnPoints are not set.");
            return;
        }

        StopSpawning();

        currentWave = waveData;
        deadMonsterCount = 0;

        aliveMonsters.Clear();
        OnAliveCountChanged?.Invoke(RemainingCount);

        spawnRoutine = StartCoroutine(SpawnWaveRoutine(waveData));
    }

    private IEnumerator SpawnWaveRoutine(WaveData waveData)
    {
        foreach (var subWave in waveData.subWaves)
        {
            yield return StartCoroutine(SpawnSubWave(subWave));

            if (subWave.delayAfterSubWave > 0f)
                yield return new WaitForSeconds(subWave.delayAfterSubWave);
        }

        spawnRoutine = null;
        OnAllMonstersSpawned?.Invoke();
    }

    private IEnumerator SpawnSubWave(SubWaveData subWave)
    {
        if (subWave == null || subWave.spawnEntries == null)
            yield break;

        foreach (MonsterSpawnEntry entry in subWave.spawnEntries)
        {
            yield return SpawnEntryRoutine(entry);

            if (entry.delayAfterGroup > 0f)
                yield return new WaitForSeconds(entry.delayAfterGroup);
        }
    }

    private IEnumerator SpawnEntryRoutine(MonsterSpawnEntry entry)
    {
        if (entry == null)
            yield break;

        if (entry.data == null)
        {
            Debug.LogWarning("MonsterSpawnEntry skipped. MonsterDataSO is null.");
            yield break;
        }

        Transform spawnPoint = GetSpawnPoint(entry.spawnPointIndex);

        if (spawnPoint == null)
            yield break;

        int count = Mathf.Max(0, entry.count);
        float interval = Mathf.Max(0f, entry.interval);

        for (int i = 0; i < count; i++)
        {
            SpawnMonster(entry.data, spawnPoint.position);

            if (interval > 0f && i < count - 1)
                yield return new WaitForSeconds(interval);
        }
    }
    private Transform GetSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("SpawnPoints are empty.");
            return null;
        }

        if (index < 0 || index >= spawnPoints.Length)
        {
            Debug.LogError($"Invalid spawnPointIndex: {index}. SpawnPoints Length: {spawnPoints.Length}");
            return null;
        }

        Transform point = spawnPoints[index];

        if (point == null)
        {
            Debug.LogError($"SpawnPoint at index {index} is null.");
            return null;
        }

        return point;
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
            return;

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private MonsterController SpawnMonster(MonsterDataSO data, Vector3 spawnPos)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogError("Spawn Monster failed. MonsterDataSO or prefab is null.");
            return null;
        }

        MonsterController prefab = data.prefab.GetComponent<MonsterController>();
        
        if(prefab == null)
        {
            Debug.LogError($"Spawn Monster failed. MonsterController not found on prefab : {data.prefab.name}");
            return null;
        }

        MonsterController monster = poolManager.Spawn(prefab, spawnPos, Quaternion.identity, PoolCategory.Monster);

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
