using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public sealed class ModalTransition : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RectTransform modalCard;
    [SerializeField, Min(0f)] private float enterDuration = 0.18f;
    [SerializeField, Min(0f)] private float exitDuration = 0.15f;
    [SerializeField] private float enterScale = 0.96f;
    [SerializeField] private float exitScale = 0.98f;

    private CanvasGroup canvasGroup;
    private Coroutine routine;
    private Action exitCompleted;
    private bool transitioning;
    private bool exiting;
    private bool stackInteractionEnabled = true;
    private bool invokingExitCallback;

    public bool IsTransitioning => transitioning;
    public bool IsInvokingExitCallback => invokingExitCallback;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("[ModalTransition] CanvasGroup is required.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        canvasGroup ??= GetComponent<CanvasGroup>();
        StartTransition(false);
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        transitioning = false;
        exiting = false;
        exitCompleted = null;
        if (modalCard != null) modalCard.localScale = Vector3.one;
    }

    public void SetStackInteraction(bool enabled)
    {
        stackInteractionEnabled = enabled;
        ApplyInteraction();
    }

    public void PlayExit(Action completed)
    {
        if (exiting) return;
        exitCompleted = completed;
        StartTransition(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!transitioning) return;
        CompleteImmediately();
        eventData.Use();
    }

    private void StartTransition(bool exit)
    {
        if (routine != null) StopCoroutine(routine);
        exiting = exit;
        transitioning = true;
        ApplyInteraction();
        routine = StartCoroutine(Animate(exit));
    }

    private IEnumerator Animate(bool exit)
    {
        float duration = exit ? exitDuration : enterDuration;
        float elapsed = 0f;
        float fromAlpha = exit ? 1f : 0f;
        float toAlpha = exit ? 0f : 1f;
        float fromScale = exit ? 1f : enterScale;
        float toScale = exit ? exitScale : 1f;

        ApplyVisual(fromAlpha, fromScale);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            ApplyVisual(Mathf.Lerp(fromAlpha, toAlpha, t), Mathf.Lerp(fromScale, toScale, t));
            yield return null;
        }

        CompleteImmediately();
    }

    private void CompleteImmediately()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        ApplyVisual(exiting ? 0f : 1f, exiting ? exitScale : 1f);
        transitioning = false;
        ApplyInteraction();

        if (!exiting) return;
        Action completed = exitCompleted;
        exitCompleted = null;
        exiting = false;
        invokingExitCallback = true;
        try
        {
            completed?.Invoke();
        }
        finally
        {
            invokingExitCallback = false;
        }
    }

    private void ApplyVisual(float alpha, float scale)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
        if (modalCard != null) modalCard.localScale = Vector3.one * scale;
    }

    private void ApplyInteraction()
    {
        if (canvasGroup == null) return;
        canvasGroup.interactable = stackInteractionEnabled && !transitioning;
        canvasGroup.blocksRaycasts = stackInteractionEnabled;
    }
}
