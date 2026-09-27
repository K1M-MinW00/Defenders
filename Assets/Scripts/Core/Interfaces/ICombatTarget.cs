using UnityEngine;

public interface ICombatTarget
{
    Transform TargetTransform { get; }
    ICombatHealth CombatHealth { get; }
    bool IsDead { get; }
}
