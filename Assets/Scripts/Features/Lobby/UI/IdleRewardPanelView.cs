using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class IdleRewardPanelView : MonoBehaviour
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
    [SerializeField] private GameObject rewardSlotPrefab;

    private readonly List<IdleRewardSlotView> slots = new();
    private Coroutine refreshRoutine;
    private bool claiming;
    private bool initialized;
    private int lastDisplayedMinute = -1;

#if UNITY_EDITOR
    public void SetRewardSlotPrefabEditor(GameObject value) => rewardSlotPrefab = value;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneInitialization()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (IdleRewardPanelView view in root.GetComponentsInChildren<IdleRewardPanelView>(true))
                view.Initialize();
    }

    private void Awake() => Initialize();

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        supplyButton?.onClick.AddListener(Open);
        closeButton?.onClick.AddListener(Close);
        claimButton?.onClick.AddListener(Claim);
        if (rewardGrid != null)
        {
            rewardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            rewardGrid.constraintCount = 5;
        }
        popup?.SetActive(false);
    }

    private void Open()
    {
        popup.SetActive(true);
        Refresh(true);
        if (refreshRoutine != null) StopCoroutine(refreshRoutine);
        refreshRoutine = StartCoroutine(RefreshEachMinute());
    }

    private void Close()
    {
        popup.SetActive(false);
        if (refreshRoutine != null) StopCoroutine(refreshRoutine);
        refreshRoutine = null;
    }

    private IEnumerator RefreshEachMinute()
    {
        while (popup.activeSelf)
        {
            Refresh(false);
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private void Refresh(bool force)
    {
        IdleRewardPreview preview = UserDataManager.Instance?.GetIdleRewardPreview();
        if (preview == null || (!force && preview.AccumulatedMinutes == lastDisplayedMinute)) return;
        lastDisplayedMinute = preview.AccumulatedMinutes;
        sectorDescriptionText.text = $"섹터 {preview.Sector} 기준 시간당 보상";
        goldHourlyText.text = FormatRate(preview.HourlyRates.FirstOrDefault(x => x.Type == RewardType.Gold));
        researchHourlyText.text = FormatRate(preview.HourlyRates.FirstOrDefault(x => x.Type == RewardType.ResearchMaterial));
        accumulatedTimeText.text = $"{preview.AccumulatedMinutes / 60}시간 {preview.AccumulatedMinutes % 60}분 / 최대 12시간";
        claimButton.interactable = preview.CanClaim && !claiming;
        RebuildSlots(preview.AccumulatedRewards);
    }

    private void RebuildSlots(IReadOnlyList<IdleRewardEntry> rewards)
    {
        foreach (IdleRewardSlotView slot in slots) if (slot != null) Destroy(slot.gameObject);
        slots.Clear();
        foreach (IdleRewardEntry reward in rewards)
        {
            GameObject instance = Instantiate(rewardSlotPrefab, rewardContent);
            IdleRewardSlotView slot = instance.GetComponent<IdleRewardSlotView>();
            instance.SetActive(true);
            slot.Bind(reward);
            slots.Add(slot);
        }
    }

    private static string FormatRate(IdleRewardEntry entry) => entry == null
        ? "0 /시간"
        : $"{entry.BaseAmount:N0}<color=#56FF69> (+{entry.BonusAmount:N0})</color> /시간";

    private async void Claim()
    {
        if (claiming) return;
        claiming = true;
        claimButton.interactable = false;
        IdleRewardClaimFailure failure = await UserDataManager.Instance.ClaimIdleRewardAsync();
        claiming = false;
        if (failure != IdleRewardClaimFailure.None)
        {
            UIFeedbackToast.Show(failure == IdleRewardClaimFailure.NotReady ? "수령할 보상이 없습니다." : "보상 수령에 실패했습니다.");
            Refresh(true);
            return;
        }
        UIFeedbackToast.Show("방치형 보상을 획득했습니다.");
        Close();
    }

    private void OnDestroy()
    {
        if (!initialized)
            return;

        supplyButton?.onClick.RemoveListener(Open);
        closeButton?.onClick.RemoveListener(Close);
        claimButton?.onClick.RemoveListener(Claim);
    }
}
