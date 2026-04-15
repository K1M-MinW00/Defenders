using UnityEngine;

public class UnitStatService : MonoBehaviour
{
    private UnitController owner;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
    }

    public void Recalculate(bool resetHp)
    {
        if (owner.UnitData == null || owner.Runtime == null)
            return;

        owner.Runtime.RefreshStageFlags();

        // 나중에는 버프까지 포함한 최종 계산으로 확장
        owner.Runtime.FinalStats = UnitStatCalculator.Calculate(owner.UnitData, owner.Runtime);

        if (resetHp)
            owner.Runtime.CurrentHp = owner.Runtime.FinalStats.MaxHp;
        else
            owner.Runtime.CurrentHp = Mathf.Min(owner.Runtime.CurrentHp, owner.Runtime.FinalStats.MaxHp);

        ApplyDerivedValues();
    }

    public void ApplyDerivedValues()
    {
        owner.Targeting.ApplyRange(owner.Runtime.FinalStats.DetectRange);
    }
}