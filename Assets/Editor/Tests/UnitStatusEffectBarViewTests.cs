using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public sealed class UnitStatusEffectBarViewTests
{
    [Test]
    public void BindAndRefresh_ShowsSlowWithRemainingTime()
    {
        GameObject unitObject = new("Unit");
        unitObject.AddComponent<NavMeshAgent>();
        unitObject.AddComponent<UnitMovementController>();
        UnitStatusEffectController effects = unitObject.AddComponent<UnitStatusEffectController>();
        UnitController unit = unitObject.AddComponent<UnitController>();
        effects.Initialize(unit);

        GameObject viewObject = new("StatusEffectBar", typeof(RectTransform));
        UnitStatusEffectBarView view = viewObject.AddComponent<UnitStatusEffectBarView>();
        view.Bind(effects);

        Assert.That(effects.Apply(StatusEffectApplication.Slow(3f, 0.35f)), Is.True);
        view.Refresh();

        Assert.That(view.VisibleCount, Is.EqualTo(1));
        Assert.That(viewObject.activeSelf, Is.True);
        Assert.That(viewObject.transform.childCount, Is.EqualTo(4));

        Object.DestroyImmediate(viewObject);
        Object.DestroyImmediate(unitObject);
    }

    [Test]
    public void Refresh_WithoutEffects_HidesBar()
    {
        GameObject viewObject = new("StatusEffectBar", typeof(RectTransform));
        UnitStatusEffectBarView view = viewObject.AddComponent<UnitStatusEffectBarView>();

        view.Bind(null);

        Assert.That(view.VisibleCount, Is.Zero);
        Assert.That(viewObject.activeSelf, Is.False);

        Object.DestroyImmediate(viewObject);
    }

    [Test]
    public void Refresh_WithFiveEffects_ShowsFourByPriority()
    {
        GameObject unitObject = new("Unit");
        unitObject.AddComponent<NavMeshAgent>();
        unitObject.AddComponent<UnitMovementController>();
        UnitStatusEffectController effects = unitObject.AddComponent<UnitStatusEffectController>();
        UnitController unit = unitObject.AddComponent<UnitController>();
        effects.Initialize(unit);

        effects.Apply(StatusEffectApplication.Slow(5f, 0.2f));
        effects.Apply(StatusEffectApplication.Stun(5f));
        effects.Apply(StatusEffectApplication.Poison(5f, 1f));
        effects.Apply(StatusEffectApplication.Burn(5f, 1f));
        effects.Apply(StatusEffectApplication.DefenseDown(5f, 20f));

        GameObject viewObject = new("StatusEffectBar", typeof(RectTransform));
        UnitStatusEffectBarView view = viewObject.AddComponent<UnitStatusEffectBarView>();
        view.Bind(effects);

        Assert.That(view.VisibleCount, Is.EqualTo(4));
        Assert.That(view.VisibleTypes.Contains(StatusEffectType.DefenseDown), Is.True);
        Assert.That(view.VisibleTypes.Contains(StatusEffectType.Slow), Is.False);

        Object.DestroyImmediate(viewObject);
        Object.DestroyImmediate(unitObject);
    }
}
