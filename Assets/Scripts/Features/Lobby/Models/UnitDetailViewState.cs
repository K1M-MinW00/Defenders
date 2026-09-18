using UnityEngine;

public sealed class UnitDetailViewState
{
    public string UnitId { get; set; }
    public UnitDataSO Definition { get; set; }
    public Sprite Icon { get; set; }
    public string DisplayName { get; set; }
    public Rarity Rarity { get; set; }
    public int Level { get; set; }
    public float Attack { get; set; }
    public float MaxHp { get; set; }
    public int Promotion { get; set; }
    public int LimitBreak { get; set; }
    public SkillDataSO ActiveSkill { get; set; }
    public SkillDataSO PassiveSkill { get; set; }
}
