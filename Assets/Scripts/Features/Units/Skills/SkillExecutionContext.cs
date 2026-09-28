using System.Collections.Generic;
using UnityEngine;

public class SkillExecutionContext
{
    public UnitController Caster { get; private set; }
    public ICombatTarget EnemyTarget { get; private set; }
    public readonly List<ICombatTarget> EnemyTargets = new();
    public readonly List<UnitController> AllyTargets = new();

    public Vector3 CastPosition { get; private set; }
    public Vector2 CastDirection { get; private set; }
    public bool IsValid { get; private set; }

    public void Initialize(UnitController caster)
    {
        Caster = caster;
        CastPosition = caster.transform.position;
        CastDirection = Vector2.zero;
        IsValid = false;
        EnemyTarget = null;
        EnemyTargets.Clear();
        AllyTargets.Clear();
    }

    public void SetEnemyTarget(ICombatTarget target)
    {
        EnemyTarget = target;
        EnemyTargets.Clear();

        if (target != null)
        {
            EnemyTargets.Add(target);
            CastPosition = target.TargetTransform.position;
            IsValid = true;
        }
    }

    public void SetEnemyTargets(IEnumerable<ICombatTarget> targets)
    {
        EnemyTargets.Clear();

        if (targets != null)
            EnemyTargets.AddRange(targets);

        EnemyTarget = EnemyTargets.Count > 0 ? EnemyTargets[0] : null;
        if (EnemyTarget != null)
        {
            CastPosition = EnemyTarget.TargetTransform.position;
            IsValid = true;
        }
    }

    public void SetAllyTargets(List<UnitController> targets)
    {
        AllyTargets.Clear();

        if (targets != null)
            AllyTargets.AddRange(targets);

        IsValid = AllyTargets.Count > 0;
    }

    public void SetAllyTarget(UnitController target)
    {
        AllyTargets.Clear();

        if (target != null)
            AllyTargets.Add(target);

        IsValid = target != null;
    }

    public void AddAllyTarget(UnitController target)
    {
        AllyTargets.Add(target);
    }

    public void SetCastPosition(Vector3 pos)
    {
        CastPosition = pos;
        IsValid = true;
    }

    public void SetCastDirection(Vector2 direction)
    {
        CastDirection = direction.normalized;
        IsValid = CastDirection.sqrMagnitude > 0.0001f;
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}
