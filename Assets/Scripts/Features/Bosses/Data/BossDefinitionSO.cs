using UnityEngine;

[CreateAssetMenu(menuName = "Game/Bosses/Boss Definition")]
public sealed class BossDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public string bossId;

    [Header("Shield")]
    [Range(0f, 1f)] public float shieldTriggerHpRatio = 0.6f;
    [Min(0f)] public float shieldMaxHpRatio = 0.2f;

    [Header("Enrage")]
    [Min(0f)] public float enragedDamageMultiplier = 1.3f;
    [Min(0.01f)] public float enragedAttackSpeedMultiplier = 1.25f;

    [Header("Area Attack Status Effects")]
    public BossAreaAttackStatusSettings areaAttackStatusEffects = new();

    [Header("Summon")]
    public BossSummonSettings summon = new();

    [Header("Status Resistance")]
    public BossStatusResistanceSettings statusResistance = new();

    [Header("Audio (Optional)")]
    [Tooltip("비어 있는 슬롯은 대체음 없이 재생을 건너뜁니다.")]
    public BossAudioSettings audio = new();

    public bool TryValidate(out string error)
    {
        if (string.IsNullOrWhiteSpace(bossId))
        {
            error = "Boss ID is missing.";
            return false;
        }

        if (shieldTriggerHpRatio <= 0f || shieldTriggerHpRatio >= 1f)
        {
            error = "Shield trigger HP ratio must be between 0 and 1.";
            return false;
        }

        if (shieldMaxHpRatio <= 0f)
        {
            error = "Shield max HP ratio must be positive.";
            return false;
        }

        if (areaAttackStatusEffects == null)
        {
            error = "Area attack status settings are missing.";
            return false;
        }

        if (!areaAttackStatusEffects.TryValidate(out error))
            return false;

        if (summon != null && !summon.TryValidate(out error))
            return false;

        error = string.Empty;
        return true;
    }
}
