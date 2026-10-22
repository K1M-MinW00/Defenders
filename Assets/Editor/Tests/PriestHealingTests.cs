using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class PriestHealingTests
{
    private const string PriestPrefabPath = "Assets/Prefabs/Units/Priest.prefab";
    private const string PriestDataPath = "Assets/Resources/GameData/Units/Priest.asset";

    private GameObject ownerObject;
    private GameObject allyObject;
    private GameObject rosterObject;
    private UnitController owner;
    private UnitController ally;
    private UnitRoster roster;

    [SetUp]
    public void SetUp()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PriestPrefabPath);
        UnitDataSO data = AssetDatabase.LoadAssetAtPath<UnitDataSO>(PriestDataPath);
        Assert.That(prefab, Is.Not.Null);
        Assert.That(data, Is.Not.Null);

        rosterObject = new GameObject("Priest Healing Test Roster");
        roster = rosterObject.AddComponent<UnitRoster>();
        ownerObject = Object.Instantiate(prefab);
        allyObject = Object.Instantiate(prefab);
        owner = ownerObject.GetComponent<UnitController>();
        ally = allyObject.GetComponent<UnitController>();

        owner.BindCombatContext(null, roster, null);
        ally.BindCombatContext(null, roster, null);
        Assert.That(owner.Initialize(CreateInitData(data, promotion: 1)), Is.True);
        Assert.That(ally.Initialize(CreateInitData(data, promotion: 0)), Is.True);
        roster.Register(owner);
        roster.Register(ally);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(ownerObject);
        Object.DestroyImmediate(allyObject);
        Object.DestroyImmediate(rosterObject);
    }

    [Test]
    public void ActiveSkill_HealsMostInjuredAlly()
    {
        ally.Health.TakeDamage(100f);
        float before = ally.Health.CurrentHp;
        Priest_Heal_Skill skill = owner.GetComponent<Priest_Heal_Skill>();

        Assert.That(skill.TryBuildContext(out SkillExecutionContext context), Is.True);
        Assert.That(context.AllyTargets, Has.Count.EqualTo(1));
        Assert.That(context.AllyTargets[0], Is.SameAs(ally));

        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Skill prefab is not assigned|Skill owner or StagePoolManager is not available"));
        skill.OnSkillApply(context);

        Assert.That(ally.Health.CurrentHp, Is.GreaterThan(before));
    }

    [Test]
    public void PassiveSkill_HealsAfterConfiguredNumberOfRangedHits()
    {
        ally.Health.TakeDamage(100f);
        float before = ally.Health.CurrentHp;
        Priest_HealingEcho passive = owner.GetComponent<Priest_HealingEcho>();
        float damage = 10f;

        for (int i = 0; i < 4; i++)
            passive.OnAttackHit(null, ref damage);

        Assert.That(ally.Health.CurrentHp, Is.EqualTo(before));

        passive.OnAttackHit(null, ref damage);

        Assert.That(ally.Health.CurrentHp, Is.GreaterThan(before));
    }

    [Test]
    public void ActiveSkill_DoesNotBuildContextWhenEveryoneIsAtFullHealth()
    {
        Priest_Heal_Skill skill = owner.GetComponent<Priest_Heal_Skill>();

        Assert.That(skill.TryBuildContext(out _), Is.False);
    }

    private static StageUnitInitData CreateInitData(UnitDataSO data, int promotion)
    {
        return new StageUnitInitData(
            data,
            new UserUnitData
            {
                UnitId = "unit_priest",
                Level = 1,
                Promotion = promotion,
            });
    }
}
