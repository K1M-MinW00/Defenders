public enum MonsterClassification
{
    Normal = 0,
    Elite = 1,
    Boss = 2
}

public static class MonsterVisualScalePolicy
{
    public const float NormalScale = 1f;
    public const float EliteScale = 1.2f;
    public const float BossScale = 1.5f;

    public static float GetDefaultScale(MonsterClassification classification)
    {
        return classification switch
        {
            MonsterClassification.Elite => EliteScale,
            MonsterClassification.Boss => BossScale,
            _ => NormalScale
        };
    }
}
