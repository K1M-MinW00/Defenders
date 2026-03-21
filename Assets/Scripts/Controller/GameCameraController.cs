using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class GameCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private StageSessionController stageSession;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeedMouse = 5f;
    [SerializeField] private float zoomSpeedPinch = 0.01f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;

    [Header("Pan")]
    [SerializeField] private float panSmoothness = 1f;
    [SerializeField] private LayerMask dragBlockLayer; // 유닛이 있는 레이어
    [SerializeField] private bool allowPanInCombat = true;

    [Header("Move Bounds")]
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 5f);

    [Header("Default Prepare Camera")]
    [SerializeField] private Vector3 defaultPosition = new Vector3(0f, 0f, -10f);
    [SerializeField] private float defaultZoom = 5f;

    private bool isDragging;
    private Vector3 lastPointerWorldPos;
    private int activeFingerId = -1;

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (!cam.orthographic)
            Debug.LogWarning($"{name}: GameCameraController 는 Orthographic Camera 기준으로 작성되었습니다.");
    }

    private void Start()
    {
        ResetCameraToDefaultImmediate();
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
        ClampCameraPosition();
    }

    public void ResetCameraToDefaultImmediate()
    {
        transform.position = defaultPosition;
        cam.orthographicSize = Mathf.Clamp(defaultZoom, minZoom, maxZoom);
        ClampCameraPosition();
    }

    public void ResetCameraToDefault(Vector3 position, float zoom)
    {
        defaultPosition = position;
        defaultZoom = zoom;
        ResetCameraToDefaultImmediate();
    }

    private void HandleZoom()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseZoom();
#else
        HandleTouchZoom();
#endif
    }

    private void HandlePan()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMousePan();
#else
        HandleTouchPan();
#endif
    }

    private void HandleMouseZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        float nextZoom = cam.orthographicSize - scroll * zoomSpeedMouse * Time.unscaledDeltaTime * 60f;
        cam.orthographicSize = Mathf.Clamp(nextZoom, minZoom, maxZoom);
        ClampCameraPosition();
    }

    private void HandleTouchZoom()
    {
        if (Input.touchCount != 2)
            return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
        Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

        float prevDistance = Vector2.Distance(prevPos0, prevPos1);
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);

        float delta = currentDistance - prevDistance;
        if (Mathf.Abs(delta) < 0.01f)
            return;

        float nextZoom = cam.orthographicSize - delta * zoomSpeedPinch;
        cam.orthographicSize = Mathf.Clamp(nextZoom, minZoom, maxZoom);
        ClampCameraPosition();

        // 핀치 중에는 드래그 해제
        isDragging = false;
        activeFingerId = -1;
    }

    private void HandleMousePan()
    {
        if (!CanPanNow())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
                return;

            if (IsPointerOverBlockingObject(Input.mousePosition))
                return;

            isDragging = true;
            lastPointerWorldPos = GetPointerWorldPosition(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentWorldPos = GetPointerWorldPosition(Input.mousePosition);
            Vector3 delta = lastPointerWorldPos - currentWorldPos;

            MoveCamera(delta);
            lastPointerWorldPos = GetPointerWorldPosition(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void HandleTouchPan()
    {
        if (!CanPanNow())
            return;

        if (Input.touchCount != 1)
        {
            isDragging = false;
            activeFingerId = -1;
            return;
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                {
                    if (IsPointerOverUI(touch.fingerId))
                        return;

                    if (IsPointerOverBlockingObject(touch.position))
                        return;

                    isDragging = true;
                    activeFingerId = touch.fingerId;
                    lastPointerWorldPos = GetPointerWorldPosition(touch.position);
                    break;
                }

            case TouchPhase.Moved:
                {
                    if (!isDragging || activeFingerId != touch.fingerId)
                        return;

                    Vector3 currentWorldPos = GetPointerWorldPosition(touch.position);
                    Vector3 delta = lastPointerWorldPos - currentWorldPos;

                    MoveCamera(delta);
                    lastPointerWorldPos = GetPointerWorldPosition(touch.position);
                    break;
                }

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                {
                    if (activeFingerId == touch.fingerId)
                    {
                        isDragging = false;
                        activeFingerId = -1;
                    }
                    break;
                }
        }
    }

    private void MoveCamera(Vector3 delta)
    {
        Vector3 nextPos = transform.position + delta * panSmoothness;
        transform.position = nextPos;
        ClampCameraPosition();
    }

    private void ClampCameraPosition()
    {
        if (!cam.orthographic)
            return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = minBounds.x + halfWidth;
        float maxX = maxBounds.x - halfWidth;
        float minY = minBounds.y + halfHeight;
        float maxY = maxBounds.y - halfHeight;

        Vector3 pos = transform.position;

        // 카메라 뷰가 bounds보다 더 큰 경우 중앙 고정
        if (minX > maxX)
            pos.x = (minBounds.x + maxBounds.x) * 0.5f;
        else
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (minY > maxY)
            pos.y = (minBounds.y + maxBounds.y) * 0.5f;
        else
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    private Vector3 GetPointerWorldPosition(Vector3 screenPos)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return worldPos;
    }

    private bool IsPointerOverBlockingObject(Vector2 screenPos)
    {
        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        Vector2 point = new Vector2(world.x, world.y);

        Collider2D hit = Physics2D.OverlapPoint(point, dragBlockLayer);
        return hit != null;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerOverUI(int fingerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    private bool CanPanNow()
    {
        if (allowPanInCombat)
            return true;

        if (stageSession == null)
            return true;

        return stageSession.CurrentState == StageState.Preparing;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}