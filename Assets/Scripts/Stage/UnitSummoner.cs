using UnityEngine;

public class UnitSummoner : MonoBehaviour
{
    [Header("Unit Pool (Inspector)")]
    [SerializeField] private UnitDataSO[] unitPool;

    [Header("Spawn Settings")]
    [SerializeField] private Transform unitsRoot;          // 생성된 유닛을 담을 부모(없으면 null 가능)
    [SerializeField] private Transform spawnPoint;         // 우선 스폰 위치(없으면 PlacementArea 중심)
    [SerializeField] private TilemapPlacementArea placementArea;
    [SerializeField] private MonsterSpawner monsterSpawner;
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
        UnitDataSO data = unitPool[Random.Range(0, unitPool.Length)];

        // 3) 스폰 위치 결정
        Vector3 pos = ResolveSpawnPosition();
        GameObject go = Instantiate(data.UnitPrefab, pos, Quaternion.identity, unitsRoot);

        // 5) 런타임 초기화
        var instance = go.GetComponent<UnitRuntime>();
        if (instance == null)
        {
            Debug.LogError("Summon failed: Unit prefab missing UnitInstance component.");
            Destroy(go);
            return;
        }

        PlayerCharacter p = go.GetComponent<PlayerCharacter>();
        p.BindCombatContext(monsterSpawner);
        instance.Initialize(data, 1);

        if (roster != null)
            roster.Register(instance);
        
        if (fusionService != null)
            fusionService.TryAutoFuse(instance);

    }

    private Vector3 ResolveSpawnPosition()
    {
        if (spawnPoint != null)
            return spawnPoint.position;

        // spawnPoint 미설정이면 PlacementArea 중심에 생성(간단 테스트용)
        if (placementArea != null)
            return placementArea.transform.position;

        // 그마저도 없으면 본 스크립트 위치
        return transform.position;
    }
}
