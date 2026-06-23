using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Game/Units/Skill")]
public class SkillDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    // public SkillType skillType; // 패시브 , 액티브

    // [Header("Tags")]
    // public List<SkillTagType> tags = new();

    //[Header("Promotion Progression")]
    //[Tooltip("스킬 해금 및 강화 단계 정보. promotionLevel 오름차순으로 작성")]
    //public List<SkillUpgradeEntry> promotionEntries = new();

    //public bool IsUnlocked(int promotionLevel)
    //{
    //    for (int i = 0; i < promotionEntries.Count; i++)
    //    {
    //        SkillUpgradeEntry entry = promotionEntries[i];
    //        if (entry.unlocksSkill && promotionLevel >= entry.promotionLevel)
    //            return true;
    //    }

    //    return false;
    //}

    //public int GetUnlockPromotionLevel()
    //{
    //    for (int i = 0; i < promotionEntries.Count; i++)
    //    {
    //        if (promotionEntries[i].unlocksSkill)
    //            return promotionEntries[i].promotionLevel;
    //    }

    //    return int.MaxValue;
    //}

    //public List<SkillUpgradeEntry> GetAvailableEntries(int promotionLevel)
    //{
    //    List<SkillUpgradeEntry> result = new();

    //    for (int i = 0; i < promotionEntries.Count; i++)
    //    {
    //        if (promotionLevel >= promotionEntries[i].promotionLevel)
    //            result.Add(promotionEntries[i]);
    //    }

    //    return result;
    //}

    //public List<SkillUpgradeEntry> GetAllEntries()
    //{
    //    return promotionEntries;
    //}
}