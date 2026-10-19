using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Owns equipment replacement candidates and the pending selection.
/// </summary>
public sealed class EquipmentChangePresenter
{
    public readonly struct ViewState
    {
        public ViewState(
            EquipmentType equipmentType,
            EquipmentItemData equipped,
            EquipmentItemData selected,
            IReadOnlyList<EquipmentItemData> candidates,
            string selectedEffect,
            bool canEquip)
        {
            EquipmentType = equipmentType;
            Equipped = equipped;
            Selected = selected;
            Candidates = candidates;
            SelectedEffect = selectedEffect;
            CanEquip = canEquip;
        }

        public EquipmentType EquipmentType { get; }
        public EquipmentItemData Equipped { get; }
        public EquipmentItemData Selected { get; }
        public IReadOnlyList<EquipmentItemData> Candidates { get; }
        public string SelectedEffect { get; }
        public bool CanEquip { get; }
    }

    private readonly UnitEquipmentDataSource dataSource;
    private readonly EquipmentBalanceConfigSO balance;
    private string unitId;
    private EquipmentType equipmentType;
    private EquipmentItemData equipped;
    private EquipmentItemData selected;

    public EquipmentChangePresenter(
        UnitEquipmentDataSource source,
        EquipmentBalanceConfigSO balanceConfig)
    {
        dataSource = source ?? throw new ArgumentNullException(nameof(source));
        balance = balanceConfig ?? throw new ArgumentNullException(nameof(balanceConfig));
    }

    public EquipmentItemData Selected => selected;

    public void Bind(string targetUnitId, EquipmentType type)
    {
        unitId = targetUnitId;
        equipmentType = type;
        equipped = dataSource.GetEquipped(unitId, equipmentType);
        selected = equipped;
    }

    public void Clear()
    {
        unitId = null;
        equipmentType = EquipmentType.None;
        equipped = null;
        selected = null;
    }

    public bool Select(EquipmentItemData item)
    {
        if (item == null ||
            dataSource.GetDefinition(item)?.EquipmentType != equipmentType)
            return false;

        selected = item;
        return true;
    }

    public ViewState BuildState(bool busy)
    {
        IReadOnlyList<EquipmentItemData> candidates =
            dataSource.GetUnequipped(equipmentType).ToList();
        bool canEquip = !busy && selected != null &&
            selected.UniqueId != equipped?.UniqueId;
        return new ViewState(
            equipmentType,
            equipped,
            selected,
            candidates,
            FormatBaseEffect(selected),
            canEquip);
    }

    private string FormatBaseEffect(EquipmentItemData item)
    {
        EquipmentDataSO definition = dataSource.GetDefinition(item);
        if (definition == null)
            return string.Empty;

        return EquipmentLocalizedText.StatValue(
            definition.BaseStatType,
            balance.GetBaseStatValue(definition, item.Level));
    }
}
