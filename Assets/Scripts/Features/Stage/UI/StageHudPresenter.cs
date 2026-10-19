using TMPro;
using UnityEngine;

public class StageHudPresenter : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI stageInfoText;
    [SerializeField] private TextMeshProUGUI monsterCountText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private StageInterestIndicator interestIndicator;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI summonCostText;
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private TextMeshProUGUI increaseCostText;
    [SerializeField] private GameObject increasePricePanel;

    [Header("Population")]
    [SerializeField] private Color populationAtCapacityColor = new(1f, 0.25f, 0.25f, 1f);

    private StageState cachedState = StageState.None;
    private Color populationDefaultColor = Color.white;
    private bool hasPopulationDefaultColor;
    private int lastMonsterCount;
    private int lastSector;
    private int lastStage;

    private EconomyManager economy;
    private PopulationManager population;
    private StagePrepareTimerController flowController;
    private MonsterSpawner monsterSpawner;
    private StagePreparationService preparationService;

    private void Awake()
    {
        CachePopulationDefaultColor();
    }

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;
    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    public void Initialize(
        StageDataSO stageData,
        StageState initialState,
        EconomyManager economy,
        PopulationManager population,
        StagePrepareTimerController flowController,
        MonsterSpawner monsterSpawner,
        StagePreparationService preparationService)
    {
        Dispose();

        this.economy = economy;
        this.population = population;
        this.flowController = flowController;
        this.monsterSpawner = monsterSpawner;
        this.preparationService = preparationService;

        cachedState = initialState;

        if (stageData != null)
            SetStageInfo(stageData.sector, stageData.stage);

        Bind();
        RefreshInitialValues();
    }

    public void Dispose()
    {
        Unbind();
        economy = null;
        population = null;
        flowController = null;
        monsterSpawner = null;
        preparationService = null;
        cachedState = StageState.None;
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
            monsterSpawner.OnRemainingCountChanged += UpdateMonsterCount;

        if (preparationService != null)
            preparationService.OnFreeRerollsChanged += UpdateRerollCost;
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
            monsterSpawner.OnRemainingCountChanged -= UpdateMonsterCount;

        if (preparationService != null)
            preparationService.OnFreeRerollsChanged -= UpdateRerollCost;
    }

    private void RefreshInitialValues()
    {
        if (economy != null)
        {
            UpdateGold(economy.CurrentGold);

            if (summonCostText != null)
                summonCostText.SetText("{0}", economy.GetSummonCost());

            UpdateRerollCost(preparationService?.FreeRerollsRemaining ?? 0);
        }

        if (population != null)
            UpdatePopulation(population.CurrentPopulation, population.MaxPopulation);
    }

    private void UpdateRerollCost(int freeRemaining)
    {
        if (rerollCostText == null || economy == null)
            return;

        rerollCostText.text = freeRemaining > 0
            ? Localize("hud.free_count", freeRemaining)
            : economy.GetRerollCost().ToString();
    }

    private void SetStageInfo(int sector, int stage)
    {
        lastSector = sector;
        lastStage = stage;
        if (stageInfoText != null)
            stageInfoText.text = Localize("hud.stage", sector, stage);
    }

    private void UpdateGold(int gold)
    {
        if (goldText != null)
            goldText.SetText("{0}", gold);

        interestIndicator?.SetInterest(economy?.CurrentInterestBonus ?? 0);
    }

    private void UpdatePopulation(int current, int max)
    {
        if (populationText != null)
        {
            CachePopulationDefaultColor();
            populationText.SetText("{0}/{1}", current, max);
            populationText.color = current >= max
                ? populationAtCapacityColor
                : populationDefaultColor;
        }

        if (population != null)
        {
            bool canIncrease = population.CanIncreaseMax();
            increasePricePanel?.SetActive(canIncrease);

            if (canIncrease && increaseCostText != null)
                increaseCostText.SetText("{0}", population.GetNextIncreaseCost());
        }
    }

    private void CachePopulationDefaultColor()
    {
        if (hasPopulationDefaultColor || populationText == null)
            return;

        populationDefaultColor = populationText.color;
        hasPopulationDefaultColor = true;
    }

    private void UpdatePrepareTimer(float time)
    {
        if (timerText != null)
            timerText.text = Mathf.Max(0f, time).ToString(
                "0.0",
                System.Globalization.CultureInfo.InvariantCulture);
    }

    private void UpdateMonsterCount(int remainCount)
    {
        lastMonsterCount = remainCount;
        if (monsterCountText == null)
            return;

        if (cachedState == StageState.Preparing)
            monsterCountText.text = Localize("hud.monsters.incoming", remainCount);
        else if (cachedState == StageState.Combat)
            monsterCountText.text = Localize("hud.monsters.remaining", remainCount);
        else
            monsterCountText.text = string.Empty;
    }

    public void RefreshMonsterCount(int count)
    {
        UpdateMonsterCount(count);
    }

    private void HandleLocaleChanged(string _)
    {
        if (lastSector > 0 && lastStage > 0)
            SetStageInfo(lastSector, lastStage);
        UpdateRerollCost(preparationService?.FreeRerollsRemaining ?? 0);
        UpdateMonsterCount(lastMonsterCount);
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Stage, key, arguments);

    private void OnDestroy() => LocalizationService.LocaleChanged -= HandleLocaleChanged;
}
