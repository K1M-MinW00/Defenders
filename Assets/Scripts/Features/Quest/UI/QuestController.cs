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
    private bool rewardClaimBusy;
    private readonly List<QuestDefinition> orderedQuests = new(16);

    public void Configure(Button open, Button close, GameObject target)
    {
        openButton = open;
        closeButton = close;
        overlay = target;
    }

    private void Awake()
    {
        openButton?.onClick.AddListener(Open);
        closeButton?.onClick.AddListener(Dismiss);
    }

    private void OnDestroy()
    {
        openButton?.onClick.RemoveListener(Open);
        closeButton?.onClick.RemoveListener(Dismiss);
    }

    public async void Open()
    {
        if (overlay == null)
            return;

        UserDataManager manager = UserDataManager.Instance;
        if (manager?.IsLoaded == true)
            await manager.RefreshQuestPeriodsAsync();

        overlay.SetActive(true);
        overlay.transform.SetAsLastSibling();
        Refresh();
        if (timerRoutine != null) StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(UpdateTimer());
    }

    public void Dismiss()
    {
        rewardDisplay?.Hide();
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        if (overlay != null)
            ModalTransitionUtility.RequestExit(overlay, () => overlay.SetActive(false));
    }

    public void Refresh()
    {
        UserQuestData data = UserDataManager.Instance?.UserData?.Quest;
        if (overlay == null || data == null) return;

        weeklyRewardTrack?.SetState(data.WeeklyScore, data.ClaimedWeeklyRewards);
        dailyRewardTrack?.SetState(data.DailyScore, data.ClaimedDailyRewards);

        BuildOrderedQuestList(data);
        for (int i = 0; i < Math.Min(questItems.Length, orderedQuests.Count); i++)
        {
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
        UserQuestData data = UserDataManager.Instance?.UserData?.Quest;
        if (data == null) return;
        if (QuestProgressService.GetProgress(data, definition.Id) < definition.Target)
        {
            NavigateToQuest(definition.EventType);
            return;
        }

        QuestScoreClaimResult result = await UserDataManager.Instance.ClaimQuestScoreAsync(definition.Id);
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
        UserQuestData data = UserDataManager.Instance?.UserData?.Quest;
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
        QuestRewardClaimResult result = await UserDataManager.Instance.ClaimQuestRewardAsync(period, threshold);
        rewardClaimBusy = false;
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

#if UNITY_EDITOR
    public void SetRewardDisplay(QuestRewardDisplay display) => rewardDisplay = display;

    public void SetViewReferences(QuestRewardTrackView dailyTrack, QuestRewardTrackView weeklyTrack,
        QuestRewardView[] rewards, QuestItemView[] items, TMP_Text timer)
    {
        dailyRewardTrack = dailyTrack;
        weeklyRewardTrack = weeklyTrack;
        rewardViews = rewards ?? Array.Empty<QuestRewardView>();
        questItems = items ?? Array.Empty<QuestItemView>();
        timerText = timer;
    }
#endif

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
}
