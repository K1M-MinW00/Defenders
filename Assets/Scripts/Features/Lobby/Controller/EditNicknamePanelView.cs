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

    private TMP_Text confirmButtonLabel;
    private string currentNickname;
    private int currentGemCost;
    private bool isSaving;

    private void Awake()
    {
        panelRoot.SetActive(false);
        nicknameInput.characterLimit = NicknamePolicy.MaxLength;
        confirmButtonLabel = confirmButton.GetComponentInChildren<TMP_Text>(true);

        cancelButton.onClick.AddListener(Close);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
        nicknameInput.onValueChanged.AddListener(HandleNicknameInputChanged);
    }

    private void OnEnable()
    {
        UserProfileData profile = UserDataManager.Instance.UserData.Profile;
        currentNickname = profile.Nickname;
        currentGemCost = profile.HasUsedFreeNicknameChange
            ? GameConfig.NewUserConfig?.NicknameChangeGemCost ?? 500
            : 0;

        nicknameInput.SetTextWithoutNotify(currentNickname);
        RefreshButton();
        nicknameInput.ActivateInputField();
    }

    private void Close()
    {
        if (!isSaving)
            panelRoot.SetActive(false);
    }

    private void HandleNicknameInputChanged(string value)
    {
        RefreshButton();
    }

    private void RefreshButton()
    {
        string normalized = NicknamePolicy.Normalize(nicknameInput.text);
        bool hasChanged = normalized != currentNickname;
        confirmButton.interactable = !isSaving && hasChanged && NicknamePolicy.IsValid(normalized);
        cancelButton.interactable = !isSaving;

        if (confirmButtonLabel != null)
        {
            confirmButtonLabel.text = currentGemCost > 0
                ? $"변경 (보석 {currentGemCost:N0})"
                : "무료 변경";
        }
    }

    private async void HandleConfirmButtonClicked()
    {
        if (isSaving)
            return;

        string nickname = NicknamePolicy.Normalize(nicknameInput.text);
        if (!NicknamePolicy.IsValid(nickname) || nickname == currentNickname)
            return;

        isSaving = true;
        RefreshButton();

        NicknameChangeResult result = await UserDataManager.Instance.UpdateNicknameAsync(nickname);

        isSaving = false;
        if (!result.Succeeded)
        {
            UIFeedbackToast.Show(GetFailureMessage(result.Failure));
            RefreshButton();
            return;
        }

        currentNickname = nickname;
        Close();
    }

    private static string GetFailureMessage(NicknameChangeFailure failure)
    {
        return failure switch
        {
            NicknameChangeFailure.InvalidNickname => "닉네임은 2~12자로 입력해주세요.",
            NicknameChangeFailure.InsufficientGem => "닉네임 변경에 필요한 보석이 부족합니다.",
            NicknameChangeFailure.NicknameAlreadyTaken => "이미 사용 중인 닉네임입니다.",
            NicknameChangeFailure.Busy => "닉네임 변경을 처리 중입니다.",
            _ => "닉네임 변경에 실패했습니다. 잠시 후 다시 시도해주세요.",
        };
    }

    private void OnDestroy()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Close);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);

        if (nicknameInput != null)
            nicknameInput.onValueChanged.RemoveListener(HandleNicknameInputChanged);
    }
}
