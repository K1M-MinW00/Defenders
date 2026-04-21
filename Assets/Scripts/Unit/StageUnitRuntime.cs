using UnityEngine;

[System.Serializable]
public class StageUnitRuntime
{
    public UnitCode UnitCode { get; private set; }
    public int Star { get; private set; }

    public UnitStats OriginStats { get; private set; } // 1성 기준 기본 스탯
    public UnitStats StageBaseStats { get; private set; } // 스테이지 안에서 구조적으로 변한 기본 스탯
    public UnitStats FinalStats { get; private set; } // 실제 전투에 쓰이는 최종 스탯
    
    public bool CanUseActive => Star >= 3;

    public StageUnitRuntime(StageUnitInitData initData)
    {
        UnitCode = initData.UserData.UnitCode;
        Star = Mathf.Max(1,initData.InitialStar);
    }

    public void SetOriginStats(UnitStats stats)
    {
        OriginStats = stats;
    }

    public void SetRuntimeBaseStats(UnitStats stats)
    {
        StageBaseStats = stats;
    }

    public void SetFinalStats(UnitStats stats)
    {
        FinalStats = stats;
    }

    public void UpgradeStar()
    {
        SetStar(Star + 1);
    }

    public void SetStar(int star)
    {
        Star = Mathf.Max(1, star);
    }
}