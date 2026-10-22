using NUnit.Framework;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public sealed class MonsterStatusEffectControllerTests
{
    private GameObject monsterObject;
    private GameObject contextObject;
    private MonsterDataSO data;
    private BossDefinitionSO definition;
    private MonsterController monster;

    [SetUp]
    public void SetUp()
    {
        monsterObject = new GameObject("Status Test Monster");
        monster = monsterObject.AddComponent<MonsterController>();
        typeof(MonsterController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(monster, null);
        contextObject = new GameObject("Combat Context");
        UnitRoster roster = contextObject.AddComponent<UnitRoster>();
        StagePoolManager pool = contextObject.AddComponent<StagePoolManager>();

        definition = ScriptableObject.CreateInstance<BossDefinitionSO>();
        definition.bossId = "resistance_test";
        definition.statusResistance.stun = 0.75f;

        data = ScriptableObject.CreateInstance<MonsterDataSO>();
        data.monsterId = "resistance_test";
        SerializedObject serialized = new(data);
        serialized.FindProperty("classification").enumValueIndex = (int)MonsterClassification.Boss;
        serialized.FindProperty("bossDefinition").objectReferenceValue = definition;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        Assert.That(monster.TryInitialize(
            roster,
            data,
            pool,
            new MonsterStats { maxHp = 100f, moveSpeed = 2f, atkDamage = 10f, atkRange = 1f, atkPerSec = 1f }), Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(monsterObject);
        Object.DestroyImmediate(contextObject);
        Object.DestroyImmediate(data);
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void Apply_UsesBossResistanceBeforeStoringEffect()
    {
        Assert.That(monster.ApplyStatusEffect(StatusEffectApplication.Stun(4f)), Is.True);
        Assert.That(monster.StatusEffects.TryGet(StatusEffectType.Stun, out float remaining, out _), Is.True);
        Assert.That(remaining, Is.EqualTo(1f).Within(0.001f));
        Assert.That(monster.IsControlLocked, Is.True);

        monster.StatusEffects.Tick(1.1f);

        Assert.That(monster.StatusEffects.TryGet(StatusEffectType.Stun, out _, out _), Is.False);
        Assert.That(monster.IsControlLocked, Is.False);
    }

    [Test]
    public void Apply_FullResistanceRaisesImmuneFeedbackWithoutStoringEffect()
    {
        definition.statusResistance.poison = 1f;
        StatusEffectType? resistedType = null;
        monster.StatusEffects.EffectImmune += type => resistedType = type;

        bool applied = monster.ApplyStatusEffect(StatusEffectApplication.Poison(4f, 10f));

        Assert.That(applied, Is.False);
        Assert.That(resistedType, Is.EqualTo(StatusEffectType.Poison));
        Assert.That(monster.StatusEffects.TryGet(StatusEffectType.Poison, out _, out _), Is.False);
    }
}
