using UnityEngine;

public interface ICombatTarget
{
    Transform TargetTransform { get; }
    ICombatHealth CombatHealth { get; }
    bool IsDead { get; }
}

public interface ICombatTargetProvider
{
    ICombatTarget FindClosestAlive(Vector3 origin);
}
