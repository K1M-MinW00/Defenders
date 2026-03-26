using UnityEngine;

[System.Serializable]
public class StageUnitRuntime
{
    [Header("Identity")]
    public UnitCode UnitCode;

    [Header("Persistent Progress")]
    public int Level;
    public int Promotion;
    public int LimitBreak;

    [Header("Stage Progress")]
    [Range(1, 4)] public int Star = 1;

    [Header("Combat Stats")]
    public UnitStats FinalStats;

    [Header("Runtime State")]
    public float CurrentHp;
    public float CurrentEnergy;
    public float MaxEnergy = 100f;
    public bool IsDead;

    [Header("Unlock Flags")]
    public bool CanUsePassive;
    public bool CanUseActive;
    public bool CanRecoverEnergy;

    public bool ActiveTier2Unlocked;
    public bool PassiveTier3Unlocked;
    public bool ActiveTier4Unlocked;

    public StageUnitRuntime(StageUnitInitData initData)
    {
        UnitCode = initData.UserData.UnitCode;
        Level = initData.UserData.Level;
        Promotion = initData.UserData.Promotion;
        LimitBreak = initData.UserData.LimitBreak;

        Star = initData.InitialStar;
        CurrentEnergy = 0f;
        IsDead = false;

        RefreshPersistentFlags();
        RefreshStageFlags();
    }
    public void RefreshPersistentFlags()
    {
        CanUsePassive = Promotion >= 1;
        ActiveTier2Unlocked = Promotion >= 2;
        PassiveTier3Unlocked = Promotion >= 3;
        ActiveTier4Unlocked = Promotion >= 4;
    }

    public void RefreshStageFlags()
    {
        CanUseActive = Star >= 3;
        CanRecoverEnergy = Star >= 3;

        if (!CanRecoverEnergy)
            CurrentEnergy = 0f;
    }

    public void RefreshAllFlags()
    {
        RefreshPersistentFlags();
        RefreshStageFlags();
    }
}