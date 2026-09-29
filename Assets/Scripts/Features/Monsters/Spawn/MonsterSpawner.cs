using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MonsterSpawner : MonoBehaviour, ICombatTargetProvider
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
    private bool spawnFailed;

    public MonsterWaveHpTracker WaveHpTracker => waveHpTracker;
    public int AliveCount => aliveMonsters.Count;

    private int plannedMonsterCount => currentWave != null ? currentWave.TotalMonsterCount : 0;
    public int RemainingCount => Mathf.Max(0, plannedMonsterCount - deadMonsterCount);

    public event Action OnAllMonstersSpawned;
    public event Action OnSpawnFailed;
    public event Action<int> OnAliveCountChanged;

    public void SetSpawnPoints(Transform[] spawnPoints)
    {
        this.spawnPoints = spawnPoints;
    }

    public bool TryStartWave(WaveData waveData)
    {
        if (waveData == null)
        {
            Debug.LogError("StartWave failed. WaveData is null.");
            return false;
        }

        if (poolManager == null || unitRoster == null ||
            spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("StartWave failed. Spawner dependencies or spawn points are not set.");
            return false;
        }

        StopSpawning();

        currentWave = waveData;
        deadMonsterCount = 0;
        spawnFailed = false;

        aliveMonsters.Clear();
        OnAliveCountChanged?.Invoke(RemainingCount);

        spawnRoutine = StartCoroutine(SpawnWaveRoutine(waveData));
        return true;
    }

    private IEnumerator SpawnWaveRoutine(WaveData waveData)
    {
        foreach (var subWave in waveData.subWaves)
        {
            yield return StartCoroutine(SpawnSubWave(subWave));

            if (spawnFailed)
                yield break;

            if (subWave.delayAfterSubWave > 0f)
                yield return new WaitForSeconds(subWave.delayAfterSubWave);
        }

        spawnRoutine = null;
        OnAllMonstersSpawned?.Invoke();
    }

    private IEnumerator SpawnSubWave(SubWaveData subWave)
    {
        if (subWave == null || subWave.spawnEntries == null)
        {
            ReportSpawnFailure("Sub-wave data is missing.");
            yield break;
        }

        foreach (MonsterSpawnEntry entry in subWave.spawnEntries)
        {
            yield return SpawnEntryRoutine(entry);

            if (spawnFailed)
                yield break;

            if (entry.delayAfterGroup > 0f)
                yield return new WaitForSeconds(entry.delayAfterGroup);
        }
    }

    private IEnumerator SpawnEntryRoutine(MonsterSpawnEntry entry)
    {
        if (entry == null)
        {
            ReportSpawnFailure("Monster spawn entry is null.");
            yield break;
        }

        if (entry.data == null)
        {
            Debug.LogWarning("MonsterSpawnEntry skipped. MonsterDataSO is null.");
            ReportSpawnFailure("Monster data is missing.");
            yield break;
        }

        Transform spawnPoint = GetSpawnPoint(entry.spawnPointIndex);

        if (spawnPoint == null)
        {
            ReportSpawnFailure("Monster spawn point is invalid.");
            yield break;
        }

        int count = Mathf.Max(0, entry.count);
        float interval = Mathf.Max(0f, entry.interval);

        for (int i = 0; i < count; i++)
        {
            if (SpawnMonster(entry.data, spawnPoint.position) == null)
            {
                ReportSpawnFailure($"Failed to spawn monster: {entry.data.name}");
                yield break;
            }

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

    public void ClearWaveRuntime()
    {
        StopSpawning();

        var snapshot = new List<MonsterController>(aliveMonsters);
        foreach (MonsterController monster in snapshot)
        {
            if (monster == null)
                continue;

            monster.Health.OnDamaged -= HandleMonsterDamaged;
            monster.OnDead -= HandleMonsterDead;
            waveHpTracker?.UnregisterMonster(monster);
        }

        aliveMonsters.Clear();
        currentWave = null;
        deadMonsterCount = 0;
        spawnFailed = false;
    }

    private void ReportSpawnFailure(string reason)
    {
        if (spawnFailed)
            return;

        spawnFailed = true;
        Debug.LogError($"[{nameof(MonsterSpawner)}] {reason}", this);
        OnSpawnFailed?.Invoke();
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

        if (!monster.Initialize(unitRoster, data, poolManager))
        {
            if (monster.TryGetComponent(out Poolable failedSpawn))
                poolManager.Despawn(failedSpawn);

            return null;
        }

        monster.Health.OnDamaged += HandleMonsterDamaged;
        monster.OnDead += HandleMonsterDead;

        waveHpTracker?.RegisterSpawnedMonster(monster);
        aliveMonsters.Add(monster);

        return monster;
    }

    private void HandleMonsterDead(MonsterController monster)
    {
        monster.Health.OnDamaged -= HandleMonsterDamaged;
        monster.OnDead -= HandleMonsterDead;

        waveHpTracker?.UnregisterMonster(monster);
        aliveMonsters.Remove(monster);

        deadMonsterCount++;

        OnAliveCountChanged?.Invoke(RemainingCount);
    }

    private void HandleMonsterDamaged(MonsterHealth health, DamageResult result)
    {
        Vector3 worldPos = health.transform.position;
        damageUIService?.Show(worldPos, result);
    }

    public MonsterController FindClosestAlive(Vector3 from)
    {
        return CombatTargetSelector.FindClosest(aliveMonsters, from);
    }

    ICombatTarget ICombatTargetProvider.FindClosestAlive(Vector3 origin)
    {
        return FindClosestAlive(origin);
    }
}
