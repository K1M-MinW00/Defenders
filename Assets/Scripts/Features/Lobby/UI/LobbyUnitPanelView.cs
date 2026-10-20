using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LobbyUnitPanelView : MonoBehaviour
{
    public event Action<UnitDetailView> UnitDetailOpened;

    [Header("Roots")]
    [SerializeField] private Transform selectedUnitRoot;
    [SerializeField] private Transform ownedUnitRoot;

    [Header("Prefab")]
    [SerializeField] private UnitCardUI unitCardPrefab;
    [SerializeField] private UnitDetailView unitDetailPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text goldText;

    [Header("Responsive Cards")]
    [SerializeField, Min(1)] private int fixedColumns = 5;
    [SerializeField, Range(0.8f, 1f)] private float minCardScale = 0.9f;
    [SerializeField, Range(1f, 1.2f)] private float maxCardScale = 1.08f;

    private Vector2 ownedBaseCellSize;
    private Vector2 ownedBaseSpacing;
    private float selectedBaseSpacing;
    private bool hasCapturedLayout;

    private readonly Dictionary<string, UnitCardUI> cardsByUnitId = new();
    private UserResourceData resource;
    private UserDataManager userDataManager;
    private LobbyUnitPanelPresenter presenter;

    private UnitCardUI pendingSwapCard;
    private string pendingSwapUnitId;
    private bool isChangingFormation;
    private bool isSubscribed;
    private int operationVersion;

    private void Awake()
    {
        if (ValidateReferences())
            return;

        enabled = false;
    }

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        SubscribeEvents();
        RefreshView();
    }

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager)
        {
            UnsubscribeEvents();
            userDataManager = manager;
            resource = null;
            presenter = null;
        }

        if (!isActiveAndEnabled || !TryInitialize())
            return;

        SubscribeEvents();
        RefreshView();
    }

    private void OnDisable()
    {
        operationVersion++;
        ClearPendingSwap();

        UnsubscribeEvents();
    }

    private bool TryInitialize()
    {
        if (userDataManager?.UserData?.Resource == null || userDataManager.RosterService == null)
        {
            Debug.LogError("[LobbyUnitPanelView] User data services are not ready.");
            return false;
        }

        resource = userDataManager.UserData.Resource;
        presenter ??= new LobbyUnitPanelPresenter(userDataManager.RosterService);
        return true;
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
            return;

        userDataManager.OnResourceUpdated += RefreshGold;
        userDataManager.OnRosterUpdated += RefreshView;
        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnResourceUpdated -= RefreshGold;
        userDataManager.OnRosterUpdated -= RefreshView;
        isSubscribed = false;
    }

    private void RefreshGold()
    {
        resource = userDataManager?.UserData?.Resource;
        if (resource != null)
            goldText.text = resource.Gold.ToString("N0");
    }

    private void RefreshView()
    {
        resource = userDataManager?.UserData?.Resource;

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
        HideStaleCards(visibleUnitIds);
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

            card.gameObject.SetActive(true);
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

    private void HideStaleCards(HashSet<string> visibleUnitIds)
    {
        foreach (KeyValuePair<string, UnitCardUI> pair in cardsByUnitId)
        {
            if (!visibleUnitIds.Contains(pair.Key) && pair.Value != null)
                pair.Value.gameObject.SetActive(false);
        }
    }

    private void RestorePendingSwapVisual()
    {
        if (string.IsNullOrWhiteSpace(pendingSwapUnitId))
            return;

        if (!cardsByUnitId.TryGetValue(pendingSwapUnitId, out UnitCardUI card) ||
            card == null || !card.gameObject.activeSelf ||
            card.ViewModel == null || !card.ViewModel.IsSelected)
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
        if (ownedUnitRoot is not RectTransform ownedRect ||
            !ownedUnitRoot.TryGetComponent(out GridLayoutGroup ownedGrid) ||
            ownedRect.rect.width <= 0f)
        {
            return;
        }

        CaptureBaseLayout(ownedGrid);

        float requiredWidth = ownedBaseCellSize.x * fixedColumns +
                              ownedBaseSpacing.x * (fixedColumns - 1) +
                              ownedGrid.padding.horizontal;
        float scale = requiredWidth > 0f
            ? Mathf.Clamp(ownedRect.rect.width / requiredWidth, minCardScale, maxCardScale)
            : 1f;

        ownedGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        ownedGrid.constraintCount = fixedColumns;
        ownedGrid.cellSize = ownedBaseCellSize * scale;
        ownedGrid.spacing = ownedBaseSpacing * scale;

        ResizeSelectedCards(scale);
    }

    private void CaptureBaseLayout(GridLayoutGroup ownedGrid)
    {
        if (hasCapturedLayout)
            return;

        ownedBaseCellSize = ownedGrid.cellSize;
        ownedBaseSpacing = ownedGrid.spacing;

        if (selectedUnitRoot != null &&
            selectedUnitRoot.TryGetComponent(out HorizontalLayoutGroup selectedLayout))
        {
            selectedBaseSpacing = selectedLayout.spacing;
        }

        hasCapturedLayout = true;
    }

    private void ResizeSelectedCards(float scale)
    {
        if (selectedUnitRoot == null)
            return;

        if (selectedUnitRoot.TryGetComponent(out HorizontalLayoutGroup selectedLayout))
            selectedLayout.spacing = selectedBaseSpacing * scale;

        for (int i = 0; i < selectedUnitRoot.childCount; i++)
        {
            if (selectedUnitRoot.GetChild(i) is not RectTransform cardRect)
                continue;

            cardRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                ownedBaseCellSize.x * scale);
            cardRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                ownedBaseCellSize.y * scale);
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
        int currentOperation = ++operationVersion;

        try
        {
            FormationChangeResult result;
            if (vm.IsSelected)
            {
                // 전투 명단 내부 위치 교환
                result = await SwapSelectedUnitPositionAsync(pendingSwapUnitId, vm.UnitId);
            }
            else
            {
                // 전투 명단 유닛 ↔ 대기 명단 유닛 교체
                result = await ReplaceSelectedUnitAsync(pendingSwapUnitId, vm.UnitId);
            }

            if (!CanApplyOperationResult(currentOperation))
                return;

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[LobbyUnitPanelView] Formation change failed: {result.Failure}");
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(result.Failure));
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (CanApplyOperationResult(currentOperation))
                UIFeedbackToast.Show(UnitOperationFeedbackMessages.Get(FormationChangeFailure.SaveFailed));
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

        unitDetailPanel.Open(vm, userDataManager);
        UnitDetailOpened?.Invoke(unitDetailPanel);
    }

    public RectTransform GetFirstOwnedUnitTarget()
    {
        foreach (UnitCardUI card in cardsByUnitId.Values)
        {
            if (card != null && card.gameObject.activeInHierarchy && card.ViewModel?.IsOwned == true)
                return card.transform as RectTransform;
        }

        return null;
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

    private Task<FormationChangeResult> SwapSelectedUnitPositionAsync(string firstUnitId, string secondUnitId)
    {
        return userDataManager.ChangeFormationAsync(
            new FormationChangeCommand(
                FormationChangeType.SwapPositions,
                firstUnitId,
                secondUnitId));
    }

    private Task<FormationChangeResult> ReplaceSelectedUnitAsync(string oldUnitId, string newUnitId)
    {
        return userDataManager.ChangeFormationAsync(
            new FormationChangeCommand(
                FormationChangeType.ReplaceUnit,
                oldUnitId,
                newUnitId));
    }

    private bool CanApplyOperationResult(int version) =>
        isActiveAndEnabled && version == operationVersion;

    private void ClearPendingSwap()
    {
        if (pendingSwapCard != null)
            pendingSwapCard.StopShake();

        pendingSwapCard = null;
        pendingSwapUnitId = null;
    }

    private bool ValidateReferences()
    {
        bool hasRoots = selectedUnitRoot != null && ownedUnitRoot != null;
        bool hasLayouts = hasRoots &&
                          selectedUnitRoot.TryGetComponent(out HorizontalLayoutGroup _) &&
                          ownedUnitRoot.TryGetComponent(out GridLayoutGroup _);
        bool isValid = hasRoots && hasLayouts && unitCardPrefab != null &&
                       unitDetailPanel != null && goldText != null;

        if (!isValid)
            Debug.LogError("[LobbyUnitPanelView] Required roots, layouts, prefab, detail view, or gold text are missing.", this);

        return isValid;
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
