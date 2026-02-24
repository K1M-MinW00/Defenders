using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StageUIController : MonoBehaviour
{
    [Header("Stage Info")]
    public TextMeshProUGUI stageInfoText;

    [Header("Currency")]
    public TextMeshProUGUI goldText;

    [Header("Population")]
    public TextMeshProUGUI populationText;

    [Header("Wave UI")]
    public Transform waveContainer;
    public GameObject waveItemPrefab;
    private List<Image> waveImages = new List<Image>();

    [Header("Timer")]
    public TextMeshProUGUI prepTimerText;

    [Header("Button")]
    public Button startButton;
    public Button summonButton;
    public Button increasePopButton;
    public GameObject reRollDropzone;

    [Header("Group")]
    [SerializeField] private GameObject defaultButtonsGroup;
    [SerializeField] private GameObject unitActionButtonsGroup;

    private StageManager stageManager;
    private PopulationManager population;
    private EconomyManager economy;

    public void Initialize(StageManager manager)
    {
        stageManager = manager;
        startButton.onClick.AddListener(OnClickStart);
        summonButton.onClick.AddListener(OnClickSummon);
        increasePopButton.onClick.AddListener(OnClickIncreasePop);

        BindEconomy();
        BindPopulation();
    }

    private void BindPopulation()
    {
        population = stageManager.Population;

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

    private void OnDestroy()
    {
        UnBindEconomy();
        UnBindPopulation();
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

    void OnClickStart()
    {
        stageManager.StartBattleEarly();
    }

    void OnClickSummon()
    {
        stageManager.TrySummonUnit();
    }
    void OnClickIncreasePop()
    {
        stageManager.TryIncreasePopulation();
    }

    private void UpdateGoldUI(int gold)
    {
        goldText.text = gold.ToString();
    }

    private void UpdatePopulationUI(int current, int max)
    {
        populationText.text = $"{current} / {max}";
    }
    public void SetStageInfo(string stageName, string stageId)
    {
        stageInfoText.text = $"{stageName} - {stageId}";
    }

    public void CreateWaveUI(List<WaveData> waves)
    {
        return;

        waveImages.Clear();

        foreach (Transform child in waveContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < waves.Count; i++)
        {
            GameObject obj = Instantiate(waveItemPrefab, waveContainer);
            Image img = obj.GetComponent<Image>();

            TextMeshProUGUI text =
                obj.GetComponentInChildren<TextMeshProUGUI>();

            text.text = (i + 1).ToString();

            img.color = GetWaveColor(waves[i].waveType);

            waveImages.Add(img);
        }
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
        prepTimerText.text = $"{time:F1}";
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
            reRollDropzone.SetActive(canReroll);
        }
    }
}
