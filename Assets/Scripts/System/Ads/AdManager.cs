using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private RewardedAd rewardedAd;
    private UnityAction onRewardComplete;
    private const string adUnitId = "ca-app-pub-8895770206395123/9792318393";

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
        var adRequest = new AdRequest();

        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError($"RewardAd 로드 실패 {error.GetMessage()}");
                rewardedAd = null;
                return;
            }

            rewardedAd = ad;

            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                onRewardComplete?.Invoke();
                rewardedAd = null;

                LoadRewardAd();
            };
        });
    }

    public void ShowRewardAd(UnityAction onComplete)
    {
        if (rewardedAd == null || !rewardedAd.CanShowAd())
        {
#if UNITY_EDITOR
            Debug.LogError("RewardAd 가 준비되지 않음");
#endif
            return;
        }

        rewardedAd.Show((Reward reward) =>
        {
            onRewardComplete = onComplete;
        });
    }
}