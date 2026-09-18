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

    private UnitDetailPresenter presenter;
    private UnitDetailViewState currentState;
    private string currentUnitId;

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

        currentUnitId = vm.UnitId;
        presenter ??= new UnitDetailPresenter(UserDataManager.Instance.RosterService);
        currentState = presenter.Build(currentUnitId);

        if (currentState == null)
        {
            Debug.LogError($"[UnitDetailView] UnitData not found: {vm.UnitId}");
            return;
        }

        if (detailRoot != null)
            detailRoot.SetActive(true);
        else
            gameObject.SetActive(true);

        Render();
        BindTabPanels();
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
        Rarity rarity = currentState.Rarity;

        if (rarity_text != null)
            rarity_text.text = rarity.ToString();
        
        switch (rarity)
        {
            case Rarity.Normal:
                if (rarity_Img != null) rarity_Img.color = Color.blue;
                break;
            case Rarity.Rare:
                if (rarity_Img != null) rarity_Img.color = Color.purple;
                break;
            case Rarity.Legend:
                if (rarity_Img != null) rarity_Img.color = Color.yellow;
                break;
            default:
                if (rarity_Img != null) rarity_Img.color = Color.white;
                break;
        }
    }

    public void Refresh()
    {
        if (presenter == null || string.IsNullOrWhiteSpace(currentUnitId))
            return;

        currentState = presenter.Build(currentUnitId);

        if (currentState == null)
        {
            Debug.LogWarning($"[UnitDetailView] Cannot refresh unit: {currentUnitId}");
            Close();
            return;
        }

        Render();
    }

    private void BindTabPanels()
    {
        trainingPanel?.Bind(currentState.Definition, this);
        promotionPanel?.Bind(currentState.Definition, this);
        limitBreakPanel?.Bind(currentState.Definition, this);
    }

    private void BindCommonInfo()
    {
        if (unitIcon_Img != null)
            unitIcon_Img.sprite = currentState.Icon;

        if (unitNameText != null)
            unitNameText.text = currentState.DisplayName;

        if (levelText != null)
            levelText.text = $"Lv {currentState.Level}";

        if (attackText != null)
            attackText.text = $"{currentState.Attack}";

        if (hpText != null)
            hpText.text = $"{currentState.MaxHp}";

        int limitBreak = currentState.LimitBreak;

        for (int i = 0; i < limitBreak_Img.Length; i++)
        {
            if (limitBreak_Img[i] != null)
                limitBreak_Img[i].sprite = i < limitBreak ? star_Sprite : emptyStar_Sprite;
        }

        int promotion = currentState.Promotion;

        if (prom_Img != null && promotion_sprites != null &&
            promotion >= 0 && promotion < promotion_sprites.Length)
        {
            prom_Img.sprite = promotion_sprites[promotion];
        }
    }

    private void BindSkillInfo()
    {
        if (activeSkillIconImage != null)
            activeSkillIconImage.sprite = currentState.ActiveSkill?.icon;

        if (passiveSkillIconImage != null)
            passiveSkillIconImage.sprite = currentState.PassiveSkill?.icon;
    }

    private void Render()
    {
        BindUpperUIs();
        BindCommonInfo();
        BindSkillInfo();
    }


    private void OpenActiveSkillPopup()
    {
        if (activeSkillDetailPopup == null || currentState?.ActiveSkill == null)
            return;

        activeSkillDetailPopup.Open(currentState.ActiveSkill, currentState.Promotion);
    }

    private void OpenPassiveSkillPopup()
    {
        if (passiveSkillDetailPopup == null || currentState?.PassiveSkill == null)
            return;

        passiveSkillDetailPopup.Open(currentState.PassiveSkill, currentState.Promotion);
    }
}
