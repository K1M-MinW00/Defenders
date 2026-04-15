using System.Collections.Generic;
using UnityEngine;

public class SkillExecutionContext
{
    public UnitController Caster { get; private set; }

    public MonsterController EnemyTarget { get; private set; }
    public readonly List<MonsterController> EnemyTargets = new();

    public readonly List<UnitController> AllyTargets = new();

    public Vector3 CastPosition { get; private set; }
    public bool IsValid { get; private set; }

    public void Initialize(UnitController caster)
    {
        Caster = caster;
        CastPosition = caster.transform.position;
        IsValid = false;
        EnemyTarget = null;
        EnemyTargets.Clear();
        AllyTargets.Clear();
    }

    public void SetEnemyTarget(MonsterController target)
    {
        EnemyTarget = target;
        EnemyTargets.Clear();

        if (target != null)
        {
            EnemyTargets.Add(target);
            CastPosition = target.transform.position;
            IsValid = true;
        }
    }

    public void SetEnemyTargets(List<MonsterController> targets)
    {
        EnemyTargets.Clear();

        if (targets != null)
            EnemyTargets.AddRange(targets);

        EnemyTarget = EnemyTargets.Count > 0 ? EnemyTargets[0] : null;
        if (EnemyTarget != null)
        {
            CastPosition = EnemyTarget.transform.position;
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

    public void SetCastPosition(Vector3 pos)
    {
        CastPosition = pos;
        IsValid = true;
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}