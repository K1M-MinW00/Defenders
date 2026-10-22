using System;
using UnityEngine;

public enum SkillStatusMagnitudeMode
{
    Fixed = 0,
    SourceAttackRatio = 1,
}

[Serializable]
public sealed class SkillOnHitStatusEffectSettings
{
    public bool enabled;
    public StatusEffectType type;

    [Range(0f, 1f)]
    public float chance = 1f;

    [Min(0f)]
    public float duration = 1f;

    [Tooltip("감속은 0~1 비율, 방어력 감소는 %p, 중독/화상은 틱당 피해량입니다.")]
    [Min(0f)]
    public float magnitude;

    [Tooltip("중독/화상 피해량을 공격력 비율로 계산할 때 사용합니다.")]
    public SkillStatusMagnitudeMode magnitudeMode;

    [Min(0.05f)]
    public float tickInterval = 1f;
}
