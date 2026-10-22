using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class MonsterClassificationTests
{
    [TestCase(MonsterClassification.Normal, MonsterVisualScalePolicy.NormalScale)]
    [TestCase(MonsterClassification.Elite, MonsterVisualScalePolicy.EliteScale)]
    [TestCase(MonsterClassification.Boss, MonsterVisualScalePolicy.BossScale)]
    public void DefaultVisualScale_MatchesClassification(
        MonsterClassification classification,
        float expectedScale)
    {
        Assert.That(MonsterVisualScalePolicy.GetDefaultScale(classification), Is.EqualTo(expectedScale));
    }

    [Test]
    public void BossCandidates_ReuseSourcePrefabAndUseBossScale()
    {
        string[] sourceNames = { "Elite_Orc", "Armored_Orc", "Armored_Skeleton", "WereWolf" };
        foreach (string sourceName in sourceNames)
        {
            MonsterDataSO source = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(
                $"Assets/GameData/Monsters/{sourceName}.asset");
            MonsterDataSO boss = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(
                $"Assets/GameData/Monsters/Bosses/Boss_{sourceName}.asset");

            Assert.That(boss, Is.Not.Null, $"Boss candidate is missing: {sourceName}");
            Assert.That(boss.prefab, Is.EqualTo(source.prefab), $"Prefab should be reused: {sourceName}");
            Assert.That(boss.Classification, Is.EqualTo(MonsterClassification.Boss));
            Assert.That(boss.VisualScale, Is.EqualTo(MonsterVisualScalePolicy.BossScale));
            Assert.That(boss.BossDefinition, Is.Not.Null);
            Assert.That(boss.TryValidate(out string error), Is.True, error);
        }
    }

    [Test]
    public void PresentationScale_ChangesOnlyModelViewTransform()
    {
        GameObject root = new("Monster");
        GameObject model = new("Model");
        model.transform.SetParent(root.transform);
        model.AddComponent<ModelView>();
        MonsterPresentationController presentation = root.AddComponent<MonsterPresentationController>();

        presentation.Initialize();
        presentation.ApplyVisualScale(MonsterVisualScalePolicy.BossScale);

        Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(model.transform.localScale, Is.EqualTo(Vector3.one * MonsterVisualScalePolicy.BossScale));

        Object.DestroyImmediate(root);
    }
}
