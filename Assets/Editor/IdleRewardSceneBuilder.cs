#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class IdleRewardSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string PrefabPath = "Assets/Prefabs/UI/IdleRewardSlot.prefab";

    [InitializeOnLoadMethod]
    private static void BuildAfterImport()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isCompiling) return;
            try
            {
                IdleRewardSlotView slot = CreateSlotPrefab();
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (NeedsBuild(scene))
                {
                    GameObject existing = Find(scene, "IdleRewardPopup");
                    if (existing != null) Object.DestroyImmediate(existing);
                    BuildScene(scene, slot);
                }
            }
            catch (System.Exception e) { Debug.LogError($"[IdleRewardSceneBuilder] {e}"); }
        };
    }

    [MenuItem("Tools/Defenders/Build Idle Reward UI")]
    public static void Build()
    {
        IdleRewardSlotView slot = CreateSlotPrefab();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject existing = Find(scene, "IdleRewardPopup");
        if (existing != null) Object.DestroyImmediate(existing);
        BuildScene(scene, slot);
        AssetDatabase.SaveAssets();
    }

    private static void BuildScene(Scene scene, IdleRewardSlotView slotPrefab)
    {
        GameObject supplyObject = Find(scene, "Button_Supply");
        Button supply = supplyObject?.GetComponent<Button>();
        Transform main = supplyObject != null ? FindAncestor(supplyObject.transform, "Main_Panel") : null;
        Transform parent = main?.parent;
        if (supply == null || parent == null) throw new System.InvalidOperationException("Battle_Panel/Panel_Reward/Button_Supply or the parent outside Main_Panel was not found.");

        GameObject popup = Panel(parent, "IdleRewardPopup", new Color(.02f, .06f, .12f, .94f));
        Stretch((RectTransform)popup.transform);
        IdleRewardPanelView view = popup.AddComponent<IdleRewardPanelView>();

        GameObject window = Panel(popup.transform, "Window", new Color(.08f, .16f, .27f, 1f));
        Anchor((RectTransform)window.transform, new Vector2(.08f, .08f), new Vector2(.92f, .92f));
        TMP_Text title = Text(window.transform, "Title", "보급품", 54, TextAlignmentOptions.Center);
        Anchor(title.rectTransform, new Vector2(.05f, .86f), new Vector2(.95f, .98f));
        TMP_Text sector = Text(window.transform, "SectorDescription", "섹터 기준 시간당 보상", 26, TextAlignmentOptions.Center);
        sector.color = new Color(.35f, .95f, 1f); Anchor(sector.rectTransform, new Vector2(.05f, .77f), new Vector2(.95f, .85f));
        TMP_Text gold = Text(window.transform, "GoldHourly", "", 27, TextAlignmentOptions.Center);
        Anchor(gold.rectTransform, new Vector2(.05f, .7f), new Vector2(.5f, .77f));
        TMP_Text research = Text(window.transform, "ResearchHourly", "", 27, TextAlignmentOptions.Center);
        Anchor(research.rectTransform, new Vector2(.5f, .7f), new Vector2(.95f, .77f));
        TMP_Text current = Text(window.transform, "CurrentRewardTitle", "현재 보상", 40, TextAlignmentOptions.Center);
        current.color = new Color(.35f, .95f, 1f); Anchor(current.rectTransform, new Vector2(.05f, .61f), new Vector2(.95f, .69f));

        GameObject scrollObject = Panel(window.transform, "RewardScrollView", new Color(.02f, .08f, .16f, .6f));
        Anchor((RectTransform)scrollObject.transform, new Vector2(.06f, .28f), new Vector2(.94f, .61f));
        ScrollRect scroll = scrollObject.AddComponent<ScrollRect>(); scroll.horizontal = false;
        GameObject viewport = Panel(scrollObject.transform, "Viewport", Color.clear); Stretch((RectTransform)viewport.transform); viewport.AddComponent<RectMask2D>();
        RectTransform content = Child(viewport.transform, "Content"); content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
        GridLayoutGroup grid = content.gameObject.AddComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(135, 145); grid.spacing = new Vector2(12, 12); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 5; grid.childAlignment = TextAnchor.UpperCenter;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = (RectTransform)viewport.transform; scroll.content = content;

        TMP_Text time = Text(window.transform, "AccumulatedTime", "0시간 0분 / 최대 12시간", 26, TextAlignmentOptions.Center);
        Anchor(time.rectTransform, new Vector2(.05f, .2f), new Vector2(.95f, .27f));
        Button claim = ButtonObject(window.transform, "ClaimButton", "획득", new Color(.94f, .2f, .36f));
        Anchor((RectTransform)claim.transform, new Vector2(.32f, .06f), new Vector2(.68f, .18f));
        Button close = ButtonObject(window.transform, "CloseButton", "닫기", new Color(.18f, .3f, .48f));
        Anchor((RectTransform)close.transform, new Vector2(.73f, .06f), new Vector2(.92f, .16f));

        SerializedObject so = new(view);
        Set(so, "supplyButton", supply); Set(so, "closeButton", close); Set(so, "claimButton", claim); Set(so, "popup", popup);
        Set(so, "sectorDescriptionText", sector); Set(so, "goldHourlyText", gold); Set(so, "researchHourlyText", research); Set(so, "accumulatedTimeText", time);
        GameObject slotAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (slotAsset == null) throw new System.InvalidOperationException("Idle reward slot prefab could not be loaded.");
        Set(so, "rewardContent", content); Set(so, "rewardGrid", grid); Set(so, "rewardSlotPrefab", slotAsset); so.ApplyModifiedPropertiesWithoutUndo();
        view.SetRewardSlotPrefabEditor(slotAsset);
        EditorUtility.SetDirty(view);
        popup.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
    }

    private static IdleRewardSlotView CreateSlotPrefab()
    {
        GameObject root = new("IdleRewardSlot", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(IdleRewardSlotView));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(135, 145); root.GetComponent<Image>().color = new Color(.18f, .24f, .34f);
        Image icon = Panel(root.transform, "Icon", Color.white).GetComponent<Image>(); Anchor((RectTransform)icon.transform, new Vector2(.15f, .25f), new Vector2(.85f, .9f)); icon.preserveAspect = true;
        TMP_Text amount = Text(root.transform, "Amount", "0", 24, TextAlignmentOptions.Center); Anchor(amount.rectTransform, new Vector2(.02f, .02f), new Vector2(.98f, .25f));
        SerializedObject so = new(root.GetComponent<IdleRewardSlotView>()); Set(so, "icon", icon); Set(so, "amountText", amount); so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)?.GetComponent<IdleRewardSlotView>();
    }

    private static bool NeedsBuild(Scene scene)
    {
        GameObject popup = Find(scene, "IdleRewardPopup");
        if (popup == null) return true;
        IdleRewardPanelView view = popup.GetComponent<IdleRewardPanelView>();
        SerializedObject so = view != null ? new SerializedObject(view) : null;
        return so == null || so.FindProperty("rewardSlotPrefab")?.objectReferenceValue == null;
    }

    private static GameObject Find(Scene scene, string name) { foreach (GameObject root in scene.GetRootGameObjects()) { Transform found = Find(root.transform, name); if (found != null) return found.gameObject; } return null; }
    private static Transform Find(Transform root, string name) { foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t; return null; }
    private static Transform FindAncestor(Transform child, string name) { for (Transform t = child; t != null; t = t.parent) if (t.name == name) return t; return null; }
    private static RectTransform Child(Transform parent, string name) { GameObject go = new(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return (RectTransform)go.transform; }
    private static GameObject Panel(Transform parent, string name, Color color) { RectTransform rect = Child(parent, name); Image image = rect.gameObject.AddComponent<Image>(); image.color = color; return rect.gameObject; }
    private static TMP_Text Text(Transform parent, string name, string value, float size, TextAlignmentOptions align) { RectTransform rect = Child(parent, name); TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.alignment = align; text.color = Color.white; return text; }
    private static Button ButtonObject(Transform parent, string name, string label, Color color) { GameObject go = Panel(parent, name, color); Button button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); TMP_Text text = Text(go.transform, "Label", label, 30, TextAlignmentOptions.Center); Stretch(text.rectTransform); return button; }
    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max) { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    private static void Set(SerializedObject so, string name, Object value) { SerializedProperty property = so.FindProperty(name) ?? throw new System.InvalidOperationException($"Missing field: {name}"); property.objectReferenceValue = value; }
}
#endif
