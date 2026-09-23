using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelView : MonoBehaviour
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
    [SerializeField] private Button closeButton;

    [Header("Setting")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private TMP_Dropdown languageDropdown;

    private SettingsPanelPresenter presenter;
    private bool isRefreshingUI;
    private bool isSubscribed;

    private void Awake()
    {
        ConfigureLanguageOptions();

        closeButton.onClick.AddListener(Close);
        editProfileButton.onClick.AddListener(OpenProfileEditor);
        editNicknameButton.onClick.AddListener(OpenNicknameEditor);
        copyUidButton.onClick.AddListener(CopyUserId);
        linkAccountButton.onClick.AddListener(OpenAccountLinkPopup);
        soundToggle.onValueChanged.AddListener(HandleSoundToggleChanged);
        languageDropdown.onValueChanged.AddListener(HandleLanguageDropdownChanged);
    }

    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
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

    public void Close()
    {
        panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        SettingsPanelViewState state = presenter?.Build();
        if (state == null)
            return;

        isRefreshingUI = true;
        iconImage.sprite = state.ProfileIcon;
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
        soundToggle.SetIsOnWithoutNotify(enabled);
    }

    private void HandleLanguageChanged(string languageCode)
    {
        languageDropdown.SetValueWithoutNotify(SettingsPanelPresenter.GetLanguageIndex(languageCode));
    }

    private void OnDestroy()
    {
        closeButton?.onClick.RemoveListener(Close);
        editProfileButton?.onClick.RemoveListener(OpenProfileEditor);
        editNicknameButton?.onClick.RemoveListener(OpenNicknameEditor);
        copyUidButton?.onClick.RemoveListener(CopyUserId);
        linkAccountButton?.onClick.RemoveListener(OpenAccountLinkPopup);
        soundToggle?.onValueChanged.RemoveListener(HandleSoundToggleChanged);
        languageDropdown?.onValueChanged.RemoveListener(HandleLanguageDropdownChanged);
    }
}
