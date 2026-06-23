public enum BuffStatType
{
    Attack,
    MaxHp,
    AttackPerSec,
    DetectRange,
}

public enum BuffModifyType
{
    Additive,
    Multiplicative,
}

public enum BuffDurationType
{
    Timed,
    UntilWaveEnd,
    WaveCount,
    UntilStageEnd,
}

public sealed class RuntimeBuff
{
    public string BuffId { get; private set; }
    public BuffStatType StatType { get; private set; }
    public BuffModifyType ModifyType { get; private set; }
    public float Value { get; private set; }

    public BuffDurationType DurationType { get; private set; }
    public float RemainingTime { get; private set; }
    public int RemainingWaves { get; private set; }

    public RuntimeBuff(
        string buffId,
        BuffStatType statType,
        BuffModifyType modifyType,
        float value,
        BuffDurationType durationType,
        float durationSeconds = 0f,
        int remainingWaves = 0)
    {
        BuffId = buffId;
        StatType = statType;
        ModifyType = modifyType;
        Value = value;
        DurationType = durationType;
        RemainingTime = durationSeconds;
        RemainingWaves = remainingWaves;
    }

    public void Tick(float deltaTime)
    {
        if (DurationType != BuffDurationType.Timed)
            return;

        RemainingTime -= deltaTime;
    }

    public void AdvanceWave()
    {
        if (DurationType != BuffDurationType.WaveCount)
            return;

        RemainingWaves--;
    }

    public bool IsExpired()
    {
        return DurationType switch
        {
            BuffDurationType.Timed => RemainingTime <= 0f,
            BuffDurationType.WaveCount => RemainingWaves <= 0,
            _ => false,
        };
    }
}