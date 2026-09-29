using System;
using UnityEngine;

public sealed class StagePhaseRuntimeController : MonoBehaviour
{
    private enum RuntimePhase
    {
        None,
        Preparation,
        Combat,
    }

    [SerializeField] private StagePrepareTimerController prepareTimerController;
    [SerializeField] private WaveController waveController;
    [SerializeField] private StagePreparationService preparationService;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private MonsterPrewarmService monsterPrewarmService;
    [SerializeField] private StageTimeController stageTimeController;
    [SerializeField] private StagePoolManager poolManager;
    private RuntimePhase currentPhase;

    public void BeginPreparation(WaveData wave, Action onFinished)
    {
        if (wave == null)
            throw new ArgumentNullException(nameof(wave));

        if (currentPhase != RuntimePhase.None)
            EndCurrentPhase();

        currentPhase = RuntimePhase.Preparation;
        try
        {
            monsterPrewarmService.PrewarmForWave(wave);
            monsterSpawner.PrepareWavePreview(wave);
            monsterSpawner.WaveHpTracker.PrepareWave(wave);
            if (!preparationService.EnterPrepareMode())
                throw new InvalidOperationException("Failed to enter stage preparation mode.");

            stageTimeController.ExitCombatPhase();
            if (!prepareTimerController.TryStartPreparePhase(onFinished))
                throw new InvalidOperationException("Failed to start the preparation timer.");
        }
        catch
        {
            EndCurrentPhase();
            throw;
        }
    }

    public void BeginCombat(WaveData wave, Action onWin, Action onLose)
    {
        if (wave == null)
            throw new ArgumentNullException(nameof(wave));

        if (currentPhase != RuntimePhase.Preparation)
            throw new InvalidOperationException($"Cannot begin combat from runtime phase {currentPhase}.");

        prepareTimerController.StopPreparePhase();
        preparationService.ExitPrepareMode();
        stageTimeController.EnterCombatPhase();
        currentPhase = RuntimePhase.Combat;

        if (!waveController.TryStartWave(wave, onWin, onLose))
            onLose?.Invoke();
    }

    public void EndCurrentPhase()
    {
        if (currentPhase == RuntimePhase.None)
            return;

        currentPhase = RuntimePhase.None;

        prepareTimerController.StopPreparePhase();
        waveController.StopWave();

        stageTimeController.ResetToNormalTime();
        preparationService.EndCurrentPhase();
        ClearTransientCombatObjects();
    }

    private void ClearTransientCombatObjects()
    {
        if (poolManager == null)
        {
            Debug.LogError($"[{nameof(StagePhaseRuntimeController)}] StagePoolManager is not assigned.", this);
            return;
        }

        poolManager.DespawnWaveObjects();
    }
}
