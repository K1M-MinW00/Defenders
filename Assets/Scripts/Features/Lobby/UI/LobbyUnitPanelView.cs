using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LobbyUnitPanelView : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform selectedUnitRoot;
    [SerializeField] private Transform ownedUnitRoot;

    [Header("Prefab")]
    [SerializeField] private UnitCardUI unitCardPrefab;
    [SerializeField] private UnitDetailView unitDetailPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text goldText;

    private readonly List<LobbyUnitViewModel> selectedUnitViewModels = new();
    private readonly List<LobbyUnitViewModel> ownedUnitListViewModels = new();

    private UserResourceData resource;
    private UserRosterData roster;

    private UnitCardUI pendingSwapCard;
    private string pendingSwapUnitId;
    private bool isChangingFormation;

    private void Awake()
    {
        resource = UserDataManager.Instance.UserData.Resource;
        roster = UserDataManager.Instance.UserData.Roster;
    }

    private void OnEnable()
    {
        UserDataManager.Instance.OnResourceUpdated += RefreshView;
        RefreshView();
    }

    private void OnDisable()
    {
        if (UserDataManager.Instance == null)
            return;

        UserDataManager.Instance.OnResourceUpdated -= RefreshView;
    }

    private void RefreshGold()
    {
        goldText.text = resource.Gold.ToString("N0");
    }

    private void RefreshView()
    {
        resource = UserDataManager.Instance.UserData.Resource;
        roster = UserDataManager.Instance.UserData.Roster;

        if (resource == null || roster == null)
            return;

        RefreshGold();

        selectedUnitViewModels.Clear();
        ownedUnitListViewModels.Clear();

        List<UserUnitData> ownedUnits = roster.OwnedUnits ?? new List<UserUnitData>();
        List<string> selectedUnitIds = roster.SelectedUnitIds ?? new List<string>();

        Dictionary<string, UserUnitData> ownedUnitMap = ownedUnits
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.UnitId))
            .GroupBy(x => x.UnitId)
            .ToDictionary(g => g.Key, g => g.First());

        HashSet<string> selectedSet = new HashSet<string>(selectedUnitIds);

        BuildSelectedUnitViewModels(selectedUnitIds, ownedUnitMap);
        BuildOwnedUnitViewModels(selectedSet, ownedUnitMap);

        BuildCardList(selectedUnitRoot, selectedUnitViewModels);
        BuildCardList(ownedUnitRoot, ownedUnitListViewModels);
    }

    private void BuildSelectedUnitViewModels(List<string> selectedUnitIds, Dictionary<string, UserUnitData> ownedUnitMap)
    {
        foreach (string unitId in selectedUnitIds)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                continue;

            UnitDataSO unitData = UnitDatabase.Get(unitId);

            if (unitData == null)
            {
                Debug.LogWarning($"[LobbyUnitTabUI] UnitDataSO not found for selected unitId: {unitId}");
                continue;
            }

            ownedUnitMap.TryGetValue(unitId, out UserUnitData userUnit);

            LobbyUnitViewModel vm = CreateViewModel(unitData, userUnit, true);
            selectedUnitViewModels.Add(vm);
        }
    }

    private void BuildOwnedUnitViewModels(HashSet<string> selectedSet, Dictionary<string, UserUnitData> ownedUnitMap)
    {
        IReadOnlyCollection<UnitDataSO> allUnitData = UnitDatabase.GetAll();

        foreach (UnitDataSO unitData in allUnitData)
        {
            if (unitData == null || string.IsNullOrWhiteSpace(unitData.unitId))
                continue;

            if (selectedSet.Contains(unitData.unitId))
                continue;

            ownedUnitMap.TryGetValue(unitData.unitId, out UserUnitData userUnit);

            LobbyUnitViewModel vm = CreateViewModel(unitData, userUnit, false);
            ownedUnitListViewModels.Add(vm);
        }
    }

    private LobbyUnitViewModel CreateViewModel(UnitDataSO unitData, UserUnitData userUnit, bool isSelected)
    {
        return new LobbyUnitViewModel
        {
            UnitId = unitData.unitId,
            Icon = unitData.icon,

            IsOwned = userUnit != null,
            IsSelected = isSelected,
        };
    }

    private void BuildCardList(Transform root, List<LobbyUnitViewModel> viewModels)
    {
        if (root == null || unitCardPrefab == null)
            return;

        ClearCardList(root);

        foreach (LobbyUnitViewModel vm in viewModels)
        {
            UnitCardUI card = Instantiate(unitCardPrefab, root);
            card.Bind(vm);

            card.OnClicked += HandleCardClicked;
            card.OnLongPressed += HandleCardLongPressed;
        }
    }

    private void ClearCardList(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    private async void HandleCardClicked(UnitCardUI card, LobbyUnitViewModel vm)
    {
        if (isChangingFormation || vm == null)
            return;

        // 교체 모드가 아닐 때는 상세 정보 표시
        if (string.IsNullOrEmpty(pendingSwapUnitId) && vm.IsOwned)
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

        isChangingFormation = true;

        try
        {
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

            if (success)
                RefreshView();
        }
        finally
        {
            isChangingFormation = false;
            ClearPendingSwap();
        }
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
        if (isChangingFormation || vm == null)
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
        FormationChangeResult result = await UserDataManager.Instance.UnitFormationUseCase.ExecuteAsync(
            new FormationChangeCommand(
                FormationChangeType.SwapPositions,
                firstUnitId,
                secondUnitId));

        if (!result.Succeeded)
            Debug.LogWarning($"[LobbyUnitPanelView] Swap formation failed: {result.Failure}");

        return result.Succeeded;
    }

    private async Task<bool> ReplaceSelectedUnitAsync(string oldUnitId, string newUnitId)
    {
        FormationChangeResult result = await UserDataManager.Instance.UnitFormationUseCase.ExecuteAsync(
            new FormationChangeCommand(
                FormationChangeType.ReplaceUnit,
                oldUnitId,
                newUnitId));

        if (!result.Succeeded)
            Debug.LogWarning($"[LobbyUnitPanelView] Replace formation failed: {result.Failure}");

        return result.Succeeded;
    }

    private void ClearPendingSwap()
    {
        if (pendingSwapCard != null)
            pendingSwapCard.StopShake();

        pendingSwapCard = null;
        pendingSwapUnitId = null;
    }
}
