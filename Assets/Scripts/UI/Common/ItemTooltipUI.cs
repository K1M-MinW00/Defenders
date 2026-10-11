using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ItemTooltipUI : MonoBehaviour, IPointerClickHandler
{
    private static ItemTooltipUI instance;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panel;
    [SerializeField] private TMP_Text label;
    private float hideAt;
    private bool isVisible;

    private void Awake()
    {
        instance = this;
        SetVisible(false);
    }

    public static void Show(string displayName, RectTransform anchor)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return;

        instance.ShowInternal(displayName, anchor);
    }

    private void ShowInternal(string displayName, RectTransform anchor)
    {
        label.text = displayName;
        transform.SetAsLastSibling();
        hideAt = Time.unscaledTime + 2f;
        SetVisible(true);

        Canvas canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        RectTransform canvasRect = canvas?.transform as RectTransform;
        if (anchor == null || canvasRect == null)
        {
            panel.anchoredPosition = Vector2.zero;
            return;
        }

        Vector3 worldPosition = anchor.TransformPoint(new Vector3(0f, anchor.rect.height * 0.65f, 0f));
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, camera, out Vector2 localPoint);
        panel.anchoredPosition = localPoint;
    }

    private void Update()
    {
        if (isVisible && Time.unscaledTime >= hideAt)
            SetVisible(false);
    }

    public void OnPointerClick(PointerEventData eventData) => SetVisible(false);

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
