using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RecruitPresentationController : MonoBehaviour
{
    private const float DefaultIntroDuration = 0.65f;
    private const float DefaultRevealInterval = 0.18f;

    [Header("Optional Intro Presentation")]
    [SerializeField] private GameObject introRoot;
    [SerializeField] private Animator introAnimator;
    [SerializeField] private AudioSource audioSource;

    private RecruitResultPopupView resultPopup;
    private RecruitPresentationConfigSO config;
    private Coroutine presentationRoutine;
    private bool isPresenting;

    public bool IsPresenting => isPresenting;
    public event Action PresentationCompleted;

    public void Initialize(RecruitResultPopupView popup, RecruitPresentationConfigSO presentationConfig)
    {
        if (resultPopup != null)
            resultPopup.SkipRequested -= Skip;

        resultPopup = popup;
        config = presentationConfig;

        if (resultPopup != null)
            resultPopup.SkipRequested += Skip;
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
        if (introDuration > 0f)
            yield return new WaitForSecondsRealtime(introDuration);

        SetIntroVisible(false);

        float revealInterval = config != null ? config.RevealInterval : DefaultRevealInterval;
        for (int i = 0; i < resultPopup.ResultCount; i++)
        {
            resultPopup.Reveal(i);

            if (revealInterval > 0f && i < resultPopup.ResultCount - 1)
                yield return new WaitForSecondsRealtime(revealInterval);
        }

        CompletePresentation();
    }

    private void PlayIntro(Rarity rarity)
    {
        SetIntroVisible(introRoot != null);

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

    private void OnDestroy()
    {
        if (resultPopup != null)
            resultPopup.SkipRequested -= Skip;
    }

    private void OnDisable()
    {
        if (isPresenting)
            StopCurrentPresentation(true);
    }
}
