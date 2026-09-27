using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monsters/Monster Base Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("Identity")]
    public string monsterId;
    public string displayName;
    public GameObject prefab;

    [Header("Audio")]
    public AudioClipSettings attackSound = new();
    public AudioClipSettings hitSound = new();

    [Header("Base Stats")]
    [SerializeField] private MonsterStats baseStats = new();

    public float BaseMaxHp => baseStats?.maxHp ?? 0f;

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

        error = string.Empty;
        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
