using System.Collections.Generic;
using UnityEngine;

public class UnitSummoner_ : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform unitsRoot;
    public MonsterSpawner monsterSpawner;
    public UnitRoster roster;
    public FusionService fusionService;
    public TilemapPlacementArea placementArea;
    public Transform spawnPoint;

    private readonly List<UserUnitData> summonCandidates = new();
    private readonly Dictionary<UnitCode, UnitDataSO> selectedUnitMap = new();

    public void SetSummonCandidates(List<UserUnitData> selectedUsers, List<UnitDataSO> selectedUnitDataSOs)
    {
        summonCandidates.Clear();
        selectedUnitMap.Clear();

        if (selectedUsers != null)
            summonCandidates.AddRange(selectedUsers);

        if (selectedUnitDataSOs != null)
        {
            foreach (var data in selectedUnitDataSOs)
            {
                if (data == null)
                    continue;

                selectedUnitMap[data.unitCode] = data;
            }
        }
    }

    public bool SummonRandomUnit(int initialStar = 1)
    {
        if (summonCandidates.Count == 0)
        {
            Debug.LogWarning("Summon blocked: no summon candidates.");
            return false;
        }

        UserUnitData selectedUser = summonCandidates[Random.Range(0, summonCandidates.Count)];

        if (!selectedUnitMap.TryGetValue(selectedUser.UnitCode, out UnitDataSO unitData))
        {
            Debug.LogWarning($"Summon failed: UnitDataSO not found for {selectedUser.UnitCode}");
            return false;
        }

        if (unitData == null || unitData.unitPrefab == null)
        {
            Debug.LogWarning("Summon failed: unitData or unitPrefab is null.");
            return false;
        }

        Vector3 pos = ResolveSpawnPosition();

        GameObject go = Instantiate(unitData.unitPrefab, pos, Quaternion.identity, unitsRoot);
        UnitController unit = go.GetComponent<UnitController>();

        if (unit == null)
        {
            Debug.LogError($"Summoned prefab does not have UnitController: {unitData.name}");
            Destroy(go);
            return false;
        }

        StageUnitInitData initData = new StageUnitInitData(unitData, selectedUser, initialStar);

        unit.BindCombatContext(monsterSpawner, roster);
        unit.Initialize(initData);
        unit.SetCombatPhase(false);

        roster?.Register(unit);
        fusionService?.TryAutoFuse(unit);

        return true;
    }

    private Vector3 ResolveSpawnPosition()
    {
        if (spawnPoint != null)
            return spawnPoint.position;

        if (placementArea != null)
            return placementArea.transform.position;

        return transform.position;
    }
}