using System;
using System.Linq;
using System.Threading.Tasks;

public sealed class UnitLimitBreakUseCase
{
    public const int MaxLimitBreak = 5;

    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public UnitLimitBreakUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public async Task<LimitBreakUnitResult> ExecuteAsync(LimitBreakUnitCommand command)
    {
        if (isExecuting || command == null || string.IsNullOrWhiteSpace(command.UnitId))
            return LimitBreakUnitResult.Fail(LimitBreakUnitFailure.InvalidRequest);

        UserUnitData currentUnit = userData.Roster?.OwnedUnits?
            .FirstOrDefault(unit => unit != null && unit.UnitId == command.UnitId);

        if (currentUnit == null)
            return LimitBreakUnitResult.Fail(LimitBreakUnitFailure.UnitNotFound);

        if (currentUnit.LimitBreak >= MaxLimitBreak)
            return LimitBreakUnitResult.Fail(LimitBreakUnitFailure.MaxLimitBreak);

        if (currentUnit.DuplicateCount <= 0)
            return LimitBreakUnitResult.Fail(LimitBreakUnitFailure.InsufficientDuplicates);

        UserRosterData nextRoster = UserDataCloner.Copy(userData.Roster);
        UserUnitData nextUnit = nextRoster.OwnedUnits
            .First(unit => unit != null && unit.UnitId == command.UnitId);

        nextUnit.DuplicateCount--;
        nextUnit.LimitBreak++;

        isExecuting = true;

        try
        {
            await repository.SaveRosterAsync(userId, nextRoster);
        }
        catch
        {
            return LimitBreakUnitResult.Fail(LimitBreakUnitFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Roster = nextRoster;

        return LimitBreakUnitResult.Success(nextUnit.LimitBreak, nextUnit.DuplicateCount);
    }
}
