using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlacementController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private TilemapPlacementArea placementArea;
    [SerializeField] private UIDropRouter uiDropRouter;
    [SerializeField] private StageUIController stageUIController;

    [Header("Section")]
    [SerializeField] private LayerMask unitLayer; // PlayerUnit 레이어만 포함

    private bool placementEnabled;
    public PlayerCharacter DraggingUnit { get;private set; }
    private Vector3 originalPos;


    private void Awake()
    {
        if (mainCam == null) 
            mainCam = Camera.main;
    }

    public void EnablePlacement(bool enable)
    {
        placementEnabled = enable;

        // 전투 시작 시 드래그 중이던 게 있으면 정리
        if (!placementEnabled && DraggingUnit != null)
        {
            CancelDrag();
        }
    }

    private void Update()
    {
        if (!placementEnabled) return;

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

        var unit = hit.GetComponent<PlayerCharacter>();
        if (unit == null)
            return;

        DraggingUnit = unit;
        originalPos = unit.transform.position;

        // 드래그 중 이동/AI 멈춤

        stageUIController?.SetUnitDragMode(true);
    }

    private void Dragging()
    {
        Vector2 world = GetMouseWorld2D();

        // (선택) 영역 밖이면 드래그는 하되, 드랍만 실패 처리할 수도 있고
        // 여기서 Clamp 처리를 할 수도 있습니다.
        DraggingUnit.transform.position = new Vector3(world.x, world.y, DraggingUnit.transform.position.z);
    }

    private void EndDrag(Vector2 screenPos)
    {
        if(uiDropRouter != null && uiDropRouter.TryGetDropAction(screenPos,out var action))
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
        switch (action)
        {
            case UnitDropAction.Sell:
                StageManager.Instance?.TrySellUnit(DraggingUnit);
                break;
            case UnitDropAction.Reroll:
                StageManager.Instance?.TryRerollUnit(DraggingUnit);
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
        DraggingUnit = null;
        stageUIController?.SetUnitDragMode(false);
    }

    private Vector2 GetMouseWorld2D()
    {
        Vector3 w = mainCam.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(w.x, w.y);
    }
}
