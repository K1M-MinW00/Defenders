using DG.Tweening;
using UnityEngine;

/// <summary>
/// Project-wide DOTween lifetime conventions.
/// Every view tween must be linked to its owner and held in a field so it can
/// be replaced or cancelled explicitly before pooled objects are reused.
/// </summary>
public static class TweenLifecycle
{
    /// <summary>
    /// Links a tween to its owner and selects its time domain.
    /// UI feedback that must continue while the game is paused should pass true.
    /// </summary>
    public static T BindTo<T>(this T tween, Component owner, bool useUnscaledTime = false)
        where T : Tween
    {
        if (tween == null || owner == null)
            return tween;

        tween.SetUpdate(useUnscaledTime);
        tween.SetLink(owner.gameObject, LinkBehaviour.KillOnDisable);
        return tween;
    }

    /// <summary>
    /// Cancels a stored tween without completing callbacks and clears its handle.
    /// Call before replaying an effect and from OnDisable for pooled views.
    /// </summary>
    public static void Kill(ref Tween tween)
    {
        if (tween != null && tween.IsActive())
            tween.Kill(complete: false);

        tween = null;
    }
}
