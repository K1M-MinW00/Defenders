using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Responsive Grid")]
    [SerializeField, Min(1)] private int maxColumns = 5;

    private readonly Dictionary<string, UnitCardUI> cardsByUnitId = new();
    private readonly List<string> staleCardIds = new();

    private UserResourceData resource;
    private LobbyUnitPanelPresenter presenter;

    private UnitCardUI pendingSwapCard;
    private string pendingSwapUnitId;
    private bool isChangingFormation;
    private bool isSubscribed;

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        SubscribeEvents();
        RefreshView();
    }

    private void OnDisable()
    {
        ClearPendingSwap();

        UnsubscribeEvents();
    }

    private bool TryInitialize()
    {
        UserDataManager manager = UserDataManager.Instance;
        if (manager?.UserData?.Resource == null || manager.RosterService == null)
        {
            Debug.LogError("[LobbyUnitPanelView] User data services are not ready.");
            return false;
        }

        resource = manager.UserData.Resource;
        presenter ??= new LobbyUnitPanelPresenter(manager.RosterService);
        return true;
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
            return;

        UserDataManager.Instance.OnResourceUpdated += RefreshGold;
        UserDataManager.Instance.OnRosterUpdated += RefreshView;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed || UserDataManager.Instance == null)
            return;

        UserDataManager.Instance.OnResourceUpdated -= RefreshGold;
        UserDataManager.Instance.OnRosterUpdated -= RefreshView;
        isSubscribed = false;
    }

    private void RefreshGold()
    {
        goldText.text = resource.Gold.ToString("N0");
    }

    private void RefreshView()
    {
        resource = UserDataManager.Instance.UserData.Resource;

        if (resource == null || presenter == null)
            return;

        RefreshGold();

        LobbyUnitPanelViewState state = presenter.Build();
        ReconcileCardLists(state.SelectedUnits, state.AvailableUnits);
        RefreshGridLayouts();
    }

    private void ReconcileCardLists(
        IReadOnlyList<LobbyUnitViewModel> selectedUnits,
        IReadOnlyList<LobbyUnitViewModel> availableUnits)
    {
        if (unitCardPrefab == null)
            return;

        HashSet<string> visibleUnitIds = new();

        ReconcileCardList(selectedUnitRoot, selectedUnits, visibleUnitIds);
        ReconcileCardList(ownedUnitRoot, availableUnits, visibleUnitIds);
        RemoveStaleCards(visibleUnitIds);
        RestorePendingSwapVisual();
    }

    private void ReconcileCardList(
        Transform root,
        IReadOnlyList<LobbyUnitViewModel> viewModels,
        HashSet<string> visibleUnitIds)
    {
        if (root == null || viewModels == null)
            return;

        for (int i = 0; i < viewModels.Count; i++)
        {
            LobbyUnitViewModel vm = viewModels[i];
            if (vm == null || string.IsNullOrWhiteSpace(vm.UnitId) || !visibleUnitIds.Add(vm.UnitId))
                continue;

            UnitCardUI card = GetOrCreateCard(vm.UnitId, root);

            if (card.transform.parent != root)
                card.transform.SetParent(root, false);

            card.transform.SetSiblingIndex(i);
            card.Bind(vm);
        }
    }

    private UnitCardUI GetOrCreateCard(string unitId, Transform root)
    {
        if (cardsByUnitId.TryGetValue(unitId, out UnitCardUI card) && card != null)
            return card;

        card = Instantiate(unitCardPrefab, root);
        card.OnClicked += HandleCardClicked;
        card.OnLongPressed += HandleCardLongPressed;
        cardsByUnitId[unitId] = card;

        return card;
    }

    private void RemoveStaleCards(HashSet<string> visibleUnitIds)
    {
        staleCardIds.Clear();

        foreach (KeyValuePair<string, UnitCardUI> pair in cardsByUnitId)
        {
            if (!visibleUnitIds.Contains(pair.Key))
                staleCardIds.Add(pair.Key);
        }

        foreach (string unitId in staleCardIds)
        {
            UnitCardUI card = cardsByUnitId[unitId];

            if (card != null)
            {
                card.OnClicked -= HandleCardClicked;
                card.OnLongPressed -= HandleCardLongPressed;
                card.gameObject.SetActive(false);
                Destroy(card.gameObject);
            }

            cardsByUnitId.Remove(unitId);
        }

        staleCardIds.Clear();
    }

    private void RestorePendingSwapVisual()
    {
        if (string.IsNullOrWhiteSpace(pendingSwapUnitId))
            return;

        if (!cardsByUnitId.TryGetValue(pendingSwapUnitId, out UnitCardUI card) ||
            card == null || card.ViewModel == null || !card.ViewModel.IsSelected)
        {
            ClearPendingSwap();
            return;
        }

        pendingSwapCard = card;
        pendingSwapCard.StartShake();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
            RefreshGridLayouts();
    }

    private void RefreshGridLayouts()
    {
        UpdateGridColumns(selectedUnitRoot);
        UpdateGridColumns(ownedUnitRoot);
    }

    private void UpdateGridColumns(Transform root)
    {
        if (root is not RectTransform rectTransform ||
            !root.TryGetComponent(out GridLayoutGroup grid) ||
            rectTransform.rect.width <= 0f)
        {
            return;
        }

        float availableWidth = rectTransform.rect.width - grid.padding.horizontal;
        float itemWidth = grid.cellSize.x + grid.spacing.x;

        if (availableWidth <= 0f || itemWidth <= 0f)
            return;

        int columns = Mathf.FloorToInt((availableWidth + grid.spacing.x) / itemWidth);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Clamp(columns, 1, maxColumns);
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
        {
            Debug.LogWarning($"[LobbyUnitPanelView] Swap formation failed: {result.Failure}");
            UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(result.Failure));
        }

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
        {
            Debug.LogWarning($"[LobbyUnitPanelView] Replace formation failed: {result.Failure}");
            UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(result.Failure));
        }

        return result.Succeeded;
    }

    private void ClearPendingSwap()
    {
        if (pendingSwapCard != null)
            pendingSwapCard.StopShake();

        pendingSwapCard = null;
        pendingSwapUnitId = null;
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();

        foreach (UnitCardUI card in cardsByUnitId.Values)
        {
            if (card == null)
                continue;

            card.OnClicked -= HandleCardClicked;
            card.OnLongPressed -= HandleCardLongPressed;
        }

        cardsByUnitId.Clear();
    }
}
