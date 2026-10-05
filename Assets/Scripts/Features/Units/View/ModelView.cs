using UnityEngine;

public class ModelView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;

    [Header("World Sorting")]
    [SerializeField] private bool sortByWorldY = true;
    [SerializeField] private int worldSortingBase = 1000;
    [SerializeField, Min(1f)] private float sortingOrdersPerUnit = 10f;

    private SpriteRenderer[] sortedRenderers;
    private int[] rendererOrderOffsets;

    private bool isFacingRight = true;

    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int SkillHash = Animator.StringToHash("Skill");
    private static readonly int AttackSpeedMultiplierHash = Animator.StringToHash("AttackSpeedMultiplier");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        CacheWorldSorting();
        RefreshWorldSorting();
    }

    private void LateUpdate()
    {
        RefreshWorldSorting();
    }

    public void PlayIdle()
    {
        animator.Play(IdleHash);
    }

    public void PlayMove()
    {
        animator.Play(MoveHash);
    }
    public void PlayAttack(float attacksPerSecond = 1f)
    {
        animator.SetFloat(
            AttackSpeedMultiplierHash,
            AttackAnimationSpeed.FromAttacksPerSecond(attacksPerSecond));
        animator.Play(AttackHash, 0, 0f);
    }
    public void PlayDie()
    {
        animator.Play(DieHash);
    }
    public void PlaySkill()
    {
        animator.Play(SkillHash, 0, 0f);
    }

    public void FaceTo(Vector3 from, Vector3 targetPos)
    {
        float dx = targetPos.x - from.x;

        if (Mathf.Abs(dx) < 0.01f)
            return;

        SetFacing(dx > 0f);
    }

    public void FaceDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) < 0.01f)
            return;

        SetFacing(dir.x > 0f);
    }

    public void SetFacing(bool faceRight)
    {
        if (faceRight == isFacingRight)
            return;

        isFacingRight = faceRight;

        Transform pivot = visualRoot != null ? visualRoot : transform;
        Vector3 scale = pivot.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        pivot.localScale = scale;
    }

    public Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    private void CacheWorldSorting()
    {
        sortedRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        rendererOrderOffsets = new int[sortedRenderers.Length];

        if (sortedRenderers.Length == 0)
            return;

        int minimumOrder = sortedRenderers[0].sortingOrder;
        for (int i = 1; i < sortedRenderers.Length; i++)
            minimumOrder = Mathf.Min(minimumOrder, sortedRenderers[i].sortingOrder);

        for (int i = 0; i < sortedRenderers.Length; i++)
            rendererOrderOffsets[i] = sortedRenderers[i].sortingOrder - minimumOrder;
    }

    private void RefreshWorldSorting()
    {
        if (!sortByWorldY || sortedRenderers == null)
            return;

        int rootOrder = worldSortingBase - Mathf.RoundToInt(transform.position.y * sortingOrdersPerUnit);
        for (int i = 0; i < sortedRenderers.Length; i++)
        {
            if (sortedRenderers[i] != null)
                sortedRenderers[i].sortingOrder = rootOrder + rendererOrderOffsets[i];
        }
    }

}
