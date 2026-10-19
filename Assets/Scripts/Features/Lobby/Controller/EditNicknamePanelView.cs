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
    private UserDataManager userDataManager;
    private bool isSaving;
    private int operationVersion;
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

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager || presenter == null)
        {
            userDataManager = manager;
            presenter = CreatePresenter(manager.UserData);
        }

        if (isActiveAndEnabled)
            RefreshEditor();
    }

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        if (!TryInitialize())
        {
            Debug.LogError("[EditNicknamePanelView] User profile is not ready.");
            confirmButton.interactable = false;
            return;
        }

        RefreshEditor();
    }

    private void RefreshEditor()
    {
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
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
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
            confirmButtonLabel.text = state.ChangeCost > 0
                ? Localize("nickname.confirm.paid", state.ChangeCost)
                : Localize("nickname.confirm.free");
    }

    private async void HandleConfirmButtonClicked()
    {
        if (isSaving || userDataManager == null)
            return;

        NicknameEditState state = presenter?.Build(nicknameInput.text, false);
        if (state?.CanSave != true)
            return;

        string nickname = state.NormalizedNickname;

        isSaving = true;
        int currentOperation = ++operationVersion;
        RefreshButton();

        try
        {
            NicknameChangeResult result;
            try
            {
                result = await userDataManager.UpdateNicknameAsync(nickname);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[EditNicknamePanelView] Save failed: {exception}");
                result = NicknameChangeResult.Fail(NicknameChangeFailure.SaveFailed);
            }

            if (!IsCurrentOperation(currentOperation))
                return;

            if (!result.Succeeded)
            {
                UIFeedbackToast.Show(GetFailureMessage(result.Failure));
                return;
            }

            presenter.MarkSaved(nickname);
            Lifecycle.Close();
        }
        finally
        {
            if (this != null)
            {
                isSaving = false;
                if (isActiveAndEnabled)
                    RefreshButton();
            }
        }
    }

    private static string GetFailureMessage(NicknameChangeFailure failure)
    {
        return failure switch
        {
            NicknameChangeFailure.InvalidNickname => Localize("nickname.failure.invalid"),
            NicknameChangeFailure.InsufficientGem => Localize("nickname.failure.insufficient_gems"),
            NicknameChangeFailure.NicknameAlreadyTaken => Localize("nickname.failure.already_taken"),
            NicknameChangeFailure.Busy => Localize("nickname.failure.busy"),
            _ => Localize("nickname.failure.save_failed"),
        };
    }

    private void HandleLocaleChanged(string _)
    {
        if (isActiveAndEnabled)
            RefreshButton();
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Dismiss);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);

        if (nicknameInput != null)
            nicknameInput.onValueChanged.RemoveListener(HandleNicknameInputChanged);
    }

    private bool IsCurrentOperation(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;

    private bool TryInitialize()
    {
        if (userDataManager?.UserData?.Profile == null)
            return false;

        presenter ??= CreatePresenter(userDataManager.UserData);
        return true;
    }

    private static NicknameEditPresenter CreatePresenter(UserDataRoot userData)
    {
        if (userData?.Profile == null)
            return null;

        int paidChangeCost = GameConfig.NewUserConfig?.NicknameChangeGemCost ?? 500;
        return new NicknameEditPresenter(userData, paidChangeCost);
    }

    private bool ValidateReferences()
    {
        bool valid = nicknameInput != null && cancelButton != null && confirmButton != null;
        if (!valid)
            Debug.LogError("[EditNicknamePanelView] Required UI references are not assigned.", this);
        return valid;
    }
}
