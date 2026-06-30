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

    private const int MaxLimitBreak = 5;


    private void Awake()
    {
        limitBreakButton.onClick.AddListener(OnClickLimitBreak);
    }

    public void Bind(UnitDataSO unitData, UnitDetailView panel)
    {
        currentUnitData = unitData;
        currentUnit = UserDataManager.Instance.UserData.Roster.GetOwnedUnit(unitData.unitId);
        detailPanel = panel;

        Refresh();
    }

    private void Refresh()
    {
        ResetView();

        RefreshMaterial();
        RefreshStars();
        RefreshEffects();

        detailPanel.Refresh();
    }

    private void ResetView()
    {
        unitIconImage.gameObject.SetActive(true);

        limitBreakButton.gameObject.SetActive(true);

        arrowObject.SetActive(true);

        nextStarsRoot.SetActive(true);
    }

    private void RefreshMaterial()
    {
        bool isMax = currentUnit.LimitBreak >= MaxLimitBreak;
     
        if (isMax)
        {
            materialText.text = "유닛이 이미 최고 품질에 도달했습니다.";

            unitIconImage.gameObject.SetActive(false);
            limitBreakButton.gameObject.SetActive(false);
            return;
        }

        unitIconImage.sprite = currentUnitData.icon;
        materialText.text = $"{currentUnit.DuplicateCount} / 1";
        limitBreakButton.interactable = currentUnit.DuplicateCount >= 1;
    }

    private void RefreshStars()
    {
        int current = currentUnit.LimitBreak;
        int next = Mathf.Min(current + 1, MaxLimitBreak);

        for (int i = 0; i < currentStars.Length; i++)
        {
            currentStars[i].sprite = i < current ? starImg : emptyStarImg;
        }

        if (current >= MaxLimitBreak)
        {
            arrowObject.SetActive(false);
            nextStarsRoot.SetActive(false);

            return;
        }

        for (int i = 0; i < nextStars.Length; i++)
        {
            nextStars[i].gameObject.SetActive(true);
            nextStars[i].sprite = i < next ? starImg : emptyStarImg;
        }
    }

    private void RefreshEffects()
    {
        foreach (Transform child in effectRoot)
        {
            Destroy(child.gameObject);
        }

        List<LimitBreakData> datas = currentUnitData.limitBreaks;

        for (int i = 0; i < datas.Count; i++)
        {
            LimitBreakEffectSlot slot = Instantiate(effectPrefab, effectRoot);

            bool unlocked = i < currentUnit.LimitBreak;

            slot.Setup(datas[i], unlocked);
        }
    }

    public void OnClickLimitBreak()
    {
        if (currentUnitData == null)
            return;

        if (currentUnit.LimitBreak >= MaxLimitBreak)
            return;

        if (currentUnit.DuplicateCount <= 0)
            return;

        bool success = UserDataManager.Instance.RosterService.TryLimitBreak(currentUnitData.unitId);

        if (!success)
            return;

        Refresh();
    }
}