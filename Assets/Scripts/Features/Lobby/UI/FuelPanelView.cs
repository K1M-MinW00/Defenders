using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FuelPanelView : MonoBehaviour
{
    public void Open() => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);

    [Header("Fuel")]
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private TMP_Text nextRecoverText;
    [SerializeField] private TMP_Text fullRecoverText;

    [Header("Reward Ad")]
    [SerializeField] private TMP_Text dailyAdText;
    [SerializeField] private Button rewardAdButton;

    [Header("Operation Values")]
    [SerializeField, Min(1)] private int rewardAdFuelAmount = 30;
    [SerializeField, Min(1)] private int purchaseFuelAmount = 30;
    [SerializeField, Min(1)] private int purchaseFuelGemCost = 100;

    private FuelPanelPresenter presenter;
    private UserDataManager userDataManager;
    private float refreshTimer;
    private bool isSubscribed;
    private bool isAdRequestPending;
    private bool isSavingAdReward;
    private bool hasAdClosed;
    private bool isPurchasingFuel;

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        SubscribeEvents();
        SubscribeAdEvents();
        AdManager.Instance?.EnsureRewardedAdLoading();
        refreshTimer = 0f;
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        UnsubscribeAdEvents();
    }

    private void Update()
    {
        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer < 1f)
            return;

        refreshTimer = 0f;
        Refresh();
    }

    private bool TryInitialize()
    {
        userDataManager = UserDataManager.Instance;
        if (userDataManager?.UserData == null ||
            userDataManager.PurchaseFuelUseCase == null ||
            userDataManager.ClaimAdFuelRewardUseCase == null)
        {
            Debug.LogError("[FuelPanelView] User data services are not ready.");
            rewardAdButton.interactable = false;
            return false;
        }

        presenter ??= new FuelPanelPresenter(userDataManager.UserData);
        return true;
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
            return;

        userDataManager.OnResourceUpdated += Refresh;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnResourceUpdated -= Refresh;
        isSubscribed = false;
    }

    private void SubscribeAdEvents()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.RewardedStateChanged += HandleRewardedAdStateChanged;
    }

    private void UnsubscribeAdEvents()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.RewardedStateChanged -= HandleRewardedAdStateChanged;
    }

    private void HandleRewardedAdStateChanged(RewardedAdState state)
    {
        Refresh();
    }

    private void Refresh()
    {
        bool isAdReady = AdManager.Instance?.IsRewardedAdReady == true;
        FuelPanelViewState state = presenter?.Build(isAdRequestPending, isAdReady, DateTime.UtcNow);
        if (state == null)
        {
            rewardAdButton.interactable = false;
            return;
        }

        fuelText.text = $"{state.Fuel}/{state.MaxFuel}";
        nextRecoverText.text = $"다음 충전까지 {FormatDuration(state.NextRecoverSeconds)}";
        fullRecoverText.text = $"최대 충전까지 {FormatDuration(state.FullRecoverSeconds)}";
        dailyAdText.text = $"일일 광고 시청 ({state.DailyAdWatchCount}/{state.DailyAdLimit})";
        rewardAdButton.interactable = state.CanRequestRewardAd;
    }

    private static string FormatDuration(int seconds)
    {
        TimeSpan duration = TimeSpan.FromSeconds(Math.Max(0, seconds));
        int totalHours = (int)duration.TotalHours;
        return $"{totalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
    }

    public void OnClickRewardAd()
    {
        if (isAdRequestPending || presenter == null)
            return;

        if (!presenter.CanWatchRewardAd(DateTime.UtcNow))
        {
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(ClaimAdFuelRewardFailure.DailyLimitReached));
            return;
        }

        if (AdManager.Instance == null)
        {
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.AdUnavailable);
            return;
        }

        isAdRequestPending = true;
        hasAdClosed = false;
        Refresh();

        RewardedAdShowResult showResult = AdManager.Instance.ShowRewardedAd(HandleAdRewardEarned, HandleAdCompleted);
        if (showResult == RewardedAdShowResult.Started)
            return;

        isAdRequestPending = false;
        UIFeedbackToast.Show(LobbyOperationFeedbackMessages.AdUnavailable);
        Refresh();
    }

    private void HandleAdCompleted(RewardedAdCompletion completion)
    {
        hasAdClosed = true;
        if (!isSavingAdReward)
            isAdRequestPending = false;

        Refresh();
    }

    private async void HandleAdRewardEarned()
    {
        if (isSavingAdReward || userDataManager == null)
            return;

        isSavingAdReward = true;

        try
        {
            ClaimAdFuelRewardResult result = await userDataManager.ClaimAdFuelRewardUseCase.ExecuteAsync(rewardAdFuelAmount);
            if (!result.Succeeded)
            {
                Debug.LogWarning($"[FuelPanelView] Ad fuel reward failed: {result.Failure}");
                UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(result.Failure));
                return;
            }

            userDataManager.RaiseResourceUpdated();
        }
        finally
        {
            isSavingAdReward = false;
            if (hasAdClosed)
                isAdRequestPending = false;

            Refresh();
        }
    }

    public async void OnClickPurchaseFuel()
    {
        if (isPurchasingFuel || userDataManager == null)
            return;

        isPurchasingFuel = true;

        try
        {
            PurchaseFuelResult result = await userDataManager.PurchaseFuelUseCase.ExecuteAsync(purchaseFuelGemCost, purchaseFuelAmount);
            if (!result.Succeeded)
            {
                Debug.LogWarning($"[FuelPanelView] Fuel purchase failed: {result.Failure}");
                UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(result.Failure));
                return;
            }

            userDataManager.RaiseResourceUpdated();
        }
        finally
        {
            isPurchasingFuel = false;
            Refresh();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        UnsubscribeAdEvents();
    }
}
