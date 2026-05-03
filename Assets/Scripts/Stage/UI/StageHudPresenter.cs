using TMPro;
using UnityEngine;

public class StageHudPresenter : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI stageInfoText;
    [SerializeField] private TextMeshProUGUI monsterCountText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI summonCostText;
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private TextMeshProUGUI increaseCostText;

    private StageState cachedState = StageState.None;

    private EconomyManager economy;
    private PopulationManager population;
    private StagePrepareTimerController flowController;
    private MonsterSpawner monsterSpawner;

    public void Initialize(
        StageData stageData,
        StageState initialState,
        EconomyManager economy,
        PopulationManager population,
        StagePrepareTimerController flowController,
        MonsterSpawner monsterSpawner)
    {
        this.economy = economy;
        this.population = population;
        this.flowController = flowController;
        this.monsterSpawner = monsterSpawner;

        cachedState = initialState;

        if (stageData != null)
            SetStageInfo(stageData.stageName, stageData.stageId);

        Bind();
        RefreshInitialValues();
    }

    public void Dispose()
    {
        Unbind();
    }

    public void SetPhase(StageState state)
    {
        cachedState = state;
    }

    private void Bind()
    {
        if (economy != null)
            economy.OnGoldChanged += UpdateGold;

        if (population != null)
            population.OnPopulationChanged += UpdatePopulation;

        if (flowController != null)
            flowController.OnPrepareTimerChanged += UpdatePrepareTimer;

        if (monsterSpawner != null)
            monsterSpawner.OnAliveCountChanged += UpdateMonsterCount;
    }

    private void Unbind()
    {
        if (economy != null)
            economy.OnGoldChanged -= UpdateGold;

        if (population != null)
            population.OnPopulationChanged -= UpdatePopulation;

        if (flowController != null)
            flowController.OnPrepareTimerChanged -= UpdatePrepareTimer;

        if (monsterSpawner != null)
            monsterSpawner.OnAliveCountChanged -= UpdateMonsterCount;
    }

    private void RefreshInitialValues()
    {
        if (economy != null)
        {
            UpdateGold(economy.CurrentGold);

            if (summonCostText != null)
                summonCostText.SetText("{0}", economy.GetSummonCost());

            if (rerollCostText != null)
                rerollCostText.SetText("{0}", economy.GetRerollCost());
        }

        if (population != null)
            UpdatePopulation(population.CurrentPopulation, population.MaxPopulation);
    }

    private void SetStageInfo(string stageName, int stageId)
    {
        if (stageInfoText != null)
            stageInfoText.text = $"{stageName} - {stageId}";
    }

    private void UpdateGold(int gold)
    {
        if (goldText != null)
            goldText.SetText("{0}", gold);
    }

    private void UpdatePopulation(int current, int max)
    {
        if (populationText != null)
            populationText.SetText("{0}/{1}", current, max);

        if (increaseCostText != null && population != null)
            increaseCostText.SetText("{0}", population.GetNextIncreaseCost());
    }

    private void UpdatePrepareTimer(float time)
    {
        if (timerText != null)
            timerText.SetText("{0:F1}", time);
    }

    private void UpdateMonsterCount(int remainCount)
    {
        if (monsterCountText == null)
            return;

        if (cachedState == StageState.Preparing)
            monsterCountText.SetText("출현 예정 : {0:00}", remainCount);
        else if (cachedState == StageState.Combat)
            monsterCountText.SetText("남은 몬스터 수 : {0:00}", remainCount);
        else
            monsterCountText.text = string.Empty;
    }

    public void RefreshMonsterCount(int count)
    {
        UpdateMonsterCount(count);
    }
}