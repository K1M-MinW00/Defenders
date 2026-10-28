---
name: character-concept-pipeline
description: Create new Defenders units or monsters through a concept-portrait approval gate before producing gameplay sprites. Use whenever the user asks to invent, design, draw, or animate a new unit, normal monster, elite, or boss for this project.
---

# Character Concept Pipeline

Use this workflow for every newly created unit or monster in the Defenders project.

## 1. Inspect project references

- Find the closest existing portrait, sprite, and animation references in the repository.
- Inspect actual images and their Unity metadata instead of relying on filenames alone.
- Use `Assets/Art/ThirdParty/RPG/Images/Portrait.png` as the primary portrait style reference unless the project has a newer approved portrait set.
- Before creating or revising any gameplay sprite, read [references/purchased-sprite-style.md](references/purchased-sprite-style.md). Treat the purchased, shadowless character sheets—not previously generated project art—as the primary gameplay-style authority.
- For monsters, also read `docs/MonsterAssetGenerationRules.md` before deciding scale, cell size, baseline, direction, or animation counts.

## 2. Define the concept

Write a short design lock before or alongside image generation. It must identify:

- role and size class;
- silhouette and body proportions;
- face, eye count, eye position, and expression;
- clothing, armor, weapons, props, and which hand holds each item;
- fixed palette and magic-effect colors;
- details that may be simplified at gameplay resolution;
- features that must never change between frames.

Do not use vague descriptions for asymmetrical details. State screen-left/screen-right or the character's left/right explicitly.

## 3. Generate only the concept portrait

- Produce one transparent-background, full-character SD concept portrait matching the project portrait style.
- Keep the entire silhouette and all important equipment visible.
- Do not create an idle sprite, sprite sheet, animation, prefab, or runtime integration during this stage.
- Save the approved-candidate image as `Assets/Art/Concepts/<CharacterName>/portrait_<character_name>.png`.
- Save the design lock beside it as `portrait_<character_name>.design.md`.

## 4. Stop for approval

Show the portrait and summarize the locked traits. Explicitly ask the user to approve it or request changes.

Do not proceed to gameplay sprites until the user clearly approves the concept. If the portrait changes, update the design lock and ask again.

## 5. Create exactly one Idle reference sprite

- Treat the approved portrait and its design lock as the authoritative references.
- After portrait approval, create exactly one neutral Idle sprite at the correct gameplay scale, cell size, baseline, and direction.
- Redraw the character directly for its native gameplay pixel bounds. Never obtain the deliverable by shrinking the approved high-resolution portrait or a detailed generated illustration.
- Use `#1F1A1A` as the dominant shared 1px outer-contour color, not as a mandatory color for every exposed edge pixel. Preserve hard integer pixels and binary alpha. Light-facing upper-left boundary segments may use a suitably dark material color when the closest purchased references do the same.
- Never apply a blanket post-process that recolors every exposed boundary pixel to `#1F1A1A`. Before approval, measure the four-neighbor exposed-boundary coverage of that exact color and compare it with the three closest purchased characters. The measured humanoid references use approximately 78–92%; unexplained 100% coverage is a rejection signal because it makes the new sprite read heavier than the purchased set.
- Choose three closest purchased characters and compare the candidate beside them in equal 100x100 cells at both 1x and nearest-neighbor 8x scale.
- Keep only details that remain readable at 1x. Replace required small equipment counts with stable 1–3px symbols; delete decorative buckles, stitches, reflections, folds, fingers, and pipes unless the closest purchased references carry equivalent detail at the same scale.
- This deliverable is a single-frame transparent PNG, not an Idle animation, sprite sheet, key-pose set, or animation preview.
- Save it as `Assets/Art/Concepts/<CharacterName>/<CharacterName>-Idle-Reference.png` and mark its design lock as awaiting Idle approval.
- Show the single Idle sprite at a nearest-neighbor enlarged preview when helpful, then explicitly ask the user to approve it or request changes.
- Do not generate any additional Idle frames, Move, Attack, Hurt, Die, effects, animation clips, controllers, prefabs, or runtime integration until the user clearly approves this exact single sprite.

## 6. Produce animations only after Idle approval

- When the user approves the single Idle sprite, mark it as the authoritative gameplay reference.
- Build the design-lock package from that approved sprite before making animation frames.
- Preserve the face, body proportions, silhouette, equipment geometry, handedness, palette, baseline, and scale in every frame.
- Animate poses by moving stable parts; never redraw the character as a different design.
- For subtle Idle motion, derive frames from the approved single Idle sprite whenever practical. Keep the same pixel matrix and use only deliberate 1–2px integer translations or minimal breathing edits; do not independently generate six near-identical frames, because outline and equipment drift reads as flicker at gameplay scale.
- Generate animation groups separately and validate each group before continuing.
- When a generated horizontal source sheet is segmented into frames, inspect every logical cell for pixels leaked from its left and right neighbors. Keep only the intended character/effect components for that frame; reject or clean any severed hand, weapon, clothing, or effect fragment before Unity import. Never assume equal-width source segmentation is clean merely because the final sheet dimensions are correct.

