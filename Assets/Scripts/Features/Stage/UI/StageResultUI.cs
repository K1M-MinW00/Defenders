using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageResultUI : MonoBehaviour
{
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject stageFailPanel;

    [Header("Clear Result")]
    [SerializeField] private TMP_Text clearStageText;
    [SerializeField] private TMP_Text clearWaveText;
    [SerializeField] private RectTransform clearRewardViewport;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;

    [Header("Fail Result")]
    [SerializeField] private TMP_Text failStageText;
    [SerializeField] private TMP_Text failWaveText;
    [SerializeField] private Image failFuelIcon;
    [SerializeField] private TMP_Text failFuelAmountText;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    private StageSessionController session;
    private RectTransform clearRewardContent;
    private bool isReturningToLobby;

    public void Initialize(StageSessionController stageSession)
    {
        session = stageSession;
        isReturningToLobby = false;
        HideAll();
    }

    public void ShowClear(StageDataSO stage, int clearedWaveCount, IReadOnlyList<RewardData> rewards)
    {
        HideAll();

        if (clearStageText != null)
            clearStageText.text = stage != null ? $"스테이지 {stage.StageKey} 클리어" : "스테이지 클리어";

        if (clearWaveText != null)
        {
            int totalWaveCount = stage?.waves?.Count ?? 0;
            clearWaveText.text = $"({clearedWaveCount} / {totalWaveCount})";
        }

        BindClearRewards(rewards);
        stageClearPanel?.SetActive(true);
    }

    public void ShowFail(StageDataSO stage, int clearedWaveCount, IReadOnlyList<RewardData> rewards)
    {
        HideAll();

        if (failStageText != null)
            failStageText.text = stage != null ? $"스테이지 {stage.StageKey} 실패" : "스테이지 실패";

        if (failWaveText != null)
        {
            int totalWaveCount = stage?.waves?.Count ?? 0;
            failWaveText.text = $"({clearedWaveCount} / {totalWaveCount})";
        }

        RewardData fuelReward = FindReward(rewards, RewardType.Fuel);
        if (failFuelIcon != null)
            failFuelIcon.sprite = GameConfig.Icons?.GetResourceIcon(RewardType.Fuel);
        if (failFuelAmountText != null)
            failFuelAmountText.text = fuelReward != null ? $"+{fuelReward.Amount:N0}" : "+0";

        stageFailPanel?.SetActive(true);
    }

    public void HideAll()
    {
        stageClearPanel?.SetActive(false);
        stageFailPanel?.SetActive(false);
    }

    private void BindClearRewards(IReadOnlyList<RewardData> rewards)
    {
        EnsureRewardScroll();
        if (clearRewardContent == null || rewardSlotPrefab == null)
            return;

        for (int i = clearRewardContent.childCount - 1; i >= 0; i--)
            Destroy(clearRewardContent.GetChild(i).gameObject);

        if (rewards == null)
            return;

        foreach (RewardData reward in rewards)
        {
            if (reward == null)
                continue;

            ShopRewardSlotView slot = Instantiate(rewardSlotPrefab, clearRewardContent);
            if (slot.transform is RectTransform slotRect)
                slotRect.sizeDelta = new Vector2(180f, 180f);
            slot.Bind(reward);
        }
    }

    private void EnsureRewardScroll()
    {
        if (clearRewardContent != null || clearRewardViewport == null)
            return;

        for (int i = clearRewardViewport.childCount - 1; i >= 0; i--)
        {
            GameObject child = clearRewardViewport.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }

        HorizontalLayoutGroup oldLayout = clearRewardViewport.GetComponent<HorizontalLayoutGroup>();
        if (oldLayout != null)
            oldLayout.enabled = false;

        if (clearRewardViewport.GetComponent<RectMask2D>() == null)
            clearRewardViewport.gameObject.AddComponent<RectMask2D>();

        ScrollRect scroll = clearRewardViewport.GetComponent<ScrollRect>() ??
                            clearRewardViewport.gameObject.AddComponent<ScrollRect>();

        GameObject contentObject = new("RewardContent", typeof(RectTransform));
        clearRewardContent = contentObject.GetComponent<RectTransform>();
        clearRewardContent.SetParent(clearRewardViewport, false);
        clearRewardContent.anchorMin = new Vector2(0f, 0f);
        clearRewardContent.anchorMax = new Vector2(0f, 1f);
        clearRewardContent.pivot = new Vector2(0f, 0.5f);
        clearRewardContent.anchoredPosition = Vector2.zero;
        clearRewardContent.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup layout = contentObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 20, 20);
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        scroll.viewport = clearRewardViewport;
        scroll.content = clearRewardContent;
        scroll.horizontal = true;
        scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;
    }

    private static RewardData FindReward(IReadOnlyList<RewardData> rewards, RewardType type)
    {
        if (rewards == null)
            return null;

        foreach (RewardData reward in rewards)
        {
            if (reward?.Type == type)
                return reward;
        }

        return null;
    }

    public async void OnClickReturnToLobby()
    {
        if (isReturningToLobby)
            return;

        isReturningToLobby = true;

        if (session == null || !await session.TryPrepareExitAsync())
        {
            isReturningToLobby = false;
            Debug.LogWarning("[StageResultUI] Progress and rewards are not saved yet. Lobby transition was blocked; press again to retry.");
            return;
        }

        SceneTransitionResult result = await SceneFlowService.Shared.LoadAsync(lobbySceneName);
        if (result != SceneTransitionResult.Succeeded && this != null)
        {
            isReturningToLobby = false;
            Debug.LogError($"[StageResultUI] Failed to return to lobby: {result}");
        }
    }
}
