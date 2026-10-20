# Loading Mascot Follow-up

The loading screen intentionally has no progress bar. Scene loading progress is not presented because the Unity scene operation can appear to jump rather than advance smoothly.

## Deferred mascot work

- Select and approve the official game mascot artwork.
- Place the mascot visual under the centered `Spinner` anchor in `LoadingScene`.
- Replace the temporary dot spinner with the approved mascot.
- Add a looping DOTween presentation around the screen center:
  - 360-degree Z rotation using unscaled time.
  - A subtle scale or vertical bounce secondary motion.
  - Kill the tween through `TweenLifecycle` when the scene closes.
- Confirm the animation at the project reference resolution and common mobile aspect ratios.
- Keep loading tips visible and readable while the mascot animates.
