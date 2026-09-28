using UnityEngine;

public static class AttackAnimationSpeed
{
    private const float MinimumPlaybackSpeed = 1f;
    private const float MaximumPlaybackSpeed = 10f;

    public static float FromAttacksPerSecond(float attacksPerSecond)
    {
        if (float.IsNaN(attacksPerSecond) || float.IsInfinity(attacksPerSecond))
            return MinimumPlaybackSpeed;

        return Mathf.Clamp(attacksPerSecond, MinimumPlaybackSpeed, MaximumPlaybackSpeed);
    }
}
