using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class BossInfoPreviewCapture
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string BossPath = "Assets/GameData/Monsters/Bosses/Boss_Armored_Orc.asset";

    [MenuItem("Tools/Game/Bosses/Capture Monster Info Preview")]
    public static void Capture()
    {
        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        StageMonsterInfoItemUI template = FindTemplate();
        MonsterDataSO boss = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(BossPath);
        if (template == null || boss == null)
            throw new System.InvalidOperationException("Monster info template or preview boss was not found.");

        Directory.CreateDirectory("Logs");
        CaptureLocale(template, boss, "ko", "Logs/boss-info-preview-ko.png");
        CaptureLocale(template, boss, "en", "Logs/boss-info-preview-en.png");
        Debug.Log("Boss monster info previews were written to Logs.");
    }

    private static void CaptureLocale(
        StageMonsterInfoItemUI template,
        MonsterDataSO boss,
        string localeCode,
        string outputPath)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        GameObject previewRoot = new("Boss Info Preview Root");
        Camera camera = previewRoot.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.1f, 0.14f, 1f);
        camera.orthographic = true;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        GameObject canvasObject = new("Preview Canvas", typeof(RectTransform));
        canvasObject.transform.SetParent(previewRoot.transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 600f);
        scaler.matchWidthOrHeight = 0.5f;

        StageMonsterInfoItemUI item = Object.Instantiate(template, canvasObject.transform);
        item.name = $"Boss Info Preview {localeCode}";
        item.gameObject.SetActive(true);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = itemRect.anchorMax = itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.sizeDelta = new Vector2(960f, 300f);
        MethodInfo awake = typeof(StageMonsterInfoItemUI).GetMethod(
            "Awake",
            BindingFlags.Instance | BindingFlags.NonPublic);
        awake?.Invoke(item, null);
        item.Bind(boss);

        RenderTexture target = new(1080, 600, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = target;
        Canvas.ForceUpdateCanvases();
        camera.Render();

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D image = new(1080, 600, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
        image.Apply();
        File.WriteAllBytes(outputPath, image.EncodeToPNG());
        RenderTexture.active = previous;
        camera.targetTexture = null;

        Object.DestroyImmediate(image);
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(previewRoot);
    }

    private static StageMonsterInfoItemUI FindTemplate()
    {
        foreach (StageMonsterInfoItemUI candidate in Object.FindObjectsByType<StageMonsterInfoItemUI>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (candidate.name == "Monster Info Item Template")
                return candidate;
        }
        return null;
    }
}
