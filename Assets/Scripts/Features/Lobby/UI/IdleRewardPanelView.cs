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
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        if (TryInitialize())
            Subscribe();
    }

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager)
        {
            Unsubscribe();
            userDataManager = manager;
        }

        if (!isActiveAndEnabled || !TryInitialize())
            return;

        Subscribe();
        if (PopupLifecycle.IsVisible)
            Refresh(true);
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        claimOperationVersion++;
        StopRefreshRoutine();
        Unsubscribe();
        PopupLifecycle.HideImmediately();
    }

    private void Open()
    {
        if (!TryInitialize())
        {
            UIFeedbackToast.Show(Localize("idle.loading_user_data"));
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
        sectorDescriptionText.text = Localize("idle.sector_hourly_rewards", preview.Sector);
        goldHourlyText.text = FormatRate(preview.HourlyRates.FirstOrDefault(x => x.Type == RewardType.Gold));
        researchHourlyText.text = FormatRate(preview.HourlyRates.FirstOrDefault(x => x.Type == RewardType.ResearchMaterial));
        accumulatedTimeText.text = Localize(
            "idle.accumulated_time",
            preview.AccumulatedMinutes / 60,
            preview.AccumulatedMinutes % 60,
            preview.MaxMinutes / 60);
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
        ? Localize("idle.hourly_rate_zero")
        : Localize("idle.hourly_rate", entry.BaseAmount, entry.BonusAmount);

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
                UIFeedbackToast.Show(failure == IdleRewardClaimFailure.NotReady
                    ? Localize("idle.claim.not_ready")
                    : Localize("idle.claim.failed"));
                Refresh(true);
                return;
            }

            UIFeedbackToast.Show(Localize("idle.claim.succeeded"));
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
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        Unsubscribe();
        supplyButton?.onClick.RemoveListener(Open);
        closeButton?.onClick.RemoveListener(Dismiss);
        claimButton?.onClick.RemoveListener(Claim);
    }

    private bool TryInitialize()
    {
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

    private void HandleLocaleChanged(string _)
    {
        if (PopupLifecycle.IsVisible)
            Refresh(true);
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

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
