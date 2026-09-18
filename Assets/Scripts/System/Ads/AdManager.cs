using UnityEngine;
using GoogleMobileAds.Api;
using UnityEngine.Events;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private RewardedAd rewardedAd;

    private UnityAction onRewardEarned;
    private UnityAction onAdClosed;
    private bool isShowing;
    private bool isFinalizing;
    private const string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // ������ ���� Test ID
    // private const string adUnitId = "ca-app-pub-8895770206395123/9792318393"; // ������ ���� ID

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        MobileAds.Initialize((initStatus) =>
        {
            LoadRewardAd();
        });
    }

    private void LoadRewardAd()
    {
        Debug.Log("LoadRewardAd");

        var adRequest = new AdRequest();

        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError($"RewardAd �ε� ���� {error.GetMessage()}");
                rewardedAd = null;
                return;
            }

            rewardedAd = ad;

            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                CompleteAdSession();
            };

            rewardedAd.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError($"RewardAd show failed: {error.GetMessage()}");
                CompleteAdSession();
            };
        });

    }

    public bool ShowRewardAd(UnityAction onEarnedReward, UnityAction onClosed = null)
    {
        if (isShowing || rewardedAd == null || !rewardedAd.CanShowAd())
        {
            Debug.LogError("RewardAd is not ready.");
            return false;
        }

        isShowing = true;
        isFinalizing = false;
        onRewardEarned = onEarnedReward;
        onAdClosed = onClosed;

        rewardedAd.Show((Reward reward) =>
        {
            UnityAction callback = onRewardEarned;
            onRewardEarned = null;
            callback?.Invoke();
        });

        return true;
    }

    private void CompleteAdSession()
    {
        if (isFinalizing)
            return;

        isFinalizing = true;
        isShowing = false;
        onRewardEarned = null;

        UnityAction closedCallback = onAdClosed;
        onAdClosed = null;
        closedCallback?.Invoke();

        rewardedAd = null;
        LoadRewardAd();
    }
}
