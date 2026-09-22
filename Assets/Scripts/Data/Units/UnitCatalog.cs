using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UnitCatalog : IUnitCatalog
{
    private readonly Dictionary<string, UnitDataSO> unitsById;

    public UnitCatalog(IEnumerable<UnitDataSO> units)
    {
        if (units == null)
            throw new ArgumentNullException(nameof(units));

        unitsById = new Dictionary<string, UnitDataSO>(StringComparer.Ordinal);
        foreach (UnitDataSO unit in units)
        {
            if (unit == null)
                throw new ArgumentException("Unit catalog contains a null asset.", nameof(units));
            if (string.IsNullOrWhiteSpace(unit.unitId))
                throw new ArgumentException($"Unit ID is empty: {unit.name}", nameof(units));
            if (!unitsById.TryAdd(unit.unitId, unit))
                throw new ArgumentException($"Duplicate unit ID: {unit.unitId}", nameof(units));
        }

        if (unitsById.Count == 0)
            throw new ArgumentException("Unit catalog is empty.", nameof(units));
    }

    public UnitDataSO Get(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return null;

        unitsById.TryGetValue(unitId, out UnitDataSO unit);
        return unit;
    }

    public Sprite GetIcon(string unitId) => Get(unitId)?.icon;
    public IReadOnlyCollection<UnitDataSO> GetAll() => unitsById.Values;
}
