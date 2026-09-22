using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

public sealed class AdManager : MonoBehaviour
{
    private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    private const float InitialRetryDelaySeconds = 2f;
    private const float MaxRetryDelaySeconds = 30f;

    public static AdManager Instance { get; private set; }

    public RewardedAdState RewardedState { get; private set; } = RewardedAdState.Uninitialized;
    public bool IsRewardedAdReady => RewardedState == RewardedAdState.Ready && rewardedAd != null && rewardedAd.CanShowAd();

    public event Action<RewardedAdState> RewardedStateChanged;

    private RewardedAd rewardedAd;
    private Action rewardEarnedCallback;
    private Action<RewardedAdCompletion> completedCallback;
    private Coroutine retryRoutine;
    private int loadGeneration;
    private int consecutiveLoadFailures;
    private bool isFinalizingSession;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAds();
    }

    public RewardedAdShowResult ShowRewardedAd(
        Action onRewardEarned,
        Action<RewardedAdCompletion> onCompleted = null)
    {
        if (onRewardEarned == null)
            return RewardedAdShowResult.InvalidRequest;

        if (RewardedState == RewardedAdState.Showing)
            return RewardedAdShowResult.AlreadyShowing;

        if (!IsRewardedAdReady)
        {
            EnsureRewardedAdLoading();
            return RewardedAdShowResult.NotReady;
        }

        RewardedAd adToShow = rewardedAd;
        rewardEarnedCallback = onRewardEarned;
        completedCallback = onCompleted;
        isFinalizingSession = false;
        SetState(RewardedAdState.Showing);

        try
        {
            adToShow.Show(_ => InvokeRewardEarnedOnce());
            return RewardedAdShowResult.Started;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[AdManager] Rewarded ad show exception: {exception}");
            CompleteSession(RewardedAdCompletion.ShowFailed);
            return RewardedAdShowResult.ShowFailed;
        }
    }

    public void EnsureRewardedAdLoading()
    {
        if (RewardedState is RewardedAdState.Initializing or RewardedAdState.Loading or RewardedAdState.Ready or RewardedAdState.Showing or RewardedAdState.RetryWaiting)
            return;

        LoadRewardedAd();
    }

    private void InitializeAds()
    {
        SetState(RewardedAdState.Initializing);
        MobileAds.Initialize(_ =>
        {
            if (this != null)
                LoadRewardedAd();
        });
    }

    private void LoadRewardedAd()
    {
        CancelRetry();
        DisposeRewardedAd();
        SetState(RewardedAdState.Loading);

        int generation = ++loadGeneration;
        RewardedAd.Load(RewardedAdUnitId, new AdRequest(), (ad, error) =>
        {
            if (this == null || generation != loadGeneration)
            {
                ad?.Destroy();
                return;
            }

            if (error != null || ad == null)
            {
                string message = error?.GetMessage() ?? "Rewarded ad load returned null.";
                Debug.LogWarning($"[AdManager] Rewarded ad load failed: {message}");
                ad?.Destroy();
                ScheduleRetry();
                return;
            }

            consecutiveLoadFailures = 0;
            rewardedAd = ad;
            rewardedAd.OnAdFullScreenContentClosed += HandleAdClosed;
            rewardedAd.OnAdFullScreenContentFailed += HandleAdShowFailed;
            SetState(RewardedAdState.Ready);
        });
    }

    private void HandleAdClosed()
    {
        CompleteSession(RewardedAdCompletion.Closed);
    }

    private void HandleAdShowFailed(AdError error)
    {
        Debug.LogWarning($"[AdManager] Rewarded ad show failed: {error?.GetMessage()}");
        CompleteSession(RewardedAdCompletion.ShowFailed);
    }

    private void InvokeRewardEarnedOnce()
    {
        Action callback = rewardEarnedCallback;
        rewardEarnedCallback = null;

        try
        {
            callback?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void CompleteSession(RewardedAdCompletion completion)
    {
        if (isFinalizingSession)
            return;

        isFinalizingSession = true;
        rewardEarnedCallback = null;

        Action<RewardedAdCompletion> callback = completedCallback;
        completedCallback = null;

        DisposeRewardedAd();
        SetState(RewardedAdState.Uninitialized);
        LoadRewardedAd();

        try
        {
            callback?.Invoke(completion);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void ScheduleRetry()
    {
        consecutiveLoadFailures++;
        float delay = Mathf.Min(
            InitialRetryDelaySeconds * Mathf.Pow(2f, consecutiveLoadFailures - 1),
            MaxRetryDelaySeconds);

        SetState(RewardedAdState.RetryWaiting);
        retryRoutine = StartCoroutine(RetryAfterDelay(delay));
    }

    private IEnumerator RetryAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        retryRoutine = null;
        LoadRewardedAd();
    }

    private void CancelRetry()
    {
        if (retryRoutine == null)
            return;

        StopCoroutine(retryRoutine);
        retryRoutine = null;
    }

    private void DisposeRewardedAd()
    {
        if (rewardedAd == null)
            return;

        rewardedAd.OnAdFullScreenContentClosed -= HandleAdClosed;
        rewardedAd.OnAdFullScreenContentFailed -= HandleAdShowFailed;
        rewardedAd.Destroy();
        rewardedAd = null;
    }

    private void SetState(RewardedAdState state)
    {
        if (RewardedState == state)
            return;

        RewardedState = state;
        RewardedStateChanged?.Invoke(state);
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        loadGeneration++;
        CancelRetry();
        DisposeRewardedAd();
        rewardEarnedCallback = null;
        completedCallback = null;
        Instance = null;
    }
}
