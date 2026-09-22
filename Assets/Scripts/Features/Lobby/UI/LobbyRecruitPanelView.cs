using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRecruitPanelView : MonoBehaviour
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
    [SerializeField] private RecruitPresentationController presentationController;
    [SerializeField] private RecruitPresentationConfigSO presentationConfig;

    private GachaDataSO currentBanner;
    private LobbyRecruitPresenter presenter;
    private RecruitPanelViewState currentState;
    private bool isRecruiting;
    private bool isSubscribed;

    private void Awake()
    {
        if (presentationController == null)
            presentationController = GetComponent<RecruitPresentationController>();

        if (presentationController == null)
            presentationController = gameObject.AddComponent<RecruitPresentationController>();

        presentationController.Initialize(resultPopup, presentationConfig);
        presentationController.PresentationCompleted += Refresh;

        normalButton.onClick.AddListener(SelectNormalBanner);
        specialButton.onClick.AddListener(SelectSpecialBanner);

        recruitOneButton.onClick.AddListener(RecruitOne);
        recruitTenButton.onClick.AddListener(RecruitTen);
        rateButton.onClick.AddListener(OpenRatePopup);
    }

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        SubscribeEvents();
        SelectBanner(normalBanner);
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private bool TryInitialize()
    {
        UserDataManager manager = UserDataManager.Instance;
        if (manager?.UserData == null || manager.InventoryService == null || manager.GachaService == null)
        {
            Debug.LogError("[LobbyRecruitPanelView] User data services are not ready.");
            return false;
        }

        presenter ??= new LobbyRecruitPresenter(manager.UserData, manager.InventoryService, manager.GachaService);
        return true;
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
            return;

        UserDataManager.Instance.OnResourceUpdated += Refresh;
        UserDataManager.Instance.OnInventoryUpdated += Refresh;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed || UserDataManager.Instance == null)
            return;

        UserDataManager.Instance.OnResourceUpdated -= Refresh;
        UserDataManager.Instance.OnInventoryUpdated -= Refresh;
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
        bannerImage.sprite = currentState.BannerImage;
        pityText.text = $"앞으로 {currentState.RemainingPity}회 모집 안에 전설 유닛 확정 획득";

        bool canInput = !isRecruiting && !presentationController.IsPresenting;
        recruitOneButton.interactable = canInput && currentState.CanRecruitOne;
        recruitTenButton.interactable = canInput && currentState.CanRecruitTen;
    }

    private void OpenRatePopup()
    {
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
        if (isRecruiting || presentationController.IsPresenting)
            return;

        GachaDataSO banner = currentBanner;
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
        if (isRecruiting)
            return;

        isRecruiting = true;
        recruitOneButton.interactable = false;
        recruitTenButton.interactable = false;

        try
        {
            RecruitUnitsResult result = await UserDataManager.Instance.GachaUseCase.ExecuteAsync(
                new RecruitUnitsCommand(banner, count));

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[LobbyRecruitPanelView] Recruit failed: {result.Failure}");
                UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(result.Failure));
                return;
            }

            UserDataManager.Instance.RaiseResourceUpdated();
            UserDataManager.Instance.RaiseInventoryUpdated();
            UserDataManager.Instance.RaiseRosterUpdated();
            presentationController.Present(result.Results);
            Refresh();
        }
        finally
        {
            isRecruiting = false;
            Refresh();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();

        if (presentationController != null)
            presentationController.PresentationCompleted -= Refresh;

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
