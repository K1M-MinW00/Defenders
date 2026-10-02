using UnityEngine;
using UnityEngine.UI;

public class StagePrepareActionUI : MonoBehaviour
{
    private const string UnitRequiredMessage = "전투를 진행할 유닛이 하나 이상 있어야 합니다";
    private const string InsufficientMineralsMessage = "광물이 모자랍니다";
    private const string PopulationFullMessage = "유닛 수가 최대입니다. 인구 수를 증가 시키거나 유닛을 판매하세요";

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
            UIFeedbackToast.Show(UnitRequiredMessage);
            return;
        }

        flowController?.ForceFinishPrepare();
    }

    private static void ShowTransactionFailure(StageUnitTransactionFailure failure)
    {
        switch (failure)
        {
            case StageUnitTransactionFailure.InsufficientMinerals:
                UIFeedbackToast.Show(InsufficientMineralsMessage);
                break;
            case StageUnitTransactionFailure.PopulationFull:
                UIFeedbackToast.Show(PopulationFullMessage);
                break;
        }
    }
}
