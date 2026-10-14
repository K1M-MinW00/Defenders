using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class IdleRewardPanelView : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private Button supplyButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject popup;
    [SerializeField] private TMP_Text sectorDescriptionText;
    [SerializeField] private TMP_Text goldHourlyText;
    [SerializeField] private TMP_Text researchHourlyText;
    [SerializeField] private TMP_Text accumulatedTimeText;
    [SerializeField] private Transform rewardContent;
    [SerializeField] private GridLayoutGroup rewardGrid;
    [SerializeField] private IdleRewardSlotView rewardSlotPrefab;

    private readonly List<IdleRewardSlotView> slots = new();
    private Coroutine refreshRoutine;
    private bool claiming;
    private int lastDisplayedMinute = -1;
    private int claimOperationVersion;
    private UserDataManager userDataManager;
    private bool isSubscribed;
    private ModalOverlayLifecycle popupLifecycle;
    private ModalOverlayLifecycle PopupLifecycle =>
        popupLifecycle ??= new ModalOverlayLifecycle(popup);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        supplyButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Dismiss);
        claimButton.onClick.AddListener(Claim);
        rewardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        rewardGrid.constraintCount = 5;
        PopupLifecycle.HideImmediately();
    }

    private void OnEnable()
    {
        TryInitialize();
        Subscribe();
    }

    private void OnDisable()
    {
        claimOperationVersion++;
        StopRefreshRoutine();
        Unsubscribe();
        PopupLifecycle.HideImmediately();
    }

    private void Open()
    {
        if (!TryInitialize())
        {
            UIFeedbackToast.Show("유저 데이터를 불러오는 중입니다.");
            return;
        }

        PopupLifecycle.Show();
        Refresh(true);
        StopRefreshRoutine();
        refreshRoutine = StartCoroutine(RefreshEachMinute());
    }

    public void Dismiss()
    {
        if (claiming)
            return;

        StopRefreshRoutine();
        PopupLifecycle.Close();
    }

    private IEnumerator RefreshEachMinute()
    {
        while (PopupLifecycle.IsVisible)
        {
            Refresh(false);
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private void Refresh(bool force)
    {
        IdleRewardPreview preview = userDataManager?.GetIdleRewardPreview();
        if (preview == null || (!force && preview.AccumulatedMinutes == lastDisplayedMinute)) return;
        lastDisplayedMinute = preview.AccumulatedMinutes;
        sectorDescriptionText.text = $"섹터 {preview.Sector} 기준 시간당 보상";
        goldHourlyText.text = FormatRate(preview.HourlyRates.FirstOrDefault(x => x.Type == RewardType.Gold));
        researchHourlyText.text = FormatRate(preview.HourlyRates.FirstOrDefault(x => x.Type == RewardType.ResearchMaterial));
        accumulatedTimeText.text = $"{preview.AccumulatedMinutes / 60}시간 {preview.AccumulatedMinutes % 60}분 / 최대 {preview.MaxMinutes / 60}시간";
        claimButton.interactable = preview.CanClaim && !claiming;
        ReconcileSlots(preview.AccumulatedRewards);
    }

    private void ReconcileSlots(IReadOnlyList<IdleRewardEntry> rewards)
    {
        int visibleCount = 0;
        if (rewards != null)
        {
            foreach (IdleRewardEntry reward in rewards)
            {
                if (reward == null)
                    continue;

                IdleRewardSlotView slot = GetOrCreateSlot(visibleCount++);
                slot.gameObject.SetActive(true);
                slot.Bind(reward);
            }
        }

        HideSlotsFrom(visibleCount);
    }

    private IdleRewardSlotView GetOrCreateSlot(int index)
    {
        if (index < slots.Count)
            return slots[index];

        IdleRewardSlotView slot = Instantiate(rewardSlotPrefab, rewardContent);
        slots.Add(slot);
        return slot;
    }

    private void HideSlotsFrom(int firstHiddenIndex)
    {
        for (int i = firstHiddenIndex; i < slots.Count; i++)
        {
            slots[i].Clear();
            slots[i].gameObject.SetActive(false);
        }
    }

    private static string FormatRate(IdleRewardEntry entry) => entry == null
        ? "0 /시간"
        : $"{entry.BaseAmount:N0}<color=#56FF69> (+{entry.BonusAmount:N0})</color> /시간";

    private async void Claim()
    {
        if (claiming || !TryInitialize()) return;

        claiming = true;
        claimButton.interactable = false;
        closeButton.interactable = false;
        int operationVersion = ++claimOperationVersion;

        try
        {
            IdleRewardClaimFailure failure;
            try
            {
                failure = await userDataManager.ClaimIdleRewardAsync();
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[IdleRewardPanelView] Claim failed: {exception}");
                failure = IdleRewardClaimFailure.SaveFailed;
            }

            if (!IsCurrentOperation(operationVersion))
                return;

            if (failure != IdleRewardClaimFailure.None)
            {
                UIFeedbackToast.Show(failure == IdleRewardClaimFailure.NotReady ? "수령할 보상이 없습니다." : "보상 수령에 실패했습니다.");
                Refresh(true);
                return;
            }

            UIFeedbackToast.Show("방치형 보상을 획득했습니다.");
            StopRefreshRoutine();
            PopupLifecycle.Close();
        }
        finally
        {
            if (this != null)
            {
                claiming = false;
                if (isActiveAndEnabled)
                {
                    closeButton.interactable = true;
                    if (PopupLifecycle.IsVisible)
                        Refresh(true);
                }
            }
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
        supplyButton?.onClick.RemoveListener(Open);
        closeButton?.onClick.RemoveListener(Dismiss);
        claimButton?.onClick.RemoveListener(Claim);
    }

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        bool initialized = userDataManager?.UserData != null;
        supplyButton.interactable = initialized;
        return initialized;
    }

    private void Subscribe()
    {
        if (isSubscribed || userDataManager == null)
            return;

        userDataManager.OnIdleRewardUpdated += HandleIdleRewardUpdated;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnIdleRewardUpdated -= HandleIdleRewardUpdated;
        isSubscribed = false;
    }

    private void HandleIdleRewardUpdated()
    {
        if (PopupLifecycle.IsVisible)
            Refresh(true);
    }

    private bool IsCurrentOperation(int version) =>
        this != null && isActiveAndEnabled && version == claimOperationVersion;

    private void StopRefreshRoutine()
    {
        if (refreshRoutine == null)
            return;

        StopCoroutine(refreshRoutine);
        refreshRoutine = null;
    }

    private bool ValidateReferences()
    {
        bool valid = supplyButton != null && closeButton != null && claimButton != null && popup != null &&
                     sectorDescriptionText != null && goldHourlyText != null && researchHourlyText != null &&
                     accumulatedTimeText != null && rewardContent != null && rewardGrid != null && rewardSlotPrefab != null;
        if (!valid)
            Debug.LogError("[IdleRewardPanelView] Required UI references are not assigned.", this);
        return valid;
    }
}
