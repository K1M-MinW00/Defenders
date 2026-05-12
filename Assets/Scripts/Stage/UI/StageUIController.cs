using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageUIController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject commonHUD;
    [SerializeField] private GameObject prepareHUD;
    [SerializeField] private GameObject combatHUD;

    [Header("Common HUD")]
    [SerializeField] private TextMeshProUGUI stageInfoText;
    [SerializeField] private TextMeshProUGUI monsterCountText;

    [Header("Prepare UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI populationText;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button increasePopButton;

    [Header("Prepare Button Groups")]
    [SerializeField] private GameObject defaultButtonsGroup;
    [SerializeField] private GameObject unitActionButtonsGroup;

    [Header("Unit Action Zones")]
    [SerializeField] private GameObject rerollZone;
    [SerializeField] private GameObject sellZone;

    [Header("Sub UIs")]
    [SerializeField] private StageWaveTrackUI waveTrackUI;
    [SerializeField] private StageHpSummaryUI hpSummaryUI;
    [SerializeField] private StageResultUI resultUI;

    [Header("Gold Texts")]
    [SerializeField] private TextMeshProUGUI summon_Text;
    [SerializeField] private TextMeshProUGUI reRoll_Text;
    [SerializeField] private TextMeshProUGUI increase_Text;
    [SerializeField] private TextMeshProUGUI sell_Text;

    [Header("References")]
    [SerializeField] private StageSessionController session;
    [SerializeField] private StagePrepareTimerController flowController;
    [SerializeField] private EconomyManager economy;
    [SerializeField] private PopulationManager population;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private UnitRosterHpTracker unitHpTracker;
    [SerializeField] private MonsterWaveHpTracker monsterHpTracker;
    [SerializeField] private StagePreparationService preparationService;

    private StageState cachedState = StageState.None;

    public void Initialize()
    {
        if (session == null || flowController == null)
        {
            Debug.LogError("StageUIController Initialize failed.");
            return;
        }

        cachedState = session.CurrentState;

        BindButtons();
        BindEconomy();
        BindPopulation();
        BindFlow();
        BindMonsterCount();

        hpSummaryUI?.Initialize(monsterHpTracker, unitHpTracker);
        resultUI?.Initialize();
        waveTrackUI?.Initialize(session.CurrentStageData);

        if (session.CurrentStageData != null)
            SetStageInfo(session.CurrentStageData.sector.ToString(), session.CurrentStageData.stage.ToString());

        SetPhase(session.CurrentState);
    }

    private void OnDestroy()
    {
        UnbindButtons();
        UnbindEconomy();
        UnbindPopulation();
        UnbindFlow();
        UnbindMonsterCount();
    }

    public void SetPhase(StageState state)
    {
        cachedState = state;

        if (commonHUD != null)
            commonHUD.SetActive(true);

        bool isPreparing = state == StageState.Preparing;
        bool isCombat = state == StageState.Combat;
        bool isResult = state == StageState.StageClear || state == StageState.StageFail;

        if (prepareHUD != null)
            prepareHUD.SetActive(isPreparing);

        if (combatHUD != null)
            combatHUD.SetActive(isCombat);

        if (isPreparing)
            SetUnitDragMode(false);

        if (isResult)
        {
            prepareHUD?.SetActive(false);
            combatHUD?.SetActive(false);
            return;
        }

        if (monsterSpawner != null && session?.CurrentWave != null)
            UpdateMonsterCountUI(session.CurrentWave.TotalMonsterCount);
    }

    public void RefreshWaveUI(int currentWaveIndex)
    {
        if (session?.CurrentStageData == null)
            return;

        waveTrackUI?.Refresh(session.CurrentStageData.waves, currentWaveIndex);
    }

    public void ShowStageClear()
    {
        resultUI?.ShowClear();
    }

    public void ShowStageFail()
    {
        resultUI?.ShowFail();
    }

    public void HideAllResultPanels()
    {
        resultUI?.HideAll();
    }

    public void SetStageInfo(string stageName, string stageId)
    {
        if (stageInfoText != null)
            stageInfoText.text = $"{stageName} - {stageId}";
    }

    public void SetUnitDragMode(bool isDraggingUnit, bool canReroll = true, int star = 1)
    {
        defaultButtonsGroup?.SetActive(!isDraggingUnit);
        unitActionButtonsGroup?.SetActive(isDraggingUnit);


        bool showRerollZone = isDraggingUnit && canReroll;
        
        rerollZone.SetActive(showRerollZone);
        sellZone.SetActive(isDraggingUnit);
        sell_Text.text = economy.GetSellCost(star).ToString();
    }

    private void BindButtons()
    {
        startButton?.onClick.AddListener(HandleStartButtonClicked);
        summonButton?.onClick.AddListener(HandleSummonButtonClicked);
        increasePopButton?.onClick.AddListener(HandleIncreasePopulationClicked);
    }

    private void UnbindButtons()
    {
        startButton?.onClick.RemoveListener(HandleStartButtonClicked);
        summonButton?.onClick.RemoveListener(HandleSummonButtonClicked);
        increasePopButton?.onClick.RemoveListener(HandleIncreasePopulationClicked);
    }

    private void BindEconomy()
    {
        if (economy == null) return;
        economy.OnGoldChanged += UpdateGoldUI;

        summon_Text.text = economy.GetSummonCost().ToString();
        reRoll_Text.text = economy.GetRerollCost().ToString();
        UpdateGoldUI(economy.CurrentGold);
    }

    private void UnbindEconomy()
    {
        if (economy == null) return;
        economy.OnGoldChanged -= UpdateGoldUI;
    }

    private void BindPopulation()
    {
        if (population == null) return;
        population.OnPopulationChanged += UpdatePopulationUI;

        UpdatePopulationUI(population.CurrentPopulation, population.MaxPopulation);
    }

    private void UnbindPopulation()
    {
        if (population == null) return;
        population.OnPopulationChanged -= UpdatePopulationUI;
    }

    private void BindFlow()
    {
        if (flowController == null) return;
        flowController.OnPrepareTimerChanged += UpdatePrepTimer;
    }

    private void UnbindFlow()
    {
        if (flowController == null) return;
        flowController.OnPrepareTimerChanged -= UpdatePrepTimer;
    }

    private void BindMonsterCount()
    {
        if (monsterSpawner == null) return;
        monsterSpawner.OnAliveCountChanged += UpdateMonsterCountUI;
    }

    private void UnbindMonsterCount()
    {
        if (monsterSpawner == null) return;
        monsterSpawner.OnAliveCountChanged -= UpdateMonsterCountUI;
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldText != null)
            goldText.text = gold.ToString();
    }

    private void UpdatePopulationUI(int current, int max)
    {
        if (populationText != null)
            populationText.text = $"{current}/{max}";

        if(increase_Text != null)
            increase_Text.text = population.GetNextIncreaseCost().ToString();

    }

    private void UpdatePrepTimer(float time)
    {
        if (timerText != null)
            timerText.text = $"{time:F1}";
    }

    private void UpdateMonsterCountUI(int remainCount)
    {
        if (monsterCountText == null)
            return;

        if (cachedState == StageState.Preparing)
            monsterCountText.text = $"출현 예정 : {remainCount:00}";
        else if (cachedState == StageState.Combat)
            monsterCountText.text = $"남은 몬스터 수 : {remainCount:00}";
        else
            monsterCountText.text = string.Empty;
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