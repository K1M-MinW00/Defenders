using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Game/Units/Skill")]
public class SkillDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public Sprite icon;
    public SkillType skillType; // 패시브 , 액티브

    [Header("Audio")]
    public AudioClipSettings skillSound = new();

    [Header("Promotion Progression")]
    [Tooltip("스킬 해금 및 강화 단계 정보. promotionLevel 오름차순으로 작성")]
    public List<SkillUpgradeData> upgrades = new();

    public string LocalizedName => Localize("name", skillName);

    public string GetLocalizedUpgradeDescription(int index)
    {
        if (upgrades == null || index < 0 || index >= upgrades.Count)
            return string.Empty;
        return Localize($"upgrade.{index}", upgrades[index]?.description);
    }

    private string Localize(string suffix, string fallback)
    {
        string value = LocalizationService.Get(LocalizationTables.Skills, $"skill.{name}.{suffix}");
        return string.IsNullOrWhiteSpace(value) || value.StartsWith("No translation found for") ? fallback : value;
    }

    public bool IsStageUnlocked(int promotion, int stageIndex)
    {
        if (upgrades == null || stageIndex < 0 || stageIndex >= upgrades.Count)
            return false;

        SkillUpgradeData stage = upgrades[stageIndex];
        return stage != null && promotion >= stage.promotionLevel;
    }

    // [Header("Tags")]
    // public List<SkillTagType> tags = new();
}
