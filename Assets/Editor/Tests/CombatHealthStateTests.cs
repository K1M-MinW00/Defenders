using NUnit.Framework;

public sealed class CombatHealthStateTests
{
    [Test]
    public void Damage_IsClampedAndMarksDead()
    {
        CombatHealthState state = new();
        state.Initialize(100f);

        float applied = state.TakeDamage(150f);

        Assert.That(applied, Is.EqualTo(100f));
        Assert.That(state.Current, Is.Zero);
        Assert.That(state.IsDead, Is.True);
    }

    [Test]
    public void RestoreFull_RevivesAndResetsHealth()
    {
        CombatHealthState state = new();
        state.Initialize(80f);
        state.Kill();

        state.RestoreFull();

        Assert.That(state.Current, Is.EqualTo(80f));
        Assert.That(state.IsDead, Is.False);
    }

    [Test]
    public void Heal_DoesNotExceedMaximum()
    {
        CombatHealthState state = new();
        state.Initialize(50f);
        state.TakeDamage(20f);

        float healed = state.Heal(100f);

        Assert.That(healed, Is.EqualTo(20f));
        Assert.That(state.Current, Is.EqualTo(50f));
    }
}
