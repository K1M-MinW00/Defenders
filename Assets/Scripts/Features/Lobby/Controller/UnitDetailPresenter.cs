using System;

public sealed class UnitDetailPresenter
{
    private readonly RosterService rosterService;
    private readonly LabBonusSnapshot labBonuses;

    public UnitDetailPresenter(RosterService rosterService, LabBonusSnapshot labBonuses)
    {
        this.rosterService = rosterService ?? throw new ArgumentNullException(nameof(rosterService));
        this.labBonuses = labBonuses ?? LabBonusSnapshot.Empty;
    }

    public UnitDetailViewState Build(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return null;

        UnitDataSO definition = GameConfig.Units.Get(unitId);
        UserUnitData userUnit = rosterService.GetUnit(unitId);

        if (definition == null || userUnit == null)
            return null;

        UnitStats stats = UnitStatCalculator.Calculate(
            definition,
            userUnit,
            rosterService.GetInventory(),
            labBonuses: labBonuses);

        return new UnitDetailViewState
        {
            UnitId = unitId,
            Definition = definition,
            Icon = definition.icon,
            DisplayName = definition.displayName,
            Rarity = definition.rarity,
            Level = userUnit.Level,
            Attack = stats.Attack,
            MaxHp = stats.MaxHp,
            Promotion = userUnit.Promotion,
            LimitBreak = userUnit.LimitBreak,
            ActiveSkill = definition.activeSkill,
            PassiveSkill = definition.passiveSkill,
        };
    }
}
