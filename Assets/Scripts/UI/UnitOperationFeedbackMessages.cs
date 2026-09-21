public static class UnitOperationFeedbackMessages
{
    public static string Get(TrainUnitFailure failure)
    {
        return failure switch
        {
            TrainUnitFailure.UnitNotFound => "유닛 정보를 찾을 수 없습니다.",
            TrainUnitFailure.MaxLevel => "이미 최고 레벨입니다.",
            TrainUnitFailure.InsufficientGold => "골드가 부족합니다.",
            TrainUnitFailure.InsufficientMaterials => "훈련 재료가 부족합니다.",
            TrainUnitFailure.SaveFailed => "저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "훈련을 진행할 수 없습니다.",
        };
    }

    public static string Get(PromoteUnitFailure failure)
    {
        return failure switch
        {
            PromoteUnitFailure.UnitNotFound => "유닛 정보를 찾을 수 없습니다.",
            PromoteUnitFailure.MaxPromotion => "이미 최고 진급 단계입니다.",
            PromoteUnitFailure.InvalidCost => "진급 비용 정보가 올바르지 않습니다.",
            PromoteUnitFailure.InsufficientMaterials => "진급 재료가 부족합니다.",
            PromoteUnitFailure.SaveFailed => "저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "진급을 진행할 수 없습니다.",
        };
    }

    public static string Get(LimitBreakUnitFailure failure)
    {
        return failure switch
        {
            LimitBreakUnitFailure.UnitNotFound => "유닛 정보를 찾을 수 없습니다.",
            LimitBreakUnitFailure.MaxLimitBreak => "이미 최고 한계돌파 단계입니다.",
            LimitBreakUnitFailure.InsufficientDuplicates => "한계돌파 재료가 부족합니다.",
            LimitBreakUnitFailure.SaveFailed => "저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "한계돌파를 진행할 수 없습니다.",
        };
    }

    public static string Get(FormationChangeFailure failure)
    {
        return failure switch
        {
            FormationChangeFailure.InvalidRoster => "현재 전투 명단을 불러올 수 없습니다.",
            FormationChangeFailure.UnitNotOwned => "보유하지 않은 유닛입니다.",
            FormationChangeFailure.UnitNotSelected => "선택 명단에 없는 유닛입니다.",
            FormationChangeFailure.UnitAlreadySelected => "이미 선택된 유닛입니다.",
            FormationChangeFailure.SaveFailed => "명단 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "전투 명단을 변경할 수 없습니다.",
        };
    }
}
