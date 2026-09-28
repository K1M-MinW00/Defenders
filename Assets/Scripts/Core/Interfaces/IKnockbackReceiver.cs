using UnityEngine;

public interface IKnockbackReceiver
{
    void ApplyKnockback(Vector2 direction, float distance, float duration);
}
