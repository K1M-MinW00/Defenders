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

    private bool placementEnabled;
    public bool IsInputEnabled => placementEnabled;
    public UnitController DraggingUnit { get; private set; }
    private Vector3 originalPos;
    private int activeFingerId = -1;

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

        if (DraggingUnit != null)
            CancelDrag();

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
        if (!placementEnabled && DraggingUnit != null)
        {
            CancelDrag();
        }

        return true;
    }

    private void OnDisable()
    {
        SetInputEnabled(false);
    }

    private void Update()
    {
        if (!placementEnabled)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            TryBeginDrag(Input.mousePosition, IsPointerOverUI());

        if (DraggingUnit != null && Input.GetMouseButton(0))
            Dragging(Input.mousePosition);

        if (DraggingUnit != null && Input.GetMouseButtonUp(0))
            EndDrag(Input.mousePosition);
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount != 1)
        {
            if (DraggingUnit != null)
                CancelDrag();
            return;
        }

        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                TryBeginDrag(touch.position, IsPointerOverUI(touch.fingerId));
                if (DraggingUnit != null)
                    activeFingerId = touch.fingerId;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (DraggingUnit != null && activeFingerId == touch.fingerId)
                    Dragging(touch.position);
                break;

            case TouchPhase.Ended:
                if (DraggingUnit != null && activeFingerId == touch.fingerId)
                    EndDrag(touch.position);
                activeFingerId = -1;
                break;

            case TouchPhase.Canceled:
                if (DraggingUnit != null && activeFingerId == touch.fingerId)
                    CancelDrag();
                activeFingerId = -1;
                break;
        }
    }

    private void TryBeginDrag(Vector2 screenPosition, bool isPointerOverUI)
    {
        if (isPointerOverUI || mainCam == null)
            return;

        Vector2 world = GetWorldPosition(screenPosition);

        // 유닛만 Raycast로 선택
        var hit = Physics2D.OverlapPoint(world, unitLayer);
        if (hit == null)
            return;

        var unit = hit.GetComponent<UnitController>();
        if (unit == null || unit.IsDead || unit.RuntimeState != UnitRuntimeState.Preparing)
            return;

        DraggingUnit = unit;
        originalPos = unit.transform.position;
        
        DraggingUnit.Movement.Stop();
        DraggingUnit.ShowRange();

        int star = unit.Star;
        bool canReroll = (star == 1);

        stageUIController.SetUnitDragMode(true, canReroll,star);
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
