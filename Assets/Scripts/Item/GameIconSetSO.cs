using UnityEngine;

[CreateAssetMenu(menuName = "Database/Game Icon Set")]
public class GameIconSetSO : ScriptableObject
{
    [Header("Resource")]
    public Sprite GoldIcon;
    public Sprite GemIcon;
    public Sprite FuelIcon;

    [Header("Rarity Frame")]
    public Sprite NormalFrame;
    public Sprite RareFrame;
    public Sprite LegendFrame;
}
public enum Rarity
{
    Normal,
    Rare,
    Legend
}