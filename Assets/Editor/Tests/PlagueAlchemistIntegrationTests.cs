using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class PlagueAlchemistIntegrationTests
{
    private const string UnitDataPath = "Assets/Resources/GameData/Units/Plague_Alchemist.asset";
    private const string UnitPrefabPath = "Assets/Prefabs/Units/Plague_Alchemist.prefab";
    private const string PoisonVialPrefabPath = "Assets/Prefabs/Projectile/PlagueAlchemist_PoisonVial.prefab";
    private const string ToxicFlaskPrefabPath = "Assets/Prefabs/Projectile/PlagueAlchemist_ToxicFlask.prefab";
    private const string ToxicPoolPrefabPath = "Assets/Prefabs/Skill_Effect/PlagueAlchemist_ToxicPool.prefab";
    private const string SkillClipPath = "Assets/Animations/Units/PlagueAlchemist/PlagueAlchemist_Skill.anim";

    [Test]
    public void UnitData_ConnectsLegendUnitPrefabAndBothSkills()
    {
        UnitDataSO data = AssetDatabase.LoadAssetAtPath<UnitDataSO>(UnitDataPath);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.unitId, Is.EqualTo("unit_plague_alchemist"));
        Assert.That(data.rarity, Is.EqualTo(Rarity.Legend));
        Assert.That(data.unitPrefab, Is.Not.Null);
        Assert.That(data.passiveSkill, Is.Not.Null);
        Assert.That(data.activeSkill, Is.Not.Null);
        Assert.That(data.activeSkill.onHitStatusEffect.enabled, Is.True);
        Assert.That(data.activeSkill.onHitStatusEffect.type, Is.EqualTo(StatusEffectType.Poison));
    }

    [Test]
    public void UnitPrefab_ConnectsArcingAttackAndToxicDistillation()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UnitPrefabPath);
        var attack = prefab.GetComponent<PlagueAlchemistProjectileAttack>();
        var active = prefab.GetComponent<PlagueAlchemist_ToxicDistillation_Skill>();

        Assert.That(prefab, Is.Not.Null);
        Assert.That(attack, Is.Not.Null);
        Assert.That(prefab.GetComponent<ProjectileUnitAttack>(), Is.Null);
        Assert.That(prefab.GetComponent<PlagueAlchemist_ToxicAccumulation>(), Is.Not.Null);
        Assert.That(active, Is.Not.Null);

        var attackData = new SerializedObject(attack);
        Assert.That(
            attackData.FindProperty("projectilePrefab").objectReferenceValue,
            Is.SameAs(AssetDatabase.LoadAssetAtPath<GameObject>(PoisonVialPrefabPath)
                .GetComponent<PoisonVialProjectile>()));

        var activeData = new SerializedObject(active);
        Assert.That(
            activeData.FindProperty("flaskPrefab").objectReferenceValue,
            Is.SameAs(AssetDatabase.LoadAssetAtPath<GameObject>(ToxicFlaskPrefabPath)
                .GetComponent<ToxicFlaskProjectile>()));
        Assert.That(
            activeData.FindProperty("poolPrefab").objectReferenceValue,
            Is.SameAs(AssetDatabase.LoadAssetAtPath<GameObject>(ToxicPoolPrefabPath)
                .GetComponent<ToxicPoolArea>()));
    }

    [Test]
    public void ProjectileAndPoolPrefabs_HaveAnimationAndRuntimeBehaviors()
    {
        GameObject vial = AssetDatabase.LoadAssetAtPath<GameObject>(PoisonVialPrefabPath);
        GameObject flask = AssetDatabase.LoadAssetAtPath<GameObject>(ToxicFlaskPrefabPath);
        GameObject pool = AssetDatabase.LoadAssetAtPath<GameObject>(ToxicPoolPrefabPath);

        Assert.That(vial.GetComponent<PoisonVialProjectile>(), Is.Not.Null);
        Assert.That(flask.GetComponent<ToxicFlaskProjectile>(), Is.Not.Null);
        Assert.That(pool.GetComponent<ToxicPoolArea>(), Is.Not.Null);
        Assert.That(vial.GetComponent<Animator>()?.runtimeAnimatorController, Is.Not.Null);
        Assert.That(flask.GetComponent<Animator>()?.runtimeAnimatorController, Is.Not.Null);
        Assert.That(pool.GetComponent<Animator>()?.runtimeAnimatorController, Is.Not.Null);
        Assert.That(vial.GetComponent<Poolable>(), Is.Not.Null);
        Assert.That(flask.GetComponent<Poolable>(), Is.Not.Null);
        Assert.That(pool.GetComponent<Poolable>(), Is.Not.Null);

        var poolSettings = new SerializedObject(pool.GetComponent<ToxicPoolArea>());
        Assert.That(pool.GetComponent<SpriteRenderer>().color.a, Is.EqualTo(0.45f).Within(0.001f));
        Assert.That(poolSettings.FindProperty("maximumOpacity").floatValue, Is.EqualTo(0.45f).Within(0.001f));
        Assert.That(poolSettings.FindProperty("fadeInDuration").floatValue, Is.EqualTo(0.15f).Within(0.001f));
        Assert.That(poolSettings.FindProperty("fadeOutDuration").floatValue, Is.EqualTo(0.4f).Within(0.001f));
    }

    [Test]
    public void SkillClip_ReleasesProjectileAndFinishesExactlyOnce()
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(SkillClipPath);
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);

        Assert.That(clip, Is.Not.Null);
        Assert.That(events.Count(item => item.functionName == nameof(UnitAnimationEvent.OnSkillApplyEvent)), Is.EqualTo(1));
        Assert.That(events.Count(item => item.functionName == nameof(UnitAnimationEvent.OnSkillFinishedEvent)), Is.EqualTo(1));
        Assert.That(events.Single(item => item.functionName == nameof(UnitAnimationEvent.OnSkillApplyEvent)).time, Is.EqualTo(0.5f).Within(0.001f));
    }
}
