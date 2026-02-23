using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    public StageUIController stageUI;

    [Header("Stage Data")]
    public StageData currentStageData;
    public EconomyConfig economyConfig;


    [Header("Reference")]
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private UnitSummoner unitSummoner;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private FusionService fusionService;

    [Header("Prepare Settings")]
    public float prepareDuration = 5f;
    private float prepareTimer;
    private int currentWaveIndex = 0;


    public TilemapPlacementArea placementArea;
    public PlacementController placementController;


    public StageState CurrentState { get; private set; }

    private WaveData CurrentWave
    {
        get
        {
            if (currentWaveIndex >= currentStageData.waves.Count)
                return null;
            
            return currentStageData.waves[currentWaveIndex];
        }
    }


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeStage();

        stageUI.Initialize(this);

        stageUI.SetStageInfo(currentStageData.stageName, currentStageData.stageId.ToString());
        stageUI.CreateWaveUI(currentStageData.waves);
    }

    private void InitializeStage()
    {
        currentWaveIndex = 0;
        EconomyManager.Instance.Init(economyConfig);

        PrepareMonsters();

        monsterSpawner.Init(objectPool);
        monsterSpawner.OnWaveCompleted += HandleWaveCompleted;

        // ToDo : 재화 설정
        StartPreparePhase();
    }

    private void PrepareMonsters()
    {
        HashSet<string> monsterIds = new();

        foreach(var wave in currentStageData.waves)
        {
            foreach(var subWave in wave.subWaves)
            {
                foreach (var group in subWave.spawnGroups)
                    monsterIds.Add(group.monsterId);
            }
        }

        foreach (var id in monsterIds)
        {
            GameObject prefab = Resources.Load<GameObject>($"Monsters/{id}");

            if (prefab == null)
            {
                Debug.LogError($"Monster Prefab not found : {id}");
                continue;
            }

            // 기본 20마리 프리워밍 (추후 계산 기반으로 변경 가능)
            objectPool.Prewarm(id, prefab, 20);
        }
    }

    private void StartPreparePhase()
    {
        CurrentState = StageState.Preparing;
        stageUI.gameObject.SetActive(true);
        prepareTimer = prepareDuration;

        // 타이머 UI 설정
        placementController?.EnablePlacement(true);
        placementArea?.SetVisible(true);

        StartCoroutine(PrepareRoutine());
    }

    private IEnumerator PrepareRoutine()
    {
        while(prepareTimer > 0f)
        {
            prepareTimer -= Time.deltaTime;
            stageUI.UpdatePrepTimer(prepareTimer);

            yield return null;
        }

        StartCombatPhase();
    }

    private void StartCombatPhase()
    {
        stageUI.gameObject.SetActive(false);
        placementController?.EnablePlacement(false);
        placementArea?.SetVisible(false);


        if (CurrentWave == null)
        {
            Debug.LogWarning("Stage Data , Wave Data Null");
            return;
        }

        StartWave();
        
    }
    public void StartBattleEarly()
    {
        StopAllCoroutines();
        StartCombatPhase();
    }

    private void StartWave()
    {
        CurrentState = StageState.Combat;

        stageUI.UpdateCurrentWave(currentWaveIndex);
        monsterSpawner.StartWave(CurrentWave);
    }

    private void HandleWaveCompleted()
    {
        currentWaveIndex++;

        if (CurrentWave == null)
            StartStageClear();

        else
            StartPreparePhase();
    }

    private void StartStageClear()
    {
        CurrentState = StageState.StageClear;

        Debug.Log("Stage Clear!");
    }

    public void TrySummonUnit()
    {
        if (CurrentState != StageState.Preparing)
            return;

        if (!EconomyManager.Instance.TrySummonUnit())
            return;

        unitSummoner.SummonRandomUnit();
    }

    public void TrySellUnit(PlayerCharacter unit)
    {
        if (CurrentState != StageState.Preparing)
            return;

        if (unit == null)
            return;

        var inst = unit.GetComponent<UnitInstance>();
        if (inst == null)
            return;

        unitRoster?.Unregister(inst);

        EconomyManager.Instance.SellUnit(inst.Star);
        Destroy(unit.gameObject);
    }

    public void TryRerollUnit(PlayerCharacter unit)
    {
        if (CurrentState != StageState.Preparing)
            return;

        if (unit == null)
            return;

        var inst = unit.GetComponent<UnitInstance>();
        if (inst == null)
            return;

        if (!EconomyManager.Instance.TryReroll())
            return;

        Vector3 pos = unit.transform.position;
        Transform parent = unit.transform.parent;

        unitRoster?.Unregister(inst);
        Destroy(unit.gameObject);

        unitSummoner.SummonRandomUnit();
    }
}

public enum StageState
{
    None,
    Preparing,
    Combat,
    Reward,
    StageClear,
    StageFail
}