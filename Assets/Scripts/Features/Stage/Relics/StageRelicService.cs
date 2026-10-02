using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class StageRelicService : MonoBehaviour
{
    private readonly List<StageRelicDefinition> acquired = new();
    private readonly HashSet<StageRelicId> acquiredIds = new();
    private StagePreparationService preparationService;
    private MonsterSpawner monsterSpawner;
    private StageRelicUI ui;

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
        UnitSummoner summoner = preparationService?.UnitSummoner;
        if (summoner == null)
            return;

        for (int i = 0; i < count; i++)
        {
            if (summoner.TryCreateRandomUnit(out UnitController unit, star))
                summoner.CommitSummonedUnit(unit);
        }
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
        if (preparationService?.UnitRoster != null)
            preparationService.UnitRoster.OnRosterChanged -= HandleRosterChanged;
    }
}
