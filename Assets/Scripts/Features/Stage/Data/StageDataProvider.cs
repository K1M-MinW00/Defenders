using UnityEngine;

public sealed class StageDataProvider
{
    private readonly IStageCatalog catalog;

    public StageDataProvider(IStageCatalog catalog)
    {
        this.catalog = catalog ?? throw new System.ArgumentNullException(nameof(catalog));
    }

    public StageDataSO Load(int sector, int stage)
    {
        StageDataSO data = catalog.Get(sector, stage);

        if (data == null)
            Debug.LogError($"StageDataSO not found in catalog: {sector}-{stage}");

        return data;
    }
}
