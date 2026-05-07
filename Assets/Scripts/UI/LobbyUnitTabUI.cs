using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LobbyUnitTabUI : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform battleSquadRoot;
    [SerializeField] private Transform waitingListRoot;

    [Header("Prefab")]
    [SerializeField] private UnitCardUI unitCardPrefab;
    [SerializeField] private UnitDetailPanel unitDetailPanel;

    private readonly List<LobbyUnitViewModel> battleSquadViewModels = new();
    private readonly List<LobbyUnitViewModel> waitingListViewModels = new();

    private UnitCardUI pendingSwapCard;
    private string pendingSwapUnitId;


    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        UserDataRoot userData = UserDataManager.Instance.Data;

        if (userData == null)
        {
            Debug.LogError("[LobbyUnitTabUI] UserData is null.");
            return;
        }

        UserRosterData roster = userData.Roster;

        if (roster == null)
        {
            Debug.LogError("[LobbyUnitTabUI] Roster is null.");
            return;
        }

        battleSquadViewModels.Clear();
        waitingListViewModels.Clear();

        List<UserUnitData> ownedUnits = roster.OwnedUnits ?? new List<UserUnitData>();
        List<string> selectedUnitIds = roster.SelectedUnitIds ?? new List<string>();

        Dictionary<string, UserUnitData> ownedUnitMap = ownedUnits
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.UnitId))
            .GroupBy(x => x.UnitId)
            .ToDictionary(g => g.Key, g => g.First());

        HashSet<string> selectedSet = new HashSet<string>(selectedUnitIds);

        BuildBattleSquadViewModels(selectedUnitIds, ownedUnitMap);
        BuildWaitingListViewModels(selectedSet, ownedUnitMap);

        RebuildCardList(battleSquadRoot, battleSquadViewModels);
        RebuildCardList(waitingListRoot, waitingListViewModels);
    }

    private void BuildBattleSquadViewModels(List<string> selectedUnitIds,Dictionary<string, UserUnitData> ownedUnitMap)
    {
        foreach (string unitId in selectedUnitIds)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                continue;

            UnitDataSO unitData = UnitMasterDataManager.Instance.GetUnitData(unitId);

            if (unitData == null)
            {
                Debug.LogWarning($"[LobbyUnitTabUI] UnitDataSO not found for selected unitId: {unitId}");
                continue;
            }

            ownedUnitMap.TryGetValue(unitId, out UserUnitData userUnit);

            LobbyUnitViewModel vm = CreateViewModel(unitData, userUnit, true);
            battleSquadViewModels.Add(vm);
        }
    }

    private void BuildWaitingListViewModels(HashSet<string> selectedSet,Dictionary<string, UserUnitData> ownedUnitMap)
    {
        IReadOnlyCollection<UnitDataSO> allUnitData = UnitMasterDataManager.Instance.GetAllUnitData();

        foreach (UnitDataSO unitData in allUnitData)
        {
            if (unitData == null || string.IsNullOrWhiteSpace(unitData.unitId))
                continue;

            if (selectedSet.Contains(unitData.unitId))
                continue;

            ownedUnitMap.TryGetValue(unitData.unitId, out UserUnitData userUnit);

            LobbyUnitViewModel vm = CreateViewModel(unitData, userUnit, false);
            waitingListViewModels.Add(vm);
        }
    }

    private LobbyUnitViewModel CreateViewModel(UnitDataSO unitData, UserUnitData userUnit, bool isSelected)
    {
        bool isOwned = userUnit != null;

        return new LobbyUnitViewModel
        {
            UnitId = unitData.unitId,
            Icon = unitData.icon,

            IsOwned = isOwned,
            IsSelected = isSelected,

            Level = isOwned ? userUnit.Level : 0,
        };
    }

    private void RebuildCardList(Transform root, List<LobbyUnitViewModel> viewModels)
    {
        if (root == null || unitCardPrefab == null)
            return;

        ClearChildren(root);

        foreach (LobbyUnitViewModel vm in viewModels)
        {
            UnitCardUI card = Instantiate(unitCardPrefab, root);
            card.Bind(vm);

            card.OnClicked += HandleCardClicked;
            card.OnLongPressed += HandleCardLongPressed;
        }
    }

    private void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    private async void HandleCardClicked(UnitCardUI card, LobbyUnitViewModel vm)
    {
        if (vm == null)
            return;

        // 교체 모드가 아닐 때는 상세 정보 표시
        if (string.IsNullOrEmpty(pendingSwapUnitId))
        {
            ShowUnitDetail(vm);
            return;
        }

        // 교체 모드 중 같은 카드 클릭 → 취소
        if (vm.UnitId == pendingSwapUnitId)
        {
            ClearPendingSwap();
            return;
        }
        
        if (!vm.IsOwned)
            return;

        bool success;

        if (vm.IsSelected)
        {
            // 전투 명단 내부 위치 교환
            success = await SwapSelectedUnitPositionAsync(pendingSwapUnitId, vm.UnitId);
        }
        else
        {
            // 전투 명단 유닛 ↔ 대기 명단 유닛 교체
            success = await ReplaceSelectedUnitAsync(pendingSwapUnitId, vm.UnitId);
        }

        ClearPendingSwap();

        if (success)
            Refresh();
    }

    private void ShowUnitDetail(LobbyUnitViewModel vm)
    {
        if (unitDetailPanel == null)
        {
            Debug.LogWarning("[LobbyUnitTabUI] UnitDetailPanel is missing.");
            return;
        }

        unitDetailPanel.Open(vm);
    }
    private void HandleCardLongPressed(UnitCardUI card, LobbyUnitViewModel vm)
    {
        if (vm == null)
            return;

        if (!vm.IsOwned)
            return;

        // 전투 부대 카드만 롱프레스로 교체 대상 지정
        if (!vm.IsSelected)
            return;

        ClearPendingSwap();

        pendingSwapCard = card;
        pendingSwapUnitId = vm.UnitId;

        pendingSwapCard.StartShake();

        Debug.Log($"[LobbyUnitTabUI] Swap mode started: {pendingSwapUnitId}");
    }

    private async Task<bool> SwapSelectedUnitPositionAsync(string firstUnitId, string secondUnitId)
    {
        UserRosterData roster = UserDataManager.Instance.Data.Roster;

        if (roster == null || roster.SelectedUnitIds == null)
            return false;

        int firstIndex = roster.SelectedUnitIds.IndexOf(firstUnitId);
        int secondIndex = roster.SelectedUnitIds.IndexOf(secondUnitId);

        if (firstIndex < 0 || secondIndex < 0)
            return false;

        (roster.SelectedUnitIds[firstIndex], roster.SelectedUnitIds[secondIndex]) =
            (roster.SelectedUnitIds[secondIndex], roster.SelectedUnitIds[firstIndex]);

        UserDataManager.Instance.MarkDirty();

        return await UserDataManager.Instance.SaveAsync();
    }

    private async Task<bool> ReplaceSelectedUnitAsync(string oldUnitId, string newUnitId)
    {
        UserRosterData roster = UserDataManager.Instance.Data.Roster;

        if (roster == null || roster.SelectedUnitIds == null)
            return false;

        int index = roster.SelectedUnitIds.IndexOf(oldUnitId);

        if (index < 0)
            return false;

        if (roster.SelectedUnitIds.Contains(newUnitId))
            return false;

        roster.SelectedUnitIds[index] = newUnitId;

        UserDataManager.Instance.MarkDirty();

        return await UserDataManager.Instance.SaveAsync();
    }
    private void ClearPendingSwap()
    {
        if (pendingSwapCard != null)
            pendingSwapCard.StopShake();

        pendingSwapCard = null;
        pendingSwapUnitId = null;
    }


}