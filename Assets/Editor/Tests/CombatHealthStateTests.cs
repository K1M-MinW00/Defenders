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

    [Test]
    public void SetMaximum_CanKeepCurrentHealthRatio()
    {
        CombatHealthState state = new();
        state.Initialize(100f);
        state.TakeDamage(25f);

        state.SetMaximum(200f, restoreFull: false);

        Assert.That(state.Max, Is.EqualTo(200f));
        Assert.That(state.Current, Is.EqualTo(150f));
    }

    [Test]
    public void SetMaximum_CanRestoreFullHealth()
    {
        CombatHealthState state = new();
        state.Initialize(100f);
        state.TakeDamage(75f);

        state.SetMaximum(120f, restoreFull: true);

        Assert.That(state.Max, Is.EqualTo(120f));
        Assert.That(state.Current, Is.EqualTo(120f));
        Assert.That(state.IsDead, Is.False);
    }
}
