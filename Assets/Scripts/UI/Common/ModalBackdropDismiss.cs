using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public sealed class ModalBackdropDismiss : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform modalContent;
    [SerializeField] private GameObject target;
    [SerializeField] private MonoBehaviour dismissHandler;
    [SerializeField] private bool dismissOnBackdropClick = true;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ModalTransition transition;

    public void Configure(RectTransform content, GameObject dismissTarget = null, MonoBehaviour handler = null, bool allowBackdropDismiss = true)
    {
        modalContent = content;
        target = dismissTarget != null ? dismissTarget : gameObject;
        dismissHandler = handler;
        dismissOnBackdropClick = allowBackdropDismiss;
        canvasGroup = GetComponent<CanvasGroup>();
        transition = GetComponent<ModalTransition>();
    }

    private void OnEnable() => LobbyModalStack.Register(this);
    private void OnDisable() => LobbyModalStack.Unregister(this);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!dismissOnBackdropClick) return;
        if (modalContent != null && RectTransformUtility.RectangleContainsScreenPoint(
                modalContent, eventData.position, eventData.pressEventCamera)) return;
        Dismiss();
    }

    public void DismissFromSystemBack() => Dismiss();

    public void SetStackInteraction(bool enabled)
    {
        if (transition != null)
        {
            transition.SetStackInteraction(enabled);
            return;
        }

        canvasGroup.interactable = enabled;
        canvasGroup.blocksRaycasts = enabled;
    }

    private void Dismiss()
    {
        // A modal view owns its cleanup and exit transition. Routing through the
        // handler first keeps X, cancel, backdrop, and system-back on one path.
        if (dismissHandler is IModalDismissHandler handler)
        {
            handler.Dismiss();
            return;
        }

        if (transition != null && gameObject.activeInHierarchy)
        {
            transition.PlayExit(DeactivateTarget);
            return;
        }

        DeactivateTarget();
    }

    private void DeactivateTarget() =>
        (target != null ? target : gameObject).SetActive(false);
}
