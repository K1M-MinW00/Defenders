using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private UIDropRouter uiDropRouter;
    [SerializeField] private StageUIController stageUIController;
    [SerializeField] private TilemapPlacementArea placementArea;

    [Header("Section")]
    [SerializeField] private LayerMask unitLayer;

    [Header("Long Press")]
    [SerializeField, Min(0.1f)] private float longPressDuration = 0.45f;

    private bool placementEnabled;
    public bool IsInputEnabled => placementEnabled;
    public UnitController DraggingUnit { get; private set; }
    private Vector3 originalPos;
    private int activeFingerId = -1;
    private UnitController pressedUnit;
    private float pressStartedAt;
    private bool isInfoPanelVisible;

    public event Action<UnitController> OnSellRequested;
    public event Action<UnitController> OnRerollRequested;

    private void Awake()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    public void Initialize(TilemapPlacementArea placementArea)
    {
        ClearStageContext();
        this.placementArea = placementArea;

        if (this.placementArea != null)
            this.placementArea.SetVisible(false);
    }

    public void ClearStageContext()
    {
        placementEnabled = false;

        if (pressedUnit != null)
            CancelInteraction();

        if (placementArea != null)
            placementArea.SetVisible(false);

        placementArea = null;
    }


    public bool SetInputEnabled(bool enable)
    {
        if (enable && placementArea == null)
        {
            Debug.LogError($"{nameof(PlacementController)} is not initialized.");
            return false;
        }

        placementEnabled = enable;
        placementArea?.SetVisible(enable);

        // 전투 시작 시 드래그 중이던 게 있으면 정리
        if (!placementEnabled && pressedUnit != null)
            CancelInteraction();

        return true;
    }

    private void OnDisable()
    {
        if (pressedUnit != null)
            CancelInteraction();

        SetInputEnabled(false);
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            TryBeginInteraction(Input.mousePosition, IsPointerOverUI());

        if (pressedUnit != null && Input.GetMouseButton(0))
            ContinueInteraction(Input.mousePosition);

        if (pressedUnit != null && Input.GetMouseButtonUp(0))
            EndInteraction(Input.mousePosition);
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount != 1)
        {
            if (pressedUnit != null)
                CancelInteraction();
            return;
        }

        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                TryBeginInteraction(touch.position, IsPointerOverUI(touch.fingerId));
                if (pressedUnit != null)
                    activeFingerId = touch.fingerId;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (pressedUnit != null && activeFingerId == touch.fingerId)
                    ContinueInteraction(touch.position);
                break;

            case TouchPhase.Ended:
                if (pressedUnit != null && activeFingerId == touch.fingerId)
                    EndInteraction(touch.position);
                activeFingerId = -1;
                break;

            case TouchPhase.Canceled:
                if (pressedUnit != null && activeFingerId == touch.fingerId)
                    CancelInteraction();
                activeFingerId = -1;
                break;
        }
    }

    private void TryBeginInteraction(Vector2 screenPosition, bool isPointerOverUI)
    {
        if (isPointerOverUI || mainCam == null)
            return;

        Vector2 world = GetWorldPosition(screenPosition);

        // 유닛만 Raycast로 선택
        var hit = Physics2D.OverlapPoint(world, unitLayer);
        if (hit == null)
            return;

        var unit = hit.GetComponent<UnitController>();
        if (unit == null || unit.IsDead)
            return;

        bool canInspect = unit.RuntimeState == UnitRuntimeState.Combat ||
                          (placementEnabled && unit.RuntimeState == UnitRuntimeState.Preparing);
        if (!canInspect)
            return;

        pressedUnit = unit;
        pressStartedAt = Time.unscaledTime;
        isInfoPanelVisible = false;

        if (!placementEnabled || unit.RuntimeState != UnitRuntimeState.Preparing)
            return;

        DraggingUnit = unit;
        originalPos = unit.transform.position;
        
        DraggingUnit.Movement.Stop();
        DraggingUnit.ShowRange();

        int star = unit.Star;
        bool canReroll = (star == 1);

        stageUIController.SetUnitDragMode(true, canReroll,star);
    }

    private void ContinueInteraction(Vector2 screenPosition)
    {
        if (pressedUnit == null)
            return;

        if (DraggingUnit != null)
            Dragging(screenPosition);

        if (!isInfoPanelVisible && Time.unscaledTime - pressStartedAt >= longPressDuration)
        {
            isInfoPanelVisible = true;
            stageUIController?.ShowUnitInfo(pressedUnit);
        }
    }

    private void EndInteraction(Vector2 screenPosition)
    {
        CloseInfoPanel();

        if (DraggingUnit != null)
            EndDrag(screenPosition);
        else
            ClearPressedUnit();
    }

    private void Dragging(Vector2 screenPosition)
    {
        if (mainCam == null)
            return;

        Vector2 world = GetWorldPosition(screenPosition);
        DraggingUnit.transform.position = new Vector3(world.x, world.y, DraggingUnit.transform.position.z);
    }

    private void EndDrag(Vector2 screenPos)
    {
        if (uiDropRouter != null && uiDropRouter.TryGetDropAction(screenPos, out var action))
        {
            HandleDropAction(action);
            FinishDrag();
            return;
        }

        Vector2 pos = DraggingUnit.transform.position;
        bool ok = placementArea != null && placementArea.CanPlace(pos);

        if (!ok)
            DraggingUnit.transform.position = originalPos;

        FinishDrag();
    }

    private void HandleDropAction(UnitDropAction action)
    {
        if (!placementEnabled || DraggingUnit == null)
            return;

        switch (action)
        {
            case UnitDropAction.Sell:
                OnSellRequested?.Invoke(DraggingUnit);
                break;

            case UnitDropAction.Reroll:
                OnRerollRequested?.Invoke(DraggingUnit);
                break;
        }
    }

    private void CancelDrag()
    {
        if(DraggingUnit != null)
            DraggingUnit.transform.position = originalPos;

        FinishDrag();
    }

    private void CancelInteraction()
    {
        CloseInfoPanel();

        if (DraggingUnit != null)
            CancelDrag();
        else
            ClearPressedUnit();
    }

    public void RestoreDraggingUnitPosition()
    {
        if (DraggingUnit != null)
            DraggingUnit.transform.position = originalPos;
    }

    private void FinishDrag()
    {
        UnitController finishedUnit = DraggingUnit;
        DraggingUnit = null;
        activeFingerId = -1;

        if (finishedUnit != null &&
            finishedUnit.gameObject.activeInHierarchy &&
            finishedUnit.RuntimeState != UnitRuntimeState.Removing)
        {
            finishedUnit.Movement.Resume();
            finishedUnit.HideRange();
        }

        stageUIController?.SetUnitDragMode(false);
        ClearPressedUnit();
    }

    private void CloseInfoPanel()
    {
        if (!isInfoPanelVisible)
            return;

        isInfoPanelVisible = false;
        stageUIController?.HideUnitInfo();
    }

    private void ClearPressedUnit()
    {
        pressedUnit = null;
        pressStartedAt = 0f;
        activeFingerId = -1;
    }

    private Vector2 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 w = mainCam.ScreenToWorldPoint(screenPosition);
        return new Vector2(w.x, w.y);
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private static bool IsPointerOverUI(int fingerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
    }
}
