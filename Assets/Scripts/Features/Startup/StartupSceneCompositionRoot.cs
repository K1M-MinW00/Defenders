using UnityEngine;

[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
public sealed class StartupSceneCompositionRoot : MonoBehaviour
{
    [SerializeField] private StartupBootstrap startupBootstrap;

    private void Awake()
    {
        if (startupBootstrap == null)
        {
            Debug.LogError("[StartupSceneCompositionRoot] Startup bootstrap is not assigned.", this);
            enabled = false;
            return;
        }

        AuthService authService = AuthService.Instance;
        UserDataManager userDataManager = UserDataManager.Instance;
        SceneFlowService sceneFlowService = SceneFlowService.Shared;

        if (authService == null || userDataManager == null || sceneFlowService == null)
        {
            Debug.LogError("[StartupSceneCompositionRoot] One or more startup services are unavailable.", this);
            enabled = false;
            return;
        }

        startupBootstrap.Initialize(authService, userDataManager, sceneFlowService);
    }
}
