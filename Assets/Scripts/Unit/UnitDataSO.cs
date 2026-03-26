using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game/Units/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private UnitCode unitCode;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject unitPrefab;

    [Header("Skills")]
    [SerializeField] private SkillDataSO passiveSkill;
    [SerializeField] private SkillDataSO activeSkill;

    [Header("Base Stats (Star 1)")]
    [SerializeField] private UnitStats baseStats = new UnitStats(88f,100f, 3f, 4f,0f,1f);

    [Tooltip("1레벨~최대레벨 공격력 성장 배율 곡선. 1레벨에서 보통 1.0")]
    [SerializeField]
    private AnimationCurve attackGrowthCurve =
        AnimationCurve.Linear(0f, 1f, 1f, 2f);

    [Tooltip("1레벨~최대레벨 체력 성장 배율 곡선. 1레벨에서 보통 1.0")]
    [SerializeField]
    private AnimationCurve hpGrowthCurve =
        AnimationCurve.Linear(0f, 1f, 1f, 2f);

    [Header("Star Multipliers")]
    [Tooltip("인덱스 0 = 1성, 1 = 2성, 2 = 3성, 3 = 4성")]
    [SerializeField] private float[] starAttackMultipliers = { 1f, 1.35f, 1.8f, 2.4f };

    [Tooltip("인덱스 0 = 1성, 1 = 2성, 2 = 3성, 3 = 4성")]
    [SerializeField] private float[] starHpMultipliers = { 1f, 1.35f, 1.8f, 2.4f };

    [Tooltip("인덱스 0 = 1성, 1 = 2성, 2 = 3성, 3 = 4성")]
    [SerializeField] private float[] starDetectRangeMultipliers = { 1f, 1f, 1.1f, 1.2f };

    public UnitCode UnitCode => unitCode;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject UnitPrefab => unitPrefab;
    public SkillDataSO PassiveSkill => passiveSkill;
    public SkillDataSO ActiveSkill => activeSkill;
    public UnitStats BaseStats => baseStats;

    public float BaseAttack => baseStats.Attack;
    public float BaseMaxHp => baseStats.MaxHp;
    public float BaseAttackPerSec => baseStats.AttackPerSec;
    public float BaseDetectRange => baseStats.DetectRange;
    public float BaseCritChance => baseStats.CritChance;
    public float BaseEnergyRecovery => baseStats.EnergyRecovery;

    public float GetAttackByLevel(int level)
    {
        float growth = EvaluateGrowthCurve(attackGrowthCurve, level);
        return baseStats.Attack * growth;
    }

    public float GetHpByLevel(int level)
    {
        float growth = EvaluateGrowthCurve(hpGrowthCurve, level);
        return baseStats.MaxHp * growth;
    }

    public float GetStarAttackMultiplier(int star)
    {
        return GetStarMultiplier(starAttackMultipliers, star);
    }

    public float GetStarHpMultiplier(int star)
    {
        return GetStarMultiplier(starHpMultipliers, star);
    }

    public float GetStarDetectRangeMultiplier(int star)
    {
        return GetStarMultiplier(starDetectRangeMultipliers, star);
    }

    private float EvaluateGrowthCurve(AnimationCurve curve, int level)
    {
        if (curve == null || curve.length == 0)
            return 1f;

        return curve.Evaluate(level);
    }

    private float GetStarMultiplier(float[] table, int star)
    {
        if (table == null || table.Length == 0)
            return 1f;

        int index = Mathf.Clamp(star, 1, 4) - 1;

        if (index >= table.Length)
            return table[table.Length - 1];

        return table[index];
    }
}