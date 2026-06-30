using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject detailRoot;

    [Header("Upper Bar")]
    [SerializeField] private TMP_Text rarity_text;
    [SerializeField] private Image rarity_Img;
    [SerializeField] private Image[] limitBreak_Img;
    [SerializeField] private Sprite star_Sprite;
    [SerializeField] private Sprite emptyStar_Sprite;

    [Header("Common UIs")]
    [SerializeField] private Image unitIcon_Img;
    [SerializeField] private Image prom_Img;
    [SerializeField] private Sprite[] promotion_sprites;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text hpText;

    [Header("Skill Buttons")]
    [SerializeField] private Button activeSkillButton;
    [SerializeField] private Image activeSkillIconImage;

    [SerializeField] private Button passiveSkillButton;
    [SerializeField] private Image passiveSkillIconImage;

    [Header("Content Tabs")]
    [SerializeField] private UnitTrainingPanel trainingPanel;
    [SerializeField] private UnitPromotionPanel promotionPanel;
    [SerializeField] private UnitLimitBreakPanel limitBreakPanel;
    //[SerializeField] private UnitEquipmentPanel equipmentPanel;

    [Header("Bottom Buttons")]
    [SerializeField] private Button backButton;

    [Header("Popup")]
    [SerializeField] private SkillDetailPopup activeSkillDetailPopup;
    [SerializeField] private SkillDetailPopup passiveSkillDetailPopup;

    private LobbyUnitViewModel currentVm;
    private UnitDataSO currentUnitData;
    private UserUnitData currentUnit;

    private SkillDataSO currentActiveSkill;
    private SkillDataSO currentPassiveSkill;

    private void Awake()
    {
        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (activeSkillButton != null)
            activeSkillButton.onClick.AddListener(OpenActiveSkillPopup);

        if (passiveSkillButton != null)
            passiveSkillButton.onClick.AddListener(OpenPassiveSkillPopup);

        Close();
    }

    public void Open(LobbyUnitViewModel vm)
    {
        if (vm == null)
            return;

        currentVm = vm;
        currentUnitData = UnitDatabase.Get(vm.UnitId);
        currentUnit = UserDataManager.Instance.UserData.Roster.GetOwnedUnit(vm.UnitId);

        if (currentUnitData == null || currentUnit == null)
        {
            Debug.LogError($"[UnitDetailView] UnitData not found: {vm.UnitId}");
            return;
        }

        if (detailRoot != null)
            detailRoot.SetActive(true);
        else
            gameObject.SetActive(true);

        BindUpperUIs();
        BindTabPanels();
        BindCommonInfo();
        BindSkillInfo();
    }


    public void Close()
    {
        if (detailRoot != null)
            detailRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private void BindUpperUIs()
    {
        Rarity rarity = currentUnitData.rarity;

        rarity_text.text = rarity.ToString();
        
        switch (rarity)
        {
            case Rarity.Normal:
                rarity_Img.color = Color.blue;
                break;
            case Rarity.Rare:
                rarity_Img.color = Color.purple;
                break;
            case Rarity.Legend:
                rarity_Img.color = Color.yellow;
                break;
            default:
                rarity_Img.color = Color.white;
                break;
        }
    }

    public void Refresh()
    {
        if (currentVm == null)
            return;

        BindCommonInfo();
    }

    private void BindTabPanels()
    {
        trainingPanel?.Bind(currentUnitData, this);
        promotionPanel?.Bind(currentUnitData, this);
        limitBreakPanel?.Bind(currentUnitData, this);
        //equipmentPanel?.Bind(currentVm, currentUnitData);
    }

    private void BindCommonInfo()
    {
        if (unitIcon_Img != null)
            unitIcon_Img.sprite = currentUnitData.icon;

        if (unitNameText != null)
            unitNameText.text = currentUnitData.displayName;

        UnitStats stats = UnitStatCalculator.Calculate(currentUnitData, currentUnit);

        if (levelText != null)
            levelText.text = $"Lv {currentUnit.Level}";

        if (attackText != null)
            attackText.text = $"{stats.Attack}";

        if (hpText != null)
            hpText.text = $"{stats.MaxHp}";

        int limitBreak = currentUnit.LimitBreak;

        for (int i = 0; i < 5; i++)
        {
            limitBreak_Img[i].sprite = i < limitBreak ? star_Sprite : emptyStar_Sprite;
        }

        int promotion = currentUnit.Promotion;
        prom_Img.sprite = promotion_sprites[promotion];
    }

    private void BindSkillInfo()
    {
        currentActiveSkill = currentUnitData.activeSkill;
        currentPassiveSkill = currentUnitData.passiveSkill;
        
        activeSkillIconImage.sprite = currentActiveSkill.icon;
        passiveSkillIconImage.sprite = currentPassiveSkill.icon;
    }


    private void OpenActiveSkillPopup()
    {
        if (activeSkillDetailPopup == null || currentActiveSkill == null)
            return;

        activeSkillDetailPopup.Open(currentActiveSkill, currentUnit.Promotion);
    }

    private void OpenPassiveSkillPopup()
    {
        if (passiveSkillDetailPopup == null || currentPassiveSkill == null)
            return;

        passiveSkillDetailPopup.Open(currentPassiveSkill, currentUnit.Promotion);
    }
}