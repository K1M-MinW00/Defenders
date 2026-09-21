using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RecruitPresentationController : MonoBehaviour
{
    private const float DefaultIntroDuration = 0.65f;
    private const float DefaultRevealInterval = 0.18f;
    private const float DefaultCardRevealDuration = 0.22f;

    [Header("Optional Intro Presentation")]
    [SerializeField] private GameObject introRoot;
    [SerializeField] private Animator introAnimator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private RecruitPresentationOverlayView overlayView;

    private RecruitResultPopupView resultPopup;
    private RecruitPresentationConfigSO config;
    private Coroutine presentationRoutine;
    private bool isPresenting;
    private bool ownsOverlayView;

    public bool IsPresenting => isPresenting;
    public event Action PresentationCompleted;

    public void Initialize(RecruitResultPopupView popup, RecruitPresentationConfigSO presentationConfig)
    {
        if (resultPopup != null)
            resultPopup.SkipRequested -= Skip;

        resultPopup = popup;
        config = presentationConfig;

        if (resultPopup != null)
        {
            resultPopup.SkipRequested += Skip;

            if (overlayView == null && resultPopup.transform.parent != null)
            {
                overlayView = RecruitPresentationOverlayView.Create(resultPopup.transform.parent);
                ownsOverlayView = true;
            }
        }

        if (overlayView != null)
        {
            overlayView.SkipRequested -= Skip;
            overlayView.SkipRequested += Skip;
        }
    }

    public void Present(IReadOnlyList<GachaResult> results)
    {
        if (resultPopup == null)
            return;

        StopCurrentPresentation(false);

        List<GachaResult> snapshot = results != null
            ? new List<GachaResult>(results)
            : new List<GachaResult>();

        resultPopup.Prepare(snapshot);
        isPresenting = true;
        presentationRoutine = StartCoroutine(PlayRoutine(snapshot));
    }

    public void Skip()
    {
        if (IsPresenting)
            StopCurrentPresentation(true);
    }

    private IEnumerator PlayRoutine(IReadOnlyList<GachaResult> results)
    {
        PlayIntro(GetHighestRarity(results));

        float introDuration = config != null ? config.IntroDuration : DefaultIntroDuration;
        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlayView?.SetIntroProgress(introDuration > 0f ? elapsed / introDuration : 1f);
            yield return null;
        }

        SetIntroVisible(false);
        overlayView?.ShowRevealMode();

        float revealInterval = config != null ? config.RevealInterval : DefaultRevealInterval;
        float cardRevealDuration = config != null ? config.CardRevealDuration : DefaultCardRevealDuration;
        for (int i = 0; i < resultPopup.ResultCount; i++)
        {
            resultPopup.Reveal(i, cardRevealDuration);

            if (revealInterval > 0f && i < resultPopup.ResultCount - 1)
                yield return new WaitForSecondsRealtime(revealInterval);
        }

        CompletePresentation();
    }

    private void PlayIntro(Rarity rarity)
    {
        SetIntroVisible(introRoot != null);
        overlayView?.ShowIntro(rarity, GetRarityColor(rarity));

        if (introAnimator != null && config != null)
        {
            string trigger = config.GetAnimatorTrigger(rarity);
            if (!string.IsNullOrWhiteSpace(trigger))
                introAnimator.SetTrigger(trigger);
        }

        if (audioSource != null && config != null)
        {
            AudioClip clip = config.GetIntroClip(rarity);
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }

    private void StopCurrentPresentation(bool revealAll)
    {
        if (!isPresenting)
            return;

        if (presentationRoutine != null)
            StopCoroutine(presentationRoutine);

        presentationRoutine = null;
        isPresenting = false;
        SetIntroVisible(false);
        overlayView?.Hide();

        if (revealAll)
        {
            if (resultPopup != null)
                resultPopup.RevealAll();

            PresentationCompleted?.Invoke();
        }
    }

    private void CompletePresentation()
    {
        presentationRoutine = null;
        isPresenting = false;
        overlayView?.Hide();
        if (resultPopup != null)
            resultPopup.RevealAll();

        PresentationCompleted?.Invoke();
    }

    private void SetIntroVisible(bool visible)
    {
        if (introRoot != null)
            introRoot.SetActive(visible);
    }

    private static Rarity GetHighestRarity(IReadOnlyList<GachaResult> results)
    {
        Rarity highest = Rarity.Normal;
        if (results == null)
            return highest;

        foreach (GachaResult result in results)
        {
            if (result?.Unit != null && result.Unit.rarity > highest)
                highest = result.Unit.rarity;
        }

        return highest;
    }

    private Color GetRarityColor(Rarity rarity)
    {
        if (config != null)
            return config.GetRarityColor(rarity);

        return rarity switch
        {
            Rarity.Normal => new Color(0.08f, 0.28f, 1f, 1f),
            Rarity.Rare => new Color(0.63f, 0.13f, 0.94f, 1f),
            Rarity.Legend => new Color(1f, 0.92f, 0.02f, 1f),
            _ => Color.white,
        };
    }

    private void OnDestroy()
    {
        if (resultPopup != null)
            resultPopup.SkipRequested -= Skip;

        if (overlayView != null)
            overlayView.SkipRequested -= Skip;

        if (ownsOverlayView && overlayView != null)
            Destroy(overlayView.gameObject);
    }

    private void OnDisable()
    {
        if (isPresenting)
            StopCurrentPresentation(true);
    }
}
