using UnityEngine;

[CreateAssetMenu(menuName = "Game/Units/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string unitId;
    public string displayName;

    [Header("Prefab")]
    public GameObject unitPrefab;

    [Header("Base Stats (Star 1)")]
    public UnitStats baseStats;


    [Header("Star Multipliers (Index = Star - 1")]
    [Tooltip("Ex) [1.0f, 1.6f, 2.3f, 3.5f] => 1 ~ 4¼º ¹èÀ²")]
    public float[] starMultipliers = { 1.0f, 1.6f, 2.3f, 3.5f };

    public UnitStats GetStats(int star)
    {
        if(starMultipliers == null || starMultipliers.Length == 0)
        {
            Debug.LogWarning($"Unit Data ({name}) has no starMultipliers. Using baseStats");
            return baseStats;
        }

        int idx = Mathf.Clamp(star - 1, 0, starMultipliers.Length - 1);
        float mul = starMultipliers[idx];
        return baseStats * mul;
    }
}