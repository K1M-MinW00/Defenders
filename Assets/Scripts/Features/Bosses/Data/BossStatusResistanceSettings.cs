using System;
using UnityEngine;

[Serializable]
public sealed class BossStatusResistanceSettings
{
    [Range(0f, 1f)] public float slow;
    [Range(0f, 1f)] public float stun;
    [Range(0f, 1f)] public float poison;
    [Range(0f, 1f)] public float burn;
    [Range(0f, 1f)] public float defenseDown;

    public float Get(StatusEffectType type) => type switch
    {
        StatusEffectType.Slow => slow,
        StatusEffectType.Stun => stun,
        StatusEffectType.Poison => poison,
        StatusEffectType.Burn => burn,
        StatusEffectType.DefenseDown => defenseDown,
        _ => 0f,
    };

    public bool HasAny => slow > 0f || stun > 0f || poison > 0f || burn > 0f || defenseDown > 0f;
}
