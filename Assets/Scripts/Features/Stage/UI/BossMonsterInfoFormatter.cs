using System.Collections.Generic;
using System.Globalization;

public static class BossMonsterInfoFormatter
{
    public static string Build(BossDefinitionSO definition)
    {
        if (definition == null)
            return string.Empty;

        List<string> lines = new()
        {
            Localize("monster.info.boss_skills"),
            Localize("monster.info.area_attack"),
            Localize(
                "monster.info.shield",
                Percent(definition.shieldTriggerHpRatio),
                Percent(definition.shieldMaxHpRatio)),
            Localize(
                "monster.info.enrage",
                Number(definition.enragedDamageMultiplier),
                Number(definition.enragedAttackSpeedMultiplier)),
        };

        BossSummonSettings summon = definition.summon;
        if (summon != null && summon.enabled)
        {
            lines.Add(Localize(
                "monster.info.summon",
                summon.count,
                Number(summon.cooldown),
                summon.maxAliveSummons));
        }

        BossAreaAttackStatusSettings effects = definition.areaAttackStatusEffects;
        if (effects != null && effects.EnabledEffectCount > 0)
        {
            lines.Add(string.Empty);
            lines.Add(Localize("monster.info.status_effects"));

            if (effects.enableSlow)
                lines.Add(Localize("monster.info.slow", Number(effects.slowDuration), Percent(effects.slowRatio)));
            if (effects.enableStun)
                lines.Add(Localize("monster.info.stun", Number(effects.stunDuration)));
            if (effects.enablePoison)
                lines.Add(Localize(
                    "monster.info.poison",
                    Number(effects.poisonDuration),
                    Number(effects.poisonTickInterval),
                    Percent(effects.poisonDamageMultiplier)));
            if (effects.enableBurn)
                lines.Add(Localize(
                    "monster.info.burn",
                    Number(effects.burnDuration),
                    Number(effects.burnTickInterval),
                    Percent(effects.burnDamageMultiplier)));
            if (effects.enableDefenseDown)
                lines.Add(Localize(
                    "monster.info.defense_down",
                    Number(effects.defenseDownDuration),
                    Number(effects.defenseDownPercentagePoints)));
        }

        lines.Add(string.Empty);
        lines.Add(Localize("monster.info.status_resistance"));
        BossStatusResistanceSettings resistance = definition.statusResistance;
        if (resistance == null || !resistance.HasAny)
        {
            lines.Add(Localize("monster.info.status_resistance.none"));
        }
        else
        {
            AddResistance(lines, "slow", resistance.slow);
            AddResistance(lines, "stun", resistance.stun);
            AddResistance(lines, "poison", resistance.poison);
            AddResistance(lines, "burn", resistance.burn);
            AddResistance(lines, "defense_down", resistance.defenseDown);
        }
        return string.Join("\n", lines);
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Stage, key, arguments);

    private static int Percent(float ratio) => UnityEngine.Mathf.RoundToInt(ratio * 100f);

    private static string Number(float value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private static void AddResistance(List<string> lines, string key, float value)
    {
        if (value > 0f)
            lines.Add(Localize($"monster.info.resistance.{key}", Percent(value)));
    }
}
