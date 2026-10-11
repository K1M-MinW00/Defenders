using UnityEngine;

public sealed class SimplePopupView : MonoBehaviour, IModalDismissHandler
{
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public bool IsVisible => Lifecycle.IsVisible;
    public void Open() => Lifecycle.Show();
    public void Close() => Lifecycle.Close();
    public void HideImmediately() => Lifecycle.HideImmediately();
    public void Dismiss() => Close();
}
