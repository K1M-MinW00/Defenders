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

    [Header("Prepare UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI populationText;

    [Header("Wave UI")]
    //public Transform waveContainer;
    //public GameObject waveItemPrefab;
    private List<Image> waveImages = new List<Image>();

    [Header("Button")]
    public Button startButton;
    public Button summonButton;
    public Button increasePopButton;

    [Header("Prepare Button Groups")]
    [SerializeField] private GameObject defaultButtonsGroup;
    [SerializeField] private GameObject unitActionButtonsGroup;

    [Header("Unit Action Zones")]
    [SerializeField] private GameObject reRollzone;
    [SerializeField] private GameObject sellZone;

    private StageManager stageManager;
    private EconomyManager economy;
    private PopulationManager population;
    private MonsterSpawner monsterSpawner;

    private StageState cachedState;
    public void Initialize(StageManager manager)
    {
        stageManager = manager;
        cachedState = stageManager.CurrentState;

        startButton.onClick.AddListener(stageManager.StartBattleEarly);
        summonButton.onClick.AddListener(stageManager.TrySummonUnit);
        increasePopButton.onClick.AddListener(stageManager.TryIncreasePopulation);

        BindEconomy();
        BindPopulation();
        BindMonsterUI();

        SetStageInfo(stageManager.currentStageData.stageName, stageManager.currentStageData.stageId.ToString());
        CreateWaveUI(stageManager.currentStageData.waves);
        SetPhase(stageManager.CurrentState);

        UpdateGoldUI(economy.CurrentGold);
        UpdatePopulationUI(population.CurrentPopulation, population.MaxPopulation);
        UpdateMonsterCountUI(monsterSpawner.AliveCount);
    }

    private void OnDestroy()
    {
        UnBindEconomy();
        UnBindPopulation();
        UnBindMonsterUI();
    }

    public void SetPhase(StageState state)
    {
        cachedState = state;

        if (common_HUD != null)
            common_HUD.SetActive(true);

        bool isPreparing = state == StageState.Preparing;
        bool isCombat = state == StageState.Combat;

        if (prepare_HUD != null)
            prepare_HUD.SetActive(isPreparing);

        if(combat_HUD != null)
            combat_HUD.SetActive(isCombat);

        if (isPreparing)
            SetUnitDragMode(false);

        if (monsterSpawner != null)
            UpdateMonsterCountUI(monsterSpawner.AliveCount);
    }


    /// <summary>
    ///  Bindings
    /// </summary>
    /// 
    private void BindMonsterUI()
    {
        monsterSpawner = stageManager != null ? stageManager.MonsterSpawner : null;

        if (monsterSpawner == null)
            return;

        monsterSpawner.OnAliveCountChanged += UpdateMonsterCountUI;
        UpdateMonsterCountUI(monsterSpawner.AliveCount);
    }

    private void BindPopulation()
    {
        population = stageManager.PopulationManager;

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


    private void UnBindMonsterUI()
    {
        if (monsterSpawner == null)
            return;

        monsterSpawner.OnAliveCountChanged -= UpdateMonsterCountUI;
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

    private void UpdateMonsterCountUI(int aliveCount)
    {
        if (cachedState == StageState.Preparing)
            monsterCountText.text = $"Expected : {aliveCount.ToString()}";
        else if (cachedState == StageState.Combat)
            monsterCountText.text = $"Remained : {aliveCount.ToString()}";
        else
            monsterCountText.text = aliveCount.ToString();
    }

    public void SetStageInfo(string stageName, string stageId)
    {
        stageInfoText.text = $"{stageName} - {stageId}";
    }

    public void CreateWaveUI(List<WaveData> waves)
    {
    }

    public void UpdateCurrentWave(int currentIndex)
    {
        for (int i = 0; i < waveImages.Count; i++)
        {
            if (i == currentIndex)
                waveImages[i].color = new Color(0.4f, 0.2f, 0f); // 갈색
        }
    }

    public void UpdatePrepTimer(float time)
    {
        timerText.text = $"{time:F1}";
    }

    private Color GetWaveColor(WaveType type)
    {
        switch (type)
        {
            case WaveType.Normal:
                return Color.gray;
            case WaveType.Elite:
                return new Color(0.6f, 0f, 0.8f); // 보라
            case WaveType.Boss:
                return Color.red;
        }
        return Color.white;
    }

    public void SetUnitDragMode(bool isDraggingUnit, bool canReroll = true)
    {
        defaultButtonsGroup?.SetActive(!isDraggingUnit);
        unitActionButtonsGroup?.SetActive(isDraggingUnit);

        if(isDraggingUnit)
        {
            reRollzone.SetActive(canReroll);
        }
    }
}
