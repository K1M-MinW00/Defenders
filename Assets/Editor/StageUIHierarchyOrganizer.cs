#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StageUIHierarchyOrganizer
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string RootName = "Stage UI Root";

    [MenuItem("Tools/Defenders/Organize Game Scene UI Components")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = FindSceneObject(scene, RootName);
        if (root == null)
            throw new InvalidOperationException($"{RootName} was not found in {ScenePath}.");

        GameObject phaseCoordinator = GetOrCreateChild(root.transform, "Stage UI Phase Coordinator");
        GameObject hudCanvas = RequireSceneObject(scene, "Stage HUD Canvas");
        GameObject prepareCanvas = RequireSceneObject(scene, "Stage Prepare Canvas");
        GameObject popupCanvas = RequireSceneObject(scene, "Stage Popup Canvas");

        var moves = new (Type type, GameObject destination)[]
        {
            (typeof(StagePhaseUIView), phaseCoordinator),
            (typeof(StageHudPresenter), hudCanvas),
            (typeof(StageWaveTrackUI), hudCanvas),
            (typeof(StageHpSummaryUI), hudCanvas),
            (typeof(StageTopControlUI), hudCanvas),
            (typeof(StagePrepareActionUI), prepareCanvas),
            (typeof(UnitDragActionUI), prepareCanvas),
            (typeof(StageResultUI), popupCanvas),
            (typeof(StagePauseUI), popupCanvas),
        };

        foreach ((Type type, GameObject destination) in moves)
            MoveComponent(scene, root, type, destination);

        ValidateRoot(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageUIHierarchyOrganizer] GameScene UI components were organized.");
    }

    private static void MoveComponent(Scene scene, GameObject sourceObject, Type type, GameObject destination)
    {
        Component source = sourceObject.GetComponent(type);
        if (source == null)
        {
            if (destination.GetComponent(type) == null)
                throw new InvalidOperationException($"{type.Name} was not found on either {sourceObject.name} or {destination.name}.");
            return;
        }

        Component existing = destination.GetComponent(type);
        if (existing != null)
            throw new InvalidOperationException($"{destination.name} already has {type.Name}; refusing to create a duplicate.");

        ComponentUtility.CopyComponent(source);
        if (!ComponentUtility.PasteComponentAsNew(destination))
            throw new InvalidOperationException($"Failed to move {type.Name} to {destination.name}.");

        Component moved = destination.GetComponent(type);
        ReplaceSceneReferences(scene, source, moved);
        UnityEngine.Object.DestroyImmediate(source);
        EditorUtility.SetDirty(destination);
    }

    private static void ReplaceSceneReferences(Scene scene, Component source, Component replacement)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null || component == source)
                    continue;

                SerializedObject serializedObject = new(component);
                SerializedProperty property = serializedObject.GetIterator();
                bool changed = false;

                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference ||
                        property.objectReferenceValue != source)
                        continue;

                    property.objectReferenceValue = replacement;
                    changed = true;
                }

                if (changed)
                {
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(component);
                }
            }
        }
    }

    private static void ValidateRoot(GameObject root)
    {
        Component[] components = root.GetComponents<Component>();
        var unexpected = new List<string>();
        foreach (Component component in components)
        {
            if (component is Transform || component is StageUIController)
                continue;
            unexpected.Add(component != null ? component.GetType().Name : "Missing Script");
        }

        if (unexpected.Count > 0)
            throw new InvalidOperationException($"Unexpected components remain on {RootName}: {string.Join(", ", unexpected)}");
    }

    private static GameObject GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.gameObject;

        GameObject child = new(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static GameObject RequireSceneObject(Scene scene, string name)
    {
        GameObject result = FindSceneObject(scene, name);
        return result != null
            ? result
            : throw new InvalidOperationException($"{name} was not found in {ScenePath}.");
    }

    private static GameObject FindSceneObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform candidate in transforms)
            {
                if (candidate.name == name)
                    return candidate.gameObject;
            }
        }

        return null;
    }
}
#endif
