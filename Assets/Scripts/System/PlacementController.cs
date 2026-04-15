using System;
using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private TilemapPlacementArea placementArea;
    [SerializeField] private UIDropRouter uiDropRouter;
    [SerializeField] private StageUIController stageUIController;

    [Header("Section")]
    [SerializeField] private LayerMask unitLayer;

    private bool placementEnabled;
    public UnitController DraggingUnit { get; private set; }
    private Vector3 originalPos;

    public event Action<UnitController> OnSellRequested;
    public event Action<UnitController> OnRerollRequested;

    private void Awake()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    public void EnablePlacement(bool enable)
    {
        placementEnabled = enable;
        placementArea.SetVisible(enable);

        // 전투 시작 시 드래그 중이던 게 있으면 정리
        if (!placementEnabled && DraggingUnit != null)
        {
            CancelDrag();
        }
    }

    private void Update()
    {
        if (!placementEnabled)
            return;

        if (Input.GetMouseButtonDown(0))
            TryBeginDrag();

        if (DraggingUnit != null && Input.GetMouseButton(0))
            Dragging();

        if (DraggingUnit != null && Input.GetMouseButtonUp(0))
            EndDrag(Input.mousePosition);
    }

    private void TryBeginDrag()
    {
        Vector2 world = GetMouseWorld2D();

        // 유닛만 Raycast로 선택
        var hit = Physics2D.OverlapPoint(world, unitLayer);
        if (hit == null)
            return;

        var unit = hit.GetComponent<UnitController>();
        if (unit == null)
            return;

        DraggingUnit = unit;
        DraggingUnit.Movement.Stop();
        DraggingUnit.ShowRange();
        originalPos = unit.transform.position;

        int star = unit.Star;

        bool canReroll = (star == 1);
        stageUIController.SetUnitDragMode(true, canReroll);
    }

    private void Dragging()
    {
        Vector2 world = GetMouseWorld2D();

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
        if (DraggingUnit == null)
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
        DraggingUnit.transform.position = originalPos;
        FinishDrag();
    }

    private void FinishDrag()
    {
        DraggingUnit.Movement.Resume();
        DraggingUnit.HideRange();
        DraggingUnit = null;

        stageUIController.SetUnitDragMode(false);
    }

    private Vector2 GetMouseWorld2D()
    {
        Vector3 w = mainCam.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(w.x, w.y);
    }
}
