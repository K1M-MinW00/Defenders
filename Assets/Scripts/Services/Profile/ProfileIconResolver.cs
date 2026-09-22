using System.Collections.Generic;
using UnityEngine;

public static class ProfileIconResolver
{
    public const string FallbackIconId = "unit_knight";

    public static string ConfiguredDefaultIconId
    {
        get
        {
            string configuredId = GameConfig.NewUserConfig?.DefaultProfileIconId;
            return string.IsNullOrWhiteSpace(configuredId) ? FallbackIconId : configuredId;
        }
    }

    public static string ResolveIconId(string iconId, UserRosterData roster)
    {
        if (IsSelectable(iconId, roster))
            return iconId;

        string configuredId = ConfiguredDefaultIconId;
        if (IsSelectable(configuredId, roster))
            return configuredId;

        if (roster?.OwnedUnits != null)
        {
            foreach (UserUnitData ownedUnit in roster.OwnedUnits)
            {
                if (ownedUnit != null && HasIcon(ownedUnit.UnitId))
                    return ownedUnit.UnitId;
            }
        }

        return HasIcon(configuredId) ? configuredId : FindFirstAvailableIconId();
    }

    public static Sprite ResolveIcon(string iconId, UserRosterData roster)
    {
        return GameConfig.Units.GetIcon(ResolveIconId(iconId, roster));
    }

    public static bool IsSelectable(string iconId, UserRosterData roster)
    {
        if (!HasIcon(iconId) || roster?.OwnedUnits == null)
            return false;

        foreach (UserUnitData ownedUnit in roster.OwnedUnits)
        {
            if (ownedUnit != null && ownedUnit.UnitId == iconId)
                return true;
        }

        return false;
    }

    private static bool HasIcon(string iconId)
    {
        return !string.IsNullOrWhiteSpace(iconId) && GameConfig.Units.GetIcon(iconId) != null;
    }

    private static string FindFirstAvailableIconId()
    {
        IReadOnlyCollection<UnitDataSO> units = GameConfig.Units.GetAll();
        if (units == null)
            return string.Empty;

        foreach (UnitDataSO unit in units)
        {
            if (unit != null && unit.icon != null)
                return unit.unitId;
        }

        return string.Empty;
    }
}
