using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class FuelPanelView : MonoBehaviour, IModalDismissHandler
{
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public void Open() => Lifecycle.Show();
    public void Close() => Lifecycle.Close();
    public void Dismiss() => Close();

    [Header("Fuel")]
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private TMP_Text nextRecoverText;
    [SerializeField] private TMP_Text fullRecoverText;

    [Header("Reward Ad")]
    [SerializeField] private TMP_Text dailyAdText;
    [SerializeField] private Button rewardAdButton;

    [Header("Product Catalog")]
    [SerializeField] private ShopProductGroup productCatalog;
    [SerializeField] private string rewardAdProductId = "fuel_rewarded_ad";
    [SerializeField] private string gemPurchaseProductId = "fuel_by_gem";

    [Header("Product Slots")]
    [SerializeField] private CommonSlotUI rewardAdRewardSlot;
    [SerializeField] private CommonSlotUI gemPurchaseRewardSlot;
    [SerializeField] private Button gemPurchaseButton;
    [SerializeField] private TMP_Text gemPurchaseButtonText;

    private FuelPanelPresenter presenter;
    private UserDataManager userDataManager;
    private AdManager adManager;
    private float refreshTimer;
    private bool isSubscribed;
    private bool isAdSubscribed;
    private bool isAdRequestPending;
    private bool isSavingAdReward;
    private bool hasAdClosed;
    private bool isPurchasingFuel;
    private ShopProductData rewardAdProduct;
    private ShopProductData gemPurchaseProduct;

    private void Awake()
    {
        ResolveProducts();
        BindProductSlots();
    }

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        if (!TryInitialize())
            return;

        SubscribeEvents();
        SubscribeAdEvents();
        adManager?.EnsureRewardedAdLoading();
        refreshTimer = 0f;
        Refresh();
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
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
        if (userDataManager?.UserData == null || !userDataManager.IsLoaded)
        {
            Debug.LogError("[FuelPanelView] User data services are not ready.");
            rewardAdButton.interactable = false;
            return false;
        }

        presenter ??= new FuelPanelPresenter(userDataManager.UserData);
        return true;
    }

    public void Initialize(UserDataManager manager, AdManager ads = null)
    {
        if (manager != null)
        {
            UnsubscribeEvents();
            userDataManager = manager;
            presenter = manager.UserData != null ? new FuelPanelPresenter(manager.UserData) : null;
        }

        if (ads != null && adManager != ads)
        {
            UnsubscribeAdEvents();
            adManager = ads;
        }

        if (isActiveAndEnabled && TryInitialize())
        {
            SubscribeEvents();
            SubscribeAdEvents();
            adManager?.EnsureRewardedAdLoading();
            Refresh();
        }
    }

    private void ResolveProducts()
    {
        rewardAdProduct = productCatalog?.GetById(rewardAdProductId);
        gemPurchaseProduct = productCatalog?.GetById(gemPurchaseProductId);

        if (!TryGetFuelRewardAmount(rewardAdProduct, out _) || rewardAdProduct.PurchaseType != ShopPurchaseType.Advertisement)
            Debug.LogError($"[FuelPanelView] Invalid rewarded-ad fuel product: {rewardAdProductId}", this);

        if (!TryGetFuelRewardAmount(gemPurchaseProduct, out _) || gemPurchaseProduct.PurchaseType != ShopPurchaseType.Gem)
            Debug.LogError($"[FuelPanelView] Invalid gem fuel product: {gemPurchaseProductId}", this);
    }

    private void BindProductSlots()
    {
        BindFuelReward(rewardAdRewardSlot, rewardAdProduct);
        BindFuelReward(gemPurchaseRewardSlot, gemPurchaseProduct);

        if (gemPurchaseButtonText != null && gemPurchaseProduct != null)
            gemPurchaseButtonText.text = Localize("fuel.gem_cost", gemPurchaseProduct.CostAmount);
    }

    private static void BindFuelReward(CommonSlotUI slot, ShopProductData product)
    {
        if (slot == null || !TryGetFuelReward(product, out RewardData reward))
        {
            slot?.Clear();
            return;
        }

        if (RewardPresentationResolver.TryResolve(reward, GameConfig.Icons, out RewardPresentation presentation))
            slot.SetupPresentation(
                presentation,
                reward.Amount,
                () => ItemTooltipUI.Show(presentation.DisplayName, slot.transform as RectTransform));
    }

    private static bool TryGetFuelRewardAmount(ShopProductData product, out int amount)
    {
        bool found = TryGetFuelReward(product, out RewardData reward);
        amount = found ? reward.Amount : 0;
        return found && amount > 0;
    }

    private static bool TryGetFuelReward(ShopProductData product, out RewardData fuelReward)
    {
        fuelReward = product?.Rewards?.Find(reward => reward != null && reward.Type == RewardType.Fuel);
        return fuelReward != null;
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
        if (isAdSubscribed)
            return;

        if (adManager == null)
            return;

        adManager.RewardedStateChanged += HandleRewardedAdStateChanged;
        isAdSubscribed = true;
    }

    private void UnsubscribeAdEvents()
    {
        if (!isAdSubscribed || adManager == null)
            return;

        adManager.RewardedStateChanged -= HandleRewardedAdStateChanged;
        isAdSubscribed = false;
    }

    private void HandleRewardedAdStateChanged(RewardedAdState state)
    {
        if (isActiveAndEnabled)
            Refresh();
    }

    private void Refresh()
    {
        bool isAdReady = adManager?.IsRewardedAdReady == true;
        FuelPanelViewState state = presenter?.Build(isAdRequestPending, isAdReady, DateTime.UtcNow);
        if (state == null)
        {
            rewardAdButton.interactable = false;
            if (gemPurchaseButton != null)
                gemPurchaseButton.interactable = false;
            return;
        }

        fuelText.text = Localize("fuel.status", state.Fuel, state.MaxFuel);
        nextRecoverText.text = Localize("fuel.next_recovery", FormatDuration(state.NextRecoverSeconds));
        fullRecoverText.text = Localize("fuel.full_recovery", FormatDuration(state.FullRecoverSeconds));
        dailyAdText.text = Localize("fuel.daily_ads", state.DailyAdWatchCount, state.DailyAdLimit);
        rewardAdButton.interactable = state.CanRequestRewardAd;
        if (gemPurchaseButton != null)
            gemPurchaseButton.interactable = !isPurchasingFuel && gemPurchaseProduct != null;
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

        if (adManager == null)
        {
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.AdUnavailable);
            return;
        }

        isAdRequestPending = true;
        hasAdClosed = false;
        Refresh();

        RewardedAdShowResult showResult = adManager.ShowRewardedAd(HandleAdRewardEarned, HandleAdCompleted);
        if (showResult == RewardedAdShowResult.Started)
            return;

        isAdRequestPending = false;
        UIFeedbackToast.Show(LobbyOperationFeedbackMessages.AdUnavailable);
        RefreshIfVisible();
    }

    private void HandleAdCompleted(RewardedAdCompletion completion)
    {
        if (this == null)
            return;

        hasAdClosed = true;
        if (!isSavingAdReward)
            isAdRequestPending = false;

        RefreshIfVisible();
    }

    private async void HandleAdRewardEarned()
    {
        if (this == null || isSavingAdReward || userDataManager == null)
            return;

        isSavingAdReward = true;

        try
        {
            if (!TryGetFuelRewardAmount(rewardAdProduct, out int fuelAmount))
            {
                ShowFeedbackIfVisible(LobbyOperationFeedbackMessages.AdUnavailable);
                return;
            }

            ClaimAdFuelRewardResult result = await userDataManager.ClaimAdFuelRewardAsync(fuelAmount);
            if (!result.Succeeded)
            {
                Debug.LogWarning($"[FuelPanelView] Ad fuel reward failed: {result.Failure}");
                ShowFeedbackIfVisible(LobbyOperationFeedbackMessages.Get(result.Failure));
                return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            ShowFeedbackIfVisible(LobbyOperationFeedbackMessages.Get(ClaimAdFuelRewardFailure.SaveFailed));
        }
        finally
        {
            isSavingAdReward = false;
            if (hasAdClosed)
                isAdRequestPending = false;

            RefreshIfVisible();
        }
    }

    public async void OnClickPurchaseFuel()
    {
        if (isPurchasingFuel || !TryInitialize())
            return;

        isPurchasingFuel = true;
        RefreshIfVisible();

        try
        {
            if (!TryGetFuelRewardAmount(gemPurchaseProduct, out int fuelAmount))
            {
                ShowFeedbackIfVisible(LobbyOperationFeedbackMessages.AdUnavailable);
                return;
            }

            PurchaseFuelResult result = await userDataManager.PurchaseFuelAsync(gemPurchaseProduct.CostAmount, fuelAmount);
            if (!result.Succeeded)
            {
                Debug.LogWarning($"[FuelPanelView] Fuel purchase failed: {result.Failure}");
                ShowFeedbackIfVisible(LobbyOperationFeedbackMessages.Get(result.Failure));
                return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            ShowFeedbackIfVisible(LobbyOperationFeedbackMessages.Get(PurchaseFuelFailure.SaveFailed));
        }
        finally
        {
            isPurchasingFuel = false;
            RefreshIfVisible();
        }
    }

    private void RefreshIfVisible()
    {
        if (isActiveAndEnabled)
            Refresh();
    }

    private void HandleLocaleChanged(string _)
    {
        BindProductSlots();
        RefreshIfVisible();
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private void ShowFeedbackIfVisible(string message)
    {
        if (isActiveAndEnabled)
            UIFeedbackToast.Show(message);
    }

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        UnsubscribeEvents();
        UnsubscribeAdEvents();
    }
}
