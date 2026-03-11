using UnityEngine;

public class FusionService : MonoBehaviour
{
    [SerializeField] private UnitRoster roster;
    [SerializeField] private int maxStar = 4;

    private void Awake()
    {
        roster = GetComponent<UnitRoster>();
    }

    // 스폰/리롤 직후 호출
    // changedUnit: 방금 생긴(또는 교체된) 유닛
    public void TryAutoFuse(UnitInstance changedUnit)
    {
        if (changedUnit == null) return;
        if (changedUnit.Data == null) return;

        // 준비 단계에서만 합성되도록 게이트
        if (StageManager.Instance != null && StageManager.Instance.CurrentState != StageState.Preparing)
            return;

        roster.CleanupNulls();

        // 연쇄 합성 처리
        // 정책: "기존 유닛을 남기고", changedUnit은 소모될 수 있음
        UnitInstance seed = changedUnit;

        while (seed != null && seed.Data != null)
        {
            int star = seed.Star;
            if (star >= maxStar)
                break;

            UnitCode unitCode = seed.Data.UnitCode;

            // 같은 (unitId, star)인 "다른 유닛" 찾기
            UnitInstance other = roster.FindAny(unitCode, star, exclude: seed);
            if (other == null)
                break;

            // 남길 유닛 결정:
            // - 연출 고려: 기존(other)을 남기고 seed(새로 뽑은 유닛)를 소모
            UnitInstance keep = other;
            UnitInstance consume = seed;

            // 만약 seed가 기존이고 other가 새로 뽑힌 쪽이 되도록 바꾸고 싶으면 정책 변경 가능
            // (현재는 seed=새 유닛으로 들어온다는 가정)

            // 승급
            keep.ApplyStarUp();

            // 소모 유닛 제거
            roster.Unregister(consume);
            Destroy(consume.gameObject);

            // 다음 연쇄 합성의 seed는 "승급된 keep"
            seed = keep;
        }
    }
}
