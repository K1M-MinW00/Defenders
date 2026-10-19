public static class UnitOperationFeedbackMessages
{
    public static string Get(TrainUnitFailure failure)
    {
        return failure switch
        {
            TrainUnitFailure.UnitNotFound => Localize("unit.not_found"),
            TrainUnitFailure.MaxLevel => Localize("unit.train.max_level"),
            TrainUnitFailure.InsufficientGold => Localize("unit.train.insufficient_gold"),
            TrainUnitFailure.InsufficientMaterials => Localize("unit.train.insufficient_materials"),
            TrainUnitFailure.SaveFailed => Localize("common.save_failed"),
            _ => Localize("unit.train.unavailable"),
        };
    }

    public static string Get(PromoteUnitFailure failure)
    {
        return failure switch
        {
            PromoteUnitFailure.UnitNotFound => Localize("unit.not_found"),
            PromoteUnitFailure.MaxPromotion => Localize("unit.promote.max_level"),
            PromoteUnitFailure.InvalidCost => Localize("unit.promote.invalid_cost"),
            PromoteUnitFailure.InsufficientMaterials => Localize("unit.promote.insufficient_materials"),
            PromoteUnitFailure.SaveFailed => Localize("common.save_failed"),
            _ => Localize("unit.promote.unavailable"),
        };
    }

    public static string Get(LimitBreakUnitFailure failure)
    {
        return failure switch
        {
            LimitBreakUnitFailure.UnitNotFound => Localize("unit.not_found"),
            LimitBreakUnitFailure.MaxLimitBreak => Localize("unit.limit_break.max_level"),
            LimitBreakUnitFailure.InsufficientDuplicates => Localize("unit.limit_break.insufficient_materials"),
            LimitBreakUnitFailure.SaveFailed => Localize("common.save_failed"),
            _ => Localize("unit.limit_break.unavailable"),
        };
    }

    public static string Get(FormationChangeFailure failure)
    {
        return failure switch
        {
            FormationChangeFailure.InvalidRoster => Localize("formation.invalid_roster"),
            FormationChangeFailure.UnitNotOwned => Localize("formation.unit_not_owned"),
            FormationChangeFailure.UnitNotSelected => Localize("formation.unit_not_selected"),
            FormationChangeFailure.UnitAlreadySelected => Localize("formation.unit_already_selected"),
            FormationChangeFailure.SaveFailed => Localize("formation.save_failed"),
            _ => Localize("formation.unavailable"),
        };
    }

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.SystemMessages, key);
}
