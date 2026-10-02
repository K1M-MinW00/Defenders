using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIFeedbackToast : MonoBehaviour
{
    private const float VisibleSeconds = 2f;
    private const float FadeSeconds = 0.2f;

    private static UIFeedbackToast instance;

    private CanvasGroup canvasGroup;
    private TMP_Text messageText;
    private Coroutine hideRoutine;

    public static void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        UIFeedbackToast toast = GetOrCreate();
        if (toast != null)
            toast.Display(message);
    }

    private static UIFeedbackToast GetOrCreate()
    {
        if (instance != null)
            return instance;

        Canvas canvas = FindTargetCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[UIFeedbackToast] Active canvas not found.");
            return null;
        }

        GameObject root = new("UI_FeedbackToast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 0.65f);
        rootRect.anchorMax = new Vector2(1f, 0.65f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(0f, 80f);

        Image background = root.GetComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
        background.raycastTarget = false;

        GameObject textObject = new("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(root.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 8f);
        textRect.offsetMax = new Vector2(-24f, -8f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 28f;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        instance = root.AddComponent<UIFeedbackToast>();
        instance.canvasGroup = root.GetComponent<CanvasGroup>();
        instance.messageText = text;
        root.SetActive(false);

        return instance;
    }

    private static Canvas FindTargetCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas selectedCanvas = null;

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || !canvas.isActiveAndEnabled || !canvas.isRootCanvas)
                continue;

            if (selectedCanvas == null || canvas.sortingOrder > selectedCanvas.sortingOrder)
                selectedCanvas = canvas;
        }

        return selectedCanvas;
    }

    private void Display(string message)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        messageText.text = message;
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(VisibleSeconds);

        float elapsed = 0f;
        while (elapsed < FadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / FadeSeconds);
            yield return null;
        }

        hideRoutine = null;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
