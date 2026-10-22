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
    [SerializeField] private BossBattleTimerController bossBattleTimerController;
    [SerializeField] private WaveController waveController;
    [SerializeField] private StagePreparationService preparationService;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private MonsterPrewarmService monsterPrewarmService;
    [SerializeField] private StageTimeController stageTimeController;
    [SerializeField] private StagePoolManager poolManager;
    [SerializeField] private MonsterPathPreview pathPreview;
    private RuntimePhase currentPhase;
    private Action pendingPrepareFinished;
    private bool isShutdown;

    public StagePreparationService PreparationService => preparationService;
    public MonsterSpawner MonsterSpawner => monsterSpawner;
    public BossBattleTimerController BossBattleTimer => bossBattleTimerController;

    public bool TryConfigure(StageDataSO stageData, out string error)
    {
        if (stageData == null)
        {
            error = "Stage data is missing.";
            return false;
        }

        if (prepareTimerController == null || waveController == null || preparationService == null ||
            monsterSpawner == null || monsterPrewarmService == null || stageTimeController == null ||
            poolManager == null || pathPreview == null || bossBattleTimerController == null)
        {
            error = "Required phase runtime references are missing.";
            return false;
        }

        prepareTimerController.Configure(stageData.prepareDuration);
        bossBattleTimerController.Initialize(monsterSpawner);
        error = string.Empty;
        return true;
    }

    public void CancelActiveUnitInteraction()
    {
        preparationService?.CancelActiveInteraction();
    }

    public void BeginPreparation(WaveData wave, bool waitForFirstUnit, Action onFinished)
    {
        if (wave == null)
            throw new ArgumentNullException(nameof(wave));

        if (currentPhase != RuntimePhase.None)
            EndCurrentPhase();

        isShutdown = false;
        currentPhase = RuntimePhase.Preparation;
        try
        {
            monsterPrewarmService.PrewarmForWave(wave);
            monsterSpawner.PrepareWavePreview(wave);
            if (!preparationService.EnterPrepareMode())
                throw new InvalidOperationException("Failed to enter stage preparation mode.");

            stageTimeController.ExitCombatPhase();
            pathPreview.Show(wave, monsterSpawner.SpawnPoints, preparationService.UnitRoster);
            pendingPrepareFinished = onFinished;
            if (waitForFirstUnit && !preparationService.HasAnyUnit)
                preparationService.OnUnitRosterChanged += HandleUnitRosterChanged;
            else
                StartPrepareTimer();
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
        pathPreview?.Hide();
        preparationService.ExitPrepareMode();
        stageTimeController.EnterCombatPhase();
        currentPhase = RuntimePhase.Combat;

        if (wave.waveType == WaveType.Boss &&
            !bossBattleTimerController.TryStart(wave, () => waveController.TryFailWave()))
        {
            onLose?.Invoke();
            return;
        }

        if (!waveController.TryStartWave(wave, onWin, onLose))
        {
            bossBattleTimerController.StopTimer();
            onLose?.Invoke();
        }
    }

    public void EndCurrentPhase(bool preserveCombatObjects = false)
    {
        if (currentPhase == RuntimePhase.None)
            return;

        currentPhase = RuntimePhase.None;
        CleanupRuntime(preserveCombatObjects);
    }

    public void Shutdown()
    {
        if (isShutdown)
            return;

        isShutdown = true;
        currentPhase = RuntimePhase.None;
        CleanupRuntime(preserveCombatObjects: false);
    }

    private void CleanupRuntime(bool preserveCombatObjects = false)
    {
        if (preparationService != null)
            preparationService.OnUnitRosterChanged -= HandleUnitRosterChanged;

        pendingPrepareFinished = null;
        pathPreview?.Hide();
        prepareTimerController?.StopPreparePhase();
        bossBattleTimerController?.StopTimer();
        waveController?.StopWave(preserveMonsters: preserveCombatObjects);

        stageTimeController?.ResetToNormalTime();
        preparationService?.EndCurrentPhase();
        ClearTransientCombatObjects(preserveCombatObjects);
    }

    private void HandleUnitRosterChanged()
    {
        if (currentPhase != RuntimePhase.Preparation || !preparationService.HasAnyUnit)
            return;

        preparationService.OnUnitRosterChanged -= HandleUnitRosterChanged;
        StartPrepareTimer();
    }

    private void StartPrepareTimer()
    {
        Action onFinished = pendingPrepareFinished;
        pendingPrepareFinished = null;

        if (!prepareTimerController.TryStartPreparePhase(onFinished))
            throw new InvalidOperationException("Failed to start the preparation timer.");
    }

    private void ClearTransientCombatObjects(bool preserveMonsters)
    {
        if (poolManager == null)
            return;

        if (!preserveMonsters)
        {
            poolManager.DespawnWaveObjects();
            return;
        }

        poolManager.DespawnAll(PoolCategory.Projectile);
        poolManager.DespawnAll(PoolCategory.Effect);
    }

    private void OnDisable()
    {
        Shutdown();
    }
}
