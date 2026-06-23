using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupBootstrap : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "LobbyScene";
    [SerializeField] private StartupLoadingView loadingView;

    private bool isReadyToStart;
    private bool isBooting;

    private const float LoadingStepDelay = 0.1f;

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

            loadingView.SetStartButtonVisible(false);
            loadingView.SetStatus("Initializing...");
            loadingView.SetProgress(0.05f);

            ItemDatabase.Initialize();
            UnitDatabase.Initialize();
            GameIconDatabase.Initialize();
            GameConfig.Initialize();

            await WaitForSecondsAsync(LoadingStepDelay);

            loadingView.SetStatus("Checking Login...");
            loadingView.SetProgress(0.3f);

            bool loginOk = await AuthService.Instance.InitializeAndLoginAsync();
            if (!loginOk || AuthService.Instance.CurrentUser == null)
            {
                SetFailed("Login Failed");
                return;
            }

            string userId = AuthService.Instance.CurrentUser.UserId;
            Debug.Log($"[StartupBootstrap] Login Success. UID: {userId}");

            await WaitForSecondsAsync(LoadingStepDelay);

            loadingView.SetStatus("Loading User Data...");
            loadingView.SetProgress(0.7f);

            bool loadOk = await UserDataManager.Instance.LoadOrCreateAsync(userId);
            if (!loadOk)
            {
                SetFailed("User Data Load Failed");
                return;
            }

            await WaitForSecondsAsync(LoadingStepDelay);

            loadingView.SetStatus("Game Ready");
            loadingView.SetProgress(1.0f);

            await WaitUntilProgressCompleted();

            loadingView.SetStartButtonVisible(true);
            isReadyToStart = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[StartupBootstrap] Boot exception: {e}");
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
        loadingView.SetStartButtonVisible(false);
        SceneManager.LoadScene(nextSceneName);
    }

    private bool ValidateReferences()
    {
        if (loadingView == null)
        {
            Debug.LogError("[StartupBootstrap] LoadingUI reference is missing.");
            return false;
        }

        if (AuthService.Instance == null)
        {
            Debug.LogError("[StartupBootstrap] AuthManager reference is missing.");
            loadingView.SetStatus("AuthManager Missing");
            return false;
        }

        return true;
    }

    private void SetFailed(string message)
    {
        Debug.LogError($"[StartupBootstrap] {message}");
        isReadyToStart = false;

        if (loadingView != null)
        {
            loadingView.SetStatus(message);
            loadingView.SetStartButtonVisible(false);
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

    private async Task WaitUntilProgressCompleted()
    {
        while (!loadingView.IsProgressCompleted())
        {
            await Task.Yield();
        }
    }
}