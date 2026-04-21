using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game/Units/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Identity")]
    public UnitCode unitCode;
    public string displayName;
    public Sprite icon;
    public GameObject unitPrefab;

    [Header("Skills")]
    public SkillDataSO passiveSkill;
    public SkillDataSO activeSkill;

    [Header("Base Stats (Lv1, 1己)")]
    public UnitStats baseStats = new UnitStats(10f,50f, 3f, 4f);

    [Header("Level Growth")]
    public int maxLevel = 100;
    public AnimationCurve attackGrowthCurve = AnimationCurve.Linear(0f, 1f, 1f, 2f);
    public AnimationCurve hpGrowthCurve = AnimationCurve.Linear(0f, 1f, 1f, 2f);

    [Header("Star Growth")]
    [Tooltip("牢郸胶 0 = 1己, 1 = 2己, 2 = 3己, 3 = 4己")]
    public float[] starAttackMultipliers = { 1f, 1.35f, 1.8f, 2.4f };
    public float[] starHpMultipliers = { 1f, 1.35f, 1.8f, 2.4f };
    public float[] starDetectRangeMultipliers = { 1f, 1.2f, 1.5f, 2f };
    public float[] starAttackPerSecondMultipliers = { 1f, 1.5f, 1.8f, 2f };

    public UnitStats GetOriginStats(int level)
    {
        level = Mathf.Clamp(level, 1, maxLevel);

        float t = Mathf.InverseLerp(1, maxLevel, level);

        UnitStats stats = baseStats;

        stats.Attack = baseStats.Attack * attackGrowthCurve.Evaluate(t);
        stats.MaxHp = baseStats.MaxHp * hpGrowthCurve.Evaluate(t);

        return stats;
    }

    public UnitStats ApplyStar(UnitStats stats, int star)
    {
        int index = Mathf.Clamp(star, 1, 4) - 1;

        UnitStats result = stats;

        result.Attack *= GetMultiplier(starAttackMultipliers, index);
        result.MaxHp *= GetMultiplier(starHpMultipliers, index);
        result.DetectRange *= GetMultiplier(starDetectRangeMultipliers, index);
        result.AttackPerSec *= GetMultiplier(starAttackPerSecondMultipliers, index);
        return result;
    }

    private float GetMultiplier(float[] table, int index)
    {
        if (table == null || table.Length == 0)
            return 1f;

        if (index >= table.Length)
            return table[table.Length - 1];

        return table[index];
    }
}