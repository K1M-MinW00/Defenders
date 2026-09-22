using UnityEngine;

[CreateAssetMenu(menuName = "Database/Game Icon Set")]
public class GameIconSetSO : ScriptableObject
{
    [Header("Resource")]
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite gemIcon;
    [SerializeField] private Sprite fuelIcon;

    [Header("Rarity Frame")]
    [SerializeField] private Sprite normalFrame;
    [SerializeField] private Sprite rareFrame;
    [SerializeField] private Sprite legendFrame;

    public Sprite Gold => goldIcon;
    public Sprite Gem => gemIcon;
    public Sprite Fuel => fuelIcon;
    public Sprite NormalRarityFrame => normalFrame;
    public Sprite RareRarityFrame => rareFrame;
    public Sprite LegendRarityFrame => legendFrame;

    public bool TryValidate(out string error)
    {
        if (goldIcon == null || gemIcon == null || fuelIcon == null)
        {
            error = "One or more resource icons are missing.";
            return false;
        }

        if (normalFrame == null || rareFrame == null || legendFrame == null)
        {
            error = "One or more rarity frames are missing.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
