using NUnit.Framework;
using UnityEngine;

public sealed class BossRuntimeTests
{
    private GameObject root;
    private BossDefinitionSO definition;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Boss Runtime Test");
        root.AddComponent<MonsterController>();
        root.AddComponent<BossController>();

        definition = ScriptableObject.CreateInstance<BossDefinitionSO>();
        definition.bossId = "test_boss";
        definition.shieldTriggerHpRatio = 0.6f;
        definition.shieldMaxHpRatio = 0.2f;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void Shield_TriggersOnceAfterHpCrossesThreshold()
    {
        MonsterHealth health = root.GetComponent<MonsterHealth>();
        BossShieldController shield = root.GetComponent<BossShieldController>();
        health.Initialize(new MonsterStats { maxHp = 100f });
        shield.Initialize(root.GetComponent<MonsterController>(), definition);

        health.ApplyDamage(new DamageRequest(39f));
        Assert.That(shield.IsActive, Is.False);

        health.ApplyDamage(new DamageRequest(2f));
        Assert.That(health.CurrentHp, Is.EqualTo(59f).Within(0.001f));
        Assert.That(shield.CurrentShield, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void Shield_AbsorbsDamageBeforeHealthAndPassesOverflow()
    {
        MonsterHealth health = root.GetComponent<MonsterHealth>();
        BossShieldController shield = root.GetComponent<BossShieldController>();
        health.Initialize(new MonsterStats { maxHp = 100f });
        shield.Initialize(root.GetComponent<MonsterController>(), definition);
        health.ApplyDamage(new DamageRequest(50f));

        health.ApplyDamage(new DamageRequest(25f));

        Assert.That(shield.CurrentShield, Is.Zero);
        Assert.That(health.CurrentHp, Is.EqualTo(45f).Within(0.001f));
    }

    [Test]
    public void Definition_DefaultRecommendedValuesAreValid()
    {
        Assert.That(definition.TryValidate(out string error), Is.True, error);
        Assert.That(definition.enragedDamageMultiplier, Is.EqualTo(1.3f));
        Assert.That(definition.enragedAttackSpeedMultiplier, Is.EqualTo(1.25f));
    }

    [Test]
    public void Definition_DefaultAudioSlotsAreOptionalAndEmpty()
    {
        Assert.That(definition.audio, Is.Not.Null);
        Assert.That(definition.audio.areaAttackTelegraph.IsValid, Is.False);
        Assert.That(definition.audio.areaAttackImpact.IsValid, Is.False);
        Assert.That(definition.audio.shieldActivated.IsValid, Is.False);
        Assert.That(definition.audio.enrage.IsValid, Is.False);
        Assert.That(definition.audio.summon.IsValid, Is.False);
        Assert.That(definition.TryValidate(out string error), Is.True, error);
    }
}
