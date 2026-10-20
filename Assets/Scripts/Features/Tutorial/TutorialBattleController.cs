using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public sealed class TutorialBattleController : MonoBehaviour
{
    private enum SignalType
    {
        Summoned,
        Rerolled,
        Fused,
        Moved,
        PopulationIncreased,
        CombatStarted,
        PreparationStarted,
        RelicShown,
        RelicChosen,
        StageCleared,
    }

    private readonly struct Signal
    {
        public SignalType Type { get; }
        public string UnitId { get; }
        public int Star { get; }
        public int WaveIndex { get; }

        public Signal(SignalType type, string unitId = null, int star = 0, int waveIndex = -1)
        {
            Type = type;
            UnitId = unitId;
            Star = star;
            WaveIndex = waveIndex;
        }
    }

    [Header("Stage References")]
    [SerializeField] private StageSessionController session;
    [SerializeField] private StagePreparationService preparationService;
    [SerializeField] private FusionService fusionService;
    [SerializeField] private StagePrepareTimerController prepareTimer;
    [SerializeField] private StagePrepareActionUI prepareActionUI;
    [SerializeField] private UnitDragActionUI unitDragActionUI;
    [SerializeField] private StageRelicUI relicUI;
    [SerializeField] private StageResultUI resultUI;
    [SerializeField] private TutorialOverlayView overlay;

    [Header("Scripted Units")]
    [SerializeField] private string unitA = "unit_axeman";
    [SerializeField] private string unitB = "unit_knight";

    private readonly Queue<Signal> pendingSignals = new();
    private readonly FirstBattleTutorialSession tutorialSession = new();
    private UserDataManager userDataManager;
    private bool isTutorialEntry;
    private bool isProcessing;
    private bool isDisposed;
    private bool isReady;
    private Coroutine initialGuideRoutine;
    private string unitAName;
    private string unitBName;

    private void Awake()
    {
        isTutorialEntry = StageEnterHolder.Data?.IsTutorial == true;
        if (!isTutorialEntry)
        {
            enabled = false;
            return;
        }

        userDataManager = UserDataManager.Instance;
        unitAName = GameConfig.Units.Get(unitA)?.LocalizedName ?? unitA;
        unitBName = GameConfig.Units.Get(unitB)?.LocalizedName ?? unitB;
        prepareTimer?.SetManualMode(true);
        preparationService?.UnitSummoner?.SetUnitSelector(
            new StepGuidedStageUnitSelector(
                () => tutorialSession.CurrentStep,
                new Dictionary<string, string>
                {
                    [FirstBattleTutorialSequence.SummonWave1Unit] = unitA,
                    [FirstBattleTutorialSequence.SummonWave2Unit] = unitA,
                    [FirstBattleTutorialSequence.SummonWave3Unit] = unitA,
                    [FirstBattleTutorialSequence.SummonPostRelicB] = unitB,
                    [FirstBattleTutorialSequence.RerollBToA] = unitA,
                }));
        ApplyPreparationActionPolicy(tutorialSession.CurrentStep);
    }

    private void OnEnable()
    {
        if (!isTutorialEntry)
            return;

        isDisposed = false;
        if (preparationService != null)
        {
            preparationService.UnitSummoned += HandleSummoned;
            preparationService.UnitRerolled += HandleRerolled;
            preparationService.UnitMoved += HandleMoved;
            preparationService.PopulationIncreased += HandlePopulationIncreased;
        }
        if (fusionService != null)
            fusionService.FusionCompleted += HandleFused;
        if (session != null)
            session.PhaseChanged += HandlePhaseChanged;
        if (relicUI != null)
            relicUI.ChoiceVisibilityChanged += HandleRelicChoiceVisibilityChanged;
        resultUI?.SetBeforeReturnToLobby(PrepareReturnToLobbyAsync);
    }

    private void OnDisable()
    {
        isDisposed = true;
        if (preparationService != null)
        {
            preparationService.UnitSummoned -= HandleSummoned;
            preparationService.UnitRerolled -= HandleRerolled;
            preparationService.UnitMoved -= HandleMoved;
            preparationService.PopulationIncreased -= HandlePopulationIncreased;
        }
        if (fusionService != null)
            fusionService.FusionCompleted -= HandleFused;
        if (session != null)
            session.PhaseChanged -= HandlePhaseChanged;
        if (relicUI != null)
            relicUI.ChoiceVisibilityChanged -= HandleRelicChoiceVisibilityChanged;
        resultUI?.SetBeforeReturnToLobby(null);
        prepareTimer?.SetManualMode(false);
        preparationService?.SetAllowedActions(StagePreparationAction.All);
        pendingSignals.Clear();
        if (initialGuideRoutine != null)
        {
            StopCoroutine(initialGuideRoutine);
            initialGuideRoutine = null;
        }
    }

    private void Start()
    {
        if (!isTutorialEntry)
            return;

        isReady = true;
        overlay?.Hide();
        if (session?.CurrentState == StageState.Preparing && session.CurrentWaveIndex == 0)
            ScheduleInitialGuide();
        if (pendingSignals.Count > 0 && !isProcessing)
            ProcessSignals();
    }

    private void HandleSummoned(string unitId) => Enqueue(new Signal(SignalType.Summoned, unitId));
    private void HandleRerolled(string unitId) => Enqueue(new Signal(SignalType.Rerolled, unitId));
    private void HandleFused(string unitId, int star) => Enqueue(new Signal(SignalType.Fused, unitId, star));
    private void HandleMoved(string unitId) => Enqueue(new Signal(SignalType.Moved, unitId));
    private void HandlePopulationIncreased() => Enqueue(new Signal(SignalType.PopulationIncreased));
    private void HandlePhaseChanged(StageState _, StageState next)
    {
        if (next == StageState.Combat)
            Enqueue(new Signal(SignalType.CombatStarted, waveIndex: session?.CurrentWaveIndex ?? -1));
        else if (next == StageState.Preparing)
        {
            int waveIndex = session?.CurrentWaveIndex ?? -1;
            if (waveIndex == 0 && tutorialSession.CurrentStep == FirstBattleTutorialSequence.SummonWave1Unit)
            {
                ScheduleInitialGuide();
            }
            else
            {
                Enqueue(new Signal(SignalType.PreparationStarted, waveIndex: waveIndex));
            }
        }
        else if (next == StageState.StageClear)
            Enqueue(new Signal(SignalType.StageCleared));
    }

    private void HandleRelicChoiceVisibilityChanged(bool visible) =>
        Enqueue(new Signal(visible ? SignalType.RelicShown : SignalType.RelicChosen));

    private void Enqueue(Signal signal)
    {
        if (isDisposed)
            return;

        pendingSignals.Enqueue(signal);
        if (isReady && !isProcessing)
            ProcessSignals();
    }

    private void ScheduleInitialGuide()
    {
        if (initialGuideRoutine == null)
            initialGuideRoutine = StartCoroutine(ShowInitialGuideAfterLayout());
    }

    private IEnumerator ShowInitialGuideAfterLayout()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        initialGuideRoutine = null;

        if (!isDisposed && session?.CurrentState == StageState.Preparing)
            ShowCurrentStep();
    }

    private void ProcessSignals()
    {
        isProcessing = true;
        try
        {
            while (!isDisposed && pendingSignals.Count > 0)
            {
                Signal signal = pendingSignals.Dequeue();
                string expectedStep = ResolveExpectedStep(signal);
                if (expectedStep == null)
                    continue;

                if (!tutorialSession.TryAdvance(expectedStep))
                    continue;

                if (expectedStep == FirstBattleTutorialSequence.SummonPostRelicB)
                    preparationService?.GrantFreeRerolls(1);
                else if (expectedStep == FirstBattleTutorialSequence.ChooseRelic)
                    prepareTimer?.SetManualMode(false);

                ShowCurrentStep();
            }
        }
        finally
        {
            isProcessing = false;
        }
    }

    private string ResolveExpectedStep(Signal signal)
    {
        string current = tutorialSession.CurrentStep;
        return current switch
        {
            FirstBattleTutorialSequence.SummonWave1Unit when IsSummon(signal, unitA) => current,
            FirstBattleTutorialSequence.MoveWave1Unit when IsMove(signal, unitA) => current,
            FirstBattleTutorialSequence.StartWave1 when IsCombat(signal, 0) => current,
            FirstBattleTutorialSequence.WaitWave1Clear when IsPreparation(signal, 1) => current,
            FirstBattleTutorialSequence.SummonWave2Unit when IsSummon(signal, unitA) => current,
            FirstBattleTutorialSequence.WaitWave2Fusion when IsFusion(signal, unitA, 2) => current,
            FirstBattleTutorialSequence.StartWave2 when IsCombat(signal, 1) => current,
            FirstBattleTutorialSequence.WaitWave2Clear when IsPreparation(signal, 2) => current,
            FirstBattleTutorialSequence.SummonWave3Unit when IsSummon(signal, unitA) => current,
            FirstBattleTutorialSequence.SummonPostRelicB when IsSummon(signal, unitB) => current,
            FirstBattleTutorialSequence.RerollBToA when IsReroll(signal, unitB) => current,
            FirstBattleTutorialSequence.WaitThreeStarFusion when IsFusion(signal, unitA, 3) => current,
            FirstBattleTutorialSequence.IncreasePopulation when signal.Type == SignalType.PopulationIncreased => current,
            FirstBattleTutorialSequence.StartWave3 when IsCombat(signal, 2) => current,
            FirstBattleTutorialSequence.WaitEliteClear when signal.Type == SignalType.RelicShown => current,
            FirstBattleTutorialSequence.ChooseRelic when signal.Type == SignalType.RelicChosen => current,
            FirstBattleTutorialSequence.FreePlay when signal.Type == SignalType.StageCleared => current,
            _ => null,
        };
    }

    private void ShowCurrentStep()
    {
        string step = tutorialSession.CurrentStep;
        ApplyPreparationActionPolicy(step);
        switch (step)
        {
            case FirstBattleTutorialSequence.SummonWave1Unit:
            case FirstBattleTutorialSequence.SummonWave2Unit:
            case FirstBattleTutorialSequence.SummonWave3Unit:
            case FirstBattleTutorialSequence.SummonPostRelicB:
                overlay?.ShowRequiredAction(GetTutorialText(step), prepareActionUI?.SummonButtonTarget);
                break;
            case FirstBattleTutorialSequence.StartWave1:
            case FirstBattleTutorialSequence.StartWave2:
            case FirstBattleTutorialSequence.StartWave3:
                overlay?.ShowRequiredAction(GetTutorialText(step), prepareActionUI?.StartButtonTarget);
                break;
            case FirstBattleTutorialSequence.IncreasePopulation:
                overlay?.ShowRequiredAction(
                    GetTutorialText(step),
                    prepareActionUI?.IncreasePopulationButtonTarget);
                break;
            case FirstBattleTutorialSequence.MoveWave1Unit:
                overlay?.ShowWaiting(GetTutorialText(step));
                break;
            case FirstBattleTutorialSequence.WaitWave2Fusion:
            case FirstBattleTutorialSequence.WaitThreeStarFusion:
                overlay?.ShowWaiting(GetTutorialText(step));
                break;
            case FirstBattleTutorialSequence.RerollBToA:
                overlay?.ShowWorldAction(GetTutorialText(step), unitDragActionUI?.RerollTarget);
                break;
            case FirstBattleTutorialSequence.WaitWave1Clear:
            case FirstBattleTutorialSequence.WaitWave2Clear:
            case FirstBattleTutorialSequence.WaitEliteClear:
            case FirstBattleTutorialSequence.FreePlay:
                overlay?.Hide();
                break;
            case FirstBattleTutorialSequence.ChooseRelic:
                overlay?.Hide();
                break;
            case FirstBattleTutorialSequence.ClearBattle:
                overlay?.Hide();
                break;
        }
    }

    private string GetTutorialText(string step) => TutorialText.Get(step, unitAName, unitBName);

    private void ApplyPreparationActionPolicy(string step)
    {
        StagePreparationAction actions = step switch
        {
            FirstBattleTutorialSequence.SummonWave1Unit => StagePreparationAction.Summon,
            FirstBattleTutorialSequence.SummonWave2Unit => StagePreparationAction.Summon,
            FirstBattleTutorialSequence.SummonWave3Unit => StagePreparationAction.Summon,
            FirstBattleTutorialSequence.SummonPostRelicB => StagePreparationAction.Summon,
            FirstBattleTutorialSequence.IncreasePopulation => StagePreparationAction.IncreasePopulation,
            FirstBattleTutorialSequence.StartWave1 => StagePreparationAction.StartWave,
            FirstBattleTutorialSequence.StartWave2 => StagePreparationAction.StartWave,
            FirstBattleTutorialSequence.StartWave3 => StagePreparationAction.StartWave,
            FirstBattleTutorialSequence.RerollBToA => StagePreparationAction.Reroll,
            FirstBattleTutorialSequence.WaitWave2Fusion => StagePreparationAction.None,
            FirstBattleTutorialSequence.WaitThreeStarFusion => StagePreparationAction.None,
            FirstBattleTutorialSequence.FreePlay => StagePreparationAction.All,
            _ => StagePreparationAction.None,
        };

        preparationService?.SetAllowedActions(actions);
    }

    private async System.Threading.Tasks.Task<bool> PrepareReturnToLobbyAsync()
    {
        if (tutorialSession.CurrentStep != FirstBattleTutorialSequence.ClearBattle)
            return false;

        if (userDataManager?.UserData?.Tutorial?.Completed == true)
            return true;

        TutorialProgressResult result = await userDataManager.CompleteTutorialAsync();
        return result.Succeeded;
    }

    private static bool IsSummon(Signal signal, string unitId) =>
        signal.Type == SignalType.Summoned && string.Equals(signal.UnitId, unitId, StringComparison.Ordinal);

    private static bool IsFusion(Signal signal, string unitId, int star) =>
        signal.Type == SignalType.Fused && signal.Star == star &&
        string.Equals(signal.UnitId, unitId, StringComparison.Ordinal);

    private static bool IsReroll(Signal signal, string unitId) =>
        signal.Type == SignalType.Rerolled &&
        string.Equals(signal.UnitId, unitId, StringComparison.Ordinal);

    private static bool IsMove(Signal signal, string unitId) =>
        signal.Type == SignalType.Moved &&
        string.Equals(signal.UnitId, unitId, StringComparison.Ordinal);

    private static bool IsCombat(Signal signal, int waveIndex) =>
        signal.Type == SignalType.CombatStarted && signal.WaveIndex == waveIndex;

    private static bool IsPreparation(Signal signal, int waveIndex) =>
        signal.Type == SignalType.PreparationStarted && signal.WaveIndex == waveIndex;
}
