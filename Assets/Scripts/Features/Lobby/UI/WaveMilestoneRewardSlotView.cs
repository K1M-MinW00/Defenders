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
    [SerializeField] private GameObject rewardItemPrefab;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Image claimButtonBackground;

    private readonly List<ShopRewardSlotView> rewardSlots = new();
    private int requiredWave;
    private Action<int> claimRequested;

    private void Awake() => claimButton?.onClick.AddListener(HandleClick);
    private void OnDestroy() => claimButton?.onClick.RemoveListener(HandleClick);

#if UNITY_EDITOR
    public void Configure(
        TMP_Text waveLabel,
        Transform itemContent,
        GameObject itemPrefab,
        TMP_Text buttonLabel,
        Button button,
        Image buttonBackground)
    {
        waveText = waveLabel;
        rewardContent = itemContent;
        rewardItemPrefab = itemPrefab;
        stateText = buttonLabel;
        claimButton = button;
        claimButtonBackground = buttonBackground;
    }
#endif

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

    private void BindRewards(IReadOnlyList<RewardData> rewards)
    {
        int count = rewards?.Count ?? 0;
        while (rewardSlots.Count < count)
        {
            GameObject instance = Instantiate(rewardItemPrefab, rewardContent);
            ShopRewardSlotView view = instance.GetComponent<ShopRewardSlotView>();
            if (view == null)
            {
                Destroy(instance);
                Debug.LogError("Wave reward item prefab is missing ShopRewardSlotView.", this);
                return;
            }
            rewardSlots.Add(view);
        }

        for (int i = 0; i < rewardSlots.Count; i++)
        {
            bool visible = i < count;
            rewardSlots[i].gameObject.SetActive(visible);
            if (visible)
                rewardSlots[i].Bind(rewards[i]);
        }
    }

    private void HandleClick() => claimRequested?.Invoke(requiredWave);
}
