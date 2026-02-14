using UnityEngine;

public class PlacementArea : MonoBehaviour
{
    [SerializeField] private Collider2D areaCollider; // PolygonCollider2D 권장
    [SerializeField] private LayerMask blockedLayer;  // 배치 불가(장애물/길 등) 레이어 (선택)
    [SerializeField] private float unitRadius = 0.35f; // 유닛 겹침 방지용 (선택)

    private void Awake()
    {
        if (areaCollider == null)
            areaCollider = GetComponent<Collider2D>();
    }

    public bool IsInside(Vector2 worldPos)
    {
        return areaCollider != null && areaCollider.OverlapPoint(worldPos);
    }

    public bool CanPlace(Vector2 worldPos)
    {
        if (!IsInside(worldPos))
            return false;

        // (선택) 장애물 위 배치 금지
        if (blockedLayer.value != 0)
        {
            var hit = Physics2D.OverlapCircle(worldPos, unitRadius, blockedLayer);
            if (hit != null) return false;
        }

        // (선택) 유닛 간 겹침 금지(유닛 레이어로 체크)
        // var unitHit = Physics2D.OverlapCircle(worldPos, unitRadius, unitLayer);

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 디버그용 표시 정도만
    }
#endif
}
