using UnityEngine;

public static class LoadingTipCatalog
{
    private static readonly string[] TipKeys =
    {
        "loading.tip.1", "loading.tip.2", "loading.tip.3",
        "loading.tip.4", "loading.tip.5", "loading.tip.6",
    };

    public static string GetRandomTip()
    {
        string key = TipKeys[Random.Range(0, TipKeys.Length)];
        return LocalizationService.Get(LocalizationTables.Tutorial, key);
    }
}
