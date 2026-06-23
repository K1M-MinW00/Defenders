using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/New User Config")]
public class NewUserConfigSO : ScriptableObject
{
    [Header("Profile")]
    public int StartLevel = 1;

    [Header("Resource")]
    public int StartGold = 0;
    public int StartGem = 0;
    public int StartFuel = 100;
    public int MaxFuel = 100;

    [Header("Roster")]
    public List<string> DefaultOwnedUnitIds = new();
}