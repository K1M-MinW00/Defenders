using UnityEngine;
using UnityEngine.UI;

public class StagePrepareActionUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button increasePopButton;

    private StagePreparationService preparationService;
    private StagePrepareTimerController flowController;
    public RectTransform StartButtonTarget => startButton?.transform as RectTransform;
    public RectTransform SummonButtonTarget => summonButton?.transform as RectTransform;
    public RectTransform IncreasePopulationButtonTarget => increasePopButton?.transform as RectTransform;

    public void Initialize(StagePreparationService preparationService, StagePrepareTimerController flowController)
    {
        Dispose();

        this.preparationService = preparationService;
        this.flowController = flowController;

        Bind();
        RefreshAllowedActions(preparationService != null
            ? preparationService.AllowedActions
            : StagePreparationAction.All);
    }

    public void Dispose()
    {
        Unbind();
        preparationService = null;
        flowController = null;
    }

    private void Bind()
    {
        if (preparationService != null)
            preparationService.AllowedActionsChanged += RefreshAllowedActions;

        startButton?.onClick.AddListener(HandleStartButtonClicked);
        summonButton?.onClick.AddListener(HandleSummonButtonClicked);
        increasePopButton?.onClick.AddListener(HandleIncreasePopulationClicked);
    }

    private void Unbind()
    {
        if (preparationService != null)
            preparationService.AllowedActionsChanged -= RefreshAllowedActions;

        startButton?.onClick.RemoveListener(HandleStartButtonClicked);
        summonButton?.onClick.RemoveListener(HandleSummonButtonClicked);
        increasePopButton?.onClick.RemoveListener(HandleIncreasePopulationClicked);
    }

    private void HandleSummonButtonClicked()
    {
        if (preparationService == null ||
            preparationService.TrySummonUnit(out StageUnitTransactionFailure failure))
            return;

        ShowTransactionFailure(failure);
    }

    private void HandleIncreasePopulationClicked()
    {
        if (preparationService == null ||
            preparationService.TryIncreasePopulation(out StageUnitTransactionFailure failure))
            return;

        ShowTransactionFailure(failure);
    }

    private void HandleStartButtonClicked()
    {
        if (preparationService == null || !preparationService.CanStartWave)
            return;

        if (!preparationService.HasAnyUnit)
        {
            UIFeedbackToast.Show(Localize("prepare.unit_required"));
            return;
        }

        flowController?.ForceFinishPrepare();
    }

    private void RefreshAllowedActions(StagePreparationAction actions)
    {
        if (startButton != null)
            startButton.interactable = (actions & StagePreparationAction.StartWave) != 0;
        if (summonButton != null)
            summonButton.interactable = (actions & StagePreparationAction.Summon) != 0;
        if (increasePopButton != null)
            increasePopButton.interactable = (actions & StagePreparationAction.IncreasePopulation) != 0;
    }

    private static void ShowTransactionFailure(StageUnitTransactionFailure failure)
    {
        switch (failure)
        {
            case StageUnitTransactionFailure.InsufficientMinerals:
                UIFeedbackToast.Show(Localize("prepare.insufficient_minerals"));
                break;
            case StageUnitTransactionFailure.PopulationFull:
                UIFeedbackToast.Show(Localize("prepare.population_full"));
                break;
        }
    }

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.Stage, key);
}
