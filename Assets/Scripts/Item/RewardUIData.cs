using UnityEngine;

public struct RewardUIData
{
    public Sprite Icon;
    public string Name;
    public int Count;

    public RewardUIData( Sprite icon, string name, int count)
    {
        Icon = icon;
        Name = name;
        Count = count;
    }
}