using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailPanel : MonoBehaviour
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
    [SerializeField] private GameObject trainingTabPanel;
    [SerializeField] private GameObject promotionTabPanel;
    [SerializeField] private GameObject limitBreakTabPanel;
    [SerializeField] private GameObject equipmentTabPanel;

    [Header("Bottom Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private UnitDetailTabButton trainingTabButton;
    [SerializeField] private UnitDetailTabButton promotionTabButton;
    [SerializeField] private UnitDetailTabButton limitBreakTabButton;
    [SerializeField] private UnitDetailTabButton equipmentTabButton;

    [Header("Popup")]
    [SerializeField] private SkillDetailPopup activeSkillDetailPopup;
    [SerializeField] private SkillDetailPopup passiveSkillDetailPopup;

    private LobbyUnitViewModel currentVm;
    private UnitDataSO currentUnitData;

    private SkillViewData currentActiveSkill;
    private SkillViewData currentPassiveSkill;

    private void Awake()
    {
        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (trainingTabButton != null)
            trainingTabButton.Button.onClick.AddListener(() => ShowTab(UnitDetailTabType.Training));

        if (promotionTabButton != null)
            promotionTabButton.Button.onClick.AddListener(() => ShowTab(UnitDetailTabType.Promotion));

        if (limitBreakTabButton != null)
            limitBreakTabButton.Button.onClick.AddListener(() => ShowTab(UnitDetailTabType.LimitBreak));

        if (equipmentTabButton != null)
            equipmentTabButton.Button.onClick.AddListener(() => ShowTab(UnitDetailTabType.Equipment));

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
        currentUnitData = UnitMasterDataManager.Instance.GetUnitData(vm.UnitId);

        if (currentUnitData == null)
        {
            Debug.LogError($"[UnitDetailPanel] UnitDataSO not found: {vm.UnitId}");
            return;
        }

        if (unitTabPanelRoot != null)
            unitTabPanelRoot.SetActive(false);

        if (detailRoot != null)
            detailRoot.SetActive(true);
        else
            gameObject.SetActive(true);

        BindCommonInfo();
        BindSkillInfo();

        ShowTab(UnitDetailTabType.Training);
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

    private void BindCommonInfo()
    {
        if (unitIconImage != null)
            unitIconImage.sprite = currentUnitData.icon;

        if (unitNameText != null)
            unitNameText.text = currentUnitData.displayName;

        if (!currentVm.IsOwned)
        {
            if (levelText != null)
                levelText.text = "Lv -";

            if (attackText != null)
                attackText.text = "공격력 -";

            if (hpText != null)
                hpText.text = "체력 -";

            return;
        }

        if (levelText != null)
            levelText.text = $"Lv {currentVm.Level}";

        // 아래 계산식은 프로젝트의 UnitDataSO 구조에 맞게 교체 필요
        int attack = CalculateLobbyAttack(currentUnitData, currentVm.Level);
        int hp = CalculateLobbyHp(currentUnitData, currentVm.Level);

        if (attackText != null)
            attackText.text = $"공격력 {attack}";

        if (hpText != null)
            hpText.text = $"체력 {hp}";
    }

    private void BindSkillInfo()
    {
        currentActiveSkill = CreateActiveSkillViewData(currentUnitData);
        currentPassiveSkill = CreatePassiveSkillViewData(currentUnitData);

        if (activeSkillIconImage != null)
            activeSkillIconImage.sprite = currentActiveSkill?.Icon;

        if (passiveSkillIconImage != null)
            passiveSkillIconImage.sprite = currentPassiveSkill?.Icon;
    }

    private void ShowTab(UnitDetailTabType tabType)
    {
        if (trainingTabPanel != null)
            trainingTabPanel.SetActive(tabType == UnitDetailTabType.Training);

        if (promotionTabPanel != null)
            promotionTabPanel.SetActive(tabType == UnitDetailTabType.Promotion);

        if (limitBreakTabPanel != null)
            limitBreakTabPanel.SetActive(tabType == UnitDetailTabType.LimitBreak);

        if (equipmentTabPanel != null)
            equipmentTabPanel.SetActive(tabType == UnitDetailTabType.Equipment);

        if (trainingTabButton != null)
            trainingTabButton.SetSelected(tabType == UnitDetailTabType.Training);

        if (promotionTabButton != null)
            promotionTabButton.SetSelected(tabType == UnitDetailTabType.Promotion);

        if (limitBreakTabButton != null)
            limitBreakTabButton.SetSelected(tabType == UnitDetailTabType.LimitBreak);

        if (equipmentTabButton != null)
            equipmentTabButton.SetSelected(tabType == UnitDetailTabType.Equipment);
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

    private int CalculateLobbyAttack(UnitDataSO unitData, int level)
    {
        // TODO: 실제 UnitDataSO 스탯 구조에 맞게 교체
        // 예: baseAtk + atkPerLevel * (level - 1)
        return 0;
    }

    private int CalculateLobbyHp(UnitDataSO unitData, int level)
    {
        // TODO: 실제 UnitDataSO 스탯 구조에 맞게 교체
        return 0;
    }

    private SkillViewData CreateActiveSkillViewData(UnitDataSO unitData)
    {
        if (unitData.activeSkill == null)
            return null;

        return new SkillViewData
        {
            Icon = unitData.activeSkill.icon,
            DisplayName = unitData.activeSkill.skillName,
            Description = unitData.activeSkill.description,
        };
    }

    private SkillViewData CreatePassiveSkillViewData(UnitDataSO unitData)
    {
        if (unitData.passiveSkill == null)
            return null;

        return new SkillViewData
        {
            Icon = unitData.passiveSkill.icon,
            DisplayName = unitData.passiveSkill.skillName,
            Description = unitData.passiveSkill.description,
        };
    }
}
public enum UnitDetailTabType
{
    Training,
    Promotion,
    LimitBreak,
    Equipment
}