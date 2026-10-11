using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RecruitPresentationController : MonoBehaviour
{
    private const float DefaultNewUnitDetailDuration = 2f;
    private const float DefaultRevealInterval = 0.18f;
    private const float DefaultCardRevealDuration = 0.22f;

    [Header("Optional Runtime Overlay Override")]
    [SerializeField] private RecruitPresentationOverlayView overlayView;

    private RecruitResultPopupView resultPopup;
    private RecruitPresentationConfigSO config;
    private Coroutine presentationRoutine;
    private bool isPresenting;

    public bool IsPresenting => isPresenting;
    public event Action PresentationCompleted;

    public void Initialize(
        RecruitResultPopupView popup,
        RecruitPresentationConfigSO presentationConfig)
    {
        if (resultPopup != null)
            resultPopup.SkipRequested -= Skip;

        resultPopup = popup;
        config = presentationConfig;

        if (resultPopup != null)
        {
            resultPopup.SkipRequested += Skip;

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
        List<UnitDataSO> introUnits = SelectNewHighestRarityUnits(results);
        float detailDuration = config != null
            ? config.NewUnitDetailDuration
            : DefaultNewUnitDetailDuration;

        foreach (UnitDataSO unit in introUnits)
        {
            PlayUnitIntro(unit);

            float elapsed = 0f;
            while (elapsed < detailDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                overlayView?.SetIntroProgress(detailDuration > 0f ? elapsed / detailDuration : 1f);
                yield return null;
            }
        }

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

    private void PlayUnitIntro(UnitDataSO unit)
    {
        if (unit == null)
            return;

        Rarity rarity = unit.rarity;
        overlayView?.ShowUnitIntro(unit, GetRarityColor(rarity));
    }

    private void StopCurrentPresentation(bool revealAll)
    {
        if (!isPresenting)
            return;

        if (presentationRoutine != null)
            StopCoroutine(presentationRoutine);

        presentationRoutine = null;
        isPresenting = false;
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

    private static List<UnitDataSO> SelectNewHighestRarityUnits(IReadOnlyList<GachaResult> results)
    {
        List<UnitDataSO> selectedUnits = new();
        if (results == null)
            return selectedUnits;

        bool foundNewUnit = false;
        Rarity highest = Rarity.Normal;

        foreach (GachaResult result in results)
        {
            if (result?.Unit == null || !result.IsNewUnit)
                continue;

            if (!foundNewUnit || result.Unit.rarity > highest)
            {
                highest = result.Unit.rarity;
                foundNewUnit = true;
            }
        }

        if (!foundNewUnit)
            return selectedUnits;

        HashSet<string> addedUnitIds = new(StringComparer.Ordinal);
        foreach (GachaResult result in results)
        {
            UnitDataSO unit = result?.Unit;
            if (unit == null || !result.IsNewUnit || unit.rarity != highest)
                continue;

            if (addedUnitIds.Add(unit.unitId))
                selectedUnits.Add(unit);
        }

        return selectedUnits;
    }

    private Color GetRarityColor(Rarity rarity)
    {
        if (config != null)
            return config.GetRarityColor(rarity);

        return RarityPalette.Get(rarity);
    }

    private void OnDestroy()
    {
        if (resultPopup != null)
            resultPopup.SkipRequested -= Skip;

        if (overlayView != null)
            overlayView.SkipRequested -= Skip;

    }

    private void OnDisable()
    {
        if (isPresenting)
            StopCurrentPresentation(true);
    }
}
