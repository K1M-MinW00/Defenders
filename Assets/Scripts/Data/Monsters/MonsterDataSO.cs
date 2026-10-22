using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monsters/Monster Base Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("Identity")]
    public string monsterId;
    public string displayName;
    public Sprite portrait;
    [TextArea(2, 5)] public string description;
    public GameObject prefab;

    [Header("Classification")]
    [SerializeField] private MonsterClassification classification = MonsterClassification.Normal;
    [Tooltip("0이면 등급 기본 크기를 사용합니다. Normal 1.0, Elite 1.2, Boss 1.5")]
    [SerializeField, Min(0f)] private float visualScaleOverride;
    [SerializeField] private BossDefinitionSO bossDefinition;

    [Header("Audio")]
    public AudioClipSettings attackSound = new();
    public AudioClipSettings hitSound = new();

    [Header("Base Stats")]
    [SerializeField] private MonsterStats baseStats = new();
    [SerializeField, Min(0.01f)] private float threatCost = 1f;

    [Header("Behavior")]
    [SerializeField] private MonsterBehaviorSettings behavior = new();

    public float BaseMaxHp => baseStats?.maxHp ?? 0f;
    public float BaseMoveSpeed => baseStats?.moveSpeed ?? 0f;
    public float BaseAttackDamage => baseStats?.atkDamage ?? 0f;
    public float BaseAttackRange => baseStats?.atkRange ?? 0f;
    public float BaseAttackPerSecond => baseStats?.atkPerSec ?? 0f;
    public float ThreatCost => Mathf.Max(0.01f, threatCost);
    public MonsterClassification Classification => classification;
    public float VisualScale => visualScaleOverride > 0f
        ? visualScaleOverride
        : MonsterVisualScalePolicy.GetDefaultScale(classification);
    public BossDefinitionSO BossDefinition => bossDefinition;
    public float IdleTargetAcquireInterval =>
        Mathf.Max(0.02f, behavior?.idleTargetAcquireInterval ?? 0.5f);
    public float MoveTargetRefreshInterval =>
        Mathf.Max(0.02f, behavior?.moveTargetRefreshInterval ?? 0.25f);

    public MonsterStats CreateRuntimeStats()
    {
        return baseStats?.CreateRuntimeCopy() ?? new MonsterStats();
    }

    public bool TryValidate(out string error)
    {
        if (string.IsNullOrWhiteSpace(monsterId))
            return Fail("Monster ID is missing.", out error);
        if (prefab == null)
            return Fail("Monster prefab is missing.", out error);
        if (baseStats == null)
            return Fail("Monster base stats are missing.", out error);
        if (baseStats.maxHp <= 0f)
            return Fail("Max HP must be positive.", out error);
        if (baseStats.moveSpeed < 0f || baseStats.atkDamage < 0f || baseStats.atkRange < 0f)
            return Fail("Move speed, attack damage, and attack range cannot be negative.", out error);
        if (baseStats.atkPerSec <= 0f)
            return Fail("Attack per second must be positive.", out error);
        if (threatCost <= 0f)
            return Fail("Threat cost must be positive.", out error);
        if (behavior == null)
            return Fail("Monster behavior settings are missing.", out error);
        if (behavior.idleTargetAcquireInterval <= 0f || behavior.moveTargetRefreshInterval <= 0f)
            return Fail("Monster target refresh intervals must be positive.", out error);
        if (visualScaleOverride < 0f)
            return Fail("Visual scale override cannot be negative.", out error);
        if (classification == MonsterClassification.Boss && bossDefinition == null)
            return Fail("Boss definition is missing for a boss monster.", out error);

        error = string.Empty;
        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
