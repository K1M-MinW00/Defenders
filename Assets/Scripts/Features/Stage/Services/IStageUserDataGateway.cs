using System.Threading.Tasks;

public interface IStageUserDataGateway
{
    UserDataRoot UserData { get; }
    RosterService RosterService { get; }
    bool IsLoaded { get; }

    Task<bool> SaveUserProgressAsync(UserProgressData progress);
    Task<bool> RefundStageEntryFuelAsync(int amount);
    Task<StageOutcomeResult> CompleteStageAsync(
        StageDataSO stage,
        int fusionCount = 0,
        int fourStarFusionCount = 0);
    Task<StageOutcomeResult> FailStageAsync(
        StageDataSO stage,
        int clearedWaveCount,
        int entryFuelCost,
        int fusionCount = 0,
        int fourStarFusionCount = 0);
}
