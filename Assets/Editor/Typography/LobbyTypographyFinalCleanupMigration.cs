using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyTypographyFinalCleanupMigration
{
    [MenuItem("Tools/Defenders/UI Typography/Apply Lobby Final Cleanup")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        TMP_Text[] texts = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<TMP_Text>(true)).ToArray();
        int configured = 0;
        foreach (TMP_Text text in texts)
        {
            string path = GetPath(text.transform);
            if (!TryResolve(path, out float minimum, out float maximum, out TextWrappingModes wrapping))
                continue;

            Undo.RecordObject(text, "Configure final lobby typography");
            text.fontSize = maximum;
            text.enableAutoSizing = true;
            text.fontSizeMin = minimum;
            text.fontSizeMax = maximum;
            text.textWrappingMode = wrapping;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
            configured++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyTypographyFinalCleanupMigration] Configured {configured} remaining lobby labels.");
    }

    private static bool TryResolve(
        string path,
        out float minimum,
        out float maximum,
        out TextWrappingModes wrapping)
    {
        minimum = 18f;
        maximum = 28f;
        wrapping = TextWrappingModes.NoWrap;

        if (path.Contains("/EquipmentDetailOverlay/"))
        {
            if (path.EndsWith("/Header/Text")) (minimum, maximum) = (36f, 46f);
            else if (path.EndsWith("/ItemNameText")) (minimum, maximum) = (30f, 42f);
            else if (path.EndsWith("/RarityText")) (minimum, maximum) = (24f, 32f);
            else if (path.EndsWith("/LevelText")) (minimum, maximum) = (22f, 28f);
            else if (path.EndsWith("/BaseEffect/EffectText")) (minimum, maximum) = (28f, 36f);
            else if (path.EndsWith("/BonusEffect/EffectText")) (minimum, maximum) = (22f, 28f);
            else if (path.EndsWith("/DescriptionText"))
            {
                (minimum, maximum) = (22f, 29f);
                wrapping = TextWrappingModes.Normal;
            }
            else if (path.EndsWith("/LeftActionButton/Text") || path.EndsWith("/PrimaryActionButton/Text") || path.EndsWith("/RightActionButton/Text"))
                (minimum, maximum) = (24f, 32f);
            else return false;
            return true;
        }

        if (path.Contains("/EquipmentRewardOverlay/"))
        {
            if (path.EndsWith("/TitleText")) (minimum, maximum) = (38f, 52f);
            else if (path.EndsWith("/HintText")) (minimum, maximum) = (24f, 32f);
            else return false;
            return true;
        }

        if (path.Contains("/ExitConfirmationOverlay/"))
        {
            if (path.EndsWith("/Header/Text")) (minimum, maximum) = (34f, 44f);
            else if (path.EndsWith("/MessageText")) (minimum, maximum) = (26f, 34f);
            else if (path.EndsWith("/ConfirmButton/Text") || path.EndsWith("/CancelButton/Text")) (minimum, maximum) = (26f, 34f);
            else return false;
            return true;
        }

        if (path.EndsWith("/BattlePanel/PanelSector/TextGroupWave/TextBestText")) (minimum, maximum) = (18f, 22f);
        else if (path.EndsWith("/BattlePanel/PanelSector/TextGroupWave/TextWaveText")) (minimum, maximum) = (16f, 20f);
        else if (path.EndsWith("/BattlePanel/PanelSector/TextStageText")) (minimum, maximum) = (48f, 70f);
        else if (path.EndsWith("/ShopNotificationBadge/Text")) (minimum, maximum) = (20f, 24f);
        else if (path.EndsWith("/UiFeedbackToast/MessageText"))
        {
            (minimum, maximum) = (20f, 28f);
            wrapping = TextWrappingModes.Normal;
        }
        else return false;
        return true;
    }

    private static string GetPath(Transform transform)
    {
        System.Collections.Generic.Stack<string> parts = new();
        while (transform != null) { parts.Push(transform.name); transform = transform.parent; }
        return string.Join("/", parts);
    }
}
