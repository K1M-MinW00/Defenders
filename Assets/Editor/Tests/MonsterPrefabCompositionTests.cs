using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class MonsterPrefabCompositionTests
{
    private const string MonsterPrefabDirectory = "Assets/Prefabs/Monsters";

    [Test]
    public void AllMonsterPrefabs_HaveRequiredRuntimeComposition()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { MonsterPrefabDirectory });
        Assert.That(prefabGuids, Is.Not.Empty, "Monster prefab directory is empty.");

        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.That(prefab, Is.Not.Null, $"Failed to load monster prefab: {path}");
            Assert.That(prefab.GetComponent<MonsterController>(), Is.Not.Null, $"MonsterController is missing: {path}");
            Assert.That(prefab.GetComponent<MonsterFSMController>(), Is.Not.Null, $"MonsterFSMController is missing: {path}");
            Assert.That(prefab.GetComponent<MonsterMovementController>(), Is.Not.Null, $"MonsterMovementController is missing: {path}");
            Assert.That(prefab.GetComponent<MonsterCombatController>(), Is.Not.Null, $"MonsterCombatController is missing: {path}");
            Assert.That(prefab.GetComponent<MonsterPresentationController>(), Is.Not.Null, $"MonsterPresentationController is missing: {path}");
            Assert.That(prefab.GetComponent<MonsterTargetingController>(), Is.Not.Null, $"MonsterTargetingController is missing: {path}");
            Assert.That(prefab.GetComponent<MonsterHealth>(), Is.Not.Null, $"MonsterHealth is missing: {path}");
            Assert.That(prefab.GetComponent<UnityEngine.AI.NavMeshAgent>(), Is.Not.Null, $"NavMeshAgent is missing: {path}");
            Assert.That(prefab.GetComponentInChildren<ModelView>(true), Is.Not.Null, $"ModelView is missing: {path}");

            if (!path.EndsWith("Monster_Base.prefab"))
            {
                int attackCount = prefab.GetComponents<MonoBehaviour>().Count(component => component is ICombatAttack);
                Assert.That(attackCount, Is.EqualTo(1), $"Expected exactly one attack behavior: {path}");
            }

            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                Assert.That(
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject),
                    Is.Zero,
                    $"Missing script found at {path}/{GetHierarchyPath(child)}");
            }
        }
    }

    [Test]
    public void MonsterVariants_KeepMonsterBaseAsTheirSource()
    {
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            Path.Combine(MonsterPrefabDirectory, "Monster_Base.prefab").Replace('\\', '/'));
        Assert.That(basePrefab, Is.Not.Null);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { MonsterPrefabDirectory });
        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == basePrefab)
                continue;

            Assert.That(PrefabUtility.GetPrefabAssetType(prefab), Is.EqualTo(PrefabAssetType.Variant),
                $"Expected a Monster_Base variant: {path}");
            Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(prefab), Is.EqualTo(basePrefab),
                $"Variant source changed: {path}");
        }
    }

    private static string GetHierarchyPath(Transform target)
    {
        string path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = $"{target.name}/{path}";
        }

        return path;
    }
}
