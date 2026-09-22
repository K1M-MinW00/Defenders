using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task<TrainUnitResult> TrainUnitAsync(TrainUnitCommand command)
    {
        if (UnitTrainingUseCase == null)
            return TrainUnitResult.Fail(TrainUnitFailure.InvalidRequest);

        TrainUnitResult result = await UnitTrainingUseCase.ExecuteAsync(command);
        if (!result.Succeeded)
            return result;

        RaiseResourceUpdated();
        RaiseInventoryUpdated();
        RaiseRosterUpdated();
        return result;
    }

    public async Task<PromoteUnitResult> PromoteUnitAsync(PromoteUnitCommand command)
    {
        if (UnitPromotionUseCase == null)
            return PromoteUnitResult.Fail(PromoteUnitFailure.InvalidRequest);

        PromoteUnitResult result = await UnitPromotionUseCase.ExecuteAsync(command);
        if (!result.Succeeded)
            return result;

        RaiseInventoryUpdated();
        RaiseRosterUpdated();
        return result;
    }

    public async Task<LimitBreakUnitResult> LimitBreakUnitAsync(LimitBreakUnitCommand command)
    {
        if (UnitLimitBreakUseCase == null)
            return LimitBreakUnitResult.Fail(LimitBreakUnitFailure.InvalidRequest);

        LimitBreakUnitResult result = await UnitLimitBreakUseCase.ExecuteAsync(command);
        if (!result.Succeeded)
            return result;

        RaiseRosterUpdated();
        return result;
    }

    public async Task<FormationChangeResult> ChangeFormationAsync(FormationChangeCommand command)
    {
        if (UnitFormationUseCase == null)
            return FormationChangeResult.Fail(FormationChangeFailure.InvalidRequest);

        FormationChangeResult result = await UnitFormationUseCase.ExecuteAsync(command);
        if (result.Succeeded)
            RaiseRosterUpdated();

        return result;
    }
}
