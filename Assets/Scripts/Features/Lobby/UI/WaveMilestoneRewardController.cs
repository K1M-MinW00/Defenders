using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
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
    private UserDataManager userDataManager;
    private bool isSubscribed;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(overlay);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        openButton.onClick.AddListener(Open);
        claimAllButton.onClick.AddListener(ClaimAll);
        closeButton.onClick.AddListener(Dismiss);
        Lifecycle.HideImmediately();
    }

    private void OnEnable()
    {
        TryInitialize();
        Subscribe();
        RefreshBadge();
    }

    private void OnDisable()
    {
        operationVersion++;
        Unsubscribe();
        Lifecycle.HideImmediately();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        openButton?.onClick.RemoveListener(Open);
        claimAllButton?.onClick.RemoveListener(ClaimAll);
        closeButton?.onClick.RemoveListener(Dismiss);
    }

    public void Open()
    {
        if (!TryInitialize())
        {
            UIFeedbackToast.Show("유저 데이터를 불러오는 중입니다.");
            return;
        }

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
        notificationBadge.SetActive(userDataManager?.HasClaimableWaveMilestoneReward() == true);
    }

    private void RefreshPopup()
    {
        UserProgressData progress = userDataManager?.UserData?.Progress;
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
        if (claimBusy || userDataManager == null || claimOperation == null) return;
        claimBusy = true;
        int currentOperation = ++operationVersion;
        RefreshPopup();

        try
        {
            WaveMilestoneClaimResult result;
            try
            {
                result = await claimOperation(userDataManager);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[WaveMilestoneRewardController] Claim failed: {exception}");
                result = WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.SaveFailed);
            }

            if (!IsCurrentOperation(currentOperation))
                return;

            ShowResult(result);
            RefreshPopup();
            RefreshBadge();
        }
        finally
        {
            if (this != null)
            {
                claimBusy = false;
                if (isActiveAndEnabled)
                {
                    RefreshPopup();
                    RefreshBadge();
                }
            }
        }
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

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        bool initialized = userDataManager?.UserData?.Progress != null;
        openButton.interactable = initialized;
        return initialized;
    }

    private void Subscribe()
    {
        if (isSubscribed || userDataManager == null)
            return;

        userDataManager.OnProgressUpdated += HandleDataUpdated;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnProgressUpdated -= HandleDataUpdated;
        isSubscribed = false;
    }

    private bool IsCurrentOperation(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;

    private bool ValidateReferences()
    {
        bool valid = openButton != null && notificationBadge != null && overlay != null && titleText != null &&
                     rewardContent != null && rewardSlotPrefab != null && claimAllButton != null &&
                     claimAllButtonBackground != null && closeButton != null;
        if (!valid)
            Debug.LogError("[WaveMilestoneRewardController] Required UI references are not assigned.", this);
        return valid;
    }
}
