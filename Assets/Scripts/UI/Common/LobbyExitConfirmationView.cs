using UnityEngine;
using UnityEngine.UI;

public sealed class LobbyExitConfirmationView : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    public void Configure(Button cancel, Button confirm)
    {
        cancelButton = cancel;
        confirmButton = confirm;
    }

    private void Awake()
    {
        cancelButton?.onClick.AddListener(Dismiss);
        confirmButton?.onClick.AddListener(QuitGame);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Dismiss() => ModalTransitionUtility.RequestExit(gameObject, () => gameObject.SetActive(false));

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        cancelButton?.onClick.RemoveListener(Dismiss);
        confirmButton?.onClick.RemoveListener(QuitGame);
    }
}
