using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class StageRelicService : MonoBehaviour
{
    private readonly List<StageRelicDefinition> acquired = new();
    private readonly HashSet<StageRelicId> acquiredIds = new();
    private readonly Queue<UnitSpawnRequest> pendingUnitSpawns = new();
    private StagePreparationService preparationService;
    private MonsterSpawner monsterSpawner;
    private StageRelicUI ui;
    private Coroutine unitSpawnRoutine;

    [SerializeField, Min(0f)] private float delayBetweenRelicUnits = 0.5f;

    public void Initialize(
        StagePreparationService preparation,
        MonsterSpawner spawner,
        StageRelicUI relicUI)
    {
        preparationService = preparation;
        monsterSpawner = spawner;
        if (preparationService?.UnitRoster != null)
            preparationService.UnitRoster.OnRosterChanged += HandleRosterChanged;

        ui = relicUI;
        ui?.Initialize();
        ui?.SetOwnedRelics(acquired);
    }

    public async Task PresentChoiceAsync()
    {
        List<StageRelicDefinition> candidates = BuildCandidates();
        if (candidates.Count == 0 || ui == null)
            return;

        int firstIndex = Random.Range(0, candidates.Count);
        StageRelicDefinition first = candidates[firstIndex];
        candidates.RemoveAt(firstIndex);
        StageRelicDefinition second = candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : null;

        StageRelicDefinition selected = await ui.ShowChoiceAsync(first, second);
        if (selected != null)
            Acquire(selected);
    }

    private List<StageRelicDefinition> BuildCandidates()
    {
        List<StageRelicDefinition> result = new();
        foreach (StageRelicDefinition definition in StageRelicCatalog.All)
        {
            if (!acquiredIds.Contains(definition.Id))
                result.Add(definition);
        }
        return result;
    }

    private void Acquire(StageRelicDefinition relic)
    {
        if (relic == null || !acquiredIds.Add(relic.Id))
            return;

        acquired.Add(relic);
        ApplyImmediateEffect(relic.Id);
        ApplyPersistentEffectsToAllUnits();
        ui?.SetOwnedRelics(acquired);
    }

    private void ApplyImmediateEffect(StageRelicId id)
    {
        switch (id)
        {
            case StageRelicId.FrostShackles:
                monsterSpawner?.SetStageMoveSpeedMultiplier(0.8f);
                break;
            case StageRelicId.ReinforcementFlare:
                SpawnUnits(Random.Range(1, 6), 1);
                break;
            case StageRelicId.EliteSummons:
                SpawnUnits(1, 2);
                break;
            case StageRelicId.AbundantVein:
                preparationService?.EconomyManager?.TryAddGold(Random.Range(10, 31));
                break;
            case StageRelicId.DiceOfFate:
                preparationService?.GrantFreeRerolls(10);
                break;
        }
    }

    private void SpawnUnits(int count, int star)
    {
        if (count <= 0 || preparationService?.UnitSummoner == null)
            return;

        pendingUnitSpawns.Enqueue(new UnitSpawnRequest(count, star));
        if (unitSpawnRoutine == null)
            unitSpawnRoutine = StartCoroutine(ProcessUnitSpawnQueue());
    }

    private IEnumerator ProcessUnitSpawnQueue()
    {
        while (pendingUnitSpawns.Count > 0)
        {
            UnitSpawnRequest request = pendingUnitSpawns.Dequeue();
            UnitSummoner summoner = preparationService?.UnitSummoner;
            if (summoner == null)
                continue;

            for (int i = 0; i < request.Count; i++)
            {
                if (!summoner.TryCreateRandomUnit(out UnitController unit, request.Star))
                    continue;

                summoner.CommitSummonedUnit(unit);

                bool hasAnotherUnit = i < request.Count - 1 || pendingUnitSpawns.Count > 0;
                if (hasAnotherUnit && delayBetweenRelicUnits > 0f)
                    yield return new WaitForSecondsRealtime(delayBetweenRelicUnits);
            }
        }

        unitSpawnRoutine = null;
    }

    private void HandleRosterChanged()
    {
        ApplyPersistentEffectsToAllUnits();
    }

    private void ApplyPersistentEffectsToAllUnits()
    {
        UnitRoster roster = preparationService?.UnitRoster;
        if (roster == null)
            return;

        foreach (UnitController unit in roster.Units)
        {
            if (unit == null || unit.BuffController == null)
                continue;

            if (acquiredIds.Contains(StageRelicId.WarlordsSeal))
                ApplyBuff(unit, "relic_attack", StatType.Attack, BuffModifyType.Percent, 0.15f);
            if (acquiredIds.Contains(StageRelicId.HawkeyeCrystal))
                ApplyBuff(unit, "relic_crit", StatType.CritChance, BuffModifyType.Flat, 0.2f);
            if (acquiredIds.Contains(StageRelicId.BerserkersSpring))
                ApplyBuff(unit, "relic_attack_speed", StatType.AttackPerSec, BuffModifyType.Percent, 0.2f);
        }
    }

    private static void ApplyBuff(UnitController unit, string id, StatType stat, BuffModifyType type, float value)
    {
        unit.BuffController.ApplyOrRefreshBuff(new BuffApplication(
            id, stat, type, value, BuffDurationType.UntilStageEnd));
    }

    private void OnDestroy()
    {
        StopUnitSpawnQueue();

        if (preparationService?.UnitRoster != null)
            preparationService.UnitRoster.OnRosterChanged -= HandleRosterChanged;
    }

    private void OnDisable()
    {
        StopUnitSpawnQueue();
    }

    private void StopUnitSpawnQueue()
    {
        if (unitSpawnRoutine != null)
            StopCoroutine(unitSpawnRoutine);
        unitSpawnRoutine = null;
        pendingUnitSpawns.Clear();
    }

    private readonly struct UnitSpawnRequest
    {
        public UnitSpawnRequest(int count, int star)
        {
            Count = count;
            Star = star;
        }

        public int Count { get; }
        public int Star { get; }
    }
}
