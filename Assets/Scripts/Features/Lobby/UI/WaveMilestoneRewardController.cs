using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    [SerializeField] private WaveMilestoneRewardSlotView rewardSlotPrefab;
    [SerializeField] private Button claimAllButton;
    [SerializeField] private Image claimAllButtonBackground;
    [SerializeField] private Button closeButton;

    private readonly List<WaveMilestoneRewardSlotView> slots = new();
    private ModalOverlayLifecycle lifecycle;
    private bool claimBusy;
    private int operationVersion;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(overlay);

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
        operationVersion++;
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

    public void Dismiss()
    {
        if (!claimBusy)
            Lifecycle.Close();
    }

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

        IReadOnlyList<WaveMilestoneReward> milestones = stage?.milestoneRewards?
            .Where(milestone => milestone != null)
            .ToList() ?? new List<WaveMilestoneReward>();
        int requiredCount = milestones.Count;
        EnsureSlotCount(requiredCount);
        bool anyClaimable = false;
        for (int i = 0; i < slots.Count; i++)
        {
            bool visible = i < requiredCount;
            slots[i].gameObject.SetActive(visible);
            if (!visible)
            {
                slots[i].Clear();
                continue;
            }
            WaveMilestoneReward milestone = milestones[i];
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

        if (closeButton != null)
            closeButton.interactable = !claimBusy;
    }

    private void EnsureSlotCount(int count)
    {
        if (rewardContent == null || rewardSlotPrefab == null) return;
        while (slots.Count < count)
        {
            WaveMilestoneRewardSlotView view = Instantiate(rewardSlotPrefab, rewardContent);
            slots.Add(view);
        }
    }

    private async void Claim(int requiredWave)
    {
        await ExecuteClaimAsync(manager => manager.ClaimWaveMilestoneRewardAsync(requiredWave));
    }

    private async void ClaimAll()
    {
        await ExecuteClaimAsync(manager => manager.ClaimAllWaveMilestoneRewardsAsync());
    }

    private async Task ExecuteClaimAsync(Func<UserDataManager, Task<WaveMilestoneClaimResult>> claimOperation)
    {
        UserDataManager manager = UserDataManager.Instance;
        if (claimBusy || manager == null || claimOperation == null) return;
        claimBusy = true;
        int currentOperation = ++operationVersion;
        RefreshPopup();

        WaveMilestoneClaimResult result;
        try
        {
            result = await claimOperation(manager);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[WaveMilestoneRewardController] Claim failed: {exception}");
            result = WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.SaveFailed);
        }

        claimBusy = false;
        if (!isActiveAndEnabled || currentOperation != operationVersion)
            return;

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
