using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyEquipmentChangeOverlayTypographyMigration
{
    private const string OverlayPath = "LobbyScreenCanvas/UnitDetailPanel/EquipmentChangeOverlay";

    [MenuItem("Tools/Defenders/UI Typography/Apply Equipment Change Overlay")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject overlay = FindByPath(scene, OverlayPath);
        if (overlay == null) throw new MissingReferenceException(OverlayPath);

        Configure(overlay, "ModalCard/Header/TitleText", 38f, 46f, TextWrappingModes.NoWrap);
        Configure(overlay, "ModalCard/GuideText", 24f, 30f, TextWrappingModes.Normal);
        Configure(overlay, "ModalCard/PreviewInfoText", 26f, 32f, TextWrappingModes.Normal);
        Configure(overlay, "ModalCard/Equip/Text", 26f, 32f, TextWrappingModes.NoWrap);
        Configure(overlay, "ModalCard/Preview/LevelText", 22f, 28f, TextWrappingModes.NoWrap);
        Configure(overlay, "ModalCard/Template/LevelText", 22f, 28f, TextWrappingModes.NoWrap);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyEquipmentChangeOverlayTypographyMigration] Configured 6 equipment change overlay labels.");
    }

    private static void Configure(
        GameObject overlay,
        string relativePath,
        float minimum,
        float maximum,
        TextWrappingModes wrapping)
    {
        Transform target = overlay.transform.Find(relativePath);
        TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
        if (text == null) throw new MissingReferenceException($"{OverlayPath}/{relativePath}");

        Undo.RecordObject(text, "Configure equipment change typography");
        text.fontSize = maximum;
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = wrapping;
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
