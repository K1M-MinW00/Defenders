using System.Collections.Generic;

public sealed class LabBonusSnapshot
{
    public static readonly LabBonusSnapshot Empty = new(new Dictionary<LabEffectType, float>());

    private readonly IReadOnlyDictionary<LabEffectType, float> totals;

    public LabBonusSnapshot(IReadOnlyDictionary<LabEffectType, float> totals)
    {
        this.totals = totals ?? new Dictionary<LabEffectType, float>();
    }

    public float GetTotal(LabEffectType effectType) =>
        totals.TryGetValue(effectType, out float value) ? value : 0f;

    public float ApplyPercent(float value, LabEffectType effectType) =>
        value * (1f + GetTotal(effectType) / 100f);
}
