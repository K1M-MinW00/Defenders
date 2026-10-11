using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Owns enhancement preview and material-selection state for one equipment item.
/// </summary>
public sealed class EquipmentEnhancementPresenter
{
    public readonly struct ViewState
    {
        public ViewState(
            EquipmentItemData equipment,
            EquipmentDataSO definition,
            string currentEffect,
            string nextEffect,
            int requiredExperience,
            int previewExperience,
            bool ready,
            IReadOnlyList<InventoryStackItem> materials)
        {
            Equipment = equipment;
            Definition = definition;
            CurrentEffect = currentEffect;
            NextEffect = nextEffect;
            RequiredExperience = requiredExperience;
            PreviewExperience = previewExperience;
            Ready = ready;
            Materials = materials;
        }

        public EquipmentItemData Equipment { get; }
        public EquipmentDataSO Definition { get; }
        public string CurrentEffect { get; }
        public string NextEffect { get; }
        public int RequiredExperience { get; }
        public int PreviewExperience { get; }
        public bool Ready { get; }
        public IReadOnlyList<InventoryStackItem> Materials { get; }
    }

    private readonly UnitEquipmentDataSource dataSource;
    private readonly EquipmentBalanceConfigSO balance;
    private readonly EquipmentEnhancementSelection selection;
    private EquipmentItemData equipment;

    public EquipmentEnhancementPresenter(
        UnitEquipmentDataSource source,
        IItemCatalog catalog,
        EquipmentBalanceConfigSO balanceConfig)
    {
        dataSource = source ?? throw new ArgumentNullException(nameof(source));
        balance = balanceConfig ?? throw new ArgumentNullException(nameof(balanceConfig));
        selection = new EquipmentEnhancementSelection(
            catalog ?? throw new ArgumentNullException(nameof(catalog)));
    }

    public EquipmentEnhancementSelection Selection => selection;
    public bool HasSelection => selection.Count > 0;

    public void Bind(EquipmentItemData target)
    {
        equipment = target;
        selection.Clear();
    }

    public void Clear()
    {
        equipment = null;
        selection.Clear();
    }

    public bool TryBuildState(out ViewState state)
    {
        EquipmentDataSO definition = dataSource.GetDefinition(equipment);
        if (equipment == null || definition == null)
        {
            state = default;
            return false;
        }

        int required = equipment.Level >= EquipmentBalanceConfigSO.MaxLevel
            ? 0
            : balance.GetRequiredExp(definition.Rarity, equipment.Level);
        int preview = equipment.EnhancementExp + selection.GetTotalExperience();
        bool ready = required > 0 && preview >= required;
        state = new ViewState(
            equipment,
            definition,
            FormatBaseEffect(definition, equipment.Level),
            FormatNextEffect(definition, equipment.Level),
            required,
            preview,
            ready,
            dataSource.GetEnhancementMaterials().ToList());
        return true;
    }

    public bool TryAddMaterial(string itemId, int ownedQuantity)
    {
        int needed = GetRemainingExperience();
        return needed > 0 && selection.TryAdd(itemId, ownedQuantity, needed);
    }

    public bool TryRemoveMaterial(string itemId) => selection.TryRemove(itemId);

    public bool TryAutoSelect()
    {
        int needed = GetRemainingExperience();
        return needed > 0 && selection.TryAutoSelect(
            dataSource.GetEnhancementMaterials(),
            needed);
    }

    public void ResetSelection() => selection.Clear();

    public IReadOnlyDictionary<string, int> CreateSelectionSnapshot()
        => selection.CreateSnapshot();

    private int GetRemainingExperience()
    {
        EquipmentDataSO definition = dataSource.GetDefinition(equipment);
        if (equipment == null || definition == null ||
            equipment.Level >= EquipmentBalanceConfigSO.MaxLevel)
            return 0;

        return Math.Max(
            0,
            balance.GetRequiredExp(definition.Rarity, equipment.Level) -
            equipment.EnhancementExp);
    }

    private string FormatBaseEffect(EquipmentDataSO definition, int level)
    {
        string stat = definition.BaseStatType == EquipmentStatType.MaxHp
            ? "최대 체력"
            : "공격력";
        return $"{stat}  +{balance.GetBaseStatValue(definition, level):N0}";
    }

    private string FormatNextEffect(EquipmentDataSO definition, int level)
        => $"+{balance.GetBaseStatValue(definition, level + 1):N0}";
}
