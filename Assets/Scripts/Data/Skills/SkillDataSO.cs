using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Game/Units/Skill")]
public class SkillDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public Sprite icon;
    public SkillType skillType; // 패시브 , 액티브

    [Header("Promotion Progression")]
    [Tooltip("스킬 해금 및 강화 단계 정보. promotionLevel 오름차순으로 작성")]
    public List<SkillUpgradeData> upgrades = new();

    // [Header("Tags")]
    // public List<SkillTagType> tags = new();
}