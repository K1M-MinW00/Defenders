using System.Collections.Generic;

public static class BossMonsterInfoFormatter
{
    public static string Build(BossDefinitionSO definition)
    {
        if (definition == null)
            return string.Empty;

        List<string> mechanics = new();
        if (definition.shieldMaxHpRatio > 0f)
            mechanics.Add(Localize("monster.info.mechanic.shield"));

        mechanics.Add(Localize("monster.info.mechanic.area_attack"));

        BossSummonSettings summon = definition.summon;
        if (summon != null && summon.enabled)
            mechanics.Add(Localize("monster.info.mechanic.summon"));

        if (definition.enragedDamageMultiplier > 1f || definition.enragedAttackSpeedMultiplier > 1f)
            mechanics.Add(Localize("monster.info.mechanic.enrage"));

        return string.Join(", ", mechanics);
    }

    private static string Localize(string key) => LocalizationService.Get(LocalizationTables.Stage, key);
}
