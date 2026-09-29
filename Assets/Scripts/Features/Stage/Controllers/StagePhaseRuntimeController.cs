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

        monsterPrewarmService.PrewarmForWave(wave);
        monsterSpawner.WaveHpTracker.PrepareWave(wave);
        preparationService.EnterPrepareMode();
        stageTimeController.ExitCombatPhase();
        prepareTimerController.StartPreparePhase(onFinished);
        currentPhase = RuntimePhase.Preparation;
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

        RuntimePhase endingPhase = currentPhase;
        currentPhase = RuntimePhase.None;

        prepareTimerController.StopPreparePhase();
        if (endingPhase == RuntimePhase.Combat)
            waveController.StopWave();

        stageTimeController.ExitCombatPhase();
        preparationService.EndCurrentPhase();
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

        poolManager.DespawnAll(PoolCategory.Monster);
        poolManager.DespawnAll(PoolCategory.Projectile);
        poolManager.DespawnAll(PoolCategory.Effect);
    }
}
