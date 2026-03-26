public interface IActiveSkill
{
    void Initialize(UnitController owner, ActiveSkillDataSO data);
    bool CanUseSkill();
    void BeginSkill();
    void OnSkillHit();
    void EndSkill();
    void CancelSkill();
}