using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class PromotionProgressionTests
{
    [Test]
    public void PromotionEffects_MatchConfiguredProgression()
    {
        PromotionProgressionSO progression = Resources.Load<PromotionProgressionSO>(
            "Database/PromotionProgression");

        Assert.That(progression, Is.Not.Null);
        Assert.That(progression.GetUnlockedStatBonuses(1), Is.Empty);

        PromotionStatBonus[] promotion2 = progression.GetUnlockedStatBonuses(2).ToArray();
        Assert.That(promotion2, Has.Length.EqualTo(1));
        Assert.That(promotion2[0].statType, Is.EqualTo(StatType.MaxHp));
        Assert.That(promotion2[0].percentValue, Is.EqualTo(10f));

        PromotionStatBonus[] promotion3 = progression.GetUnlockedStatBonuses(3).ToArray();
        Assert.That(promotion3, Has.Length.EqualTo(2));
        Assert.That(promotion3.Any(x => x.statType == StatType.Attack && x.percentValue == 10f), Is.True);

        Assert.That(progression.GetStartingEnergyPercent(3), Is.Zero);
        Assert.That(progression.GetStartingEnergyPercent(4), Is.EqualTo(50f));
    }

    [Test]
    public void SkillUpgradeData_UnlocksAtConfiguredPromotionLevels()
    {
        SkillDataSO activeSkill = ScriptableObject.CreateInstance<SkillDataSO>();
        SkillDataSO passiveSkill = ScriptableObject.CreateInstance<SkillDataSO>();

        activeSkill.upgrades.Add(new SkillUpgradeData { promotionLevel = 0 });
        activeSkill.upgrades.Add(new SkillUpgradeData { promotionLevel = 2 });
        activeSkill.upgrades.Add(new SkillUpgradeData { promotionLevel = 4 });
        passiveSkill.upgrades.Add(new SkillUpgradeData { promotionLevel = 1 });
        passiveSkill.upgrades.Add(new SkillUpgradeData { promotionLevel = 3 });

        Assert.That(passiveSkill.IsStageUnlocked(0, 0), Is.False);
        Assert.That(passiveSkill.IsStageUnlocked(1, 0), Is.True);
        Assert.That(activeSkill.IsStageUnlocked(1, 1), Is.False);
        Assert.That(activeSkill.IsStageUnlocked(2, 1), Is.True);
        Assert.That(passiveSkill.IsStageUnlocked(3, 1), Is.True);
        Assert.That(activeSkill.IsStageUnlocked(4, 2), Is.True);

        Object.DestroyImmediate(activeSkill);
        Object.DestroyImmediate(passiveSkill);
    }
}
