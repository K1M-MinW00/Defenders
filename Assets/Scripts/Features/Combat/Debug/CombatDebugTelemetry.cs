using System;
using System.Diagnostics;

public static class CombatDebugTelemetry
{
    public readonly struct SkillAppliedEvent
    {
        public UnitController Caster { get; }
        public ActiveSkillBase Skill { get; }
        public float Time { get; }

        public SkillAppliedEvent(UnitController caster, ActiveSkillBase skill, float time)
        {
            Caster = caster;
            Skill = skill;
            Time = time;
        }
    }

    public readonly struct DamageAppliedEvent
    {
        public ICombatHealth Target { get; }
        public DamageRequest Request { get; }
        public DamageResult Result { get; }

        public DamageAppliedEvent(
            ICombatHealth target,
            DamageRequest request,
            DamageResult result)
        {
            Target = target;
            Request = request;
            Result = result;
        }
    }

    public static event Action<SkillAppliedEvent> SkillApplied;
    public static event Action<DamageAppliedEvent> DamageApplied;

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void ReportSkillApplied(UnitController caster, ActiveSkillBase skill, float time)
    {
        if (caster != null && skill != null)
            SkillApplied?.Invoke(new SkillAppliedEvent(caster, skill, time));
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void ReportDamageApplied(
        ICombatHealth target,
        DamageRequest request,
        DamageResult result)
    {
        if (result.WasApplied)
            DamageApplied?.Invoke(new DamageAppliedEvent(target, request, result));
    }

    public static void ClearListeners()
    {
        SkillApplied = null;
        DamageApplied = null;
    }
}
