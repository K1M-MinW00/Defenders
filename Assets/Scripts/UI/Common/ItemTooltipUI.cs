using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ItemTooltipUI : MonoBehaviour, IPointerClickHandler
{
    private static ItemTooltipUI instance;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panel;
    [SerializeField] private TMP_Text label;
    private float hideAt;
    private bool isVisible;
    private Canvas rootCanvas;
    private RectTransform rootCanvasRect;

    private void Awake()
    {
        if (canvasGroup == null || panel == null || label == null)
        {
            Debug.LogError("[ItemTooltipUI] Required UI references are not assigned.", this);
            enabled = false;
            return;
        }

        if (instance != null && instance != this)
            Debug.LogWarning("[ItemTooltipUI] Multiple tooltip views are active. The latest view will be used.", this);

        instance = this;
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        rootCanvasRect = rootCanvas?.transform as RectTransform;
        SetVisible(false);
    }

    public static void Show(string displayName, RectTransform anchor)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return;

        if (instance == null)
        {
            Debug.LogWarning($"[ItemTooltipUI] Tooltip UI is not initialized. Item: {displayName}");
            return;
        }

        instance.ShowInternal(displayName, anchor);
    }

    private void ShowInternal(string displayName, RectTransform anchor)
    {
        label.text = displayName;
        transform.SetAsLastSibling();
        hideAt = Time.unscaledTime + 2f;
        SetVisible(true);

        if (anchor == null || rootCanvasRect == null)
        {
            panel.anchoredPosition = Vector2.zero;
            return;
        }

        Vector3 worldPosition = anchor.TransformPoint(new Vector3(0f, anchor.rect.height * 0.65f, 0f));
        Camera camera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, screenPoint, camera, out Vector2 localPoint);
        panel.anchoredPosition = localPoint;
    }

    private void Update()
    {
        if (isVisible && Time.unscaledTime >= hideAt)
            SetVisible(false);
    }

    public void OnPointerClick(PointerEventData eventData) => SetVisible(false);

    private void OnDisable()
    {
        if (canvasGroup != null)
            SetVisible(false);
    }

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
