using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public sealed class ModalBackdropDismiss : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform modalContent;
    [SerializeField] private GameObject target;
    [SerializeField] private MonoBehaviour dismissHandler;
    [SerializeField] private bool dismissOnBackdropClick = true;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ModalTransition transition;

    private void Awake()
    {
        target ??= gameObject;
        canvasGroup ??= GetComponent<CanvasGroup>();
        transition ??= GetComponent<ModalTransition>();

        if (dismissHandler != null && dismissHandler is not IModalDismissHandler)
            Debug.LogWarning("[ModalBackdropDismiss] The assigned handler does not implement IModalDismissHandler.", this);
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
        // handler first keeps explicit close buttons and backdrop taps on one path.
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
