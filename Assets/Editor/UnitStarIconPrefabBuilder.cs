#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class UnitStarIconPrefabBuilder
{
    private const string HudPrefabPath = "Assets/Prefabs/UnitHUD_Canvas.prefab";
    private const string UnitPrefabPath = "Assets/Prefabs/Units/Unit_Base.prefab";

    [MenuItem("Tools/Defenders/Build Unit Star Icons")]
    public static void Build()
    {
        GameObject hudRoot = PrefabUtility.LoadPrefabContents(HudPrefabPath);
        try
        {
            Transform starTransform = FindDeep(hudRoot.transform, "Star_Text") ??
                                      FindDeep(hudRoot.transform, "StarIcon");
            if (starTransform == null)
                throw new System.InvalidOperationException("Unit HUD star object was not found.");

            starTransform.name = "StarIcon";
            TextMeshProUGUI text = starTransform.GetComponent<TextMeshProUGUI>();
            if (text != null)
                Object.DestroyImmediate(text);

            Image image = starTransform.GetComponent<Image>();
            if (image == null)
                image = starTransform.gameObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;

            UnitStarIconView view = starTransform.GetComponent<UnitStarIconView>();
            if (view == null)
                view = starTransform.gameObject.AddComponent<UnitStarIconView>();
            Configure(view, image);

            PrefabUtility.SaveAsPrefabAsset(hudRoot, HudPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(hudRoot);
        }

        GameObject unitRoot = PrefabUtility.LoadPrefabContents(UnitPrefabPath);
        try
        {
            UnitHUDController controller = unitRoot.GetComponent<UnitHUDController>();
            UnitStarIconView view = unitRoot.GetComponentInChildren<UnitStarIconView>(true);
            if (controller == null || view == null)
                throw new System.InvalidOperationException("Unit HUD controller or star icon was not found.");

            SerializedObject serialized = new(controller);
            serialized.FindProperty("starIcon").objectReferenceValue = view;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(unitRoot, UnitPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(unitRoot);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[UnitStarIconPrefabBuilder] Unit star text replaced with rank icons.");
    }

    private static void Configure(UnitStarIconView view, Image image)
    {
        SerializedObject serialized = new(view);
        serialized.FindProperty("targetImage").objectReferenceValue = image;
        SerializedProperty sprites = serialized.FindProperty("starSprites");
        sprites.arraySize = 4;
        for (int i = 0; i < 4; i++)
            sprites.GetArrayElementAtIndex(i).objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/UI/Generated/rank_{i + 1}.png");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        view.SetStar(1);
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null)
                return found;
        }

        return null;
    }
}
#endif
