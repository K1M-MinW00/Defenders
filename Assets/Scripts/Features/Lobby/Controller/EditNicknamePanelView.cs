using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class EditNicknamePanelView : MonoBehaviour, IModalDismissHandler
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Input")]
    [SerializeField] private TMP_InputField nicknameInput;

    [Header("Button")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text confirmButtonLabel;

    private NicknameEditPresenter presenter;
    private bool isSaving;
    private int operationVersion;
    private ModalOverlayLifecycle lifecycle;
    private GameObject Root => panelRoot != null ? panelRoot : gameObject;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(Root);

    private void Awake()
    {
        nicknameInput.characterLimit = NicknamePolicy.MaxLength;

        cancelButton.onClick.AddListener(Dismiss);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
        nicknameInput.onValueChanged.AddListener(HandleNicknameInputChanged);
    }

    public void Open()
    {
        if (!isSaving)
            Lifecycle.Show();
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

    public void Dismiss()
    {
        if (!isSaving)
            Lifecycle.Close();
    }

    public void HideImmediately() => Lifecycle.HideImmediately();

    private void OnDisable()
    {
        operationVersion++;
    }

    // Inspector compatibility for scenes created before the modal convention.
    public void Close() => Dismiss();

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
        int currentOperation = ++operationVersion;
        RefreshButton();

        NicknameChangeResult result;
        try
        {
            result = await UserDataManager.Instance.UpdateNicknameAsync(nickname);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[EditNicknamePanelView] Save failed: {exception}");
            result = NicknameChangeResult.Fail(NicknameChangeFailure.SaveFailed);
        }

        isSaving = false;
        if (!isActiveAndEnabled || currentOperation != operationVersion)
            return;

        if (!result.Succeeded)
        {
            UIFeedbackToast.Show(GetFailureMessage(result.Failure));
            RefreshButton();
            return;
        }

        presenter.MarkSaved(nickname);
        Dismiss();
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
            cancelButton.onClick.RemoveListener(Dismiss);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);

        if (nicknameInput != null)
            nicknameInput.onValueChanged.RemoveListener(HandleNicknameInputChanged);
    }
}
