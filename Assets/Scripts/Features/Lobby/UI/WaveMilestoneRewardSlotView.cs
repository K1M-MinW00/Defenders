using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WaveMilestoneRewardSlotView : MonoBehaviour
{
    private static readonly Color ClaimableColor = new(0.91f, 0.15f, 0.24f, 1f);
    private static readonly Color DisabledColor = new(0.32f, 0.34f, 0.38f, 1f);

    [SerializeField] private TMP_Text waveText;
    [SerializeField] private Transform rewardContent;
    [SerializeField] private ShopRewardSlotView rewardItemPrefab;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Image claimButtonBackground;

    private readonly List<ShopRewardSlotView> rewardSlots = new();
    private int requiredWave;
    private Action<int> claimRequested;

    private void Awake() => claimButton?.onClick.AddListener(HandleClick);
    private void OnDestroy() => claimButton?.onClick.RemoveListener(HandleClick);

    public void Bind(WaveMilestoneReward milestone, bool unlocked, bool claimed, bool busy, Action<int> onClaimRequested)
    {
        requiredWave = milestone.requiredWave;
        claimRequested = onClaimRequested;
        waveText.text = $"WAVE\n<size=150%>{requiredWave}</size>";
        BindRewards(milestone.rewards);

        bool claimable = unlocked && !claimed && !busy;
        stateText.text = claimed ? "수령 완료" : unlocked ? "수령" : "잠김";
        claimButton.interactable = claimable;
        if (claimButtonBackground != null)
            claimButtonBackground.color = claimable ? ClaimableColor : DisabledColor;
    }

    public void Clear()
    {
        requiredWave = 0;
        claimRequested = null;
        if (waveText != null) waveText.text = string.Empty;
        if (stateText != null) stateText.text = string.Empty;
        if (claimButton != null) claimButton.interactable = false;
        if (claimButtonBackground != null) claimButtonBackground.color = DisabledColor;
        HideRewardSlotsFrom(0);
    }

    private void BindRewards(IReadOnlyList<RewardData> rewards)
    {
        int count = rewards?.Count ?? 0;
        while (rewardSlots.Count < count)
        {
            ShopRewardSlotView view = Instantiate(rewardItemPrefab, rewardContent);
            rewardSlots.Add(view);
        }

        for (int i = 0; i < rewardSlots.Count; i++)
        {
            bool visible = i < count;
            rewardSlots[i].gameObject.SetActive(visible);
            if (visible)
                rewardSlots[i].Bind(rewards[i]);
            else
                rewardSlots[i].Clear();
        }
    }

    private void HideRewardSlotsFrom(int firstHiddenIndex)
    {
        for (int i = firstHiddenIndex; i < rewardSlots.Count; i++)
        {
            rewardSlots[i].Clear();
            rewardSlots[i].gameObject.SetActive(false);
        }
    }

    private void HandleClick() => claimRequested?.Invoke(requiredWave);
}
