using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class UnitFormationUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public UnitFormationUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public async Task<FormationChangeResult> ExecuteAsync(FormationChangeCommand command)
    {
        if (isExecuting || command == null ||
            string.IsNullOrWhiteSpace(command.CurrentUnitId) ||
            string.IsNullOrWhiteSpace(command.TargetUnitId) ||
            command.CurrentUnitId == command.TargetUnitId)
        {
            return FormationChangeResult.Fail(FormationChangeFailure.InvalidRequest);
        }

        UserRosterData currentRoster = userData.Roster;

        if (!IsValidRoster(currentRoster))
            return FormationChangeResult.Fail(FormationChangeFailure.InvalidRoster);

        if (!IsOwned(currentRoster, command.CurrentUnitId) || !IsOwned(currentRoster, command.TargetUnitId))
            return FormationChangeResult.Fail(FormationChangeFailure.UnitNotOwned);

        UserRosterData nextRoster = UserDataCloner.Copy(currentRoster);
        FormationChangeFailure failure = command.Type switch
        {
            FormationChangeType.SwapPositions => SwapPositions(nextRoster, command),
            FormationChangeType.ReplaceUnit => ReplaceUnit(nextRoster, command),
            _ => FormationChangeFailure.InvalidRequest,
        };

        if (failure != FormationChangeFailure.None)
            return FormationChangeResult.Fail(failure);

        isExecuting = true;

        try
        {
            await repository.SaveRosterAsync(userId, nextRoster);
        }
        catch
        {
            return FormationChangeResult.Fail(FormationChangeFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Roster = nextRoster;
        return FormationChangeResult.Success();
    }

    private static FormationChangeFailure SwapPositions(
        UserRosterData roster,
        FormationChangeCommand command)
    {
        int currentIndex = roster.SelectedUnitIds.IndexOf(command.CurrentUnitId);
        int targetIndex = roster.SelectedUnitIds.IndexOf(command.TargetUnitId);

        if (currentIndex < 0 || targetIndex < 0)
            return FormationChangeFailure.UnitNotSelected;

        (roster.SelectedUnitIds[currentIndex], roster.SelectedUnitIds[targetIndex]) =
            (roster.SelectedUnitIds[targetIndex], roster.SelectedUnitIds[currentIndex]);
        return FormationChangeFailure.None;
    }

    private static FormationChangeFailure ReplaceUnit(
        UserRosterData roster,
        FormationChangeCommand command)
    {
        int currentIndex = roster.SelectedUnitIds.IndexOf(command.CurrentUnitId);

        if (currentIndex < 0)
            return FormationChangeFailure.UnitNotSelected;

        if (roster.SelectedUnitIds.Contains(command.TargetUnitId))
            return FormationChangeFailure.UnitAlreadySelected;

        roster.SelectedUnitIds[currentIndex] = command.TargetUnitId;
        return FormationChangeFailure.None;
    }

    private static bool IsValidRoster(UserRosterData roster)
    {
        if (roster?.OwnedUnits == null || roster.SelectedUnitIds == null ||
            roster.SelectedUnitIds.Any(string.IsNullOrWhiteSpace))
        {
            return false;
        }

        return roster.SelectedUnitIds.Distinct().Count() == roster.SelectedUnitIds.Count;
    }

    private static bool IsOwned(UserRosterData roster, string unitId)
    {
        return roster.OwnedUnits.Any(unit => unit != null && unit.UnitId == unitId);
    }
}
