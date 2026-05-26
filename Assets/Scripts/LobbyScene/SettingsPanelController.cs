using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject editProfileRoot;
    [SerializeField] private GameObject editNicknameRoot;

    [Header("Profile")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text uidText;
    [SerializeField] private Button editProfileButton;
    [SerializeField] private Button editNicknameButton;
    [SerializeField] private Button copyUidButton;
    [SerializeField] private Button linkAccountButton;

    [Header("Settings")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("References")]
    [SerializeField] private LocalSettingsManager localSettingsManager;
    [SerializeField] private ProfileIconDatabase iconDatabase;

    private bool suppressEvents;

    private void Awake()
    {
        root.SetActive(false);

        closeButton.onClick.AddListener(Close);

        editProfileButton.onClick.AddListener(OnClickEditProfile);
        editNicknameButton.onClick.AddListener(OnClickEditNickname);
        copyUidButton.onClick.AddListener(OnClickCopyUid);
        linkAccountButton.onClick.AddListener(OnClickLinkAccount);

        soundToggle.onValueChanged.AddListener(OnSoundChanged);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Close()
    {
        root.SetActive(false);
    }

    public void Refresh()
    {
        suppressEvents = true;

        var profile = UserDataManager.Instance.UserData.Profile;

        nicknameText.text = profile.Nickname;
        uidText.text = profile.UserId;
        levelText.text = profile.Level.ToString();
        Sprite icon = iconDatabase.GetIcon(profile.IconId);
        iconImage.sprite = icon;

        soundToggle.isOn = localSettingsManager.SoundEnabled;
        languageDropdown.value = GetLanguageDropdownIndex(localSettingsManager.LanguageCode);

        suppressEvents = false;
    }

    private void OnClickEditProfile()
    {
        editProfileRoot.SetActive(true);
    }

    private void OnClickEditNickname()
    {
        editNicknameRoot.SetActive(true);
    }

    private void OnClickCopyUid()
    {
        var uid = UserDataManager.Instance.UserData.Profile.UserId;
        GUIUtility.systemCopyBuffer = uid;

        Debug.Log($"UID copied: {uid}");
    }

    private void OnClickLinkAccount()
    {
        // TODO: AccountLinkPanel 열기
        // Google / Apple 연동은 AuthManager에서 처리
    }

    private void OnSoundChanged(bool isOn)
    {
        if (suppressEvents)
            return;

        localSettingsManager.SetSoundEnabled(isOn);
    }

    private void OnLanguageChanged(int index)
    {
        if (suppressEvents)
            return;

        string languageCode = GetLanguageCodeByIndex(index);
        localSettingsManager.SetLanguage(languageCode);
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