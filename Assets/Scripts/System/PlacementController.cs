using UnityEngine;
using UnityEngine.AI;

public class PlacementController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private PlacementArea placementArea;
    [SerializeField] private LayerMask unitLayer; // PlayerUnit 레이어만 포함
    [SerializeField] private float raycastMaxDistance = 50f;

    private bool placementEnabled;

    private PlayerCharacter draggingUnit;
    private Vector3 originalPos;
    private NavMeshAgent cachedAgent;
    private bool wasStopped;

    private void Awake()
    {
        if (mainCam == null) 
            mainCam = Camera.main;
    }

    public void EnablePlacement(bool enable)
    {
        placementEnabled = enable;

        // 전투 시작 시 드래그 중이던 게 있으면 정리
        if (!placementEnabled && draggingUnit != null)
        {
            CancelDrag();
        }
    }

    private void Update()
    {
        if (!placementEnabled) return;

        if (Input.GetMouseButtonDown(0))
            TryBeginDrag();

        if (draggingUnit != null && Input.GetMouseButton(0))
            Dragging();

        if (draggingUnit != null && Input.GetMouseButtonUp(0))
            EndDrag();
    }

    private void TryBeginDrag()
    {
        Vector2 world = GetMouseWorld2D();

        // 유닛만 Raycast로 선택
        var hit = Physics2D.Raycast(world, Vector2.zero, raycastMaxDistance, unitLayer);
        if (hit.collider == null)
            return;

        var unit = hit.collider.GetComponent<PlayerCharacter>();
        if (unit == null)
            return;

        draggingUnit = unit;
        originalPos = unit.transform.position;

        // 드래그 중 이동/AI 멈춤
        cachedAgent = unit.GetComponent<NavMeshAgent>();
        if (cachedAgent != null)
        {
            wasStopped = cachedAgent.isStopped;
            cachedAgent.isStopped = true;
        }
    }

    private void Dragging()
    {
        Vector2 world = GetMouseWorld2D();

        // (선택) 영역 밖이면 드래그는 하되, 드랍만 실패 처리할 수도 있고
        // 여기서 Clamp 처리를 할 수도 있습니다.
        draggingUnit.transform.position = new Vector3(world.x, world.y, draggingUnit.transform.position.z);
    }

    private void EndDrag()
    {
        Vector2 pos = draggingUnit.transform.position;

        bool ok = placementArea != null && placementArea.CanPlace(pos);

        if (!ok)
        {
            draggingUnit.transform.position = originalPos;
        }

        // agent 복구
        if (cachedAgent != null)
            cachedAgent.isStopped = wasStopped;

        draggingUnit = null;
        cachedAgent = null;
    }

    private void CancelDrag()
    {
        if (draggingUnit == null) return;

        draggingUnit.transform.position = originalPos;

        if (cachedAgent != null)
            cachedAgent.isStopped = wasStopped;

        draggingUnit = null;
        cachedAgent = null;
    }

    private Vector2 GetMouseWorld2D()
    {
        Vector3 w = mainCam.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(w.x, w.y);
    }
}
