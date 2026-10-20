using UnityEngine;

public static class LobbyLockVisualFactory
{
    public static GameObject Create(Transform parent)
    {
        GameObject root = new("LevelLockVisual", typeof(RectTransform));
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(42f, 52f);
        rootRect.SetAsLastSibling();

        CreatePart(rootRect, "ShackleTop", new Vector2(0f, 13f), new Vector2(30f, 8f));
        CreatePart(rootRect, "ShackleLeft", new Vector2(-11f, 5f), new Vector2(8f, 18f));
        CreatePart(rootRect, "ShackleRight", new Vector2(11f, 5f), new Vector2(8f, 18f));
        CreatePart(rootRect, "Body", new Vector2(0f, -10f), new Vector2(38f, 30f));
        return root;
    }

    private static void CreatePart(RectTransform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject part = new(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform rect = (RectTransform)part.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        UnityEngine.UI.Image image = part.GetComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        image.raycastTarget = false;
    }
}
