using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monsters/Monster Data")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private float maxHp = 50f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float atk = 10f;
    [SerializeField] private float atkPerSec = .5f;
    [SerializeField] private float attackRange = 1.5f;

    public float MaxHp { get => maxHp; }
    public float MoveSpeed { get => moveSpeed; }
    public float Atk {  get => atk; }
    public float AtkPerSec { get => atkPerSec; }
    public float AttackRange { get => attackRange; }
}