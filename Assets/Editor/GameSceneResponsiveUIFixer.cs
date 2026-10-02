#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameSceneResponsiveUIFixer
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private static readonly Vector2 ReferenceResolution = new(1080f, 1920f);

    [MenuItem("Tools/Defenders/Fix Game Scene Responsive UI")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        ConfigureScreenCanvases();
        ConfigureUnitInfoPanel();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneResponsiveUIFixer] Responsive UI settings applied.");
    }

    private static void ConfigureScreenCanvases()
    {
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
                continue;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EditorUtility.SetDirty(scaler);
        }
    }

    private static void ConfigureUnitInfoPanel()
    {
        GameObject root = FindSceneObject("UI_StageUnitInfoPanel");
        if (root == null)
            throw new System.InvalidOperationException("UI_StageUnitInfoPanel was not found.");

        root.GetComponent<RectTransform>().localScale = Vector3.one;

        RectTransform panel = RequireRect(root.transform, "Panel");
        panel.anchorMin = new Vector2(0.02f, 1f);
        panel.anchorMax = new Vector2(0.98f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(0f, -24f);
        panel.sizeDelta = new Vector2(0f, 430f);

        LayoutElement panelElement = panel.GetComponent<LayoutElement>();
        if (panelElement != null)
            Object.DestroyImmediate(panelElement);

        VerticalLayoutGroup panelLayout = panel.GetComponent<VerticalLayoutGroup>();
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        RectTransform header = RequireRect(panel, "Header");
        ConfigureHeight(header, 56f, 64f, 0f);

        TMP_Text unitName = header.Find("UnitName")?.GetComponent<TMP_Text>();
        if (unitName != null)
        {
            unitName.enableAutoSizing = true;
            unitName.fontSizeMin = 20f;
            unitName.fontSizeMax = 31f;
            unitName.overflowMode = TextOverflowModes.Ellipsis;
        }

        RectTransform content = RequireRect(panel, "Content");
        ConfigureHeight(content, 0f, 326f, 1f);
        HorizontalLayoutGroup contentLayout = content.GetComponent<HorizontalLayoutGroup>();
        contentLayout.childControlWidth = false;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = true;

        ConfigurePortrait(content);
        ConfigureSkillCard(content, "Active");
        ConfigureSkillCard(content, "Passive");

        StageUnitInfoPanel infoPanel = root.GetComponent<StageUnitInfoPanel>();
        SerializedObject serializedPanel = new(infoPanel);
        serializedPanel.FindProperty("shownAnchoredPosition").vector2Value = new Vector2(0f, -24f);
        serializedPanel.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(root);
    }

    private static void ConfigurePortrait(RectTransform content)
    {
        RectTransform portrait = RequireRect(content, "Portrait");
        portrait.sizeDelta = new Vector2(184f, 326f);
        RemoveLayoutElement(portrait);
    }

    private static void ConfigureSkillCard(RectTransform content, string name)
    {
        RectTransform card = RequireRect(content, name);
        card.sizeDelta = new Vector2(396f, 326f);
        RemoveLayoutElement(card);

        VerticalLayoutGroup layout = card.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text skillName = card.Find("SkillName")?.GetComponent<TMP_Text>();
        if (skillName != null)
        {
            skillName.enableAutoSizing = true;
            skillName.fontSizeMin = 14f;
            skillName.fontSizeMax = 21f;
            skillName.overflowMode = TextOverflowModes.Ellipsis;
        }

        TMP_Text description = card.Find("Description")?.GetComponent<TMP_Text>();
        if (description == null)
            return;

        description.enableAutoSizing = true;
        description.fontSizeMin = 12f;
        description.fontSizeMax = 17f;
        description.textWrappingMode = TextWrappingModes.Normal;
        description.overflowMode = TextOverflowModes.Ellipsis;

        LayoutElement descriptionElement = GetOrAddLayoutElement(description.rectTransform);
        descriptionElement.minHeight = 60f;
        descriptionElement.preferredHeight = 110f;
        descriptionElement.flexibleHeight = 1f;
    }

    private static void ConfigureHeight(RectTransform rect, float min, float preferred, float flexible)
    {
        LayoutElement layout = GetOrAddLayoutElement(rect);
        layout.minHeight = min;
        layout.preferredHeight = preferred;
        layout.flexibleHeight = flexible;
    }

    private static LayoutElement GetOrAddLayoutElement(RectTransform rect)
    {
        LayoutElement layout = rect.GetComponent<LayoutElement>();
        return layout != null ? layout : rect.gameObject.AddComponent<LayoutElement>();
    }

    private static void RemoveLayoutElement(RectTransform rect)
    {
        LayoutElement layout = rect.GetComponent<LayoutElement>();
        if (layout != null)
            Object.DestroyImmediate(layout);
    }

    private static RectTransform RequireRect(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child == null || child is not RectTransform rect)
            throw new System.InvalidOperationException($"{parent.name}/{childName} was not found.");
        return rect;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == objectName)
                    return child.gameObject;
            }
        }

        return null;
    }
}
#endif
