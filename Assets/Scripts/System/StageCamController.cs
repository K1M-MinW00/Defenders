using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Camera))]
public class StageCamController : MonoBehaviour
{
    [Header("Pan")]
    [SerializeField] private float panSpeed = 1.0f;
    [SerializeField] private bool clampPan = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

    [Header("Zoom (Orthographic Size)")]
    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float minOrthoSize = 3f;
    [SerializeField] private float maxOrthoSize = 12f;

    [Header("Input")]
    [SerializeField] private bool blockWhenPointerOverUI = true;

    private Camera cam;

    private Vector2 lastPanScreenPos;
    private bool isPanning;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        var touches = Touch.activeTouches;

        if (blockWhenPointerOverUI && IsTouchOverUI(touches))
        {
            isPanning = false;
            return;
        }

        if (touches.Count == 1)
        {
            HandlePan(touches[0]);
        }
        else
        {
            isPanning = false;
        }

        if (touches.Count == 2)
        {
            HandlePinchZoom(touches[0], touches[1]);
        }
    }

    private void HandlePan(Touch t)
    {
        if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            isPanning = true;
            lastPanScreenPos = t.screenPosition;
            return;
        }

        if (!isPanning) return;

        if (t.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 delta = t.screenPosition - lastPanScreenPos;
            lastPanScreenPos = t.screenPosition;

            // 화면 픽셀 이동을 월드 이동으로 변환 (orthographic 기준)
            Vector3 worldDelta = ScreenDeltaToWorldDelta(delta);
            transform.position -= worldDelta * panSpeed;

            ClampCameraPosition();
        }

        if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
            t.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            isPanning = false;
        }
    }

    private void HandlePinchZoom(Touch t0, Touch t1)
    {
        // 이전 프레임 거리 / 현재 프레임 거리 비교
        Vector2 t0Prev = t0.screenPosition - t0.delta;
        Vector2 t1Prev = t1.screenPosition - t1.delta;

        float prevDist = Vector2.Distance(t0Prev, t1Prev);
        float currDist = Vector2.Distance(t0.screenPosition, t1.screenPosition);

        float diff = currDist - prevDist;

        // orthographicSize는 작아질수록 줌인
        cam.orthographicSize -= diff * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);

        ClampCameraPosition();
    }

    private Vector3 ScreenDeltaToWorldDelta(Vector2 screenDelta)
    {
        // orthographic 카메라에서 화면 픽셀 이동을 월드 이동으로 변환하는 방법:
        // (screenDelta / Screen.height) * (2 * orthographicSize) = 월드 Y 이동량
        float worldPerPixel = (2f * cam.orthographicSize) / Screen.height;
        return new Vector3(screenDelta.x * worldPerPixel, screenDelta.y * worldPerPixel, 0f);
    }

    private void ClampCameraPosition()
    {
        if (!clampPan) return;

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, minBounds.x, maxBounds.x);
        p.y = Mathf.Clamp(p.y, minBounds.y, maxBounds.y);
        transform.position = p;
    }

    private bool IsTouchOverUI(System.Collections.Generic.IReadOnlyList<Touch> touches)
    {
        if (!blockWhenPointerOverUI) return false;
        if (EventSystem.current == null) return false;

        // 하나라도 UI 위면 카메라 입력 차단
        for (int i = 0; i < touches.Count; i++)
        {
            int fingerId = touches[i].touchId; // EnhancedTouch의 touchId가 pointerId로 매칭
            if (EventSystem.current.IsPointerOverGameObject(fingerId))
                return true;
        }
        return false;
    }
}