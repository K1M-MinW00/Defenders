using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game/Units/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string unitId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Prefab")]
    [SerializeField] private GameObject unitPrefab;

    [Header("Base Stats (Star 1)")]
    [SerializeField] private UnitStats baseStats;

    public string UnitId => unitId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject UnitPrefab => unitPrefab;
    public UnitStats BaseStats => baseStats;
}