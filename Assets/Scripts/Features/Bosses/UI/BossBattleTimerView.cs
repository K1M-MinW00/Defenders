using System.Collections;
using TMPro;
using UnityEngine;

public sealed class BossBattleTimerView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private CanvasGroup blinkGroup;
    [SerializeField, Min(0.05f)] private float blinkInterval = 0.35f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new(1f, 0.25f, 0.2f, 1f);

    private BossBattleTimerController timer;
    private Coroutine blinkRoutine;

    public void Initialize(BossBattleTimerController controller)
    {
        Dispose();
        timer = controller;
        if (timer != null)
        {
            timer.TimerChanged += HandleTimerChanged;
            timer.WarningStarted += HandleWarningStarted;
            timer.TimerStopped += HandleTimerStopped;
        }

        SetVisible(false);
        SetTimerColor(normalColor);
    }

    public void Dispose()
    {
        if (timer != null)
        {
            timer.TimerChanged -= HandleTimerChanged;
            timer.WarningStarted -= HandleWarningStarted;
            timer.TimerStopped -= HandleTimerStopped;
        }

        timer = null;
        StopBlinking();
        SetVisible(false);
    }

    private void HandleTimerChanged(float remaining)
    {
        SetVisible(true);
        if (timerText != null)
        {
            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remaining));
            timerText.SetText("{0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
        }

        if (remaining <= 0f)
            StopBlinking();
    }

    private void HandleWarningStarted()
    {
        StopBlinking();
        SetTimerColor(warningColor);
        if (isActiveAndEnabled)
            blinkRoutine = StartCoroutine(Blink());
    }

    private void HandleTimerStopped()
    {
        StopBlinking();
        SetTimerColor(normalColor);
        SetVisible(false);
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            if (blinkGroup != null)
                blinkGroup.alpha = blinkGroup.alpha > 0.5f ? 0.25f : 1f;

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void StopBlinking()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (blinkGroup != null)
            blinkGroup.alpha = 1f;
    }

    private void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
    }

    private void SetTimerColor(Color color)
    {
        if (timerText != null)
            timerText.color = color;
    }

    private void OnDisable()
    {
        StopBlinking();
    }
}
