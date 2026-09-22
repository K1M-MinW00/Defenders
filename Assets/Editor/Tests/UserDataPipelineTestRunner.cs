using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class UserDataPipelineTestRunner
{
    private const int TestCount = 43;

    [MenuItem("Tools/Tests/Run User Data Pipeline Tests")]
    public static async void RunFromMenu()
    {
        try
        {
            await RunAllAsync();
            Debug.Log($"[UserDataPipelineTests] All {TestCount} tests passed.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public static void RunFromCommandLine()
    {
        try
        {
            RunAllAsync().GetAwaiter().GetResult();
            Debug.Log($"[UserDataPipelineTests] All {TestCount} tests passed.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static async Task RunAllAsync()
    {
        GameConfig.Initialize();
        UserDataPipelineTests tests = new();

        await tests.LoadOrCreateAsync_CreatesDefaultData_WhenUserDoesNotExist();
        await tests.LoadOrCreateAsync_DoesNotSave_WhenCurrentDataNeedsNoChanges();
        await tests.LoadOrCreateAsync_SavesAll_WhenSchemaMigrationRuns();
        await tests.LoadOrCreateAsync_SavesOnlyResources_WhenFuelRecovers();
        await tests.ProfileUpdate_CommitsCopy_AfterSaveSucceeds();
        await tests.ProfileUpdate_KeepsOriginalData_WhenSaveFails();
        await tests.NicknameUpdate_ChargesGem_AfterFreeChange();
        await tests.NicknameUpdate_RejectsInsufficientGem();
        tests.Normalize_AssignsDefaultProfileIcon_WhenMissing();
        await tests.ProfileUpdate_RejectsUnownedIcon();
        tests.LobbyBattlePresenter_BuildsStateAndStageEntry();
        tests.LobbyBattlePresenter_RejectsInvalidFormation();

        StartupFlowTests startupTests = new();
        await startupTests.RunAsync_CompletesStagesInOrder();
        await startupTests.RunAsync_StopsWhenLoginFails();
        await startupTests.RunAsync_ReturnsUnexpectedError();

        LobbyTabSelectionTests lobbyTabTests = new();
        lobbyTabTests.TrySelect_RejectsInvalidAndDuplicateSelection();
        lobbyTabTests.TrySelect_AllowsChangingSelection();

        FuelPanelPresenterTests fuelPanelTests = new();
        fuelPanelTests.Build_CreatesFuelAndAdState();
        fuelPanelTests.Build_ResetsExpiredAdCountAndHonorsBusyState();

        NicknameEditPresenterTests nicknameEditTests = new();
        nicknameEditTests.Build_UsesFreeChangeAndValidatesInput();
        nicknameEditTests.Build_UsesPaidCostAndHonorsSavingState();

        AdDailyLimitPolicyTests adPolicyTests = new();
        adPolicyTests.TryConsume_ResetsAtUtcDateBoundary();
        adPolicyTests.GetWatchCount_ClampsInvalidNegativeData();

        OneShotConfirmationTests confirmationTests = new();
        confirmationTests.TryConfirm_InvokesActionOnlyOnce();
        confirmationTests.Cancel_ClearsPendingAction();

        GachaDataSOTests gachaDataTests = new();
        gachaDataTests.TryValidate_AcceptsCompleteBanner();
        gachaDataTests.TryValidate_RejectsUnitInWrongRarityPool();
        gachaDataTests.ProjectBanners_AreValid();
        gachaDataTests.TryResolvePickup_UsesConfiguredSpecialUnit();
        gachaDataTests.GachaRoller_AppliesFiftyPercentPickupWithinRolledRarity();
        gachaDataTests.EconomyConfig_LoadsConfiguredDuplicateRewards();
        gachaDataTests.RatePreview_SplitsPickupFromItsRarityRate();

        GameIconProviderTests iconProviderTests = new();
        iconProviderTests.ProjectIconSet_IsComplete();
        iconProviderTests.Provider_MapsResourcesAndRarityFrames();

        GameCatalogTests catalogTests = new();
        catalogTests.ProjectCatalogs_LoadAndResolveEveryAsset();
        catalogTests.UnitCatalog_RejectsDuplicateIds();
        catalogTests.ItemCatalog_RejectsDuplicateIds();
        catalogTests.NewUserConfig_RejectsUnknownDefaultUnit();
        catalogTests.ProjectConfigs_AreFullyInitialized();

        GameDataProjectValidatorTests validationTests = new();
        validationTests.ProjectData_HasNoBlockingValidationErrors();

        SceneFlowServiceTests sceneFlowTests = new();
        await sceneFlowTests.LoadAsync_CompletesValidScene();
        await sceneFlowTests.LoadAsync_RejectsConcurrentTransition();
        await sceneFlowTests.LoadAsync_RecoversAfterFailure();
    }
}
