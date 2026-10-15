using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestController : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject overlay;
    [SerializeField] private LobbyTabView lobbyTabView;
    [SerializeField] private QuestRewardDisplay rewardDisplay;
    [SerializeField] private QuestRewardTrackView dailyRewardTrack;
    [SerializeField] private QuestRewardTrackView weeklyRewardTrack;
    [SerializeField] private QuestRewardView[] rewardViews = Array.Empty<QuestRewardView>();
    [SerializeField] private QuestItemView[] questItems = Array.Empty<QuestItemView>();
    [SerializeField] private TMP_Text timerText;
    private Coroutine timerRoutine;
    private bool questActionBusy;
    private bool rewardClaimBusy;
    private int openVersion;
    private bool isSubscribed;
    private UserDataManager userDataManager;
    private ModalOverlayLifecycle overlayLifecycle;
    private readonly List<QuestDefinition> orderedQuests = new(16);

    private ModalOverlayLifecycle OverlayLifecycle =>
        overlayLifecycle ??= new ModalOverlayLifecycle(overlay);

    private void Awake()
    {
        openButton?.onClick.AddListener(Open);
        closeButton?.onClick.AddListener(Dismiss);
    }

    private void OnEnable()
    {
        TryInitialize();
        Subscribe();
    }

    private void OnDisable()
    {
        openVersion++;
        questActionBusy = false;
        rewardClaimBusy = false;
        StopTimer();
        rewardDisplay?.Hide();
        OverlayLifecycle.HideImmediately();
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        openButton?.onClick.RemoveListener(Open);
        closeButton?.onClick.RemoveListener(Dismiss);
    }

    public async void Open()
    {
        if (overlay == null || !TryInitialize())
        {
            UIFeedbackToast.Show("퀘스트 정보를 불러오는 중입니다.");
            return;
        }

        Subscribe();
        int currentOpen = ++openVersion;
        OverlayLifecycle.Show();

        try
        {
            if (userDataManager.IsLoaded)
                await userDataManager.RefreshQuestPeriodsAsync();
        }
        catch (Exception exception)
        {
            if (IsCurrentOpen(currentOpen))
            {
                Debug.LogError($"[QuestController] Period refresh failed: {exception}");
                UIFeedbackToast.Show("퀘스트 정보를 갱신하지 못했습니다.");
            }
        }

        if (!IsCurrentOpen(currentOpen))
            return;

        Refresh();
        if (timerRoutine != null) StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(UpdateTimer());
    }

    public void Dismiss()
    {
        openVersion++;
        questActionBusy = false;
        rewardClaimBusy = false;
        rewardDisplay?.Hide();
        StopTimer();
        OverlayLifecycle.Close();
    }

    public void Refresh()
    {
        UserQuestData data = userDataManager?.UserData?.Quest;
        if (!OverlayLifecycle.IsVisible || data == null) return;

        weeklyRewardTrack?.SetState(data.WeeklyScore, data.ClaimedWeeklyRewards);
        dailyRewardTrack?.SetState(data.DailyScore, data.ClaimedDailyRewards);

        BuildOrderedQuestList(data);
        for (int i = 0; i < questItems.Length; i++)
        {
            bool visible = i < orderedQuests.Count;
            if (questItems[i] == null)
                continue;

            questItems[i].gameObject.SetActive(visible);
            if (!visible)
            {
                questItems[i].Clear();
                continue;
            }

            QuestDefinition definition = orderedQuests[i];
            int progress = QuestProgressService.GetProgress(data, definition.Id);
            bool claimed = QuestProgressService.IsScoreClaimed(data, definition.Id);
            questItems[i].Configure(definition.Score, definition.DisplayName,
                progress, definition.Target, claimed,
                () => HandleQuestAction(definition));
        }

        foreach (QuestRewardView reward in rewardViews)
        {
            int threshold = reward.Threshold;
            QuestRewardPeriod period = reward.Period;
            reward.SetClickHandler(() => HandleRewardClick(period, threshold, reward.transform as RectTransform));
        }
    }

    private void BuildOrderedQuestList(UserQuestData data)
    {
        orderedQuests.Clear();
        IReadOnlyList<QuestDefinition> definitions = DailyQuestCatalog.All;
        for (int sortOrder = 0; sortOrder <= 2; sortOrder++)
            for (int i = 0; i < definitions.Count; i++)
                if (GetSortOrder(data, definitions[i]) == sortOrder)
                    orderedQuests.Add(definitions[i]);
    }

    private static int GetSortOrder(UserQuestData data, QuestDefinition definition)
    {
        bool claimed = QuestProgressService.IsScoreClaimed(data, definition.Id);
        bool completed = QuestProgressService.GetProgress(data, definition.Id) >= definition.Target;
        return claimed ? 2 : completed ? 0 : 1;
    }

    private async void HandleQuestAction(QuestDefinition definition)
    {
        if (questActionBusy)
            return;

        UserQuestData data = userDataManager?.UserData?.Quest;
        if (data == null) return;
        if (QuestProgressService.GetProgress(data, definition.Id) < definition.Target)
        {
            NavigateToQuest(definition.EventType);
            return;
        }

        questActionBusy = true;
        int operationVersion = openVersion;
        QuestScoreClaimResult result;
        try
        {
            result = await userDataManager.ClaimQuestScoreAsync(definition.Id);
        }
        catch (Exception exception)
        {
            if (IsCurrentOpen(operationVersion))
            {
                Debug.LogError($"[QuestController] Score claim failed: {exception}");
                UIFeedbackToast.Show("퀘스트 점수를 획득하지 못했습니다.");
            }
            return;
        }
        finally
        {
            if (operationVersion == openVersion)
                questActionBusy = false;
        }
        if (!IsCurrentOpen(operationVersion)) return;
        if (result.Succeeded)
        {
            UIFeedbackToast.Show($"퀘스트 점수 {definition.Score}점을 획득했습니다.");
            Refresh();
            return;
        }
        UIFeedbackToast.Show(result.Failure == QuestScoreClaimFailure.AlreadyClaimed
            ? "이미 점수를 획득한 퀘스트입니다."
            : "퀘스트 점수를 획득하지 못했습니다.");
    }

    private async void HandleRewardClick(QuestRewardPeriod period, int threshold, RectTransform anchor)
    {
        if (rewardClaimBusy) return;
        QuestRewardMilestone milestone = GameConfig.QuestRewards?.Get(period, threshold);
        UserQuestData data = userDataManager?.UserData?.Quest;
        if (milestone == null || data == null) return;
        int score = period == QuestRewardPeriod.Daily ? data.DailyScore : data.WeeklyScore;
        IReadOnlyCollection<int> claimed = period == QuestRewardPeriod.Daily
            ? data.ClaimedDailyRewards
            : data.ClaimedWeeklyRewards;
        if (rewardDisplay == null)
        {
            Debug.LogError("[Quest] QuestRewardDisplay reference is missing.", this);
            return;
        }
        if (score < threshold || ContainsThreshold(claimed, threshold))
        {
            rewardDisplay.ShowPreview(milestone.Rewards, anchor);
            return;
        }

        rewardClaimBusy = true;
        int operationVersion = openVersion;
        QuestRewardClaimResult result;
        try
        {
            result = await userDataManager.ClaimQuestRewardAsync(period, threshold);
        }
        catch (Exception exception)
        {
            if (IsCurrentOpen(operationVersion))
            {
                Debug.LogError($"[QuestController] Reward claim failed: {exception}");
                UIFeedbackToast.Show("퀘스트 보상을 받을 수 없습니다.");
            }
            return;
        }
        finally
        {
            if (operationVersion == openVersion)
                rewardClaimBusy = false;
        }
        if (!IsCurrentOpen(operationVersion)) return;
        if (result.Succeeded)
        {
            Refresh();
            rewardDisplay.ShowAcquired(result.GrantedRewards);
            return;
        }
        UIFeedbackToast.Show(result.Failure switch
        {
            QuestRewardClaimFailure.InsufficientScore => "퀘스트 점수가 부족합니다.",
            QuestRewardClaimFailure.AlreadyClaimed => "이미 받은 보상입니다.",
            QuestRewardClaimFailure.SaveFailed => "보상 저장에 실패했습니다.",
            _ => "보상을 받을 수 없습니다.",
        });
    }

    private static bool ContainsThreshold(IReadOnlyCollection<int> claimed, int threshold)
    {
        if (claimed == null) return false;
        foreach (int value in claimed)
            if (value == threshold)
                return true;
        return false;
    }

    private void NavigateToQuest(QuestEventType eventType)
    {
        string panelName = eventType switch
        {
            QuestEventType.UnitRecruited => "RecruitPanel",
            QuestEventType.UnitLevelUp => "UnitPanel",
            QuestEventType.LabDeveloped => "LabPanel",
            _ => "BattlePanel",
        };
        if (lobbyTabView == null || !lobbyTabView.ShowPanel(panelName))
        {
            UIFeedbackToast.Show("이동할 화면을 찾지 못했습니다.");
            return;
        }
        Dismiss();
    }

    private IEnumerator UpdateTimer()
    {
        while (overlay != null && overlay.activeSelf)
        {
            if (timerText != null)
            {
                TimeSpan remaining = QuestPeriodPolicy.GetNextDailyResetUtc(DateTimeOffset.UtcNow) - DateTimeOffset.UtcNow;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
                timerText.text = $"{(int)remaining.TotalHours}시간 {remaining.Minutes:00}분";
            }
            yield return new WaitForSecondsRealtime(30f);
        }
        timerRoutine = null;
    }

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        return userDataManager?.UserData?.Quest != null;
    }

    private void Subscribe()
    {
        if (isSubscribed || userDataManager == null)
            return;

        userDataManager.OnQuestUpdated += HandleQuestUpdated;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnQuestUpdated -= HandleQuestUpdated;
        isSubscribed = false;
    }

    private void HandleQuestUpdated()
    {
        if (OverlayLifecycle.IsVisible)
            Refresh();
    }

    private bool IsCurrentOpen(int version) =>
        this != null && isActiveAndEnabled && version == openVersion && OverlayLifecycle.IsVisible;

    private void StopTimer()
    {
        if (timerRoutine == null)
            return;

        StopCoroutine(timerRoutine);
        timerRoutine = null;
    }
}
