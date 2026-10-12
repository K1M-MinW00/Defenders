using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public enum WaveMilestoneClaimFailure
{
    None,
    InvalidRequest,
    Locked,
    AlreadyClaimed,
    InvalidReward,
    SaveFailed,
}

public sealed class WaveMilestoneClaimResult
{
    public bool Succeeded => Failure == WaveMilestoneClaimFailure.None;
    public WaveMilestoneClaimFailure Failure { get; }
    public IReadOnlyList<RewardData> GrantedRewards { get; }

    private WaveMilestoneClaimResult(WaveMilestoneClaimFailure failure, IReadOnlyList<RewardData> rewards)
    {
        Failure = failure;
        GrantedRewards = rewards ?? System.Array.Empty<RewardData>();
    }

    public static WaveMilestoneClaimResult Success(IReadOnlyList<RewardData> rewards) => new(WaveMilestoneClaimFailure.None, rewards);
    public static WaveMilestoneClaimResult Fail(WaveMilestoneClaimFailure failure) => new(failure, null);
}

public sealed class WaveMilestoneRewardClaimUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private readonly IStageCatalog stages;

    public WaveMilestoneRewardClaimUseCase(IUserDataRepository repository, string userId, UserDataRoot userData, IStageCatalog stages)
    {
        this.repository = repository;
        this.userId = userId;
        this.userData = userData;
        this.stages = stages;
    }

    public Task<WaveMilestoneClaimResult> ClaimAsync(int requiredWave) => ClaimInternalAsync(requiredWave, false);
    public Task<WaveMilestoneClaimResult> ClaimAllAsync() => ClaimInternalAsync(0, true);

    private async Task<WaveMilestoneClaimResult> ClaimInternalAsync(int requiredWave, bool claimAll)
    {
        UserProgressData progress = userData?.Progress;
        StageDataSO stage = WaveMilestoneRewardRules.ResolveRewardStage(progress, stages);
        if (stage == null)
            return WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.InvalidRequest);

        List<WaveMilestoneReward> targets = stage.milestoneRewards
            .Where(milestone => milestone != null &&
                (claimAll || milestone.requiredWave == requiredWave) &&
                WaveMilestoneRewardRules.IsUnlocked(progress, stage.sector, stage.stage, milestone.requiredWave) &&
                !WaveMilestoneRewardRules.IsClaimed(progress.WaveRewardProgress, milestone.requiredWave))
            .ToList();

        if (targets.Count == 0)
        {
            WaveMilestoneReward requested = stage.milestoneRewards.FirstOrDefault(item => item?.requiredWave == requiredWave);
            if (!claimAll && requested != null && WaveMilestoneRewardRules.IsClaimed(progress.WaveRewardProgress, requiredWave))
                return WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.AlreadyClaimed);
            return WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.Locked);
        }

        List<RewardData> rewards = targets.SelectMany(target => target.rewards).ToList();
        RewardGrantResult grant = RewardGrantCalculator.Calculate(userData, rewards);
        if (!grant.Succeeded)
            return WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.InvalidReward);

        UserProgressData nextProgress = CopyProgress(progress);
        nextProgress.WaveRewardProgress.Sector = stage.sector;
        nextProgress.WaveRewardProgress.Stage = stage.stage;
        foreach (WaveMilestoneReward target in targets)
            nextProgress.WaveRewardProgress.ClaimedWaves.Add(target.requiredWave);
        nextProgress.WaveRewardProgress.ClaimedWaves = nextProgress.WaveRewardProgress.ClaimedWaves.Distinct().OrderBy(value => value).ToList();

        bool allClaimed = stage.milestoneRewards.All(milestone =>
            milestone != null && nextProgress.WaveRewardProgress.ClaimedWaves.Contains(milestone.requiredWave));
        if (allClaimed)
        {
            StageDataSO next = WaveMilestoneRewardRules.GetNextRewardStage(stage, stages);
            if (next != null)
            {
                nextProgress.WaveRewardProgress.Sector = next.sector;
                nextProgress.WaveRewardProgress.Stage = next.stage;
                nextProgress.WaveRewardProgress.ClaimedWaves.Clear();
            }
        }

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = grant.Resources,
                Inventory = grant.Inventory,
                Roster = grant.Roster,
                Profile = grant.Profile,
                Progress = nextProgress,
            });
        }
        catch
        {
            return WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.SaveFailed);
        }

        userData.Resource = grant.Resources;
        userData.Inventory = grant.Inventory;
        userData.Roster = grant.Roster;
        userData.Profile = grant.Profile;
        userData.Progress = nextProgress;
        return WaveMilestoneClaimResult.Success(rewards);
    }

    private static UserProgressData CopyProgress(UserProgressData source) => new()
    {
        CurrentSector = source.CurrentSector,
        CurrentStage = source.CurrentStage,
        BestWaveCleared = source.BestWaveCleared,
        WaveRewardProgress = new UserWaveRewardProgressData
        {
            Sector = source.WaveRewardProgress?.Sector ?? 1,
            Stage = source.WaveRewardProgress?.Stage ?? 1,
            ClaimedWaves = source.WaveRewardProgress?.ClaimedWaves?.ToList() ?? new List<int>(),
        },
    };
}
