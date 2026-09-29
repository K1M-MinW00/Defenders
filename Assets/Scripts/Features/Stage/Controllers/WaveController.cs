using System;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private UnitRoster unitRoster;

    private bool waveEnded = true;
    private bool allMonstersSpawned;
    private Action onWaveWin;
    private Action onWaveLose;

    public bool IsRunning => !waveEnded;

    public bool TryStartWave(WaveData waveData, Action onWin, Action onLose)
    {
        if (waveData == null || monsterSpawner == null || unitRoster == null)
            return false;

        StopWave();

        waveEnded = false;
        allMonstersSpawned = false;
        onWaveWin = onWin;
        onWaveLose = onLose;

        monsterSpawner.OnAliveCountChanged += HandleMonsterAliveChanged;
        monsterSpawner.OnAllMonstersSpawned += HandleAllMonstersSpawned;
        monsterSpawner.OnSpawnFailed += HandleSpawnFailed;
        unitRoster.OnAliveCountChanged += HandleUnitAliveChanged;

        if (monsterSpawner.TryStartWave(waveData))
            return true;

        UnsubscribeRuntimeEvents();
        waveEnded = true;
        ClearCallbacks();
        return false;
    }

    private void HandleMonsterAliveChanged(int aliveCount)
    {
        EvaluateWaveResult();
    }

    private void HandleAllMonstersSpawned()
    {
        allMonstersSpawned = true;
        EvaluateWaveResult();
    }

    private void HandleUnitAliveChanged()
    {
        EvaluateWaveResult();
    }

    private void HandleSpawnFailed()
    {
        FinishWave(false);
    }

    private void EvaluateWaveResult()
    {
        if (waveEnded)
            return;

        int aliveUnits = unitRoster.CountAliveUnits();
        int aliveMonsters = monsterSpawner.AliveCount;

        if (aliveUnits == 0)
        {
            FinishWave(false);
            return;
        }

        if (aliveMonsters == 0 && allMonstersSpawned && aliveUnits > 0)
        {
            FinishWave(true);
        }
    }

    private void FinishWave(bool isWin)
    {
        if (waveEnded)
            return;

        waveEnded = true;

        UnsubscribeRuntimeEvents();
        Action callback = isWin ? onWaveWin : onWaveLose;
        ClearCallbacks();
        callback?.Invoke();
    }

    public void StopWave()
    {
        UnsubscribeRuntimeEvents();
        monsterSpawner?.ClearWaveRuntime();

        waveEnded = true;
        allMonstersSpawned = false;
        ClearCallbacks();
    }

    private void ClearCallbacks()
    {
        onWaveWin = null;
        onWaveLose = null;
    }

    private void UnsubscribeRuntimeEvents()
    {
        if (monsterSpawner != null)
        {
            monsterSpawner.OnAliveCountChanged -= HandleMonsterAliveChanged;
            monsterSpawner.OnAllMonstersSpawned -= HandleAllMonstersSpawned;
            monsterSpawner.OnSpawnFailed -= HandleSpawnFailed;
        }

        if (unitRoster != null)
            unitRoster.OnAliveCountChanged -= HandleUnitAliveChanged;
    }

    private void OnDisable()
    {
        StopWave();
    }
}
