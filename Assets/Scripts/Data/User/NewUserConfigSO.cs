using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/New User Config")]
public class NewUserConfigSO : ScriptableObject
{
    [Header("Profile")]
    public int StartLevel = 1;
    public string DefaultProfileIconId = "unit_knight";
    [Min(0)] public int NicknameChangeGemCost = 500;

    [Header("Resource")]
    public int StartGold = 0;
    public int StartGem = 0;
    public int StartFuel = 100;
    public int MaxFuel = 100;

    [Header("Roster")]
    public List<string> DefaultOwnedUnitIds = new();

    public bool TryValidate(IUnitCatalog units, out string error)
    {
        if (units == null)
        {
            error = "Unit catalog is missing.";
            return false;
        }
        if (StartLevel <= 0)
        {
            error = "Start level must be positive.";
            return false;
        }
        if (StartGold < 0 || StartGem < 0 || StartFuel < 0 || MaxFuel <= 0 || StartFuel > MaxFuel)
        {
            error = "Starting resources are invalid.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(DefaultProfileIconId) || units.Get(DefaultProfileIconId) == null)
        {
            error = $"Default profile icon unit does not exist: {DefaultProfileIconId}";
            return false;
        }
        if (DefaultOwnedUnitIds == null || DefaultOwnedUnitIds.Count == 0)
        {
            error = "At least one default owned unit is required.";
            return false;
        }

        HashSet<string> uniqueIds = new();
        foreach (string unitId in DefaultOwnedUnitIds)
        {
            if (string.IsNullOrWhiteSpace(unitId) || units.Get(unitId) == null)
            {
                error = $"Default owned unit does not exist: {unitId}";
                return false;
            }
            if (!uniqueIds.Add(unitId))
            {
                error = $"Default owned unit is duplicated: {unitId}";
                return false;
            }
        }

        if (!uniqueIds.Contains(DefaultProfileIconId))
        {
            error = "Default profile icon must be one of the default owned units.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
