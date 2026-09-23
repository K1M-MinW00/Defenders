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
    [SerializeField] private Button backdropButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    private TMP_Text confirmButtonLabel;
    private NicknameEditPresenter presenter;
    private bool isSaving;

    private void Awake()
    {
        backdropButton?.onClick.AddListener(Close);
        nicknameInput.characterLimit = NicknamePolicy.MaxLength;
        confirmButtonLabel = confirmButton.GetComponentInChildren<TMP_Text>(true);

        cancelButton.onClick.AddListener(Close);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
        nicknameInput.onValueChanged.AddListener(HandleNicknameInputChanged);
    }

    public void Open()
    {
        if (!isSaving && panelRoot != null)
            panelRoot.SetActive(true);
    }

    private void OnEnable()
    {
        UserDataRoot userData = UserDataManager.Instance?.UserData;
        if (userData?.Profile == null)
        {
            Debug.LogError("[EditNicknamePanelView] User profile is not ready.");
            confirmButton.interactable = false;
            return;
        }

        int paidChangeCost = GameConfig.NewUserConfig?.NicknameChangeGemCost ?? 500;
        presenter ??= new NicknameEditPresenter(userData, paidChangeCost);
        string currentNickname = presenter.Open();

        nicknameInput.SetTextWithoutNotify(currentNickname);
        RefreshButton();
        nicknameInput.ActivateInputField();
    }

    public void Close()
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
        NicknameEditState state = presenter?.Build(nicknameInput.text, isSaving);
        confirmButton.interactable = state?.CanSave == true;
        cancelButton.interactable = !isSaving;

        if (confirmButtonLabel != null && state != null)
            confirmButtonLabel.text = state.ConfirmLabel;
    }

    private async void HandleConfirmButtonClicked()
    {
        if (isSaving)
            return;

        NicknameEditState state = presenter?.Build(nicknameInput.text, false);
        if (state?.CanSave != true)
            return;

        string nickname = state.NormalizedNickname;

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

        presenter.MarkSaved(nickname);
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
        if (backdropButton != null)
            backdropButton.onClick.RemoveListener(Close);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Close);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);

        if (nicknameInput != null)
            nicknameInput.onValueChanged.RemoveListener(HandleNicknameInputChanged);
    }
}
