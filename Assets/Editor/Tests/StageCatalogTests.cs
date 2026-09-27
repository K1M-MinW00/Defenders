using NUnit.Framework;
using UnityEngine;

public sealed class StageCatalogTests
{
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
