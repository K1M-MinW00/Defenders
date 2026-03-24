using UnityEngine;

public class ModelView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;

    private bool isFacingRight = true;

    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int SkillHash = Animator.StringToHash("Skill");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayIdle()
    {
        animator.Play(IdleHash);
    }

    public void PlayMove()
    {
        animator.Play(MoveHash);
    }
    public void PlayAttack()
    {
        animator.Play(AttackHash);
    }
    public void PlayDie()
    {
        animator.Play(DieHash);
    }
    public void PlaySkill()
    {
        animator.Play(SkillHash);
    }

    public void FaceTo(Vector3 from, Vector3 targetPos)
    {
        float dx = targetPos.x - from.x;

        if (Mathf.Abs(dx) < 0.01f)
            return;

        bool shouldFaceRight = dx > 0f;

        if (shouldFaceRight == isFacingRight)
            return;

        isFacingRight = shouldFaceRight;

        Transform pivot = visualRoot != null ? visualRoot : transform;
        Vector3 scale = pivot.localScale;
        scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1f : -1f);
        pivot.localScale = scale;
    }

    public Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }
}