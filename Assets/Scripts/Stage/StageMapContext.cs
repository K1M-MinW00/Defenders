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
}