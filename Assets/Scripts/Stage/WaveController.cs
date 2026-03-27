using System;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private UnitRoster unitRoster;

    private bool waveEnded;
    private bool allMonstersSpawned;
    private Action onWaveWin;
    private Action onWaveLose;

    public void StartWave(WaveData waveData, Action onWin, Action onLose)
    {
        if (waveData == null)
            return;

        waveEnded = false;
        allMonstersSpawned = false;
        onWaveWin = onWin;
        onWaveLose = onLose;

        monsterSpawner.OnAliveCountChanged += HandleMonsterAliveChanged;
        monsterSpawner.OnAllMonstersSpawned += HandleAllMonstersSpawned;
        unitRoster.OnAliveCountChanged += HandleUnitAliveChanged;
        
        monsterSpawner.StartWave(waveData);
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

        monsterSpawner.OnAliveCountChanged -= HandleMonsterAliveChanged;
        monsterSpawner.OnAllMonstersSpawned -= HandleAllMonstersSpawned;
        unitRoster.OnAliveCountChanged -= HandleUnitAliveChanged;

        if (isWin)
        {
            onWaveWin?.Invoke();
        }
        else
        {
            onWaveLose?.Invoke();
        }
    }
}