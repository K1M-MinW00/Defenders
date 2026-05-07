using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "LobbyScene";
    [SerializeField] private LoadingUI loadingUI;

    private bool isReadyToStart;
    private bool isBooting;

    private async void Start()
    {
        await BootAsync();
    }

    private async Task BootAsync()
    {
        if (isBooting)
            return;

        isBooting = true;
        isReadyToStart = false;

        try
        {
            if (!ValidateReferences())
                return;

            loadingUI.ShowStartButton(false);
            loadingUI.SetStatus("Initializing...");
            loadingUI.SetProgress(0.05f);

            if (!ValidateMasterData())
            {
                SetFailed("Master Data Load Failed");
                return;
            }

            await WaitForSecondsAsync(0.1f);

            loadingUI.SetStatus("Checking Login...");
            loadingUI.SetProgress(0.3f);

            bool loginOk = await AuthManager.Instance.InitializeAndLoginAsync();
            if (!loginOk || AuthManager.Instance.CurrentUser == null)
            {
                SetFailed("Login Failed");
                return;
            }

            string userId = AuthManager.Instance.CurrentUser.UserId;
            Debug.Log($"[GameBootstrap] Login Success. UID: {userId}");

            await WaitForSecondsAsync(0.1f);

            loadingUI.SetStatus("Loading User Data...");
            loadingUI.SetProgress(0.7f);

            bool loadOk = await UserDataManager.Instance.LoadOrCreateAsync(userId);
            if (!loadOk)
            {
                SetFailed("User Data Load Failed");
                return;
            }

            await WaitForSecondsAsync(0.1f);

            loadingUI.SetStatus("Game Ready");
            loadingUI.SetProgress(1.0f);

            await WaitUntilProgressArrives();

            loadingUI.ShowStartButton(true);
            isReadyToStart = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameBootstrap] Boot exception: {e}");
            SetFailed("Boot Failed");
        }
        finally
        {
            isBooting = false;
        }
    }

    public void StartGame()
    {
        if (!isReadyToStart)
            return;

        isReadyToStart = false;
        loadingUI.ShowStartButton(false);
        SceneManager.LoadScene(nextSceneName);
    }

    private bool ValidateReferences()
    {
        if (loadingUI == null)
        {
            Debug.LogError("[GameBootstrap] LoadingUI reference is missing.");
            return false;
        }

        if (AuthManager.Instance == null)
        {
            Debug.LogError("[GameBootstrap] AuthManager reference is missing.");
            loadingUI.SetStatus("AuthManager Missing");
            return false;
        }

        if (UserDataManager.Instance == null)
        {
            Debug.LogError("[GameBootstrap] UserDataManager reference is missing.");
            loadingUI.SetStatus("UserDataManager Missing");
            return false;
        }

        if (UnitMasterDataManager.Instance == null)
        {
            Debug.LogError("[GameBootstrap] UnitMasterDataManager reference is missing.");
            loadingUI.SetStatus("MasterDataManager Missing");
            return false;
        }

        return true;
    }

    private bool ValidateMasterData()
    {
        var master = UnitMasterDataManager.Instance;

        if (!master.IsLoaded)
        {
            Debug.LogError("[GameBootstrap] Unit master data is not loaded.");
            return false;
        }

        if (master.DefaultOwnedUnitIds == null || master.DefaultOwnedUnitIds.Count == 0)
        {
            Debug.LogError("[GameBootstrap] DefaultOwnedUnitIds is empty.");
            return false;
        }

        if (master.DefaultOwnedUnitIds.Count < 5)
        {
            Debug.LogError("[GameBootstrap] DefaultOwnedUnitIds must contain at least 5 units.");
            return false;
        }

        return true;
    }

    private void SetFailed(string message)
    {
        Debug.LogError($"[GameBootstrap] {message}");
        isReadyToStart = false;

        if (loadingUI != null)
        {
            loadingUI.SetStatus(message);
            loadingUI.ShowStartButton(false);
        }
    }

    private async Task WaitForSecondsAsync(float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
    }

    private async Task WaitUntilProgressArrives()
    {
        while (!loadingUI.IsProgressArrived())
        {
            await Task.Yield();
        }
    }
}