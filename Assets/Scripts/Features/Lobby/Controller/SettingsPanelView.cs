using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelView : MonoBehaviour, IModalDismissHandler
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
    private bool isRefreshingUI;
    private bool isSubscribed;
    private ModalOverlayLifecycle lifecycle;
    private GameObject Root => panelRoot != null ? panelRoot : gameObject;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(Root);

    private void Awake()
    {
        ConfigureLanguageOptions();

        editProfileButton?.onClick.AddListener(OpenProfileEditor);
        editNicknameButton?.onClick.AddListener(OpenNicknameEditor);
        copyUidButton?.onClick.AddListener(CopyUserId);
        linkAccountButton?.onClick.AddListener(OpenAccountLinkPopup);
        soundToggle?.onValueChanged.AddListener(HandleSoundToggleChanged);
        languageDropdown?.onValueChanged.AddListener(HandleLanguageDropdownChanged);
    }

    public void Open()
    {
        Lifecycle.Show();
    }

    private void OnEnable()
    {
        if (UserDataManager.Instance?.UserData == null || GameSettingsManager.Instance == null)
            return;

        presenter ??= new SettingsPanelPresenter(UserDataManager.Instance.UserData, GameSettingsManager.Instance);
        SubscribeEvents();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
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
        if (iconImage != null) iconImage.sprite = state.ProfileIcon;
        if (nicknameText != null) nicknameText.text = state.Nickname;
        if (uidText != null) uidText.text = state.UserId;
        if (levelText != null) levelText.text = state.Level.ToString();
        soundToggle?.SetIsOnWithoutNotify(state.SoundEnabled);
        languageDropdown?.SetValueWithoutNotify(state.LanguageIndex);
        isRefreshingUI = false;
    }

    private void ConfigureLanguageOptions()
    {
        if (languageDropdown == null)
            return;

        languageDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new();
        foreach (string label in SettingsPanelPresenter.LanguageLabels)
            options.Add(new TMP_Dropdown.OptionData(label));
        languageDropdown.AddOptions(options);
    }

    private void OpenProfileEditor()
    {
        editProfilePopup?.Open();
    }

    private void OpenNicknameEditor()
    {
        editNicknamePopup?.Open();
    }

    private void CopyUserId()
    {
        string userId = UserDataManager.Instance?.UserData?.Profile?.UserId;
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
        accountLinkPopup?.Open();
    }

    private void CleanupChildPopups()
    {
        editProfilePopup?.HideImmediately();
        editNicknamePopup?.HideImmediately();
        accountLinkPopup?.HideImmediately();
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

        UserDataManager.Instance.OnProfileUpdated += Refresh;
        GameSettingsManager.Instance.OnSoundChanged += HandleSoundChanged;
        GameSettingsManager.Instance.OnLanguageChanged += HandleLanguageChanged;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed)
            return;

        if (UserDataManager.Instance != null)
            UserDataManager.Instance.OnProfileUpdated -= Refresh;

        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.OnSoundChanged -= HandleSoundChanged;
            GameSettingsManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }

        isSubscribed = false;
    }

    private void HandleSoundChanged(bool enabled)
    {
        soundToggle?.SetIsOnWithoutNotify(enabled);
    }

    private void HandleLanguageChanged(string languageCode)
    {
        languageDropdown?.SetValueWithoutNotify(SettingsPanelPresenter.GetLanguageIndex(languageCode));
    }

    private void OnDestroy()
    {
        editProfileButton?.onClick.RemoveListener(OpenProfileEditor);
        editNicknameButton?.onClick.RemoveListener(OpenNicknameEditor);
        copyUidButton?.onClick.RemoveListener(CopyUserId);
        linkAccountButton?.onClick.RemoveListener(OpenAccountLinkPopup);
        soundToggle?.onValueChanged.RemoveListener(HandleSoundToggleChanged);
        languageDropdown?.onValueChanged.RemoveListener(HandleLanguageDropdownChanged);
    }
}
