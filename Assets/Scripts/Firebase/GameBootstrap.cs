using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "LobbyScene";
    [SerializeField] private LoadingUI loadingUI;

    private bool isReadyToStart;

    private async void Start()
    {
        await BootAsync();
    }

    private async Task BootAsync()
    {
        if (AuthManager.Instance == null)
        {
            Debug.LogError("[GameBootstrap] AuthManager reference is missing.");
            return;
        }

        if (UserDataManager.Instance == null)
        {
            Debug.LogError("[GameBootstrap] UserDataManager reference is missing.");
            return;
        }

        isReadyToStart = false;
        loadingUI.ShowStartButton(false);

        loadingUI.SetStatus("Checking Login..");
        loadingUI.SetProgress(0.3f);

        await AuthManager.Instance.InitializeAndLoginAsync();

        var user = AuthManager.Instance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("[GameBootstrap] Login failed. CurrentUser is Null.");
            return;
        }

        Debug.Log($"[GameBootstrap] Login Success. UID : {user.UserId}");

        await WaitForProgressOrDelay(0.15f);

        loadingUI.SetStatus("Loading User Data");
        loadingUI.SetProgress(0.7f);

        bool loadOk = await UserDataManager.Instance.LoadOrCreateAsync(user.UserId);
        if (!loadOk)
        {
            Debug.LogError("[GameBootstrap] User data load failed");
            return;
        }

        await WaitForProgressOrDelay(0.15f);

        loadingUI.SetStatus("Game Ready");
        loadingUI.SetProgress(1.0f);

        await WaitUnitlProgressArrives();

        loadingUI.ShowStartButton(true);
        isReadyToStart = true;
    }

    public void StartGame()
    {
        if (!isReadyToStart)
            return;

        SceneManager.LoadScene(nextSceneName);
    }
    private async Task WaitForProgressOrDelay(float minSeconds)
    {
        float elapsed = 0f;

        while (elapsed < minSeconds)
        {
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
    }

    private async Task WaitUnitlProgressArrives()
    {
        while (!loadingUI.IsProgressArrived())
        {
            await Task.Yield();
        }
    }
}
