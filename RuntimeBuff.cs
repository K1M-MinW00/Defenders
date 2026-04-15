public enum BuffStatType
{
    Attack,
    AttackSpeed,
    MaxHp,
    EnergyRecovery
}

public enum BuffModifyType
{
    Additive,   // +10
    Multiplicative // +50% => 1.5배
}

public enum BuffDurationType
{
    UntilWaveEnd,
    WaveCount,
    UntilStageEnd
}

public sealed class RuntimeBuff
{
    public string BuffId { get; private set; }
    public UnitController Source { get; private set; }

    public BuffStatType StatType { get; private set; }
    public BuffModifyType ModifyType { get; private set; }
    public float Value { get; private set; }

    public BuffDurationType DurationType { get; private set; }
    public int RemainingWaves { get; private set; }

    public RuntimeBuff(
        string buffId,
        UnitController source,
        BuffStatType statType,
        BuffModifyType modifyType,
        float value,
        BuffDurationType durationType,
        int remainingWaves = 0)
    {
        BuffId = buffId;
        Source = source;
        StatType = statType;
        ModifyType = modifyType;
        Value = value;
        DurationType = durationType;
        RemainingWaves = remainingWaves;
    }

    public void DecrementWave()
    {
        if (DurationType != BuffDurationType.WaveCount)
            return;

        RemainingWaves--;
    }

    public bool IsExpired()
    {
        return DurationType switch
        {
            BuffDurationType.UntilWaveEnd => false,   // 웨이브 종료 시 외부에서 일괄 제거
            BuffDurationType.WaveCount => RemainingWaves <= 0,
            BuffDurationType.UntilStageEnd => false,  // 스테이지 종료 시 외부에서 일괄 제거
            _ => false
        };
    }
}