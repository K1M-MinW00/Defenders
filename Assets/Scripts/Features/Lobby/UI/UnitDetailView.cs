using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnitDetailView : MonoBehaviour, IModalDismissHandler
{
    [Header("Root")]
    [SerializeField] private GameObject detailRoot;

    [Header("Upper Bar")]
    [FormerlySerializedAs("rarity_text")]
    [SerializeField] private TMP_Text rarityText;
    [FormerlySerializedAs("rarity_Img")]
    [SerializeField] private Image rarityBackgroundImage;
    [FormerlySerializedAs("limitBreak_Img")]
    [SerializeField] private Image[] limitBreakStarImages;
    [SerializeField] private UnitVisualConfigSO visualConfig;

    [Header("Common UIs")]
    [FormerlySerializedAs("unitIcon_Img")]
    [SerializeField] private Image unitIconImage;
    [FormerlySerializedAs("prom_Img")]
    [SerializeField] private Image promotionImage;
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
    [SerializeField] private UnitEquipmentPanel equipmentPanel;

    [Header("Bottom Buttons")]
    [SerializeField] private Button backButton;

    [Header("Popup")]
    [SerializeField] private SkillDetailPopup activeSkillDetailPopup;
    [SerializeField] private SkillDetailPopup passiveSkillDetailPopup;

    private UnitDetailPresenter presenter;
    private UnitDetailViewState currentState;
    private string currentUnitId;
    private UserDataManager userDataManager;
    private bool isRosterSubscribed;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            SetVisibleImmediately(false);
            enabled = false;
            return;
        }

        backButton.onClick.AddListener(Dismiss);
        activeSkillButton.onClick.AddListener(OpenActiveSkillPopup);
        passiveSkillButton.onClick.AddListener(OpenPassiveSkillPopup);

        SetVisibleImmediately(false);
    }

    public void Open(LobbyUnitViewModel vm)
    {
        if (vm == null)
            return;

        currentUnitId = vm.UnitId;
        userDataManager ??= UserDataManager.Instance;
        if (userDataManager?.UserData == null || !userDataManager.IsLoaded)
        {
            Debug.LogError("[UnitDetailView] Loaded user data is required.", this);
            return;
        }

        presenter ??= new UnitDetailPresenter(
            userDataManager.RosterService,
            LabBonusProvider.CreateSnapshot(userDataManager.UserData, GameConfig.Lab));
        SubscribeToRosterUpdates();
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


    public void Dismiss()
    {
        GameObject root = detailRoot != null ? detailRoot : gameObject;
        ModalTransitionUtility.RequestExit(root, () =>
        {
            root.SetActive(false);
            UnsubscribeFromRosterUpdates();
        });
    }

    public void Close() => Dismiss();

    private void SetVisibleImmediately(bool visible)
    {
        GameObject root = detailRoot != null ? detailRoot : gameObject;
        root.SetActive(visible);
    }

    private void BindUpperUIs()
    {
        Rarity rarity = currentState.Rarity;

        if (rarityText != null)
            rarityText.text = rarity.ToString();
        
        if (rarityBackgroundImage != null)
            rarityBackgroundImage.color = visualConfig != null ? visualConfig.GetRarityColor(rarity) : Color.white;
    }

    public void Refresh()
    {
        if (presenter == null || string.IsNullOrWhiteSpace(currentUnitId))
            return;

        currentState = presenter.Build(currentUnitId);

        if (currentState == null)
        {
            Debug.LogWarning($"[UnitDetailView] Cannot refresh unit: {currentUnitId}");
            Dismiss();
            return;
        }

        Render();
        BindTabPanels();
    }

    private void BindTabPanels()
    {
        trainingPanel?.Bind(currentState.Definition, userDataManager);
        promotionPanel?.Bind(currentState.Definition, userDataManager);
        limitBreakPanel?.Bind(currentState.Definition);
        equipmentPanel?.Bind(currentUnitId);
    }

    private void BindCommonInfo()
    {
        if (unitIconImage != null)
            unitIconImage.sprite = currentState.Icon;

        if (unitNameText != null)
            unitNameText.text = currentState.DisplayName;

        if (levelText != null)
            levelText.text = $"Lv {currentState.Level}";

        if (attackText != null)
            attackText.text = $"{currentState.Attack}";

        if (hpText != null)
            hpText.text = $"{currentState.MaxHp}";

        int limitBreak = currentState.LimitBreak;

        for (int i = 0; i < limitBreakStarImages.Length; i++)
        {
            if (limitBreakStarImages[i] != null && visualConfig != null)
                limitBreakStarImages[i].sprite = i < limitBreak
                    ? visualConfig.FilledStarSprite
                    : visualConfig.EmptyStarSprite;
        }

        int promotion = currentState.Promotion;
        Sprite promotionSprite = visualConfig?.GetPromotionSprite(promotion);

        if (promotionImage != null && promotionSprite != null)
            promotionImage.sprite = promotionSprite;
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

    private void SubscribeToRosterUpdates()
    {
        if (isRosterSubscribed || userDataManager == null)
            return;

        userDataManager.OnRosterUpdated += HandleRosterUpdated;
        isRosterSubscribed = true;
    }

    private void UnsubscribeFromRosterUpdates()
    {
        if (!isRosterSubscribed || userDataManager == null)
            return;

        userDataManager.OnRosterUpdated -= HandleRosterUpdated;
        isRosterSubscribed = false;
    }

    private void HandleRosterUpdated()
    {
        bool isOpen = detailRoot != null ? detailRoot.activeSelf : gameObject.activeSelf;

        if (isOpen)
            Refresh();
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

    private bool ValidateReferences()
    {
        bool hasStars = limitBreakStarImages != null && limitBreakStarImages.Length > 0;
        if (hasStars)
        {
            for (int i = 0; i < limitBreakStarImages.Length; i++)
                hasStars &= limitBreakStarImages[i] != null;
        }

        bool isValid = detailRoot != null && rarityText != null && rarityBackgroundImage != null &&
                       hasStars && visualConfig != null && unitIconImage != null && promotionImage != null &&
                       unitNameText != null && levelText != null && attackText != null && hpText != null &&
                       activeSkillButton != null && activeSkillIconImage != null &&
                       passiveSkillButton != null && passiveSkillIconImage != null &&
                       trainingPanel != null && promotionPanel != null && limitBreakPanel != null &&
                       equipmentPanel != null && backButton != null &&
                       activeSkillDetailPopup != null && passiveSkillDetailPopup != null;

        if (!isValid)
            Debug.LogError("[UnitDetailView] One or more required scene references are missing.", this);

        return isValid;
    }

    private void OnDisable()
    {
        UnsubscribeFromRosterUpdates();
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(Dismiss);

        if (activeSkillButton != null)
            activeSkillButton.onClick.RemoveListener(OpenActiveSkillPopup);

        if (passiveSkillButton != null)
            passiveSkillButton.onClick.RemoveListener(OpenPassiveSkillPopup);

        UnsubscribeFromRosterUpdates();
    }
}
