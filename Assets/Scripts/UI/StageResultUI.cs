using UnityEngine;
using UnityEngine.SceneManagement;

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
    public void OnClickReturnToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(lobbySceneName);
    }
}