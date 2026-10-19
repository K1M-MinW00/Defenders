using UnityEngine;
using UnityEngine.UI;

public class StagePrepareActionUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button increasePopButton;

    private StagePreparationService preparationService;
    private StagePrepareTimerController flowController;

    public void Initialize(StagePreparationService preparationService, StagePrepareTimerController flowController)
    {
        Dispose();

        this.preparationService = preparationService;
        this.flowController = flowController;

        Bind();
    }

    public void Dispose()
    {
        Unbind();
        preparationService = null;
        flowController = null;
    }

    private void Bind()
    {
        startButton?.onClick.AddListener(HandleStartButtonClicked);
        summonButton?.onClick.AddListener(HandleSummonButtonClicked);
        increasePopButton?.onClick.AddListener(HandleIncreasePopulationClicked);
    }

    private void Unbind()
    {
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
        if (preparationService == null || !preparationService.HasAnyUnit)
        {
            UIFeedbackToast.Show(Localize("prepare.unit_required"));
            return;
        }

        flowController?.ForceFinishPrepare();
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
