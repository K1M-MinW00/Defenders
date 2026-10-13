using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnitLimitBreakPanel : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TMP_Text materialText;

    [Header("Star")]
    [SerializeField] private Image[] currentStars;
    [SerializeField] private GameObject nextStarsRoot;
    [SerializeField] private Image[] nextStars;
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private UnitVisualConfigSO visualConfig;

    [Header("Effects")]
    [SerializeField] private Transform effectRoot;
    [SerializeField] private LimitBreakEffectSlot effectPrefab;

    [Header("Button")]
    [SerializeField] private Button limitBreakButton;

    private UnitDataSO currentUnitData;
    private UserUnitData currentUnit;
    private bool isLimitBreaking;
    private readonly List<LimitBreakEffectSlot> effectSlotPool = new();

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        limitBreakButton.onClick.AddListener(OnClickLimitBreak);
    }

    public void Bind(UnitDataSO unitData)
    {
        currentUnitData = unitData;

        Refresh();
    }

    private void Refresh()
    {
        if (currentUnitData == null || UserDataManager.Instance == null)
            return;

        currentUnit = UserDataManager.Instance.RosterService.GetUnit(currentUnitData.unitId);

        if (currentUnit == null)
        {
            Debug.LogWarning($"[UnitLimitBreakPanel] Owned unit data not found: {currentUnitData.unitId}");
            return;
        }

        ResetView();

        RefreshMaterial();
        RefreshStars();
        RefreshEffects();
    }

    private void ResetView()
    {
        unitIconImage?.gameObject.SetActive(true);
        limitBreakButton?.gameObject.SetActive(true);
        arrowObject?.SetActive(true);
        nextStarsRoot?.SetActive(true);
    }

    private void RefreshMaterial()
    {
        bool isMax = currentUnit.LimitBreak >= UnitLimitBreakUseCase.MaxLimitBreak;
     
        if (isMax)
        {
            if (materialText != null)
                materialText.text = "유닛이 이미 최고 품질에 도달했습니다.";

            unitIconImage?.gameObject.SetActive(false);
            limitBreakButton?.gameObject.SetActive(false);
            return;
        }

        if (unitIconImage != null)
            unitIconImage.sprite = currentUnitData.icon;
        if (materialText != null)
            materialText.text = $"{currentUnit.DuplicateCount} / 1";
        if (limitBreakButton != null)
            limitBreakButton.interactable = currentUnit.DuplicateCount >= 1;
    }

    private void RefreshStars()
    {
        int current = currentUnit.LimitBreak;
        int next = Mathf.Min(current + 1, UnitLimitBreakUseCase.MaxLimitBreak);

        SetStars(currentStars, current);

        if (current >= UnitLimitBreakUseCase.MaxLimitBreak)
        {
            arrowObject?.SetActive(false);
            nextStarsRoot?.SetActive(false);

            return;
        }

        SetStars(nextStars, next);
    }

    private void SetStars(Image[] stars, int filledCount)
    {
        if (stars == null)
            return;

        for (int i = 0; i < stars.Length; i++)
        {
            Image star = stars[i];

            if (star == null)
                continue;

            star.gameObject.SetActive(true);
            star.color = Color.white;
            if (visualConfig != null)
                star.sprite = i < filledCount ? visualConfig.FilledStarSprite : visualConfig.EmptyStarSprite;
        }
    }

    private void RefreshEffects()
    {
        if (effectRoot == null || effectPrefab == null)
            return;

        HideEffectSlots();

        List<LimitBreakData> datas = currentUnitData.limitBreaks;

        if (datas == null)
            return;

        for (int i = 0; i < datas.Count; i++)
        {
            LimitBreakEffectSlot slot = GetOrCreateEffectSlot(i);
            bool unlocked = i < currentUnit.LimitBreak;

            slot.gameObject.SetActive(true);
            slot.Setup(datas[i], unlocked);
        }
    }

    private LimitBreakEffectSlot GetOrCreateEffectSlot(int index)
    {
        if (index < effectSlotPool.Count)
            return effectSlotPool[index];

        LimitBreakEffectSlot slot = Instantiate(effectPrefab, effectRoot);
        slot.name = $"LimitBreakEffectSlot_{effectSlotPool.Count + 1}";
        effectSlotPool.Add(slot);
        return slot;
    }

    private void HideEffectSlots()
    {
        for (int i = 0; i < effectSlotPool.Count; i++)
        {
            if (effectSlotPool[i] != null)
                effectSlotPool[i].gameObject.SetActive(false);
        }
    }

    private async void OnClickLimitBreak()
    {
        if (isLimitBreaking || currentUnitData == null)
            return;

        isLimitBreaking = true;
        if (limitBreakButton != null)
            limitBreakButton.interactable = false;

        try
        {
            LimitBreakUnitResult result = await UserDataManager.Instance.LimitBreakUnitAsync(
                new LimitBreakUnitCommand(currentUnitData.unitId));

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[UnitLimitBreakPanel] Limit break failed: {result.Failure}");
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(result.Failure));
                return;
            }

        }
        finally
        {
            isLimitBreaking = false;

            if (limitBreakButton != null && limitBreakButton.gameObject.activeSelf && currentUnit != null)
                limitBreakButton.interactable = currentUnit.DuplicateCount > 0;
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = unitIconImage != null && materialText != null &&
                       HasValidStars(currentStars) && nextStarsRoot != null && HasValidStars(nextStars) &&
                       arrowObject != null && visualConfig != null &&
                       effectRoot != null && effectPrefab != null && limitBreakButton != null;

        if (!isValid)
            Debug.LogError("[UnitLimitBreakPanel] One or more required UI references are missing.", this);

        return isValid;
    }

    private static bool HasValidStars(Image[] stars)
    {
        if (stars == null || stars.Length == 0)
            return false;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (limitBreakButton != null)
            limitBreakButton.onClick.RemoveListener(OnClickLimitBreak);
    }
}
