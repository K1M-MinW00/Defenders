using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameEditPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Input")]
    [SerializeField] private TMP_InputField nicknameInput;

    [Header("Buttons")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    [Header("References")]
    [SerializeField] private SettingsPanelController settingsPanel;

    [Header("Validation")]
    [SerializeField] private int minLength = 2;
    [SerializeField] private int maxLength = 12;

    private bool isSaving;

    private void Awake()
    {
        root.SetActive(false);

        cancelButton.onClick.AddListener(Close);
        confirmButton.onClick.AddListener(OnClickConfirm);

        nicknameInput.onValueChanged.AddListener(OnNicknameChanged);
    }

    public void Open()
    {
        root.SetActive(true);

        string currentNickname = UserDataManager.Instance.UserData.Profile.Nickname;
        nicknameInput.text = currentNickname;

        confirmButton.interactable = true;

        nicknameInput.ActivateInputField();
    }

    private void Close()
    {
        if (isSaving)
            return;

        root.SetActive(false);
    }

    private void OnNicknameChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        ValidateNickname(value, false);
    }

    private async void OnClickConfirm()
    {
        if (isSaving)
            return;

        string nickname = nicknameInput.text.Trim();

        if (!ValidateNickname(nickname, true))
            return;

        string currentNickname = UserDataManager.Instance.UserData.Profile.Nickname;

        if (nickname == currentNickname)
        {
            root.SetActive(false);
            return;
        }

        isSaving = true;
        confirmButton.interactable = false;
        cancelButton.interactable = false;

        await UserDataManager.Instance.UpdateNicknameAsync(nickname);

        isSaving = false;
        confirmButton.interactable = true;
        cancelButton.interactable = true;

        settingsPanel.Refresh();
        root.SetActive(false);
    }

    private bool ValidateNickname(string nickname, bool showEmptyError)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            return false;
        }

        if (nickname.Length < minLength)
        {
            return false;
        }

        if (nickname.Length > maxLength)
        {
            return false;
        }

        return true;
    }

}