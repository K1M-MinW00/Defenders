using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StartupBootstrap : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "LobbyScene";
    [SerializeField] private StartupLoadingView loadingView;

    private bool isReadyToStart;
    private bool isBooting;
    private bool hasFailed;
    private int operationVersion;
    private AuthService authService;
    private UserDataManager userDataManager;
    private SceneFlowService sceneFlowService;

    public void Initialize(
        AuthService authentication,
        UserDataManager userData,
        SceneFlowService sceneFlow)
    {
        authService = authentication;
        userDataManager = userData;
        sceneFlowService = sceneFlow;
    }

    private void Awake()
    {
        if (loadingView == null || string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("[StartupBootstrap] Required startup references are not assigned.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (loadingView != null)
            loadingView.ActionRequested += StartGame;
    }

    private void OnDisable()
    {
        operationVersion++;
        if (loadingView != null)
            loadingView.ActionRequested -= StartGame;
    }

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
        int currentOperation = ++operationVersion;

        try
        {
            if (!TryCreateFlow(out StartupFlow flow, out string validationError))
            {
                SetFailed(validationError);
                return;
            }

            loadingView.SetActionButton("Start", false);
            StartupBootResult result = await flow.RunAsync(stage =>
            {
                if (IsCurrentOperation(currentOperation))
                    HandleStageChanged(stage);
            });
            if (!IsCurrentOperation(currentOperation))
                return;

            if (!result.Succeeded)
            {
                Debug.LogError($"[StartupBootstrap] Boot failed ({result.Failure}): {result.ErrorMessage}");
                SetFailed(GetFailureMessage(result.Failure));
                return;
            }

            await WaitUntilProgressCompleted(currentOperation);
            if (!IsCurrentOperation(currentOperation))
                return;

            loadingView.SetActionButton("Start", true);
            isReadyToStart = true;
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[StartupBootstrap] Unexpected boot failure: {exception}");
            if (IsCurrentOperation(currentOperation))
                SetFailed("Boot Failed");
        }
        finally
        {
            isBooting = false;
        }
    }

    private async void StartGame()
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

        if (sceneFlowService == null)
        {
            SetFailed("Required Service Missing");
            return;
        }

        SceneTransitionResult result = await sceneFlowService.LoadAsync(nextSceneName);

        if (result != SceneTransitionResult.Succeeded && this != null && isActiveAndEnabled)
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

        if (authService == null)
        {
            errorMessage = "Authentication service is missing.";
            return false;
        }

        if (userDataManager == null)
        {
            errorMessage = "User data service is missing.";
            return false;
        }

        flow = new StartupFlow(InitializeLocalData, authService.SignInAsync, userDataManager.LoadOrCreateAsync);
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

    private async Task WaitUntilProgressCompleted(int version)
    {
        while (IsCurrentOperation(version) && !loadingView.IsProgressCompleted())
            await Task.Yield();
    }

    private bool IsCurrentOperation(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;
}
