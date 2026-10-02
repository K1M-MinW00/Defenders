using System.Collections;
using System.Collections.Generic;
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
    [Header("Animation")]
    [SerializeField, Min(0f)] private float spawnSettleDelay = 0.5f;
    [SerializeField, Min(0.05f)] private float absorbDuration = 0.28f;
    [SerializeField] private AnimationCurve absorbEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private const int MaxStar = 4;
    private readonly HashSet<UnitController> animatingUnits = new();

    public void BeginAutoFuse(UnitController changedUnit)
    {
        if (!IsValid(changedUnit) || roster == null || animatingUnits.Contains(changedUnit))
            return;

        animatingUnits.Add(changedUnit);
        changedUnit.SetInteractionLocked(true);
        StartCoroutine(AutoFuseRoutine(changedUnit));
    }

    private IEnumerator AutoFuseRoutine(UnitController changedUnit)
    {
        UnitController seed = changedUnit;
        DropSpawnView spawnView = seed.GetComponent<DropSpawnView>();
        while (spawnView != null && spawnView.IsPlaying && IsValid(seed))
            yield return null;

        if (spawnSettleDelay > 0f)
            yield return new WaitForSecondsRealtime(spawnSettleDelay);

        while (IsValid(seed) && seed.Star < MaxStar)
        {
            UnitController target = FindAvailableMatch(seed);
            if (target == null)
                break;

            animatingUnits.Add(target);
            target.SetInteractionLocked(true);

            UnitHUDController seedHud = seed.GetComponent<UnitHUDController>();
            seedHud?.SetTransitionHidden(true);
            seed.Movement?.Stop();

            Vector3 start = seed.transform.position;
            float elapsed = 0f;
            while (elapsed < absorbDuration && IsValid(seed) && IsValid(target))
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / absorbDuration);
                float eased = absorbEase != null ? absorbEase.Evaluate(normalized) : normalized;
                seed.transform.position = Vector3.LerpUnclamped(start, target.transform.position, eased);
                yield return null;
            }

            if (!IsValid(seed) || !IsValid(target) ||
                !seed.TryBeginRemoval(UnitRemovalReason.Fused))
            {
                seedHud?.SetTransitionHidden(false);
                ReleaseAnimationLock(target);
                break;
            }

            UnitController consumed = seed;
            target.ApplyStarUp();
            roster.Unregister(consumed);
            animatingUnits.Remove(consumed);
            consumed.ReturnToPool();

            seed = target;
            if (spawnSettleDelay > 0f)
                yield return new WaitForSecondsRealtime(spawnSettleDelay);
        }

        ReleaseAnimationLock(seed);
    }

    private UnitController FindAvailableMatch(UnitController seed)
    {
        IReadOnlyList<UnitController> units = roster.Units;
        for (int i = 0; i < units.Count; i++)
        {
            UnitController candidate = units[i];
            if (candidate == null || candidate == seed || animatingUnits.Contains(candidate))
                continue;

            if (IsValid(candidate) && candidate.UnitId == seed.UnitId && candidate.Star == seed.Star)
                return candidate;
        }

        return null;
    }

    private void ReleaseAnimationLock(UnitController unit)
    {
        if (unit == null)
            return;

        animatingUnits.Remove(unit);
        unit.SetInteractionLocked(false);
        unit.GetComponent<UnitHUDController>()?.SetTransitionHidden(false);
    }

    private static bool IsValid(UnitController unit)
    {
        return unit != null &&
               unit.gameObject.activeInHierarchy &&
               unit.UnitData != null &&
               unit.RuntimeState != UnitRuntimeState.Removing &&
               unit.RuntimeState != UnitRuntimeState.Despawned;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        foreach (UnitController unit in animatingUnits)
        {
            if (unit == null)
                continue;

            unit.SetInteractionLocked(false);
            unit.GetComponent<UnitHUDController>()?.SetTransitionHidden(false);
        }

        animatingUnits.Clear();
    }

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
