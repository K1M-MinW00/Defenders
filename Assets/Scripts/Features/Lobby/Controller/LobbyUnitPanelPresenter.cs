using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LobbyUnitPanelPresenter
{
    private readonly RosterService rosterService;

    public LobbyUnitPanelPresenter(RosterService rosterService)
    {
        this.rosterService = rosterService ?? throw new ArgumentNullException(nameof(rosterService));
    }

    public LobbyUnitPanelViewState Build()
    {
        IReadOnlyList<UserUnitData> ownedUnits = rosterService.GetOwnedUnits();
        IReadOnlyList<string> selectedUnitIds = rosterService.GetSelectedUnitIds();
        Dictionary<string, UserUnitData> ownedUnitMap = BuildOwnedUnitMap(ownedUnits);
        HashSet<string> selectedSet = new();
        List<LobbyUnitViewModel> selectedUnits = new();
        List<LobbyUnitViewModel> availableUnits = new();

        BuildSelectedUnits(selectedUnitIds, ownedUnitMap, selectedSet, selectedUnits);
        BuildAvailableUnits(ownedUnitMap, selectedSet, availableUnits);

        return new LobbyUnitPanelViewState(selectedUnits, availableUnits);
    }

    private static Dictionary<string, UserUnitData> BuildOwnedUnitMap(IReadOnlyList<UserUnitData> ownedUnits)
    {
        Dictionary<string, UserUnitData> ownedUnitMap = new();

        foreach (UserUnitData userUnit in ownedUnits)
        {
            if (userUnit == null || string.IsNullOrWhiteSpace(userUnit.UnitId))
                continue;

            ownedUnitMap.TryAdd(userUnit.UnitId, userUnit);
        }

        return ownedUnitMap;
    }

    private static void BuildSelectedUnits(
        IReadOnlyList<string> selectedUnitIds,
        IReadOnlyDictionary<string, UserUnitData> ownedUnitMap,
        HashSet<string> selectedSet,
        ICollection<LobbyUnitViewModel> selectedUnits)
    {
        foreach (string unitId in selectedUnitIds)
        {
            if (string.IsNullOrWhiteSpace(unitId) || !selectedSet.Add(unitId))
                continue;

            UnitDataSO definition = GameConfig.Units.Get(unitId);
            if (definition == null)
            {
                Debug.LogWarning($"[LobbyUnitPanelPresenter] Unit definition not found: {unitId}");
                continue;
            }

            ownedUnitMap.TryGetValue(unitId, out UserUnitData userUnit);
            selectedUnits.Add(CreateViewModel(definition, userUnit, true));
        }
    }

    private static void BuildAvailableUnits(
        IReadOnlyDictionary<string, UserUnitData> ownedUnitMap,
        HashSet<string> selectedSet,
        ICollection<LobbyUnitViewModel> availableUnits)
    {
        foreach (UnitDataSO definition in GameConfig.Units.GetAll())
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.unitId) ||
                selectedSet.Contains(definition.unitId))
            {
                continue;
            }

            ownedUnitMap.TryGetValue(definition.unitId, out UserUnitData userUnit);
            availableUnits.Add(CreateViewModel(definition, userUnit, false));
        }
    }

    private static LobbyUnitViewModel CreateViewModel(
        UnitDataSO definition,
        UserUnitData userUnit,
        bool isSelected)
    {
        return new LobbyUnitViewModel
        {
            UnitId = definition.unitId,
            Icon = definition.icon,
            Rarity = definition.rarity,
            Level = userUnit?.Level ?? 0,
            Promotion = userUnit?.Promotion ?? 0,
            LimitBreak = userUnit?.LimitBreak ?? 0,
            IsOwned = userUnit != null,
            IsSelected = isSelected,
        };
    }
}
