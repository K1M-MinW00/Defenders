# Asset organization

## GameData boundary

- `Assets/Resources/GameData`: runtime bootstrap data loaded by a `Resources.Load` or
  `Resources.LoadAll` string path. Moving an asset here requires updating its load path.
- `Assets/GameData`: ordinary ScriptableObjects referenced through serialized GUIDs and
  editor tooling. Organize these by gameplay domain (`Units`, `Monsters`, `Economy`, etc.).
- New data should use a serialized reference (or Addressables when appropriate) instead of
  being added to `Resources` merely for convenience.
- Gacha banners live in `Assets/GameData/Economy/Gacha/Banners`. Their rarity pools are
  generated automatically from `Assets/Resources/GameData/Units`; designers only configure
  rates, costs, and the special-banner pickup unit.

## Art boundary

- `Assets/Art/FirstParty/Concepts`: design references; never use these directly in gameplay.
- `Assets/Art/FirstParty/Runtime`: project-owned/generated sprites used by the game.
- `Assets/Art/ThirdParty`: purchased/vendor source art. Character source variants stay in
  the vendor tree; animation controllers and clips do not.
- `Assets/Art/UI`: runtime UI images grouped by screen or feature. Shared UI goes in
  `Common`.
- `Assets/Art/UI/LegacyAtlases`: still-referenced TMP sprite atlases. Keep these until all
  numeric `<sprite=N>` markup has been replaced; new UI should use named sprite references
  or ordinary serialized `Image.sprite` fields.
- `Assets/Animations/Effects/<Character>`: character VFX clips and controllers. Use the
  suffix `_VFX`; do not use the ambiguous name `Aura` for attack or skill effects.

## Purchased character sprites

- Runtime character clips use the `Split Effects` variant so baked projectile frames are
  not mixed with spawned projectile GameObjects.
- Imported gameplay sprite sheets use 16 PPU, Point filtering, no mipmaps, Uncompressed
  texture compression, Clamp wrapping, and Full Rect meshes.
