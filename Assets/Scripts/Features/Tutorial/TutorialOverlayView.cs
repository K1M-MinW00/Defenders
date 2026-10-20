using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TutorialOverlayView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image blocker;
    [SerializeField] private TutorialRaycastFilter raycastFilter;
    [SerializeField] private RectTransform focusFrame;
    [SerializeField] private RectTransform shadeTop;
    [SerializeField] private RectTransform shadeBottom;
    [SerializeField] private RectTransform shadeLeft;
    [SerializeField] private RectTransform shadeRight;
    [SerializeField] private RectTransform guidePanel;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private TMP_Text messageText;
    [SerializeField, Min(0.5f)] private float guideVisibleDuration = 2.5f;

    private RectTransform root;
    private RectTransform trackedTarget;
    private Vector2 arrowBasePosition;
    private Tween focusTween;
    private Coroutine guideHideRoutine;

    private void Awake()
    {
        root = transform as RectTransform;
        Hide();
    }

    private void LateUpdate()
    {
        if (canvasGroup != null && canvasGroup.alpha > 0f && trackedTarget != null)
        {
            AlignFocus(trackedTarget);
            float pulse = 10f + Mathf.Sin(Time.unscaledTime * 5f) * 8f;
            if (arrow != null)
                arrow.anchoredPosition = arrowBasePosition + (Vector2)arrow.up * pulse;
        }
    }

    public void ShowExplanation(string message, RectTransform target = null)
    {
        Show(message, target);
    }

    public void ShowRequiredAction(string message, RectTransform target)
    {
        Show(message, target);
    }

    public void ShowWorldAction(string message, RectTransform target = null)
    {
        Show(message, target);
    }

    public void ShowWaiting(string message)
    {
        Show(message, null);
    }

    public void Hide()
    {
        trackedTarget = null;
        StopGuideHideRoutine();
        focusTween?.Kill();
        focusTween = null;
        raycastFilter?.SetAllowedTarget(null);
        if (focusFrame != null)
            focusFrame.gameObject.SetActive(false);
        if (arrow != null)
            arrow.gameObject.SetActive(false);
        SetSpotlightVisible(false);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Show(string message, RectTransform target)
    {
        trackedTarget = target;
        StopGuideHideRoutine();
        if (guidePanel != null)
        {
            guidePanel.gameObject.SetActive(true);
            guideHideRoutine = StartCoroutine(HideGuidePanelAfterDelay());
        }
        if (messageText != null)
            messageText.text = message ?? string.Empty;
        if (focusFrame != null)
            focusFrame.gameObject.SetActive(target != null);
        if (arrow != null)
            arrow.gameObject.SetActive(target != null);
        if (target != null)
        {
            AlignFocus(target);
            StartFocusTween();
        }
        else
        {
            focusTween?.Kill();
            focusTween = null;
            SetSpotlightVisible(false);
        }

        raycastFilter?.SetAllowedTarget(null);
        if (blocker != null)
            blocker.raycastTarget = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void AlignFocus(RectTransform target)
    {
        if (root == null || focusFrame == null || target == null)
            return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root,
            RectTransformUtility.WorldToScreenPoint(camera, corners[0]),
            camera,
            out Vector2 min);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root,
            RectTransformUtility.WorldToScreenPoint(camera, corners[2]),
            camera,
            out Vector2 max);

        const float padding = 12f;
        min -= Vector2.one * padding;
        max += Vector2.one * padding;
        focusFrame.anchoredPosition = (min + max) * 0.5f;
        focusFrame.sizeDelta = max - min;
        SetSpotlightVisible(false);

        bool targetIsUpper = (min.y + max.y) * 0.5f > 0f;
        if (arrow != null)
        {
            arrow.localRotation = Quaternion.Euler(0f, 0f, targetIsUpper ? 180f : 0f);
            arrowBasePosition = targetIsUpper
                ? new Vector2((min.x + max.x) * 0.5f, min.y - 42f)
                : new Vector2((min.x + max.x) * 0.5f, max.y + 42f);
        }

        if (guidePanel != null)
        {
            guidePanel.anchorMin = targetIsUpper ? new Vector2(0.05f, 0.12f) : new Vector2(0.05f, 0.66f);
            guidePanel.anchorMax = targetIsUpper ? new Vector2(0.95f, 0.34f) : new Vector2(0.95f, 0.88f);
            guidePanel.offsetMin = Vector2.zero;
            guidePanel.offsetMax = Vector2.zero;
        }
    }

    private void StartFocusTween()
    {
        focusTween?.Kill();
        if (focusFrame == null)
            return;

        focusFrame.localScale = Vector3.one;
        focusTween = focusFrame
            .DOScale(1.08f, 0.55f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void AlignSpotlight(Vector2 min, Vector2 max)
    {
        if (root == null)
            return;

        Rect bounds = root.rect;
        min.x = Mathf.Clamp(min.x, bounds.xMin, bounds.xMax);
        min.y = Mathf.Clamp(min.y, bounds.yMin, bounds.yMax);
        max.x = Mathf.Clamp(max.x, bounds.xMin, bounds.xMax);
        max.y = Mathf.Clamp(max.y, bounds.yMin, bounds.yMax);

        SetShadeRect(shadeTop, bounds.xMin, max.y, bounds.xMax, bounds.yMax);
        SetShadeRect(shadeBottom, bounds.xMin, bounds.yMin, bounds.xMax, min.y);
        SetShadeRect(shadeLeft, bounds.xMin, min.y, min.x, max.y);
        SetShadeRect(shadeRight, max.x, min.y, bounds.xMax, max.y);
        SetSpotlightVisible(true);
    }

    private static void SetShadeRect(RectTransform shade, float minX, float minY, float maxX, float maxY)
    {
        if (shade == null)
            return;

        shade.anchorMin = shade.anchorMax = new Vector2(0.5f, 0.5f);
        shade.anchoredPosition = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        shade.sizeDelta = new Vector2(Mathf.Max(0f, maxX - minX), Mathf.Max(0f, maxY - minY));
    }

    private void SetSpotlightVisible(bool visible)
    {
        SetActive(shadeTop, visible);
        SetActive(shadeBottom, visible);
        SetActive(shadeLeft, visible);
        SetActive(shadeRight, visible);
    }

    private IEnumerator HideGuidePanelAfterDelay()
    {
        yield return new WaitForSecondsRealtime(guideVisibleDuration);
        guideHideRoutine = null;
        if (guidePanel != null)
            guidePanel.gameObject.SetActive(false);
    }

    private void StopGuideHideRoutine()
    {
        if (guideHideRoutine == null)
            return;

        StopCoroutine(guideHideRoutine);
        guideHideRoutine = null;
    }

    private static void SetActive(Component component, bool active)
    {
        if (component != null)
            component.gameObject.SetActive(active);
    }

    private void OnDestroy()
    {
        StopGuideHideRoutine();
        focusTween?.Kill();
    }

}
