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

            loadingView.SetActionButtonKey("startup.action.start", false);
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
                SetFailed(Localize("startup.error.boot_failed"));
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
        loadingView.SetActionButtonKey("startup.action.start", false);
        loadingView.SetStatusKey("startup.status.loading_lobby");

        if (sceneFlowService == null)
        {
            SetFailed(Localize("startup.error.required_service_missing"));
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
            errorMessage = Localize("startup.error.required_service_missing");
            return false;
        }

        if (authService == null)
        {
            errorMessage = Localize("startup.error.required_service_missing");
            return false;
        }

        if (userDataManager == null)
        {
            errorMessage = Localize("startup.error.required_service_missing");
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
                loadingView.SetStatusKey("startup.status.initializing");
                loadingView.SetProgress(0.05f);
                break;
            case StartupBootStage.SigningIn:
                loadingView.SetStatusKey("startup.status.checking_login");
                loadingView.SetProgress(0.3f);
                break;
            case StartupBootStage.LoadingUserData:
                loadingView.SetStatusKey("startup.status.loading_user_data");
                loadingView.SetProgress(0.7f);
                break;
            case StartupBootStage.Ready:
                loadingView.SetStatusKey("startup.status.ready");
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
            loadingView.SetActionButtonKey("startup.action.retry", true);
        }
    }

    private static string GetFailureMessage(StartupBootFailure failure)
    {
        return failure switch
        {
            StartupBootFailure.LoginFailed => Localize("startup.error.login_failed"),
            StartupBootFailure.UserDataLoadFailed => Localize("startup.error.user_data_load_failed"),
            StartupBootFailure.MissingService => Localize("startup.error.required_service_missing"),
            _ => Localize("startup.error.boot_failed")
        };
    }

    private static string GetSceneTransitionFailureMessage(SceneTransitionResult result)
    {
        return result switch
        {
            SceneTransitionResult.InvalidScene => Localize("startup.error.lobby_scene_missing"),
            SceneTransitionResult.AlreadyLoading => Localize("startup.error.scene_already_loading"),
            _ => Localize("startup.error.lobby_load_failed"),
        };
    }

    private async Task WaitUntilProgressCompleted(int version)
    {
        while (IsCurrentOperation(version) && !loadingView.IsProgressCompleted())
            await Task.Yield();
    }

    private bool IsCurrentOperation(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.Common, key);
}
