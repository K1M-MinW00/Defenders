using UnityEngine;

public class SoldierR_ArrowRain_Skill : ActiveSkillBase
{
    [Header("Arrow Rain")]
    [SerializeField] private float damageMultiplier = 1.2f;
    [SerializeField] private int arrowCount = 6;
    [SerializeField] private float rainRadius = 1.5f;
    [SerializeField] private float hitRadius = 0.4f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Arrow Rain Visual")]
    [SerializeField] private GameObject fallingArrowPrefab;
    [SerializeField] private float spawnHeight = 3.5f;
    [SerializeField] private float horizontalScatter = 0.15f;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.WaitUntilFound;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();
        if (target == null)
            return false;

        context.SetEnemyTarget(target);
        context.SetCastPosition(target.transform.position);
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = context.CastPosition;
        float damage = owner.Attack * damageMultiplier;

        for (int i = 0; i < arrowCount; i++)
        {
            Vector2 landingOffset = Random.insideUnitCircle * rainRadius;
            Vector2 landingPoint = center + landingOffset;

            Vector2 spawnOffset = new Vector2(
                Random.Range(-horizontalScatter, horizontalScatter),
                0f
            );

            Vector3 spawnPos = (Vector3)(landingPoint + spawnOffset) + Vector3.up * spawnHeight;

            GameObject arrowObj = Instantiate(fallingArrowPrefab, spawnPos, Quaternion.identity);

            if (arrowObj.TryGetComponent<ArrowRainFallingArrow>(out var fallingArrow))
            {
                fallingArrow.Initialize(
                    landingPoint,
                    damage,
                    hitRadius,
                    enemyLayer
                );
            }
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }
}