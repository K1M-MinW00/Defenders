using UnityEngine;

public class UnitSummoner : MonoBehaviour
{
    [Header("Unit Pool (Inspector)")]
    [SerializeField] private UnitData[] unitPool;

    [Header("Spawn Settings")]
    [SerializeField] private Transform unitsRoot;          // 생성된 유닛을 담을 부모(없으면 null 가능)
    [SerializeField] private Transform spawnPoint;         // 우선 스폰 위치(없으면 PlacementArea 중심)

    [Header("Fusion")]
    [SerializeField] private UnitRoster roster;
    [SerializeField] private FusionService fusionService;

    public void SummonRandomUnit()
    {
        // 1) 풀 체크
        if (unitPool == null || unitPool.Length == 0)
        {
            Debug.LogWarning("Summon blocked: unitPool is empty.");
            return;
        }

        // 2) 랜덤 UnitData 선택
        UnitData data = unitPool[Random.Range(0, unitPool.Length)];
        if (data == null || data.unitPrefab == null)
        {
            Debug.LogError("Summon failed: UnitData or unitPrefab is null.");
            return;
        }

        // 3) 스폰 위치 결정
        Vector3 pos = ResolveSpawnPosition();

        // 4) 프리팹 생성
        GameObject go = Instantiate(data.unitPrefab, pos, Quaternion.identity, unitsRoot);

        // 5) 런타임 초기화
        var instance = go.GetComponent<UnitInstance>();
        if (instance == null)
        {
            Debug.LogError("Summon failed: Unit prefab missing UnitInstance component.");
            Destroy(go);
            return;
        }

        instance.Initialize(data, 1);

        // (선택) 생성 직후 타겟/경로 초기화
        var pc = go.GetComponent<PlayerCharacter>();
        if (pc != null)
        {
            pc.ClearTarget();
            pc.agent.ResetPath();
            pc.agent.isStopped = true; // 준비 단계는 기본 정지
        }

        if(roster != null)
            roster.Register(instance);

        if (fusionService != null)
            fusionService.TryAutoFuse(instance);

        // (선택) 드래그 배치 컨트롤러에 등록
        // 필요 시 PlacementController.Register(...) 형태로 확장 권장
        // StageManager.Instance.placementController?.Register( ... );
    }

    private Vector3 ResolveSpawnPosition()
    {
        if (spawnPoint != null)
            return spawnPoint.position;

        // spawnPoint 미설정이면 PlacementArea 중심에 생성(간단 테스트용)
        if (StageManager.Instance != null && StageManager.Instance.placementArea != null)
            return StageManager.Instance.placementArea.transform.position;

        // 그마저도 없으면 본 스크립트 위치
        return transform.position;
    }
}
