using NUnit.Framework;
using UnityEngine;

public sealed class StageCatalogTests
{
    [TestCase(1, 1)]
    [TestCase(1, 2)]
    [TestCase(1, 3)]
    public void StageResource_IsRegisteredAndValid(int sector, int stageNumber)
    {
        StageDataSO[] stages = Resources.LoadAll<StageDataSO>("GameData/Stages");
        StageCatalog catalog = new(stages);
        StageDataSO stage = catalog.Get(sector, stageNumber);

        Assert.That(stage, Is.Not.Null, $"Stage {sector}-{stageNumber} is not registered.");
        Assert.That(stage.TryValidate(out string error), Is.True, error);
    }

    [Test]
    public void Catalog_ResolvesStageBySectorAndStage()
    {
        StageDataSO stage = ScriptableObject.CreateInstance<StageDataSO>();
        stage.sector = 1;
        stage.stage = 2;

        try
        {
            StageCatalog catalog = new(new[] { stage });
            Assert.That(catalog.Get(1, 2), Is.SameAs(stage));
            Assert.That(catalog.Get(2, 1), Is.Null);
        }
        finally
        {
            Object.DestroyImmediate(stage);
        }
    }

    [Test]
    public void Catalog_RejectsDuplicateStageId()
    {
        StageDataSO first = ScriptableObject.CreateInstance<StageDataSO>();
        StageDataSO second = ScriptableObject.CreateInstance<StageDataSO>();
        first.sector = second.sector = 1;
        first.stage = second.stage = 1;

        try
        {
            Assert.Throws<System.ArgumentException>(() => new StageCatalog(new[] { first, second }));
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }
    }
}
