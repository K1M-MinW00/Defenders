using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SettingsPanelView : MonoBehaviour, IModalDismissHandler
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private EditProfilePanelView editProfilePopup;
    [SerializeField] private EditNicknamePanelView editNicknamePopup;
    [SerializeField] private SimplePopupView accountLinkPopup;

    [Header("Profile")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text uidText;

    [Header("Button")]
    [SerializeField] private Button editProfileButton;
    [SerializeField] private Button editNicknameButton;
    [SerializeField] private Button copyUidButton;
    [SerializeField] private Button linkAccountButton;

    [Header("Setting")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private TMP_Dropdown languageDropdown;

    private SettingsPanelPresenter presenter;
    private UserDataManager userDataManager;
    private GameSettingsManager gameSettingsManager;
    private bool isRefreshingUI;
    private bool isSubscribed;
    private ModalOverlayLifecycle lifecycle;
    private GameObject Root => panelRoot != null ? panelRoot : gameObject;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(Root);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        ConfigureLanguageOptions();

        editProfileButton.onClick.AddListener(OpenProfileEditor);
        editNicknameButton.onClick.AddListener(OpenNicknameEditor);
        copyUidButton.onClick.AddListener(CopyUserId);
        linkAccountButton.onClick.AddListener(OpenAccountLinkPopup);
        soundToggle.onValueChanged.AddListener(HandleSoundToggleChanged);
        languageDropdown.onValueChanged.AddListener(HandleLanguageDropdownChanged);
    }

    public void Open()
    {
        Lifecycle.Show();
    }

    public void Initialize(UserDataManager manager, GameSettingsManager settings)
    {
        if (manager != null && userDataManager != manager)
        {
            UnsubscribeEvents();
            userDataManager = manager;
            presenter = null;
        }

        if (settings != null && gameSettingsManager != settings)
        {
            UnsubscribeEvents();
            gameSettingsManager = settings;
            presenter = null;
        }

        if (!TryInitialize())
            return;

        ConfigureChildEditors();
        if (isActiveAndEnabled)
        {
            SubscribeEvents();
            Refresh();
        }
    }

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        SubscribeEvents();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        CleanupChildPopups();
    }

    public void Dismiss()
    {
        Lifecycle.Close(CleanupChildPopups);
    }

    public void Close() => Dismiss();

    public void Refresh()
    {
        SettingsPanelViewState state = presenter?.Build();
        if (state == null)
            return;

        isRefreshingUI = true;
        iconImage.sprite = state.ProfileIcon;
        iconImage.enabled = state.ProfileIcon != null;
        nicknameText.text = state.Nickname;
        uidText.text = state.UserId;
        levelText.text = state.Level.ToString();
        soundToggle.SetIsOnWithoutNotify(state.SoundEnabled);
        languageDropdown.SetValueWithoutNotify(state.LanguageIndex);
        isRefreshingUI = false;
    }

    private void ConfigureLanguageOptions()
    {
        languageDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new();
        foreach (string label in SettingsPanelPresenter.LanguageLabels)
            options.Add(new TMP_Dropdown.OptionData(label));
        languageDropdown.AddOptions(options);
    }

    private void OpenProfileEditor()
    {
        editProfilePopup.Open();
    }

    private void OpenNicknameEditor()
    {
        editNicknamePopup.Open();
    }

    private void CopyUserId()
    {
        string userId = userDataManager?.UserData?.Profile?.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            UIFeedbackToast.Show("사용자 ID를 확인할 수 없습니다.");
            return;
        }

        GUIUtility.systemCopyBuffer = userId;
        UIFeedbackToast.Show("사용자 ID를 복사했습니다.");
    }

    private void OpenAccountLinkPopup()
    {
        accountLinkPopup.Open();
    }

    private void CleanupChildPopups()
    {
        editProfilePopup.HideImmediately();
        editNicknamePopup.HideImmediately();
        accountLinkPopup.HideImmediately();
    }

    private void HandleSoundToggleChanged(bool enabled)
    {
        if (!isRefreshingUI)
            presenter?.SetSound(enabled);
    }

    private void HandleLanguageDropdownChanged(int index)
    {
        if (!isRefreshingUI)
            presenter?.SetLanguage(index);
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
            return;

        userDataManager.OnProfileUpdated += Refresh;
        gameSettingsManager.OnSoundChanged += HandleSoundChanged;
        gameSettingsManager.OnLanguageChanged += HandleLanguageChanged;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed)
            return;

        if (userDataManager != null)
            userDataManager.OnProfileUpdated -= Refresh;

        if (gameSettingsManager != null)
        {
            gameSettingsManager.OnSoundChanged -= HandleSoundChanged;
            gameSettingsManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        isSubscribed = false;
    }

    private void HandleSoundChanged(bool enabled)
    {
        soundToggle.SetIsOnWithoutNotify(enabled);
    }

    private void HandleLanguageChanged(string languageCode)
    {
        languageDropdown.SetValueWithoutNotify(SettingsPanelPresenter.GetLanguageIndex(languageCode));
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        editProfileButton?.onClick.RemoveListener(OpenProfileEditor);
        editNicknameButton?.onClick.RemoveListener(OpenNicknameEditor);
        copyUidButton?.onClick.RemoveListener(CopyUserId);
        linkAccountButton?.onClick.RemoveListener(OpenAccountLinkPopup);
        soundToggle?.onValueChanged.RemoveListener(HandleSoundToggleChanged);
        languageDropdown?.onValueChanged.RemoveListener(HandleLanguageDropdownChanged);
    }

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        gameSettingsManager ??= GameSettingsManager.Instance;
        if (userDataManager?.UserData == null || gameSettingsManager == null)
        {
            SetInteraction(false);
            return false;
        }

        presenter ??= new SettingsPanelPresenter(userDataManager.UserData, gameSettingsManager);
        ConfigureChildEditors();
        SetInteraction(true);
        return true;
    }

    private void ConfigureChildEditors()
    {
        editProfilePopup?.Initialize(userDataManager);
        editNicknamePopup?.Initialize(userDataManager);
    }

    private void SetInteraction(bool interactable)
    {
        editProfileButton.interactable = interactable;
        editNicknameButton.interactable = interactable;
        copyUidButton.interactable = interactable;
        linkAccountButton.interactable = interactable;
        soundToggle.interactable = interactable;
        languageDropdown.interactable = interactable;
    }

    private bool ValidateReferences()
    {
        bool valid = editProfilePopup != null && editNicknamePopup != null && accountLinkPopup != null &&
                     iconImage != null && nicknameText != null && levelText != null && uidText != null &&
                     editProfileButton != null && editNicknameButton != null && copyUidButton != null &&
                     linkAccountButton != null && soundToggle != null && languageDropdown != null;
        if (!valid)
            Debug.LogError("[SettingsPanelView] Required UI references are not assigned.", this);
        return valid;
    }
}
