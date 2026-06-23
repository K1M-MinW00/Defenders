using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monsters/Monster Base Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("Identity")]
    public string monsterId;
    public string displayName;
    public GameObject prefab;

    [Header("Base Stats")]
    [SerializeField] private MonsterStats baseStats;

    public MonsterStats Stats { get => baseStats; }
}