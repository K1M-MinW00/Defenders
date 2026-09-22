using UnityEngine;

public class StageResultUI : MonoBehaviour
{
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject stageFailPanel;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    public void Initialize()
    {
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
        Time.timeScale = 1f;
        SceneTransitionResult result = await SceneFlowService.Shared.LoadAsync(lobbySceneName);
        if (result != SceneTransitionResult.Succeeded && this != null)
            Debug.LogError($"[StageResultUI] Failed to return to lobby: {result}");
    }
}
