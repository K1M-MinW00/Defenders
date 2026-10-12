using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WaveMilestoneRewardController : MonoBehaviour, IModalDismissHandler
{
    [Header("Entry")]
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject notificationBadge;
    [Header("Overlay")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform rewardContent;
    [SerializeField] private GameObject rewardSlotPrefab;
    [SerializeField] private Button claimAllButton;
    [SerializeField] private Image claimAllButtonBackground;
    [SerializeField] private Button closeButton;

    private readonly List<WaveMilestoneRewardSlotView> slots = new();
    private ModalOverlayLifecycle lifecycle;
    private bool claimBusy;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(overlay);

#if UNITY_EDITOR
    public void Configure(
        Button entryButton,
        GameObject badge,
        GameObject overlayObject,
        TMP_Text title,
        Transform content,
        GameObject slotPrefab,
        Button claimAll,
        Image claimAllBackground,
        Button close)
    {
        openButton = entryButton;
        notificationBadge = badge;
        overlay = overlayObject;
        titleText = title;
        rewardContent = content;
        rewardSlotPrefab = slotPrefab;
        claimAllButton = claimAll;
        claimAllButtonBackground = claimAllBackground;
        closeButton = close;
    }
#endif

    private void Awake()
    {
        openButton?.onClick.AddListener(Open);
        claimAllButton?.onClick.AddListener(ClaimAll);
        closeButton?.onClick.AddListener(Dismiss);
    }

    private void OnEnable()
    {
        if (UserDataManager.Instance != null) UserDataManager.Instance.OnProgressUpdated += HandleDataUpdated;
        RefreshBadge();
    }

    private void OnDisable()
    {
        if (UserDataManager.Instance != null) UserDataManager.Instance.OnProgressUpdated -= HandleDataUpdated;
    }

    private void OnDestroy()
    {
        openButton?.onClick.RemoveListener(Open);
        claimAllButton?.onClick.RemoveListener(ClaimAll);
        closeButton?.onClick.RemoveListener(Dismiss);
    }

    public void Open()
    {
        RefreshPopup();
        Lifecycle.Show();
    }

    public void Dismiss() => Lifecycle.Close();

    private void HandleDataUpdated()
    {
        RefreshBadge();
        if (Lifecycle.IsVisible) RefreshPopup();
    }

    public void RefreshBadge()
    {
        if (notificationBadge != null)
            notificationBadge.SetActive(UserDataManager.Instance?.HasClaimableWaveMilestoneReward() == true);
    }

    private void RefreshPopup()
    {
        UserProgressData progress = UserDataManager.Instance?.UserData?.Progress;
        StageDataSO stage = WaveMilestoneRewardRules.ResolveRewardStage(progress, GameConfig.Stages);
        if (titleText != null) titleText.text = stage == null ? "웨이브 달성 보상" : $"스테이지\n{stage.StageKey}";

        int requiredCount = stage?.milestoneRewards?.Count ?? 0;
        EnsureSlotCount(requiredCount);
        bool anyClaimable = false;
        for (int i = 0; i < slots.Count; i++)
        {
            bool visible = i < requiredCount;
            slots[i].gameObject.SetActive(visible);
            if (!visible) continue;
            WaveMilestoneReward milestone = stage.milestoneRewards[i];
            bool unlocked = WaveMilestoneRewardRules.IsUnlocked(progress, stage.sector, stage.stage, milestone.requiredWave);
            bool claimed = WaveMilestoneRewardRules.IsClaimed(progress.WaveRewardProgress, milestone.requiredWave);
            anyClaimable |= unlocked && !claimed;
            slots[i].Bind(milestone, unlocked, claimed, claimBusy, Claim);
        }
        if (claimAllButton != null)
        {
            bool claimAllEnabled = anyClaimable && !claimBusy;
            claimAllButton.interactable = claimAllEnabled;
            if (claimAllButtonBackground != null)
                claimAllButtonBackground.color = claimAllEnabled
                    ? new Color(0.91f, 0.15f, 0.24f, 1f)
                    : new Color(0.32f, 0.34f, 0.38f, 1f);
        }
    }

    private void EnsureSlotCount(int count)
    {
        if (rewardContent == null || rewardSlotPrefab == null) return;
        while (slots.Count < count)
        {
            GameObject instance = Instantiate(rewardSlotPrefab, rewardContent);
            WaveMilestoneRewardSlotView view = instance.GetComponent<WaveMilestoneRewardSlotView>();
            if (view == null)
            {
                Destroy(instance);
                Debug.LogError("Wave milestone reward slot prefab is missing its view component.", this);
                return;
            }
            slots.Add(view);
        }
    }

    private async void Claim(int requiredWave)
    {
        if (claimBusy) return;
        claimBusy = true;
        RefreshPopup();
        WaveMilestoneClaimResult result = await UserDataManager.Instance.ClaimWaveMilestoneRewardAsync(requiredWave);
        claimBusy = false;
        ShowResult(result);
        RefreshPopup();
        RefreshBadge();
    }

    private async void ClaimAll()
    {
        if (claimBusy) return;
        claimBusy = true;
        RefreshPopup();
        WaveMilestoneClaimResult result = await UserDataManager.Instance.ClaimAllWaveMilestoneRewardsAsync();
        claimBusy = false;
        ShowResult(result);
        RefreshPopup();
        RefreshBadge();
    }

    private static void ShowResult(WaveMilestoneClaimResult result)
    {
        UIFeedbackToast.Show(result.Succeeded ? "웨이브 달성 보상을 받았습니다." : result.Failure switch
        {
            WaveMilestoneClaimFailure.Locked => "아직 달성하지 않은 보상입니다.",
            WaveMilestoneClaimFailure.AlreadyClaimed => "이미 받은 보상입니다.",
            WaveMilestoneClaimFailure.SaveFailed => "보상 저장에 실패했습니다.",
            _ => "보상을 받을 수 없습니다.",
        });
    }
}
