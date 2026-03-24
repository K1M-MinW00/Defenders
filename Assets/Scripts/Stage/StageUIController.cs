using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StageUIController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject common_HUD;
    [SerializeField] private GameObject prepare_HUD;
    [SerializeField] private GameObject combat_HUD;

    [Header("Common HUD")]
    [SerializeField] private TextMeshProUGUI stageInfoText;
    [SerializeField] private TextMeshProUGUI monsterCountText;

    [Header("HP Summary UI")]
    [SerializeField] private Slider unit_Slider;
    [SerializeField] private TextMeshProUGUI unitHP_Text;

    [SerializeField] private Slider enemy_Slider;
    [SerializeField] private TextMeshProUGUI enemyHP_Text;

    [Header("Result UI")]
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject stageFailPanel;

    [Header("Prepare UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI populationText;

    [Header("Button")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button increasePopButton;

    [Header("Prepare Button Groups")]
    [SerializeField] private GameObject defaultButtonsGroup;
    [SerializeField] private GameObject unitActionButtonsGroup;

    [Header("Unit Action Zones")]
    [SerializeField] private GameObject reRollzone;
    [SerializeField] private GameObject sellZone;

    [Header("Wave Track UI")]
    [SerializeField] private Transform waveTrackContainer;
    [SerializeField] private WaveNodeUI waveNodePrefab;
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private GameObject ellipsisPrefab;

    [Header("Wave Sprites")]
    [SerializeField] private Sprite normalWaveSprite;
    [SerializeField] private Sprite eliteWaveSprite;
    [SerializeField] private Sprite bossWaveSprite;

    [Header("Connector Colors")]
    [SerializeField] private Color clearedConnectorColor = Color.green;
    [SerializeField] private Color upcomingConnectorColor = Color.gray;


    private StageSessionController session;
    private EconomyManager economy;
    private PopulationManager population;
    private MonsterSpawner monsterSpawner;
    private UnitRoster unitRoster;

    private StageState cachedState = StageState.None;
    public void Initialize(StageSessionController controller)
    {
        session = controller;

        if (session == null)
        {
            Debug.LogError("StageUIController initialze failed");
            return;
        }

        cachedState = session.CurrentState;

        BindButtons();
        BindEconomy();
        BindPopulation();
        BindMonsterUI();
        BindUnitRoster();
        
        
        HideAllResultPanels();

        if (session.CurrentStageData != null)
        {
            SetStageInfo(session.CurrentStageData.stageName, session.CurrentStageData.stageId.ToString());
            // CreateWaveUI(session.CurrentStageData.waves);
        }

        SetPhase(session.CurrentState);
    }


    private void OnDestroy()
    {
        UnBindButtons();
        UnBindEconomy();
        UnBindPopulation();
        UnBindMonsterUI();
        UnBindUnitRoster();
    }

    public void SetPhase(StageState state)
    {
        cachedState = state;

        if (common_HUD != null)
            common_HUD.SetActive(true);

        bool isPreparing = state == StageState.Preparing;
        bool isCombat = state == StageState.Combat;
        bool isResult = state == StageState.StageClear || state == StageState.StageFail;

        if (prepare_HUD != null)
            prepare_HUD.SetActive(isPreparing);

        if (combat_HUD != null)
            combat_HUD.SetActive(isCombat);

        if (isPreparing)
            SetUnitDragMode(false);

        if (isResult)
        {
            if (prepare_HUD != null)
                prepare_HUD.SetActive(false);

            if (combat_HUD != null)
                combat_HUD.SetActive(false);

            return;
        }

        if (monsterSpawner != null)
            UpdateMonsterCountUI(session.CurrentWave.TotalMonsterCount);
    }

    private void BindButtons()
    {
        startButton.onClick.AddListener(session.StartBattleEarly);
        summonButton.onClick.AddListener(session.RequestSummonUnit);
        increasePopButton.onClick.AddListener(session.RequestIncreasePopulation);
    }

    private void BindMonsterUI()
    {
        monsterSpawner = session != null ? session.MonsterSpawner : null;

        if (monsterSpawner == null)
            return;

        monsterSpawner.OnAliveCountChanged += UpdateMonsterCountUI;
        monsterSpawner.WaveHpTracker.OnWaveHpChanged += HandleEnemySlider;
        UpdateMonsterCountUI(session.CurrentWave.TotalMonsterCount);
    }

    private void BindPopulation()
    {
        population = session != null ? session.PopulationManager : null;

        if (population == null)
            return;

        population.OnPopulationChanged += UpdatePopulationUI;
        UpdatePopulationUI(population.CurrentPopulation, population.MaxPopulation);
    }

    private void BindEconomy()
    {
        economy = EconomyManager.Instance;

        if (economy == null)
            return;

        economy.OnGoldChanged += UpdateGoldUI;
        UpdateGoldUI(economy.CurrentGold);
    }
    private void BindUnitRoster()
    {
        unitRoster = session != null ? session.UnitRoster : null;

        unitRoster.OnTotalHpChanged += HandleUnitSlider;
        unitRoster.ForceRefreshHp();
    }

    private void UnBindButtons()
    {
        startButton.onClick.RemoveListener(session.StartBattleEarly);
        summonButton.onClick.RemoveListener(session.RequestSummonUnit);
        increasePopButton.onClick.RemoveListener(session.RequestIncreasePopulation);
    }
    private void UnBindMonsterUI()
    {
        if (monsterSpawner == null)
            return;

        monsterSpawner.OnAliveCountChanged -= UpdateMonsterCountUI;
        monsterSpawner.WaveHpTracker.OnWaveHpChanged -= HandleEnemySlider;
        monsterSpawner = null;
    }

    private void UnBindEconomy()
    {
        if (economy == null)
            return;

        economy.OnGoldChanged -= UpdateGoldUI;
        economy = null;
    }

    private void UnBindPopulation()
    {
        if (population == null)
            return;

        population.OnPopulationChanged -= UpdatePopulationUI;
        population = null;
    }
    private void UnBindUnitRoster()
    {
        unitRoster.OnTotalHpChanged -= HandleUnitSlider;
    }

    /// <summary>
    /// Update UI
    /// </summary>
    /// 
    private void UpdateGoldUI(int gold)
    {
        goldText.text = gold.ToString();
    }

    private void UpdatePopulationUI(int current, int max)
    {
        populationText.text = $"{current}/{max}";
    }


    private void UpdateMonsterCountUI(int remainCount)
    {
        if (cachedState == StageState.Preparing)
        {
            monsterCountText.text = $"출현 예정 : {remainCount: 00}";
            monsterCountText.text = $"출현 예정 : {remainCount: 00}";
        }
        else if (cachedState == StageState.Combat)
        {
            monsterCountText.text = $"남은 몬스터 수 : {remainCount : 00}";
        }
        else
            monsterCountText.text = string.Empty;
    }

    private void HandleUnitSlider(float curHp, float maxHp)
    {
        unit_Slider.value = unitRoster.GetHpRatio();
        if(unitHP_Text != null)
            unitHP_Text.text = curHp.ToString();
    }

    private void HandleEnemySlider(float curHp, float maxHp)
    {
        enemy_Slider.value = monsterSpawner.WaveHpTracker.GetHpRatio();

        if(enemyHP_Text != null)
            enemyHP_Text.text = curHp.ToString();
    }
    public void SetStageInfo(string stageName, string stageId)
    {
        stageInfoText.text = $"{stageName} - {stageId}";
    }

    public void ShowStageClear()
    {
        if (stageClearPanel != null)
            stageClearPanel.SetActive(true);
    }

    public void ShowStageFail()
    {
        if (stageFailPanel != null)
            stageFailPanel.SetActive(true);
    }
    public void HideAllResultPanels()
    {
        if (stageClearPanel != null)
            stageClearPanel.SetActive(false);

        if (stageFailPanel != null)
            stageFailPanel.SetActive(false);
    }

    public void UpdatePrepTimer(float time)
    {
        timerText.text = $"{time:F1}";
    }

    public void SetUnitDragMode(bool isDraggingUnit, bool canReroll = true)
    {
        defaultButtonsGroup?.SetActive(!isDraggingUnit);
        unitActionButtonsGroup?.SetActive(isDraggingUnit);

        if (isDraggingUnit)
        {
            reRollzone.SetActive(canReroll);
        }
    }

    public void RefreshWaveUI(List<WaveData> waves, int currentIndex)
    {
        if (waveTrackContainer == null || waveNodePrefab == null || waves == null || waves.Count == 0)
            return;

        ClearWaveTrack();

        List<int> visibleIndices = BuildVisibleWaveIndices(waves.Count, currentIndex);
        bool showEllipsis = ShouldShowEllipsis(visibleIndices);

        for (int i = 0; i < visibleIndices.Count; i++)
        {
            int waveIndex = visibleIndices[i];

            WaveNodeUI node = Instantiate(waveNodePrefab, waveTrackContainer);
            node.Setup(GetWaveSprite(waves[waveIndex].waveType), waveIndex + 1);

            if (waveIndex == currentIndex)
                node.SetAsCurrent();
            else
                node.SetAsUpcoming();
            

            bool needConnector = i < visibleIndices.Count - 1;
            if (!needConnector)
                continue;

            // 3번째와 4번째 사이가 많이 떨어져 있으면 ... 출력
            bool insertEllipsisHere = showEllipsis && i == 2 && visibleIndices.Count == 4;

            if (insertEllipsisHere)
            {
                if (ellipsisPrefab != null)
                    Instantiate(ellipsisPrefab, waveTrackContainer);
            }
            else
            {
                if (connectorPrefab != null)
                {
                    GameObject connectorObj = Instantiate(connectorPrefab, waveTrackContainer);
                    Image connectorImage = connectorObj.GetComponent<Image>();

                    if (connectorImage != null)
                    {
                        int leftWaveIndex = visibleIndices[i];
                        int rightWaveIndex = visibleIndices[i + 1];

                        bool isClearedConnector = currentIndex >= rightWaveIndex;
                        connectorImage.color = isClearedConnector
                            ? clearedConnectorColor
                            : upcomingConnectorColor;
                    }
                }
            }
        }
    }
    private bool ShouldShowEllipsis(List<int> visibleIndices)
    {
        if (visibleIndices == null || visibleIndices.Count < 4)
            return false;

        // 예: 1,2,3,10 처럼 마지막 직전이 끊겨 있으면 ... 표시
        int third = visibleIndices[2];
        int fourth = visibleIndices[3];

        return fourth - third > 1;
    }
    private void ClearWaveTrack()
    {
        for (int i = waveTrackContainer.childCount - 1; i >= 0; i--)
            Destroy(waveTrackContainer.GetChild(i).gameObject);
    }

    private List<int> BuildVisibleWaveIndices(int totalCount, int currentIndex)
    {
        List<int> result = new();

        if (totalCount <= 0)
            return result;

        currentIndex = Mathf.Clamp(currentIndex, 0, totalCount - 1);

        // 4개 이하: 전부 표시
        if (totalCount <= 4)
        {
            for (int i = 0; i < totalCount; i++)
                result.Add(i);

            return result;
        }

        int lastWindowStart = totalCount - 4;

        if (currentIndex >= lastWindowStart)
        {
            for (int i = lastWindowStart; i < totalCount; i++)
                result.Add(i);

            return result;
        }

        int blockStart = (currentIndex / 3) * 3;

        result.Add(blockStart);
        result.Add(blockStart + 1);
        result.Add(blockStart + 2);
        result.Add(totalCount - 1);

        return result;
    }

    private Sprite GetWaveSprite(WaveType type)
    {
        switch (type)
        {
            case WaveType.Normal:
                return normalWaveSprite;
            case WaveType.Elite:
                return eliteWaveSprite;
            case WaveType.Boss:
                return bossWaveSprite;
            default:
                return normalWaveSprite;
        }
    }
}
