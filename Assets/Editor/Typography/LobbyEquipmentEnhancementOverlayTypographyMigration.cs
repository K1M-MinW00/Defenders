using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyEquipmentEnhancementOverlayTypographyMigration
{
    private const string OverlayPath = "LobbyScreenCanvas/UnitDetailPanel/EquipmentEnhancementOverlay";

    [MenuItem("Tools/Defenders/UI Typography/Apply Equipment Enhancement Overlay")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject overlay = FindByPath(scene, OverlayPath);
        if (overlay == null) throw new MissingReferenceException(OverlayPath);

        Configure(overlay, "ModalCard/Header/TitleText", 36f, 46f);
        Configure(overlay, "ModalCard/CloseButton/Text", 28f, 32f);
        Configure(overlay, "ModalCard/Confirm/Text", 27f, 32f);
        Configure(overlay, "ModalCard/Reset/Text", 25f, 32f);
        Configure(overlay, "ModalCard/Auto/Text", 25f, 32f);
        Configure(overlay, "ModalCard/EffectText", 25f, 32f);
        Configure(overlay, "ModalCard/ExpText", 20f, 25f);
        Configure(overlay, "ModalCard/GuideText", 25f, 32f);
        Configure(overlay, "ModalCard/LevelText", 32f, 42f);
        Configure(overlay, "ModalCard/NextLevelText", 30f, 42f);
        Configure(overlay, "ModalCard/Slot/LevelText", 22f, 28f);
        Configure(overlay, "ModalCard/Template/CountText", 20f, 25f);
        Configure(overlay, "ModalCard/Template/Minus/Text", 28f, 32f);
        Configure(overlay, "ModalCard/Template/SelectedCountText", 32f, 44f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyEquipmentEnhancementOverlayTypographyMigration] Configured 14 equipment enhancement overlay labels.");
    }

    private static void Configure(GameObject overlay, string relativePath, float minimum, float maximum)
    {
        Transform target = overlay.transform.Find(relativePath);
        TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
        if (text == null) throw new MissingReferenceException($"{OverlayPath}/{relativePath}");

        Undo.RecordObject(text, "Configure equipment enhancement typography");
        text.fontSize = maximum;
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        EditorUtility.SetDirty(text);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }
}
