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

        UserResourceData resources = userData.Resource;
        UserAdData adData = userData.Ad;

        StaminaService.RefreshFuel(resources);

        bool adReset = AdDailyLimitService.Refresh(adData);

        if (adReset)
            UserDataManager.Instance.MarkDirty();

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
        int count = AdDailyLimitService.GetWatchCount(adData, DailyAdType.Fuel);

        dailyAdText.text = $"일일 광고 시청 ({count}/{AdDailyLimitService.DailyAdLimit})";

        rewardAdButton.interactable = count < AdDailyLimitService.DailyAdLimit;
    }

    public void OnClickRewardAd()
    {
        UserAdData adData = UserDataManager.Instance.UserData.Ad;

        if (!AdDailyLimitService.CanWatch(adData,DailyAdType.Fuel))
            return;

        AdManager.Instance.ShowRewardAd(() =>
        {
            UserDataManager.Instance.ResourceService.AddFuel(rewardAdFuelAmount);

            AdDailyLimitService.Consume(adData,DailyAdType.Fuel);

            Refresh();
        });
    }

    public void OnClickPurchaseFuel()
    {
        bool success = UserDataManager.Instance.ResourceService.SpendGem(purchaseFuelGemCost);

        if (!success)
            return;

        UserDataManager.Instance.ResourceService.AddFuel(purchaseFuelAmount);
        Refresh();
    }
}