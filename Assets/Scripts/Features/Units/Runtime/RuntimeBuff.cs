public enum StatType
{
    Attack,
    MaxHp,

    AttackPerSec,
    DetectRange,

    CritChance,
    CritDamage,

    EnergyRecovery
}

public enum BuffModifyType
{
    Flat,
    Percent,
}

public enum BuffDurationType
{
    Timed,
    UntilWaveEnd,
    WaveCount,
    UntilStageEnd,
}

public readonly struct BuffApplication
{
    public string BuffId { get; }
    public StatType StatType { get; }
    public BuffModifyType ModifyType { get; }
    public float Value { get; }
    public BuffDurationType DurationType { get; }
    public float DurationSeconds { get; }
    public int RemainingWaves { get; }

    public BuffApplication(
        string buffId,
        StatType statType,
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
        DurationSeconds = durationSeconds;
        RemainingWaves = remainingWaves;
    }

    public bool IsValid => !string.IsNullOrWhiteSpace(BuffId);

    public RuntimeBuff CreateRuntimeBuff()
    {
        return new RuntimeBuff(
            BuffId,
            StatType,
            ModifyType,
            Value,
            DurationType,
            DurationSeconds,
            RemainingWaves);
    }
}

public sealed class RuntimeBuff
{
    public string BuffId { get; private set; }
    public StatType StatType { get; private set; }
    public BuffModifyType ModifyType { get; private set; }
    public float Value { get; private set; }

    public BuffDurationType DurationType { get; private set; }
    public float RemainingTime { get; private set; }
    public int RemainingWaves { get; private set; }

    public RuntimeBuff(
        string buffId,
        StatType statType,
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

    public bool CompleteWave()
    {
        switch (DurationType)
        {
            case BuffDurationType.WaveCount:
                AdvanceWave();
                return IsExpired();

            case BuffDurationType.UntilStageEnd:
                return false;

            case BuffDurationType.Timed:
            case BuffDurationType.UntilWaveEnd:
            default:
                return true;
        }
    }

    public bool CanRefreshFrom(BuffApplication application)
    {
        return BuffId == application.BuffId &&
               StatType == application.StatType &&
               ModifyType == application.ModifyType &&
               DurationType == application.DurationType;
    }

    public bool RefreshFrom(BuffApplication application)
    {
        if (!CanRefreshFrom(application))
            return false;

        bool statChanged = Value != application.Value;
        Value = application.Value;
        RemainingTime = application.DurationSeconds;
        RemainingWaves = application.RemainingWaves;
        return statChanged;
    }
}
