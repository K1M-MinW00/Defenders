using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class UserDataPipelineTestRunner
{
    private const int TestCount = 10;

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
        UserDataPipelineTests tests = new();

        await tests.LoadOrCreateAsync_CreatesDefaultData_WhenUserDoesNotExist();
        await tests.LoadOrCreateAsync_DoesNotSave_WhenCurrentDataNeedsNoChanges();
        await tests.LoadOrCreateAsync_SavesAll_WhenSchemaMigrationRuns();
        await tests.LoadOrCreateAsync_SavesOnlyResources_WhenFuelRecovers();
        await tests.ProfileUpdate_CommitsCopy_AfterSaveSucceeds();
        await tests.ProfileUpdate_KeepsOriginalData_WhenSaveFails();
        tests.Normalize_AssignsDefaultProfileIcon_WhenMissing();
        await tests.ProfileUpdate_RejectsUnownedIcon();
        tests.LobbyBattlePresenter_BuildsStateAndStageEntry();
        tests.LobbyBattlePresenter_RejectsInvalidFormation();
    }
}
