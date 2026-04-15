public sealed class RuntimeBuff
{
    public string BuffId { get; private set; }
    public BuffStatType StatType { get; private set; }
    public BuffModifyType ValueType { get; private set; }
    public float Value { get; private set; }
    public BuffDurationType DurationType { get; private set; }
    public int RemainingWaves { get; private set; }

    public RuntimeBuff(
        string buffId,
        BuffStatType statType,
        BuffModifyType valueType,
        float value,
        BuffDurationType durationType,
        int remainingWaves = 0)
    {
        BuffId = buffId;
        StatType = statType;
        ValueType = valueType;
        Value = value;
        DurationType = durationType;
        RemainingWaves = remainingWaves;
    }

    public void AdvanceWave()
    {
        if (DurationType == BuffDurationType.WaveCount)
            RemainingWaves--;
    }

    public bool IsExpired()
    {
        return DurationType == BuffDurationType.WaveCount && RemainingWaves <= 0;
    }
}