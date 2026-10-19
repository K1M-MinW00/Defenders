using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LobbyRecruitPanelView : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text ticketText;
    [SerializeField] private Image ticketImage;

    [Header("Banner")]
    [SerializeField] private Image bannerImage;
    [SerializeField] private TMP_Text pityText;

    [Header("Buttons")]
    [SerializeField] private Button normalButton;
    [SerializeField] private Button specialButton;

    [SerializeField] private Button recruitOneButton;
    [SerializeField] private Button recruitTenButton;

    [SerializeField] private Button rateButton;

    [Header("Banner Data")]
    [SerializeField] private GachaDataSO normalBanner;
    [SerializeField] private GachaDataSO specialBanner;

    [Header("Popup")]
    [SerializeField] private RatePopupPanelView ratePopup;
    [SerializeField] private RecruitResultPopupView resultPopup;
    [SerializeField] private GemConfirmPopupView gemConfirmPopup;

    [Header("Presentation")]
    [Tooltip("Optional. Leave empty to use the built-in timing and rarity colors.")]
    [SerializeField] private RecruitPresentationConfigSO presentationConfig;
    [SerializeField] private RecruitPresentationController presentationController;

    private GachaDataSO currentBanner;
    private LobbyRecruitPresenter presenter;
    private RecruitPanelViewState currentState;
    private UserDataManager userDataManager;
    private IReadOnlyList<GachaResult> pendingPresentationResults;
    private bool isRecruiting;
    private bool isSubscribed;
    private int operationVersion;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        presentationController.Initialize(resultPopup, presentationConfig);
        presentationController.PresentationCompleted += HandlePresentationCompleted;

        normalButton.onClick.AddListener(SelectNormalBanner);
        specialButton.onClick.AddListener(SelectSpecialBanner);

        recruitOneButton.onClick.AddListener(RecruitOne);
        recruitTenButton.onClick.AddListener(RecruitTen);
        rateButton.onClick.AddListener(OpenRatePopup);
    }

    private bool ValidateReferences()
    {
        bool isValid = gemText != null && ticketText != null && ticketImage != null &&
                       bannerImage != null && pityText != null &&
                       normalButton != null && specialButton != null &&
                       recruitOneButton != null && recruitTenButton != null && rateButton != null &&
                       normalBanner != null && specialBanner != null &&
                       ratePopup != null && resultPopup != null && gemConfirmPopup != null &&
                       presentationController != null;

        if (!isValid)
            Debug.LogError("[LobbyRecruitPanelView] One or more required scene references are missing.", this);

        return isValid;
    }

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        if (!TryInitialize())
            return;

        SubscribeEvents();
        currentBanner ??= normalBanner;
        Refresh();
        PresentPendingResults();
    }

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager)
        {
            UnsubscribeEvents();
            userDataManager = manager;
            presenter = null;
        }

        if (!isActiveAndEnabled || !TryInitialize())
            return;

        SubscribeEvents();
        currentBanner ??= normalBanner;
        Refresh();
        PresentPendingResults();
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        operationVersion++;
        gemConfirmPopup?.Dismiss();
        UnsubscribeEvents();
    }

    private bool TryInitialize()
    {
        if (userDataManager?.UserData == null || !userDataManager.IsLoaded ||
            userDataManager.InventoryService == null || userDataManager.GachaService == null)
        {
            Debug.LogError("[LobbyRecruitPanelView] User data services are not ready.");
            return false;
        }

        presenter ??= new LobbyRecruitPresenter(
            userDataManager.UserData,
            userDataManager.InventoryService,
            userDataManager.GachaService);
        return true;
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
            return;

        userDataManager.OnResourceUpdated += Refresh;
        userDataManager.OnInventoryUpdated += Refresh;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnResourceUpdated -= Refresh;
        userDataManager.OnInventoryUpdated -= Refresh;
        isSubscribed = false;
    }

    private void SelectNormalBanner()
    {
        SelectBanner(normalBanner);
    }

    private void SelectSpecialBanner()
    {
        SelectBanner(specialBanner);
    }

    private void SelectBanner(GachaDataSO banner)
    {
        currentBanner = banner;
        Refresh();
    }

    private void Refresh()
    {
        currentState = presenter?.Build(currentBanner);
        if (currentState == null)
        {
            recruitOneButton.interactable = false;
            recruitTenButton.interactable = false;
            return;
        }

        gemText.text = currentState.GemCount.ToString("N0");
        ticketText.text = currentState.TicketCount.ToString("N0");
        ticketImage.sprite = currentState.TicketIcon;
        bannerImage.sprite = currentState.PortraitImage;
        bannerImage.preserveAspect = currentState.HasPickup;
        bannerImage.enabled = currentState.PortraitImage != null;
        pityText.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            "recruit.pity",
            currentState.RemainingPity);

        bool canInput = !isRecruiting && !presentationController.IsPresenting;
        recruitOneButton.interactable = canInput && currentState.CanRecruitOne;
        recruitTenButton.interactable = canInput && currentState.CanRecruitTen;
    }

    private void OpenRatePopup()
    {
        if (currentBanner == null)
            return;

        ratePopup.Open(currentBanner);
    }

    private void RecruitOne()
    {
        TryRecruit(1);
    }

    private void RecruitTen()
    {
        TryRecruit(10);
    }

    private void TryRecruit(int count)
    {
        if (count <= 0 || isRecruiting || presentationController.IsPresenting || !TryInitialize())
            return;

        GachaDataSO banner = currentBanner;
        if (banner == null)
            return;

        RecruitCostModel cost = presenter.CalculateCost(banner, count);

        if (cost.NeedGem == false)
        {
            ExecuteRecruit(banner, count);
            return;
        }

        gemConfirmPopup.Open(cost.GemUseCount, () => { ExecuteRecruit(banner, count); });
    }

    private async void ExecuteRecruit(GachaDataSO banner, int count)
    {
        if (isRecruiting || banner == null || count <= 0 || !TryInitialize())
            return;

        isRecruiting = true;
        int currentOperation = ++operationVersion;
        recruitOneButton.interactable = false;
        recruitTenButton.interactable = false;

        try
        {
            RecruitUnitsResult result = await userDataManager.RecruitUnitsAsync(
                new RecruitUnitsCommand(banner, count));

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[LobbyRecruitPanelView] Recruit failed: {result.Failure}");
                if (CanApplyOperationResult(currentOperation))
                    UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(result.Failure));
                return;
            }

            if (CanApplyOperationResult(currentOperation))
                presentationController.Present(result.Results);
            else
            {
                pendingPresentationResults = result.Results != null
                    ? new List<GachaResult>(result.Results)
                    : new List<GachaResult>();

                if (isActiveAndEnabled)
                    PresentPendingResults();
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (CanApplyOperationResult(currentOperation))
                UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(RecruitUnitsFailure.SaveFailed));
        }
        finally
        {
            isRecruiting = false;
            if (CanApplyOperationResult(currentOperation))
                Refresh();
        }
    }

    private void PresentPendingResults()
    {
        if (pendingPresentationResults == null || pendingPresentationResults.Count == 0 ||
            presentationController.IsPresenting)
        {
            return;
        }

        IReadOnlyList<GachaResult> results = pendingPresentationResults;
        pendingPresentationResults = null;
        presentationController.Present(results);
    }

    private void HandlePresentationCompleted()
    {
        if (isActiveAndEnabled)
            Refresh();
    }

    private void HandleLocaleChanged(string _)
    {
        if (isActiveAndEnabled)
            Refresh();
    }

    private bool CanApplyOperationResult(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        UnsubscribeEvents();

        if (presentationController != null)
            presentationController.PresentationCompleted -= HandlePresentationCompleted;

        if (normalButton != null)
            normalButton.onClick.RemoveListener(SelectNormalBanner);

        if (specialButton != null)
            specialButton.onClick.RemoveListener(SelectSpecialBanner);

        if (recruitOneButton != null)
            recruitOneButton.onClick.RemoveListener(RecruitOne);

        if (recruitTenButton != null)
            recruitTenButton.onClick.RemoveListener(RecruitTen);

        if (rateButton != null)
            rateButton.onClick.RemoveListener(OpenRatePopup);
    }
}
