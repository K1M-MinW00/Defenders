using UnityEngine;

public readonly struct FusionResult
{
    public static FusionResult None => new(null, 0);

    public UnitController FinalUnit { get; }
    public int FusionCount { get; }
    public bool Fused => FusionCount > 0;

    private FusionResult(UnitController finalUnit, int fusionCount)
    {
        FinalUnit = finalUnit;
        FusionCount = fusionCount;
    }

    public static FusionResult WithoutFusion(UnitController unit) => new(unit, 0);
    public static FusionResult Completed(UnitController unit, int count) => new(unit, count);
}

public class FusionService : MonoBehaviour
{
    [SerializeField] private UnitRoster roster;
    private const int MaxStar = 4;

    public FusionResult TryAutoFuse(UnitController changedUnit)
    {
        if (roster == null || changedUnit == null || changedUnit.UnitData == null)
            return FusionResult.None;

        UnitController seed = changedUnit;
        int fusionCount = 0;

        while (seed != null && seed.UnitData != null)
        {
            int star = seed.Star;

            if (star >= MaxStar)
                break;

            string unitId = seed.UnitId;

            UnitController other = roster.FindAny(unitId, star, exclude: seed);

            if (other == null)
                break;

            UnitController keep = other;
            UnitController consume = seed;

            if (!consume.TryBeginRemoval(UnitRemovalReason.Fused))
                break;

            keep.ApplyStarUp();
            roster.Unregister(consume);
            consume.ReturnToPool();
            seed = keep;
            fusionCount++;
        }

        return fusionCount > 0
            ? FusionResult.Completed(seed, fusionCount)
            : FusionResult.WithoutFusion(seed);
    }
}
