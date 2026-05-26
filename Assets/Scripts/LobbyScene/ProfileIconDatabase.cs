using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Profile Icon Database")]
public class ProfileIconDatabase : ScriptableObject
{
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private List<ProfileIconEntry> icons;

    private Dictionary<string, Sprite> iconMap;

    public Sprite GetIcon(string iconId)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(iconId))
            return defaultIcon;

        return iconMap.TryGetValue(iconId, out var sprite) ? sprite : defaultIcon;
    }

    private void EnsureInitialized()
    {
        if (iconMap != null)
            return;

        iconMap = new Dictionary<string, Sprite>();

        foreach (var icon in icons)
        {
            if (icon == null || string.IsNullOrEmpty(icon.IconId))
                continue;

            iconMap[icon.IconId] = icon.Sprite;
        }
    }
}

[Serializable]
public class ProfileIconEntry
{
    public string IconId;
    public Sprite Sprite;
}