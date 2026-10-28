using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Keeps serialized gacha pools in sync with the canonical UnitDataSO catalog.
/// Designers configure rates and pickup units; pool membership is derived from rarity.
/// </summary>
public static class GachaPoolSynchronizer
{
    private const string UnitRoot = "Assets/Resources/GameData/Units";
    private static bool isSynchronizing;

    [MenuItem("Tools/Defenders/Game Data/Synchronize Gacha Pools")]
    public static void SynchronizeAll()
    {
        if (isSynchronizing)
            return;

        isSynchronizing = true;
        try
        {
            UnitDataSO[] units = AssetDatabase.FindAssets("t:UnitDataSO", new[] { UnitRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<UnitDataSO>)
                .Where(unit => unit != null && !string.IsNullOrWhiteSpace(unit.unitId))
                .OrderBy(unit => unit.unitId, StringComparer.Ordinal)
                .ToArray();

            foreach (string guid in AssetDatabase.FindAssets("t:GachaDataSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GachaDataSO banner = AssetDatabase.LoadAssetAtPath<GachaDataSO>(path);
                if (banner == null)
                    continue;

                var serializedBanner = new SerializedObject(banner);
                bool changed = false;
                changed |= ReplacePool(serializedBanner.FindProperty("normalPool"), units, Rarity.Normal);
                changed |= ReplacePool(serializedBanner.FindProperty("rarePool"), units, Rarity.Rare);
                changed |= ReplacePool(serializedBanner.FindProperty("epicPool"), units, Rarity.Epic);
                changed |= ReplacePool(serializedBanner.FindProperty("legendPool"), units, Rarity.Legend);

                if (changed)
                {
                    serializedBanner.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(banner);
                }
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            isSynchronizing = false;
        }
    }

    private static bool ReplacePool(SerializedProperty pool, IEnumerable<UnitDataSO> units, Rarity rarity)
    {
        UnitDataSO[] expected = units.Where(unit => unit.rarity == rarity).ToArray();
        if (pool.arraySize == expected.Length)
        {
            bool identical = true;
            for (int i = 0; i < expected.Length; i++)
                identical &= pool.GetArrayElementAtIndex(i).objectReferenceValue == expected[i];
            if (identical)
                return false;
        }

        pool.arraySize = expected.Length;
        for (int i = 0; i < expected.Length; i++)
            pool.GetArrayElementAtIndex(i).objectReferenceValue = expected[i];
        return true;
    }
}

public sealed class GachaPoolAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (ContainsUnitDataChange(importedAssets) || ContainsUnitDataChange(deletedAssets) ||
            ContainsUnitDataChange(movedAssets) || ContainsUnitDataChange(movedFromAssetPaths))
        {
            EditorApplication.delayCall += GachaPoolSynchronizer.SynchronizeAll;
        }
    }

    private static bool ContainsUnitDataChange(IEnumerable<string> paths)
    {
        return paths.Any(path => path.StartsWith(UnitRootPrefix, StringComparison.Ordinal) &&
                                 path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase));
    }

    private const string UnitRootPrefix = "Assets/Resources/GameData/Units/";
}
