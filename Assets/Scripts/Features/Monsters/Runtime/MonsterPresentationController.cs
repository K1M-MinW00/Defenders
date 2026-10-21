using UnityEngine;

public sealed class MonsterPresentationController : MonoBehaviour
{
    private ModelView modelView;

    public void Initialize()
    {
        modelView = GetComponentInChildren<ModelView>(true);
    }

    public void FaceMovement(Vector3 velocity)
    {
        if (velocity.sqrMagnitude > 0.0001f)
            modelView?.FaceDirection(velocity);
    }

    public void FaceTarget(Vector3 ownerPosition, Transform target)
    {
        if (target != null)
            modelView?.FaceTo(ownerPosition, target.position);
    }

    public void PlayIdle()
    {
        modelView?.PlayIdle();
    }

    public void PlayMove()
    {
        modelView?.PlayMove();
    }

    public void PlayAttack(float attacksPerSecond)
    {
        modelView?.PlayAttack(Mathf.Max(0.01f, attacksPerSecond));
    }
}
