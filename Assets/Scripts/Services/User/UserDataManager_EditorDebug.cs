#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class UserDataManager
{
    public async Task<RewardGrantResult> GrantEditorTestRewardsAsync(
        IReadOnlyCollection<RewardData> rewards,
        bool persistToFirebase = false)
    {
        if (!IsLoaded || rewards == null || rewards.Count == 0)
            return RewardGrantResult.Fail(RewardGrantFailure.InvalidData);

        return await RunSerializedMutationAsync(async () =>
        {
            RewardGrantResult result = RewardGrantCalculator.Calculate(UserData, rewards);
            if (!result.Succeeded)
                return result;

            if (persistToFirebase)
            {
                try
                {
                    await repository.SaveSectionsAsync(CurrentUserId, new UserDataUpdate
                    {
                        Resources = result.Resources,
                        Inventory = result.Inventory,
                        Roster = result.Roster,
                        Profile = result.Profile,
                    });
                }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception);
                    return RewardGrantResult.Fail(RewardGrantFailure.InvalidData);
                }
            }

            UserData.Resource = result.Resources;
            UserData.Inventory = result.Inventory;
            UserData.Roster = result.Roster;
            UserData.Profile = result.Profile;
            RaiseResourceUpdated();
            RaiseInventoryUpdated();
            RaiseRosterUpdated();
            RaiseProfileUpdated();
            Debug.Log(persistToFirebase
                ? "[Test Reward Grant] Applied and saved to Firebase."
                : "[Test Reward Grant] Applied to this Play Mode session only.");
            return result;
        });
    }

}
#endif
