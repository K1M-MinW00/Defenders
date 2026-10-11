#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [MenuItem("Tools/Defenders/Configure Stage Result Reward Scroll")]
    public static void ConfigureStageResultRewardScroll()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StageResultUI result = UnityEngine.Object.FindFirstObjectByType<StageResultUI>(FindObjectsInactive.Include);
        if (result == null)
            throw new InvalidOperationException("StageResultUI was not found in GameScene.");

        Transform scrollTransform = FindChild(result.transform, "ClearRewardScroll") ??
                                    FindChild(result.transform, "Reward_Panel");
        if (scrollTransform is not RectTransform viewport)
            throw new InvalidOperationException("Stage clear reward viewport was not found.");

        viewport.name = "ClearRewardScroll";
        HorizontalLayoutGroup oldLayout = viewport.GetComponent<HorizontalLayoutGroup>();
        if (oldLayout != null)
            UnityEngine.Object.DestroyImmediate(oldLayout);

        if (viewport.GetComponent<RectMask2D>() == null)
            viewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = viewport.GetComponent<ScrollRect>() ?? viewport.gameObject.AddComponent<ScrollRect>();

        for (int i = viewport.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(viewport.GetChild(i).gameObject);

        GameObject contentObject = new("RewardContent", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 0f);
        content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 0.5f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup layout = contentObject.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 20, 20);
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = true;
        scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;

        SerializedObject serialized = new(result);
        serialized.FindProperty("clearRewardContent").objectReferenceValue = content;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(result);
        EditorUtility.SetDirty(scroll);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageUIHierarchyOrganizer] Stage result reward scroll configured.");
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

    private static Transform FindChild(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        foreach (Transform child in root)
        {
            Transform found = FindChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
#endif
