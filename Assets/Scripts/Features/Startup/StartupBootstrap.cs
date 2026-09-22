using System.Threading.Tasks;
using UnityEngine;

public class StartupBootstrap : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "LobbyScene";
    [SerializeField] private StartupLoadingView loadingView;

    private bool isReadyToStart;
    private bool isBooting;
    private bool hasFailed;

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
        hasFailed = false;

        try
        {
            if (!TryCreateFlow(out StartupFlow flow, out string validationError))
            {
                SetFailed(validationError);
                return;
            }

            loadingView.SetActionButton("Start", false);
            StartupBootResult result = await flow.RunAsync(HandleStageChanged);
            if (!result.Succeeded)
            {
                Debug.LogError($"[StartupBootstrap] Boot failed ({result.Failure}): {result.ErrorMessage}");
                SetFailed(GetFailureMessage(result.Failure));
                return;
            }

            await WaitUntilProgressCompleted();
            loadingView.SetActionButton("Start", true);
            isReadyToStart = true;
        }
        finally
        {
            isBooting = false;
        }
    }

    public async void StartGame()
    {
        if (hasFailed && !isBooting)
        {
            _ = BootAsync();
            return;
        }

        if (!isReadyToStart)
            return;

        isReadyToStart = false;
        loadingView.SetActionButton("Start", false);
        loadingView.SetStatus("Loading Lobby...");

        SceneTransitionResult result = await SceneFlowService.Shared.LoadAsync(nextSceneName);

        if (result != SceneTransitionResult.Succeeded && this != null)
            SetFailed(GetSceneTransitionFailureMessage(result));
    }

    private bool TryCreateFlow(out StartupFlow flow, out string errorMessage)
    {
        flow = null;
        errorMessage = null;

        if (loadingView == null)
        {
            errorMessage = "Loading view is missing.";
            return false;
        }

        if (AuthService.Instance == null)
        {
            errorMessage = "Authentication service is missing.";
            return false;
        }

        if (UserDataManager.Instance == null)
        {
            errorMessage = "User data service is missing.";
            return false;
        }

        flow = new StartupFlow(InitializeLocalData, AuthService.Instance.SignInAsync, UserDataManager.Instance.LoadOrCreateAsync);
        return true;
    }

    private static void InitializeLocalData()
    {
        GameConfig.Initialize();
    }

    private void HandleStageChanged(StartupBootStage stage)
    {
        switch (stage)
        {
            case StartupBootStage.Initializing:
                loadingView.SetStatus("Initializing...");
                loadingView.SetProgress(0.05f);
                break;
            case StartupBootStage.SigningIn:
                loadingView.SetStatus("Checking Login...");
                loadingView.SetProgress(0.3f);
                break;
            case StartupBootStage.LoadingUserData:
                loadingView.SetStatus("Loading User Data...");
                loadingView.SetProgress(0.7f);
                break;
            case StartupBootStage.Ready:
                loadingView.SetStatus("Game Ready");
                loadingView.SetProgress(1f);
                break;
        }
    }

    private void SetFailed(string message)
    {
        Debug.LogError($"[StartupBootstrap] {message}");
        isReadyToStart = false;
        hasFailed = true;

        if (loadingView != null)
        {
            loadingView.SetStatus(message);
            loadingView.SetActionButton("Retry", true);
        }
    }

    private static string GetFailureMessage(StartupBootFailure failure)
    {
        return failure switch
        {
            StartupBootFailure.LoginFailed => "Login Failed",
            StartupBootFailure.UserDataLoadFailed => "User Data Load Failed",
            StartupBootFailure.MissingService => "Required Service Missing",
            _ => "Boot Failed"
        };
    }

    private static string GetSceneTransitionFailureMessage(SceneTransitionResult result)
    {
        return result switch
        {
            SceneTransitionResult.InvalidScene => "Lobby Scene Missing",
            SceneTransitionResult.AlreadyLoading => "Scene Is Already Loading",
            _ => "Lobby Load Failed",
        };
    }

    private async Task WaitUntilProgressCompleted()
    {
        while (!loadingView.IsProgressCompleted())
            await Task.Yield();
    }
}
