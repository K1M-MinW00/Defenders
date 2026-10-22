using NUnit.Framework;
using System.Linq;
using System.Reflection;
using UnityEngine;

public sealed class MonsterStatusEffectBarViewTests
{
    [Test]
    public void BindAndRefresh_ShowsAtMostFourEffectsByPriority()
    {
        GameObject monsterObject = new("Monster");
        MonsterController monster = monsterObject.AddComponent<MonsterController>();
        typeof(MonsterController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(monster, null);
        GameObject contextObject = new("CombatContext");
        UnitRoster roster = contextObject.AddComponent<UnitRoster>();
        StagePoolManager pool = contextObject.AddComponent<StagePoolManager>();
        MonsterDataSO data = ScriptableObject.CreateInstance<MonsterDataSO>();
        data.monsterId = "status_view_test";
        Assert.That(monster.TryInitialize(
            roster,
            data,
            pool,
            new MonsterStats { maxHp = 100f, moveSpeed = 2f, atkDamage = 10f, atkRange = 1f, atkPerSec = 1f }),
            Is.True);
        MonsterStatusEffectController effects = monster.StatusEffects;

        effects.Apply(StatusEffectApplication.Slow(5f, 0.2f));
        effects.Apply(StatusEffectApplication.Stun(5f));
        effects.Apply(StatusEffectApplication.Poison(5f, 1f));
        effects.Apply(StatusEffectApplication.Burn(5f, 1f));
        effects.Apply(StatusEffectApplication.DefenseDown(5f, 20f));

        GameObject viewObject = new("MonsterStatusEffectView", typeof(RectTransform));
        MonsterStatusEffectBarView view = viewObject.AddComponent<MonsterStatusEffectBarView>();
        view.Bind(effects);

        Assert.That(view.VisibleCount, Is.EqualTo(4));
        Assert.That(viewObject.transform.childCount, Is.EqualTo(5));
        Assert.That(view.VisibleTypes.Contains(StatusEffectType.DefenseDown), Is.True);
        Assert.That(view.VisibleTypes.Contains(StatusEffectType.Slow), Is.False);

        Object.DestroyImmediate(viewObject);
        Object.DestroyImmediate(monsterObject);
        Object.DestroyImmediate(contextObject);
        Object.DestroyImmediate(data);
    }

    [Test]
    public void ShowImmune_ActivatesReusableFeedbackWithoutAddingObjects()
    {
        GameObject owner = new("Monster");
        MonsterStatusEffectBarView view = MonsterStatusEffectBarView.Create(owner.transform);
        view.Bind(null);
        int childCount = view.transform.childCount;

        view.ShowImmune(StatusEffectType.Poison);
        view.ShowImmune(StatusEffectType.Poison);

        Assert.That(view.gameObject.activeSelf, Is.True);
        Assert.That(view.transform.childCount, Is.EqualTo(childCount));
        Assert.That(view.transform.Find("ImmuneFeedback").gameObject.activeSelf, Is.True);
        Object.DestroyImmediate(owner);
    }

    [TestCase(MonsterClassification.Normal, 1.45f)]
    [TestCase(MonsterClassification.Elite, 1.7f)]
    [TestCase(MonsterClassification.Boss, 2.05f)]
    public void ConfigureHeight_UsesClassificationHeight(MonsterClassification classification, float expected)
    {
        GameObject owner = new("Monster");
        MonsterStatusEffectBarView view = MonsterStatusEffectBarView.Create(owner.transform);

        view.ConfigureHeight(classification);

        Assert.That(view.transform.localPosition.y, Is.EqualTo(expected).Within(0.001f));
        Assert.That(view.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
        float expectedScale = classification switch
        {
            MonsterClassification.Boss => 0.0095f,
            MonsterClassification.Elite => 0.0085f,
            _ => 0.0075f,
        };
        Assert.That(view.transform.localScale.x, Is.EqualTo(expectedScale).Within(0.0001f));
        Object.DestroyImmediate(owner);
    }
}
