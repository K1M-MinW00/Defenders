using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditNicknamePanelView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Input")]
    [SerializeField] private TMP_InputField nicknameInput;

    [Header("Button")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    [Header("Validation")]
    [SerializeField] private int minLength = 2;
    [SerializeField] private int maxLength = 12;

    private bool isSaving;

    private void Awake()
    {
        panelRoot.SetActive(false);

        cancelButton.onClick.AddListener(Close);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);

        nicknameInput.onValueChanged.AddListener(HandleCicknameInputChanged);
    }

    private void OnEnable()
    {
        string currentNickname = UserDataManager.Instance.UserData.Profile.Nickname;
        nicknameInput.text = currentNickname;

        confirmButton.interactable = true;

        nicknameInput.ActivateInputField();
    }

    private void Close()
    {
        if (isSaving)
            return;

        panelRoot.SetActive(false);
    }

    private void HandleCicknameInputChanged(string value)
    {
        confirmButton.interactable = ValidateNickname(value);
    }

    private async void HandleConfirmButtonClicked()
    {
        if (isSaving)
            return;

        string nickname = nicknameInput.text.Trim();

        if (!ValidateNickname(nickname))
            return;

        string currentNickname = UserDataManager.Instance.UserData.Profile.Nickname;

        if (nickname == currentNickname)
        {
            Close();
            return;
        }

        isSaving = true;

        confirmButton.interactable = false;
        cancelButton.interactable = false;

        await UserDataManager.Instance.UpdateNicknameAsync(nickname);

        confirmButton.interactable = true;
        cancelButton.interactable = true;

        isSaving = false;
        Close();
    }

    private bool ValidateNickname(string nickname)
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