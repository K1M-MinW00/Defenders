//using UnityEngine;

//public class UnitDragHandler : MonoBehaviour
//{
//    [SerializeField] private Camera mainCam;
//    [SerializeField] private PlacementArea placementArea;
//    [SerializeField] private StageSessionController stageSession;

//    private Vector3 originalPos;
//    private bool dragging;

//    private void Awake()
//    {
//        if (mainCam == null) mainCam = Camera.main;
//    }

//    public void Bind(PlacementArea area)
//    {
//        placementArea = area;
//    }

//    private void OnMouseDown()
//    {
//        if (!CanDragNow()) return;
//        dragging = true;
//        originalPos = transform.position;
//    }

//    private void OnMouseDrag()
//    {
//        if (!dragging) return;

//        Vector3 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
//        world.z = transform.position.z;
//        transform.position = world;
//    }

//    private void OnMouseUp()
//    {
//        if (!dragging) return;
//        dragging = false;

//        Vector2 pos2D = transform.position;

//        if (placementArea != null && placementArea.CanPlace(pos2D))
//        {
//            // 확정
//            transform.position = new Vector3(pos2D.x, pos2D.y, transform.position.z);
//        }
//        else
//        {
//            // 실패: 원위치
//            transform.position = originalPos;
//        }
//    }

//    private bool CanDragNow()
//    {
//        if (stageSession == null)
//            return false;

//        return stageSession.CurrentState == StageState.Preparing;
//    }
//}
