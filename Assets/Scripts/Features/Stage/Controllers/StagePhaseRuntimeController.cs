using System;
using UnityEngine;

public sealed class StagePhaseRuntimeController : MonoBehaviour
{
    [SerializeField] private StagePrepareTimerController prepareTimerController;
    [SerializeField] private WaveController waveController;
    [SerializeField] private StagePreparationService preparationService;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private MonsterPrewarmService monsterPrewarmService;
    [SerializeField] private StageTimeController stageTimeController;
    [SerializeField] private StagePoolManager poolManager;

    public void BeginPreparation(WaveData wave, Action onFinished)
    {
        if (wave == null)
            throw new ArgumentNullException(nameof(wave));

        monsterPrewarmService.PrewarmForWave(wave);
        monsterSpawner.WaveHpTracker.PrepareWave(wave);
        preparationService.EnterPrepareMode();
        stageTimeController.ExitCombatPhase();
        prepareTimerController.StartPreparePhase(onFinished);
    }

    public void BeginCombat(WaveData wave, Action onWin, Action onLose)
    {
        if (wave == null)
            throw new ArgumentNullException(nameof(wave));

        preparationService.ExitPrepareMode();
        stageTimeController.EnterCombatPhase();
        waveController.StartWave(wave, onWin, onLose);
    }

    public void CompleteCombat()
    {
        stageTimeController.ExitCombatPhase();
        ClearTransientCombatObjects();
    }

    public void StopCurrentPhase()
    {
        prepareTimerController.StopPreparePhase();
        waveController.StopWave();
        preparationService.ExitPrepareMode();
        ClearTransientCombatObjects();
    }

    public void ResumeTime()
    {
        stageTimeController.Resume();
    }

    private void ClearTransientCombatObjects()
    {
        if (poolManager == null)
        {
            Debug.LogError($"[{nameof(StagePhaseRuntimeController)}] StagePoolManager is not assigned.", this);
            return;
        }

        poolManager.DespawnAll(PoolCategory.Projectile);
        poolManager.DespawnAll(PoolCategory.Effect);
    }
}
