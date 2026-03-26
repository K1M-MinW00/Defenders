public interface IActiveSkill
{
    bool IsUsingSkill { get; }
    void Initialize(UnitController owner, ActiveSkillDataSO data, int promotionLevel);
    bool CanUseSkill();
    void BeginSkill();
    void OnSkillHit();
    void EndSkill();
    void CancelSkill();
}