using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject unitTabPanelRoot;
    [SerializeField] private GameObject detailRoot;

    [Header("Common Top")]
    [SerializeField] private Image unitIconImage;
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
    //[SerializeField] private UnitPromotionPanel promotionPanel;
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

    private SkillViewData currentActiveSkill;
    private SkillViewData currentPassiveSkill;

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

        if (unitTabPanelRoot != null)
            unitTabPanelRoot.SetActive(false);

        if (detailRoot != null)
            detailRoot.SetActive(true);
        else
            gameObject.SetActive(true);

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

        if (unitTabPanelRoot != null)
            unitTabPanelRoot.SetActive(true);
    }

    public void Refresh()
    {
        if (currentVm == null)
            return;

        BindCommonInfo();
    }

    private void BindTabPanels()
    {
        trainingPanel?.Bind(currentVm, currentUnitData, this);
        //promotionPanel?.Bind(currentVm, currentUnitData);
        limitBreakPanel?.Bind(currentUnitData, this);
        //equipmentPanel?.Bind(currentVm, currentUnitData);
    }

    private void BindCommonInfo()
    {
        if (unitIconImage != null)
            unitIconImage.sprite = currentUnitData.icon;

        if (unitNameText != null)
            unitNameText.text = currentUnitData.displayName;

        UnitStats stats = UnitStatCalculator.Calculate(currentUnitData, currentUnit);

        if (levelText != null)
            levelText.text = $"Lv {currentUnit.Level}";

        if (attackText != null)
            attackText.text = $"{stats.Attack}";

        if (hpText != null)
            hpText.text = $"{stats.MaxHp}";
    }

    private void BindSkillInfo()
    {
        currentActiveSkill = CreateSkillViewData(currentUnitData.activeSkill);
        currentPassiveSkill = CreateSkillViewData(currentUnitData.passiveSkill);

        if (activeSkillIconImage != null)
            activeSkillIconImage.sprite = currentActiveSkill?.Icon;

        if (passiveSkillIconImage != null)
            passiveSkillIconImage.sprite = currentPassiveSkill?.Icon;
    }


    private void OpenActiveSkillPopup()
    {
        if (activeSkillDetailPopup == null || currentActiveSkill == null)
            return;

        activeSkillDetailPopup.Open(currentActiveSkill);
    }

    private void OpenPassiveSkillPopup()
    {
        if (passiveSkillDetailPopup == null || currentPassiveSkill == null)
            return;

        passiveSkillDetailPopup.Open(currentPassiveSkill);
    }

    private SkillViewData CreateSkillViewData(SkillDataSO skill)
    {
        if (skill == null)
            return null;

        return new SkillViewData
        {
            Icon = skill.icon,
            DisplayName = skill.skillName,
            Description = skill.description,
        };
    }
}