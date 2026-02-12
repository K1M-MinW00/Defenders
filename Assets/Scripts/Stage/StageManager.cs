using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Stage Data")]
    public StageData currentStageData;

    [Header("Reference")]
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private MonsterSpawner monsterSpawner;

    [Header("Prepare Settings")]
    public float prepareDuration = 5f;
    private float prepareTimer;
    private int currentWaveIndex = 0;


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
    }

    private void InitializeStage()
    {
        currentWaveIndex = 0;

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

        prepareTimer = prepareDuration;

        // 타이머 UI 설정

        StartCoroutine(PrepareRoutine());
    }

    private IEnumerator PrepareRoutine()
    {
        while(prepareTimer >0f)
        {
            prepareTimer -= Time.deltaTime;

            // UI 업데이트

            yield return null;
        }

        StartCombatPhase();
    }

    private void StartCombatPhase()
    {

        // UI Show Prepare UI

        if(CurrentWave == null)
        {
            // StartStageClear();
            return;
        }

        CurrentState = StageState.Combat;
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