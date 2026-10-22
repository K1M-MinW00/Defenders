using UnityEngine;

public readonly struct BossAbilityTarget
{
    public UnitController Unit { get; }
    public Vector3 Position { get; }
    public bool IsValid { get; }

    public BossAbilityTarget(UnitController unit, Vector3 position)
    {
        Unit = unit;
        Position = position;
        IsValid = unit != null;
    }
}
