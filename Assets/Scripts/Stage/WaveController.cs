using System;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private UnitResetService unitResetService;

    public MonsterSpawner MonsterSpawner => monsterSpawner;

    private bool waveEnded;
    private Action onWaveWin;
    private Action onWaveLose;

    public void StartWave(WaveData waveData, Action onWin, Action onLose)
    {
        if (waveData == null)
            return;

        waveEnded = false;
        onWaveWin = onWin;
        onWaveLose = onLose;

        unitResetService?.CapturePreWavePositions(unitRoster);

        monsterSpawner.OnAliveCountChanged += HandleMonsterAliveChanged;
        monsterSpawner.OnAllMonstersSpawned += HandleAllMonstersSpawned;

        monsterSpawner.StartWave(waveData);
    }

    private void HandleMonsterAliveChanged(int aliveCount)
    {
        EvaluateWaveResult();
    }

    private void HandleAllMonstersSpawned()
    {
        EvaluateWaveResult();
    }

    private void EvaluateWaveResult()
    {
        if (waveEnded)
            return;

        int aliveUnits = unitRoster.CountAliveUnits();
        int aliveMonsters = monsterSpawner.AliveCount;

        if (aliveUnits == 0 && (monsterSpawner.IsSpawning || aliveMonsters > 0))
        {
            waveEnded = true;
            FinishWave(false);
            return;
        }

        if (aliveMonsters == 0 && !monsterSpawner.IsSpawning && aliveUnits > 0)
        {
            waveEnded = true;
            FinishWave(true);
        }
    }

    private void FinishWave(bool isWin)
    {
        monsterSpawner.OnAliveCountChanged -= HandleMonsterAliveChanged;
        monsterSpawner.OnAllMonstersSpawned -= HandleAllMonstersSpawned;

        if (isWin)
        {
            unitResetService?.ResetUnitsForPrepare(unitRoster);
            onWaveWin?.Invoke();
        }
        else
        {
            onWaveLose?.Invoke();
        }
    }
}