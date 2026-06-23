using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPlacementArea : MonoBehaviour
{
    [Header("Tilemap Area")]
    [SerializeField] private Tilemap placementTilemap;          // 표시용(흰색)
    [SerializeField] private TilemapRenderer tilemapRenderer;          // 표시용(흰색)
    [SerializeField] private CompositeCollider2D areaCollider;    // 영역 판정용(필수)

    [Header("Blocked (Optional)")]
    [SerializeField] private LayerMask blockedLayer;            // 장애물/길 레이어
    [SerializeField] private float unitRadius = 0.35f;          // 유닛 겹침/장애물 체크 반경
    [SerializeField] private LayerMask unitLayer;               // 유닛 레이어(겹침 방지)

    private void Awake()
    {
        if (placementTilemap == null)
            placementTilemap = GetComponent<Tilemap>();

        if(tilemapRenderer == null)
            tilemapRenderer = GetComponent<TilemapRenderer>();

        if (areaCollider == null)
            areaCollider = GetComponent<CompositeCollider2D>();
    }


    public void SetVisible(bool visible)
    {
        if (tilemapRenderer != null)
            tilemapRenderer.enabled = visible;
    }

    // “영역 안인지” 판정: 콜라이더 기반 (연속 좌표 OK)
    public bool IsInside(Vector2 worldPos)
    {
        return areaCollider != null && areaCollider.OverlapPoint(worldPos);
    }

    public bool CanPlace(Vector2 worldPos, Collider2D ignoreUnitCollider = null)
    {
        if (!IsInside(worldPos))
        {
            Debug.Log("it's not Inside");
            return false;
        }

        // (선택) 장애물 위 배치 금지
        if (blockedLayer.value != 0)
        {
            Debug.Log("it is above the obstacle");
            var hit = Physics2D.OverlapCircle(worldPos, unitRadius, blockedLayer);
            if (hit != null) return false;
        }

        return true;
    }
}