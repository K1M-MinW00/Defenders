using UnityEngine;

public class StageResultUI : MonoBehaviour
{
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject stageFailPanel;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    private StageSessionController session;

    public void Initialize(StageSessionController stageSession)
    {
        session = stageSession;
        HideAll();
    }

    public void ShowClear()
    {
        HideAll();
        if (stageClearPanel != null)
            stageClearPanel.SetActive(true);
    }

    public void ShowFail()
    {
        HideAll();
        if (stageFailPanel != null)
            stageFailPanel.SetActive(true);
    }

    public void HideAll()
    {
        if (stageClearPanel != null)
            stageClearPanel.SetActive(false);

        if (stageFailPanel != null)
            stageFailPanel.SetActive(false);
    }
    public async void OnClickReturnToLobby()
    {
        if (session == null || !await session.TryPrepareExitAsync())
        {
            Debug.LogWarning("[StageResultUI] Progress is not saved yet. Lobby transition was blocked; press again to retry.");
            return;
        }

        Time.timeScale = 1f;
        SceneTransitionResult result = await SceneFlowService.Shared.LoadAsync(lobbySceneName);
        if (result != SceneTransitionResult.Succeeded && this != null)
            Debug.LogError($"[StageResultUI] Failed to return to lobby: {result}");
    }
}
