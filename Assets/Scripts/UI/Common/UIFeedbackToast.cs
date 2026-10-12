using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIFeedbackToast : MonoBehaviour
{
    private const float VisibleSeconds = 2f;
    private const float FadeSeconds = 0.2f;
    private static UIFeedbackToast instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;
    private Coroutine hideRoutine;

    private void Awake()
    {
        instance = this;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public static void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (instance == null)
        {
            Debug.LogWarning($"[UIFeedbackToast] Toast UI is not initialized. Message: {message}");
            return;
        }

        instance.Display(message);
    }

    private void Display(string message)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

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
        canvasGroup.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