## 7. Integrate monster animations with the existing Unity architecture

After each approved monster sprite sheet is added to the project:

- Import it as `Sprite (2D and UI)` with `Sprite Mode: Multiple`, `Pixels Per Unit: 16`, Point filtering, no mipmaps, and uncompressed texture data.
- Before integration, measure the opaque pixel bounds of every frame and compare `opaque width / 16` and `opaque height / 16` against at least two existing monsters in the same size class. Match the visible world-space body size, not merely the logical cell size. Regenerate or rescale the artwork when weapons, flames, or empty padding make it materially larger or smaller than its peers.
- For Attack and Die sheets, never scale the whole sheet from the tallest weapon, raised arm, flame, or collapsed pose. Scale from a neutral/reference body frame so its body height matches the approved Idle frame within 1px, then allow action extremities to occupy additional space in the logical cell. Automatically reject an action sheet when any standing frame becomes shorter than the Idle reference because an effect or weapon determined the global scale.
- Lay out ordinary-monster source sheets in logical `100x100` frame cells, but register each Unity sprite rectangle from that cell's tight non-transparent bounds, matching the project's existing Automatic slicing convention. Do not leave every imported sprite rect at 100x100 when most of it is transparent.
- Preserve frame order and use consistent center pivots after tight slicing. Check animation playback for pivot jitter; if a motion needs a stable world anchor, calculate the custom pivot from the sprite's original logical-cell position rather than adding transparent padding.
- For bosses, apply the same tight-bound import rule inside the approved larger native logical cells.
- Give slices deterministic names in left-to-right frame order, such as `<MonsterName>_Idle_0`.
- Create one Unity `AnimationClip` per animation sheet and bind the frame sprites to the `Body` object's `SpriteRenderer.m_Sprite` curve.
- Keep Idle and Move looped. Keep Attack and Die non-looping unless the design explicitly requires otherwise.
- Reuse `Assets/Animations/Monster/Monster_Base.controller`. Create a monster-specific `AnimatorOverrideController` and override its `Monster_Idle`, `Monster_Move`, `Monster_Attack`, and `Monster_Die` clips with the matching approved clips.
- Preserve the `Monster_Base` prefab-variant pattern. Keep the inherited root components and the `Body` child. Assign the initial sprite and monster override controller on `Body`; do not replace the base prefab with an unrelated standalone hierarchy.
- Ensure `Body` retains `SpriteRenderer`, `Animator`, and `MonsterAnimationEvent`. Add the appropriate attack component to the prefab root (`MeleeAttack`, `RangedAttack`, or the project's matching specialized attack component) and fully configure its required references instead of leaving a nonfunctional placeholder.
- Do not invent projectile or attack configuration. If the required projectile, fire point, or specialized behavior has not been designed yet, defer that attack component until those inputs are available while keeping the prefab variant and animation controller ready.

For every Attack clip:

- Add `OnAttackHit` at the exact impact/projectile-release frame.
- Add `OnAttackFinished` near the end of the recovery, before the clip completes.
- Compare timing with existing monsters that have a similar attack cadence; never copy event times blindly when frame counts differ.
- Verify the prefab has `MonsterAnimationEvent` so both events can be received and forwarded to the configured attack component.

## 8. Consistency checks

Reject and regenerate any frame where:

- the face, skull, eye count, eye side, or expression structure changes;
- equipment swaps hands, bends into a different object, changes size materially, or disappears unintentionally;
- the body gains or loses parts;
- clothing, armor, or silhouette changes design;
- palette drifts beyond lighting or effect colors;
- scale, baseline, facing direction, or transparent margins violate the project rules;
- an ordinary monster is upscaled into a boss-sized cell instead of being drawn for its assigned cell size.
- its contour is materially darker/lighter than the purchased set, varies in thickness without a structural reason, or mechanically uses `#1F1A1A` on every exposed edge instead of matching the reference mixture of shared and material-dark contour pixels;
- it was produced by direct high-resolution downscaling and contains blended color noise, broken outlines, or unreadable micro-details;
- it only looks correct when enlarged and its role, facing direction, or primary equipment cannot be read at 1x.

For bosses, select the larger native cell size required by `docs/MonsterAssetGenerationRules.md`; never enlarge a 100x100 ordinary-monster drawing as the production asset.
