# DOTween conventions

Use DOTween only inside presentation/view components. Gameplay services decide
when an effect starts, while views own its timing and visuals.

1. Store every running tween or sequence in a `Tween` field.
2. Kill the previous tween before replaying the same effect.
3. Bind every tween with `BindTo(owner)`. This kills it automatically when a
   pooled object or scene-owned view is disabled.
4. Also call `TweenLifecycle.Kill(ref tween)` from `OnDisable` when the view
   resets transform, scale, alpha, or other visual state.
5. Use scaled time by default. Pass `useUnscaledTime: true` only for UI that is
   intentionally allowed to animate while gameplay is paused.
6. Never make rewards, fusion results, purchases, or other gameplay state depend
   solely on an animation completion callback. Apply state safely, then present it.
7. Do not use `DOTween.KillAll`. A view may only cancel tweens that it owns.
8. Pooled objects must restore their baseline visual state after cancelling a tween.
