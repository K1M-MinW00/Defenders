using UnityEngine;
using UnityEngine.UI;

public class StagePrepareActionUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button increasePopButton;

    private StagePreparationService preparationService;
    private StagePrepareTimerController flowController;

    public void Initialize(StagePreparationService preparationService,StagePrepareTimerController flowController)
    {
        this.preparationService = preparationService;
        this.flowController = flowController;

        Bind();
    }

    public void Dispose()
    {
        Unbind();
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
        preparationService?.TrySummonUnit();
    }

    private void HandleIncreasePopulationClicked()
    {
        preparationService?.TryIncreasePopulation();
    }

    private void HandleStartButtonClicked()
    {
        flowController?.ForceFinishPrepare();
    }
}