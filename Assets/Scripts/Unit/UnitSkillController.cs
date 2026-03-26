using UnityEngine;

public class UnitSkillController : MonoBehaviour
{
    [SerializeField] private UnitController owner;

    private ActiveSkillDataSO activeSkillData;
    private IActiveSkill activeSkill;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponent<UnitController>();
    }

    public void Initialize(UnitController unit)
    {
        owner = unit;

        if (owner == null || owner.UnitData == null)
            return;

        activeSkillData = owner.UnitData.ActiveSkill;
        if (activeSkillData != null)
        {
            activeSkill = activeSkillData.CreateRuntimeSkill();
            activeSkill?.Initialize(owner, activeSkillData);
        }
    }

    public bool CanUseActiveSkill()
    {
        if (owner.IsDead)
            return false;

        if (activeSkill == null)
            return false;

        return activeSkill.CanUseSkill();
    }

    public void BeginActiveSkill()
    {
        if (!CanUseActiveSkill())
            return;

        activeSkill.BeginSkill();
    }

    public void OnActiveSkillHit()
    {
        activeSkill.OnSkillHit();
    }

    public void EndActiveSkill()
    {
        activeSkill.EndSkill();
    }

    public void CancelActiveSkill()
    {
        activeSkill.CancelSkill();
    }
}