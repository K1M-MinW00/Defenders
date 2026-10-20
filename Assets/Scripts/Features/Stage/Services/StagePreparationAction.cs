using System;

[Flags]
public enum StagePreparationAction
{
    None = 0,
    Summon = 1 << 0,
    IncreasePopulation = 1 << 1,
    Sell = 1 << 2,
    Reroll = 1 << 3,
    StartWave = 1 << 4,
    All = Summon | IncreasePopulation | Sell | Reroll | StartWave,
}
