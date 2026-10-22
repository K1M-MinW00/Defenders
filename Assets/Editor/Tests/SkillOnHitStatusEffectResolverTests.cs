using NUnit.Framework;

public sealed class SkillOnHitStatusEffectResolverTests
{
    [Test]
    public void TryBuildApplication_UsesSourceAttackRatioForPeriodicDamage()
    {
        SkillOnHitStatusEffectSettings settings = new()
        {
            enabled = true,
            type = StatusEffectType.Burn,
            chance = 1f,
            duration = 4f,
            magnitude = 0.2f,
            magnitudeMode = SkillStatusMagnitudeMode.SourceAttackRatio,
            tickInterval = 0.5f,
        };

        bool built = SkillOnHitStatusEffectResolver.TryBuildApplication(
            settings, null, 50f, 0.99f, out StatusEffectApplication application);

        Assert.That(built, Is.True);
        Assert.That(application.Type, Is.EqualTo(StatusEffectType.Burn));
        Assert.That(application.Duration, Is.EqualTo(4f));
        Assert.That(application.Magnitude, Is.EqualTo(10f));
        Assert.That(application.TickInterval, Is.EqualTo(0.5f));
    }

    [Test]
    public void TryBuildApplication_RejectsFailedChanceRoll()
    {
        SkillOnHitStatusEffectSettings settings = new()
        {
            enabled = true,
            type = StatusEffectType.Stun,
            chance = 0.5f,
            duration = 1f,
            magnitude = 1f,
        };

        Assert.That(SkillOnHitStatusEffectResolver.TryBuildApplication(
            settings, null, 100f, 0.5f, out _), Is.False);
    }

    [Test]
    public void TryBuildApplication_FullChanceAcceptsMaximumRoll()
    {
        SkillOnHitStatusEffectSettings settings = new()
        {
            enabled = true,
            type = StatusEffectType.Stun,
            chance = 1f,
            duration = 1f,
            magnitude = 1f,
        };

        Assert.That(SkillOnHitStatusEffectResolver.TryBuildApplication(
            settings, null, 100f, 1f, out _), Is.True);
    }

    [Test]
    public void TryBuildApplication_RejectsDisabledOrExpiredConfiguration()
    {
        SkillOnHitStatusEffectSettings settings = new() { enabled = false, duration = 3f };
        Assert.That(SkillOnHitStatusEffectResolver.TryBuildApplication(
            settings, null, 100f, 0f, out _), Is.False);

        settings.enabled = true;
        settings.duration = 0f;
        Assert.That(SkillOnHitStatusEffectResolver.TryBuildApplication(
            settings, null, 100f, 0f, out _), Is.False);
    }
}
