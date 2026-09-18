using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FuelPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private TMP_Text nextRecoverText;
    [SerializeField] private TMP_Text fullRecoverText;

    [Header("Reward Ad")]
    [SerializeField] private TMP_Text dailyAdText;
    [SerializeField] private Button rewardAdButton;


    [SerializeField] private int rewardAdFuelAmount = 30;
    [SerializeField] private int purchaseFuelAmount = 30;
    [SerializeField] private int purchaseFuelGemCost = 100;

    private float timer;
    private bool isAdRequestPending;
    private bool isSavingAdReward;
    private bool hasAdClosed;
    private bool isPurchasingFuel;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < 1f)
            return;

        timer = 0f;
        Refresh();
    }

    private void Refresh()
    {
        if (UserDataManager.Instance == null)
            return;

        UserDataRoot userData = UserDataManager.Instance.UserData;

        UserResourceData resources = UserDataCloner.Copy(userData.Resource);
        UserAdData adData = UserDataCloner.Copy(userData.Ad);

        StaminaService.RefreshFuel(resources);
        AdDailyLimitPolicy.Refresh(adData, DateTime.UtcNow);

        RefreshFuelUI(resources);
        RefreshRewardAdUI(adData);
    }

    private void RefreshFuelUI(UserResourceData resources)
    {
        fuelText.text = $"{resources.Fuel}/{resources.MaxFuel}";

        int nextSeconds = StaminaService.GetRemainingSecondsToNextFuel(resources);

        nextRecoverText.text = $"다음 충전까지 {Format(nextSeconds)}";

        int fullSeconds = StaminaService.GetRemainingSecondsToFullFuel(resources);

        fullRecoverText.text = $"최대 충전까지 {Format(fullSeconds)}";
    }

    private string Format(int seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);

        return $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00}";
    }

    private void RefreshRewardAdUI(UserAdData adData)
    {
        int count = AdDailyLimitPolicy.GetWatchCount(adData, DailyAdType.Fuel);

        dailyAdText.text = $"일일 광고 시청 ({count}/{AdDailyLimitPolicy.DailyAdLimit})";

        rewardAdButton.interactable = !isAdRequestPending && count < AdDailyLimitPolicy.DailyAdLimit;
    }

    public void OnClickRewardAd()
    {
        if (isAdRequestPending)
            return;

        UserAdData adData = UserDataCloner.Copy(UserDataManager.Instance.UserData.Ad);

        if (!AdDailyLimitPolicy.CanWatch(adData, DailyAdType.Fuel, DateTime.UtcNow))
            return;

        isAdRequestPending = true;
        hasAdClosed = false;
        rewardAdButton.interactable = false;

        bool shown = AdManager.Instance.ShowRewardAd(
            HandleAdRewardEarned,
            () =>
            {
                hasAdClosed = true;

                if (!isSavingAdReward)
                    isAdRequestPending = false;

                Refresh();
            });

        if (!shown)
        {
            isAdRequestPending = false;
            Refresh();
        }
    }

    private async void HandleAdRewardEarned()
    {
        isSavingAdReward = true;

        try
        {
            ClaimAdFuelRewardResult result = await UserDataManager.Instance.ClaimAdFuelRewardUseCase
                .ExecuteAsync(rewardAdFuelAmount);

            if (!result.Succeeded)
                Debug.LogWarning($"[FuelPanelView] Ad fuel reward failed: {result.Failure}");
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
        if (isPurchasingFuel)
            return;

        isPurchasingFuel = true;

        try
        {
            PurchaseFuelResult result = await UserDataManager.Instance.PurchaseFuelUseCase
                .ExecuteAsync(purchaseFuelGemCost, purchaseFuelAmount);

            if (!result.Succeeded)
                Debug.LogWarning($"[FuelPanelView] Fuel purchase failed: {result.Failure}");

            Refresh();
        }
        finally
        {
            isPurchasingFuel = false;
        }
    }
}
