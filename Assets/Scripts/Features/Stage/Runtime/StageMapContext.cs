using Unity.AI.Navigation;
using UnityEngine;

public class StageMapContext : MonoBehaviour
{
    [Header("Camera Bounds")]
    [SerializeField] private Transform minBounds;
    [SerializeField] private Transform maxBounds;

    [Header("Monster")]
    [SerializeField] private Transform[] monsterSpawnPoints;
    [SerializeField] private Transform unitSpawnPoint;

    [Header("Placement")]
    [SerializeField] private TilemapPlacementArea placementArea;


    public Transform MinBound => minBounds;
    public Transform MaxBound => maxBounds;
    public Transform[] MonsterSpawnPoints => monsterSpawnPoints;
    public Transform UnitSpawnPoint => unitSpawnPoint;
    public TilemapPlacementArea PlacementArea => placementArea;

    public bool TryValidate(out string error)
    {
        if (minBounds == null || maxBounds == null)
        {
            error = "Camera bounds are missing.";
            return false;
        }

        if (unitSpawnPoint == null)
        {
            error = "Unit spawn point is missing.";
            return false;
        }

        if (placementArea == null)
        {
            error = "Placement area is missing.";
            return false;
        }

        if (monsterSpawnPoints == null || monsterSpawnPoints.Length == 0)
        {
            error = "Monster spawn points are missing.";
            return false;
        }

        for (int i = 0; i < monsterSpawnPoints.Length; i++)
        {
            if (monsterSpawnPoints[i] != null)
                continue;

            error = $"Monster spawn point at index {i} is missing.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
