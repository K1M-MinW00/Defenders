using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class UnitProgressionStringTableSetup
{
    private static readonly Dictionary<string, string> SkillNames = new()
    {
        ["Archer_Active"] = "Piercing Shot", ["Archer_Passive"] = "Rapid Fire",
        ["Axeman_Active"] = "Devastating Whirlwind", ["Axeman_Passive"] = "Berserker Instinct",
        ["Knight_Active"] = "Slashing Wave", ["Knight_Passive"] = "Iron Will",
        ["KnightTemplar_Active"] = "Ray of Judgment", ["KnightTemplar_Passive"] = "Formation Breaker",
        ["Lancer_Active"] = "Battle Standard", ["Lancer_Passive"] = "Battle Fervor",
        ["Priest_Active"] = "Divine Healing", ["Priest_Passive"] = "Healing Resonance",
        ["SoldierM_Active"] = "Leaping Strike", ["SoldierM_Passive"] = "Fighting Spirit",
        ["SoldierR_Active"] = "Rain of Arrows", ["SoldierR_Passive"] = "Weakness Exploit",
        ["Swordsman_Active"] = "Blade Dance", ["Swordsman_Passive"] = "Swordmaster",
        ["Werebear_Active"] = "Earthshatter", ["Werebear_Passive"] = "Blood Thirst",
        ["Wizard_Active"] = "Meteor", ["Wizard_Passive"] = "Arcane Resonance",
    };

    private static readonly Dictionary<string, string> KoreanBaseDescriptions = new()
    {
        ["Archer_Active"] = "관통 화살로 적을 공격한다.",
        ["Archer_Passive"] = "연속 공격 시 공격 속도가 증가한다.",
        ["Axeman_Active"] = "도끼를 휘둘러 주변 적을 공격한다.",
        ["Axeman_Passive"] = "피격 시 잠시 공격력과 공격 속도가 증가한다.",
        ["Knight_Active"] = "검기로 전방의 적을 벤다.",
        ["Knight_Passive"] = "일정 횟수 피격 후 잠시 받는 피해가 감소한다.",
        ["KnightTemplar_Active"] = "성스러운 빛으로 직선상의 적을 관통한다.",
        ["KnightTemplar_Passive"] = "공격할 때마다 적을 밀어낸다.",
        ["Lancer_Active"] = "깃발을 세워 아군의 공격력과 공격 속도를 잠시 높인다.",
        ["Lancer_Passive"] = "같은 적을 공격할수록 피해가 증가한다.",
        ["Priest_Active"] = "체력이 가장 낮은 아군을 회복한다.",
        ["Priest_Passive"] = "일정 횟수 공격 후 체력이 가장 낮은 아군을 회복한다.",
        ["SoldierM_Active"] = "적에게 도약해 범위 피해를 준다.",
        ["SoldierM_Passive"] = "피격 시 일정 확률로 받는 피해가 감소한다.",
        ["SoldierR_Active"] = "화살을 쏟아 범위 피해를 준다.",
        ["SoldierR_Passive"] = "적의 약점을 노려 추가 피해를 준다.",
        ["Swordsman_Active"] = "빠르게 연속 찌르기를 가한다.",
        ["Swordsman_Passive"] = "연속 공격 시 공격 속도가 증가한다.",
        ["Werebear_Active"] = "지면을 강타해 주변 적에게 큰 피해를 준다.",
        ["Werebear_Passive"] = "적의 피를 흡수해 체력을 회복한다.",
        ["Wizard_Active"] = "불덩이를 떨어뜨려 범위 피해를 준다.",
        ["Wizard_Passive"] = "스킬 사용 후 에너지를 일부 회복한다.",
    };

    private static readonly Dictionary<string, string> EnglishBaseDescriptions = new()
    {
        ["Archer_Active"] = "Fires a piercing arrow.",
        ["Archer_Passive"] = "Consecutive attacks increase Attack Speed.",
        ["Axeman_Active"] = "Swings an axe at nearby enemies.",
        ["Axeman_Passive"] = "Taking damage briefly boosts Attack and Attack Speed.",
        ["Knight_Active"] = "Cuts through enemies ahead with a blade wave.",
        ["Knight_Passive"] = "After several hits, briefly reduces damage taken.",
        ["KnightTemplar_Active"] = "Holy light pierces enemies in a line.",
        ["KnightTemplar_Passive"] = "Each attack knocks enemies back.",
        ["Lancer_Active"] = "Briefly boosts allied Attack and Attack Speed.",
        ["Lancer_Passive"] = "Repeated attacks on one enemy deal more damage.",
        ["Priest_Active"] = "Heals the ally with the lowest HP.",
        ["Priest_Passive"] = "After several attacks, heals the ally with the lowest HP.",
        ["SoldierM_Active"] = "Leaps at an enemy and deals area damage.",
        ["SoldierM_Passive"] = "Taking a hit may reduce its damage.",
        ["SoldierR_Active"] = "Rains arrows to deal area damage.",
        ["SoldierR_Passive"] = "Targets weak points for bonus damage.",
        ["Swordsman_Active"] = "Unleashes rapid thrusting attacks.",
        ["Swordsman_Passive"] = "Consecutive attacks increase Attack Speed.",
        ["Werebear_Active"] = "Slams the ground for heavy area damage.",
        ["Werebear_Passive"] = "Drains enemy blood to restore HP.",
        ["Wizard_Active"] = "Drops a fireball for area damage.",
        ["Wizard_Passive"] = "Restores some Energy after using a skill.",
    };

    private static readonly Dictionary<string, string> UpgradeDescriptions = new()
    {
        ["데미지 증가"] = "Increases damage.", ["전투 시작 시 에너지 50% 획득"] = "Gain 50% Energy at the start of battle.",
        ["공격 속도 증가"] = "Increases Attack Speed.", ["쿨타임 감소"] = "Reduces cooldown.",
        ["발동 조건 감소"] = "Reduces the activation requirement.", ["넉백 거리 증가"] = "Increases knockback distance.",
        ["버프 지속 시간 증가"] = "Increases buff duration.", ["회복량 증가"] = "Increases healing.",
        ["피해 감소량 증가"] = "Increases damage reduction.", ["화살 개수 및 데미지 증가"] = "Increases arrow count and damage.",
        ["추가 피해량 증가"] = "Increases bonus damage.", ["회복 마력량 증가"] = "Increases Energy restored.",
    };

    private static readonly Dictionary<string, string> LimitBreakDescriptions = new()
    {
        ["공격 속도 10% 증가"] = "Attack Speed +10%", ["공격력 10% 증가"] = "Attack +10%",
        ["에너지 회복 10% 증가"] = "Energy Recovery +10%", ["에너지 회복 15% 증가"] = "Energy Recovery +15%",
        ["체력 10% 증가"] = "HP +10%", ["체력 12% 증가"] = "HP +12%",
        ["치명타 피해 15% 증가"] = "Critical Damage +15%", ["치명타 확률 15% 증가"] = "Critical Chance +15%",
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTable skillsKo = GetTable(LocalizationTables.Skills, korean, english, tablesDirectory, korean);
        StringTable skillsEn = GetTable(LocalizationTables.Skills, korean, english, tablesDirectory, english);
        Set(skillsKo, "skill.effect.base", "기본 효과"); Set(skillsEn, "skill.effect.base", "Base Effect");
        Set(skillsKo, "skill.effect.promotion", "{0} 진급 효과", true); Set(skillsEn, "skill.effect.promotion", "Promotion {0} Effect", true);
        foreach (string guid in AssetDatabase.FindAssets("t:SkillDataSO", new[] { "Assets/GameData/Units" }))
        {
            SkillDataSO skill = AssetDatabase.LoadAssetAtPath<SkillDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (skill == null) continue;
            Set(skillsKo, $"skill.{skill.name}.name", skill.skillName);
            Set(skillsEn, $"skill.{skill.name}.name", SkillNames.GetValueOrDefault(skill.name, skill.skillName));
            for (int i = 0; i < skill.upgrades.Count; i++)
            {
                string source = skill.upgrades[i]?.description ?? string.Empty;
                string ko = i == 0 ? KoreanBaseDescriptions.GetValueOrDefault(skill.name, source) : source;
                string en = i == 0 ? EnglishBaseDescriptions.GetValueOrDefault(skill.name, ko) : UpgradeDescriptions.GetValueOrDefault(source, source);
                Set(skillsKo, $"skill.{skill.name}.upgrade.{i}", ko); Set(skillsEn, $"skill.{skill.name}.upgrade.{i}", en);
            }
        }

        StringTable unitsKo = GetTable(LocalizationTables.Units, korean, english, tablesDirectory, korean);
        StringTable unitsEn = GetTable(LocalizationTables.Units, korean, english, tablesDirectory, english);
        foreach (string guid in AssetDatabase.FindAssets("t:UnitDataSO", new[] { "Assets/Resources/GameData/Units" }))
        {
            UnitDataSO unit = AssetDatabase.LoadAssetAtPath<UnitDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (unit == null) continue;
            string englishName = unit.displayName?.Replace('_', ' ') ?? string.Empty;
            Set(unitsKo, $"unit.{unit.unitId}.name", englishName);
            Set(unitsEn, $"unit.{unit.unitId}.name", englishName);
            for (int i = 0; i < unit.limitBreaks.Count; i++)
            {
                string ko = unit.limitBreaks[i]?.description ?? string.Empty;
                Set(unitsKo, $"unit.{unit.unitId}.limit_break.{i}", ko);
                Set(unitsEn, $"unit.{unit.unitId}.limit_break.{i}", LimitBreakDescriptions.GetValueOrDefault(ko, ko));
            }
        }
        Dirty(skillsKo, skillsEn, unitsKo, unitsEn);
    }

    private static StringTable GetTable(string name, Locale ko, Locale en, string path, Locale locale)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(name) ??
            LocalizationEditorSettings.CreateStringTableCollection(name, path, new List<Locale> { ko, en });
        return collection.GetTable(locale.Identifier) as StringTable ?? collection.AddNewTable(locale.Identifier) as StringTable;
    }

    private static void Set(StringTable table, string key, string value, bool smart = false)
    {
        StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, value);
        entry.Value = value; entry.IsSmart = smart;
    }

    private static void Dirty(params StringTable[] tables)
    {
        foreach (StringTable table in tables) { EditorUtility.SetDirty(table); EditorUtility.SetDirty(table.SharedData); }
    }
}
