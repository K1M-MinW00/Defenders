using UnityEngine;

public interface ICombatTargetProvider
{
    ICombatTarget FindClosestAlive(Vector3 origin);
}
