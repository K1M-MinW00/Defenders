using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private GameObject editProfilePopupRoot;
    [SerializeField] private GameObject editNicknamePopupRoot;

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

    private bool isRefreshingUI;
    private GameSettingsManager Settings => GameSettingsManager.Instance;

    private void Awake()
    {
        panelRoot.SetActive(false);

        closeButton.onClick.AddListener(Close);

        editProfileButton.onClick.AddListener(OnClickEditProfile);
        editNicknameButton.onClick.AddListener(OnClickEditNickname);
        copyUidButton.onClick.AddListener(OnClickCopyUid);
        linkAccountButton.onClick.AddListener(OnClickLinkAccount);

        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
    }

    private void OnEnable()
    {
        Refresh();

        if(UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnProfileUpdated += Refresh;
        }

        if (Settings != null)
        {
            Settings.OnSoundChanged += HandleSoundChanged;
            Settings.OnLanguageChanged += HandleLanguageChanged;
        }
    }

    private void OnDisable()
    {
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnProfileUpdated -= Refresh;
        }

        if (Settings != null)
        {
            Settings.OnSoundChanged -= HandleSoundChanged;
            Settings.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    public void Close()
    {
        panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        isRefreshingUI = true;

        UserProfileData profile = UserDataManager.Instance.UserData.Profile;

        nicknameText.text = profile.Nickname;
        uidText.text = profile.UserId;
        levelText.text = profile.Level.ToString();
        
        iconImage.sprite = UnitDatabase.GetIcon(profile.IconId);
        
        soundToggle.isOn = Settings.SoundEnabled;
        languageDropdown.value = GetLanguageDropdownIndex(Settings.LanguageCode);

        isRefreshingUI = false;
    }

    private void OnClickEditProfile()
    {
        editProfilePopupRoot.SetActive(true);
    }

    private void OnClickEditNickname()
    {
        editNicknamePopupRoot.SetActive(true);
    }

    private void OnClickCopyUid()
    {
        var uid = UserDataManager.Instance.UserData.Profile.UserId;
        GUIUtility.systemCopyBuffer = uid;
    }

    private void OnClickLinkAccount()
    {
        // TODO: AccountLinkPanel 열기
        // Google / Apple 연동은 AuthManager에서 처리
    }


    private void OnSoundToggleChanged(bool isOn)
    {
        if (isRefreshingUI)
            return;

        Settings.SetSound(isOn);
    }

    private void OnLanguageDropdownChanged(int index)
    {
        if (isRefreshingUI)
            return;

        string languageCode = GetLanguageCodeByIndex(index);
        Settings.SetLanguage(languageCode);
    }

    private void HandleSoundChanged(bool enabled)
    {
        isRefreshingUI = true;
        soundToggle.isOn = enabled;
        isRefreshingUI = false;
    }

    private void HandleLanguageChanged(string languageCode)
    {
        isRefreshingUI = true;
        languageDropdown.value = GetLanguageDropdownIndex(languageCode);
        isRefreshingUI = false;
    }

    private int GetLanguageDropdownIndex(string languageCode)
    {
        return languageCode switch
        {
            "ko" => 0,
            "ja" => 1,
            "en" => 2,
            "vn" => 3,
            _ => 0
        };
    }

    private string GetLanguageCodeByIndex(int index)
    {
        return index switch
        {
            0 => "ko",
            1 => "ja",
            2 => "en",
            3 => "vn",
            _ => "ko"
        };
    }
}