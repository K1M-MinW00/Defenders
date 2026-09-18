using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitLimitBreakPanel : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private TMP_Text materialText;

    [Header("Star")]
    [SerializeField] private Image[] currentStars;
    [SerializeField] private GameObject nextStarsRoot;
    [SerializeField] private Image[] nextStars;
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private Sprite emptyStarImg;
    [SerializeField] private Sprite starImg;

    [Header("Effects")]
    [SerializeField] private Transform effectRoot;
    [SerializeField] private LimitBreakEffectSlot effectPrefab;

    [Header("Button")]
    [SerializeField] private Button limitBreakButton;

    private UnitDetailView detailPanel;
    private UnitDataSO currentUnitData;
    private UserUnitData currentUnit;
    private bool isLimitBreaking;

    private void Awake()
    {
        if (limitBreakButton != null)
            limitBreakButton.onClick.AddListener(OnClickLimitBreak);
    }

    public void Bind(UnitDataSO unitData, UnitDetailView panel)
    {
        currentUnitData = unitData;
        detailPanel = panel;

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

        detailPanel?.Refresh();
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
            star.sprite = i < filledCount ? starImg : emptyStarImg;
        }
    }

    private void RefreshEffects()
    {
        if (effectRoot == null || effectPrefab == null)
            return;

        foreach (Transform child in effectRoot)
        {
            Destroy(child.gameObject);
        }

        List<LimitBreakData> datas = currentUnitData.limitBreaks;

        if (datas == null)
            return;

        for (int i = 0; i < datas.Count; i++)
        {
            LimitBreakEffectSlot slot = Instantiate(effectPrefab, effectRoot);

            bool unlocked = i < currentUnit.LimitBreak;

            slot.Setup(datas[i], unlocked);
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
            LimitBreakUnitResult result = await UserDataManager.Instance.UnitLimitBreakUseCase.ExecuteAsync(
                new LimitBreakUnitCommand(currentUnitData.unitId));

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[UnitLimitBreakPanel] Limit break failed: {result.Failure}");
                return;
            }

            Refresh();
        }
        finally
        {
            isLimitBreaking = false;

            if (limitBreakButton != null && limitBreakButton.gameObject.activeSelf && currentUnit != null)
                limitBreakButton.interactable = currentUnit.DuplicateCount > 0;
        }
    }

    private void OnDestroy()
    {
        if (limitBreakButton != null)
            limitBreakButton.onClick.RemoveListener(OnClickLimitBreak);
    }
}
