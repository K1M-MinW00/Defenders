using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LobbyExitConfirmationView : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (cancelButton == null || confirmButton == null)
        {
            Debug.LogError("[LobbyExitConfirmationView] Required UI references are not assigned.", this);
            enabled = false;
            return;
        }

        cancelButton.onClick.AddListener(Dismiss);
        confirmButton.onClick.AddListener(QuitGame);
    }

    public void Show()
    {
        Lifecycle.Show();
    }

    public void Dismiss() => Lifecycle.Close();

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
