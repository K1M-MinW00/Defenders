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
}
