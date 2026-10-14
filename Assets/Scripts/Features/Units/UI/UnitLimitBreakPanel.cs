using System.Collections.Generic;
using System;
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
    private UserDataManager userDataManager;
    private bool isLimitBreaking;
    private int operationVersion;
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

    private void OnEnable()
    {
        if (currentUnitData != null && userDataManager != null)
            Refresh();
    }

    private void OnDisable()
    {
        operationVersion++;
    }

    public bool Bind(UnitDataSO unitData, UserDataManager manager)
    {
        if (unitData == null || manager?.UserData == null || !manager.IsLoaded || manager.RosterService == null)
        {
            Debug.LogError("[UnitLimitBreakPanel] Loaded user data and unit definition are required.", this);
            limitBreakButton.interactable = false;
            return false;
        }

        currentUnitData = unitData;
        userDataManager = manager;

        Refresh();
        return currentUnit != null;
    }

    private void Refresh()
    {
        if (currentUnitData == null || userDataManager == null)
            return;

        currentUnit = userDataManager.RosterService.GetUnit(currentUnitData.unitId);

        if (currentUnit == null)
        {
            Debug.LogWarning($"[UnitLimitBreakPanel] Owned unit data not found: {currentUnitData.unitId}");
            limitBreakButton.interactable = false;
            HideEffectSlots();
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
            limitBreakButton.interactable = !isLimitBreaking && currentUnit.DuplicateCount >= 1;
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

        int slotIndex = 0;
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i] == null)
                continue;

            LimitBreakEffectSlot slot = GetOrCreateEffectSlot(slotIndex++);
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
        int currentOperation = ++operationVersion;
        if (limitBreakButton != null)
            limitBreakButton.interactable = false;

        try
        {
            LimitBreakUnitResult result = await userDataManager.LimitBreakUnitAsync(
                new LimitBreakUnitCommand(currentUnitData.unitId));

            if (!isActiveAndEnabled || currentOperation != operationVersion)
                return;

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[UnitLimitBreakPanel] Limit break failed: {result.Failure}");
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(result.Failure));
                return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (isActiveAndEnabled && currentOperation == operationVersion)
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(LimitBreakUnitFailure.SaveFailed));
        }
        finally
        {
            isLimitBreaking = false;

            if (isActiveAndEnabled && currentOperation == operationVersion)
                Refresh();
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
