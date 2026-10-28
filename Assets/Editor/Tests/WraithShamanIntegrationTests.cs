using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class WraithShamanIntegrationTests
{
    private const string DataPath = "Assets/GameData/Monsters/Normal/Wraith_Shaman.asset";
    private const string PrefabPath = "Assets/Prefabs/Monsters/WraithShaman.prefab";
    private const string ProjectilePath = "Assets/Prefabs/Projectile/WraithShaman_SoulBolt.prefab";
    private const string ProjectileSpritePath = "Assets/Art/FirstParty/Runtime/Projectiles/WraithShaman/WraithShaman-SoulBolt-Flight.png";
    private const string ProjectileClipPath = "Assets/Animations/Projectiles/WraithShaman/WraithShaman_SoulBolt_Flight.anim";
    private const string AttackClipPath = "Assets/Animations/Monsters/WraithShaman/WraithShaman_Attack.anim";
    private const string Stage21Path = "Assets/Resources/GameData/Stages/Stage_2_1.asset";

    [Test]
    public void Definition_MatchesApprovedRangedPressureRole()
    {
        MonsterDataSO data = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(DataPath);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.TryValidate(out string error), Is.True, error);
        Assert.That(data.monsterId, Is.EqualTo("Wraith_Shaman"));
        Assert.That(data.Classification, Is.EqualTo(MonsterClassification.Normal));
        Assert.That(data.BaseMaxHp, Is.EqualTo(90f));
        Assert.That(data.BaseMoveSpeed, Is.EqualTo(2.5f));
        Assert.That(data.BaseAttackDamage, Is.EqualTo(12f));
        Assert.That(data.BaseAttackRange, Is.EqualTo(5.5f));
        Assert.That(data.BaseAttackPerSecond, Is.EqualTo(0.65f));
        Assert.That(data.ThreatCost, Is.EqualTo(3f));
    }

    [Test]
    public void Prefab_HasConfiguredAnimationDrivenRangedAttack()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject projectile = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePath);

        Assert.That(prefab, Is.Not.Null);
        Assert.That(projectile, Is.Not.Null);
        Assert.That(prefab.GetComponent<RangedMonsterAttack>(), Is.Not.Null);
        Assert.That(prefab.GetComponents<MonoBehaviour>().Count(component => component is ICombatAttack), Is.EqualTo(1));
        Assert.That(prefab.transform.Find("Body/FirePoint"), Is.Not.Null);
        Assert.That(prefab.GetComponentInChildren<MonsterAnimationEvent>(true), Is.Not.Null);
        Assert.That(projectile.GetComponent<ArrowProjectile>(), Is.Not.Null);
        Assert.That(projectile.GetComponent<Poolable>(), Is.Not.Null);
        Assert.That(projectile.GetComponent<Collider2D>()?.isTrigger, Is.True);
        Assert.That(projectile.GetComponent<SpriteRenderer>()?.sprite, Is.Not.Null);
        Assert.That(projectile.GetComponent<Animator>()?.runtimeAnimatorController, Is.Not.Null);

        var attack = new SerializedObject(prefab.GetComponent<RangedMonsterAttack>());
        Assert.That(
            attack.FindProperty("projectilePrefab").objectReferenceValue,
            Is.EqualTo(projectile.GetComponent<ArrowProjectile>()));
    }

    [Test]
    public void SoulBolt_UsesSixFrameLoopFromDedicatedSpriteSheet()
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(ProjectileSpritePath)
            .OfType<Sprite>()
            .ToArray();
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ProjectileClipPath);

        Assert.That(sprites, Has.Length.EqualTo(6));
        Assert.That(sprites.All(sprite => sprite.rect.width == 11f && sprite.rect.height == 12f), Is.True);
        Assert.That(clip, Is.Not.Null);
        Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime, Is.True);
    }

    [Test]
    public void AttackClip_FiresAndCompletesExactlyOnce()
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AttackClipPath);
        Assert.That(clip, Is.Not.Null);

        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        Assert.That(events.Count(item => item.functionName == "OnAttackHit"), Is.EqualTo(1));
        Assert.That(events.Count(item => item.functionName == "OnAttackFinished"), Is.EqualTo(1));
        Assert.That(events.Single(item => item.functionName == "OnAttackHit").time,
            Is.LessThan(events.Single(item => item.functionName == "OnAttackFinished").time));
    }

    [Test]
    public void Stage21_IntroducesShamanEarlyAndTestsBacklinePressureLater()
    {
        StageDataSO stage = AssetDatabase.LoadAssetAtPath<StageDataSO>(Stage21Path);
        MonsterDataSO shaman = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(DataPath);

        Assert.That(stage, Is.Not.Null);
        Assert.That(stage.TryValidate(out string error), Is.True, error);
        Assert.That(stage.waves[0].subWaves[0].spawnEntries
            .Single(entry => entry.data == shaman).count, Is.EqualTo(1));
        Assert.That(stage.waves[3].subWaves[1].spawnEntries
            .Single(entry => entry.data == shaman).count, Is.EqualTo(2));
    }

    [Test]
    public void MonsterInfoItem_CanBindNormalMonsterBeforeAwakeInitialization()
    {
        GameObject itemObject = new("Monster Info Item");
        StageMonsterInfoItemUI item = itemObject.AddComponent<StageMonsterInfoItemUI>();
        MonsterDataSO shaman = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(DataPath);

        try
        {
            Assert.DoesNotThrow(() => item.Bind(shaman));
        }
        finally
        {
            Object.DestroyImmediate(itemObject);
        }
    }
}
