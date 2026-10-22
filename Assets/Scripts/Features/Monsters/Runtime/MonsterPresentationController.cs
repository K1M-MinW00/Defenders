using UnityEngine;

public sealed class MonsterPresentationController : MonoBehaviour
{
    private ModelView modelView;
    private Vector3 baseModelScale = Vector3.one;

    public void Initialize()
    {
        modelView = GetComponentInChildren<ModelView>(true);
        if (modelView != null)
            baseModelScale = modelView.transform.localScale;
    }

    public void ApplyVisualScale(float multiplier)
    {
        if (modelView != null)
            modelView.transform.localScale = baseModelScale * Mathf.Max(0.01f, multiplier);
    }

    public void ResetVisualScale()
    {
        if (modelView != null)
            modelView.transform.localScale = baseModelScale;
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

    public void FacePosition(Vector3 ownerPosition, Vector3 targetPosition)
    {
        modelView?.FaceTo(ownerPosition, targetPosition);
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
