# Stage Balance Workflow

## Source of truth

`StageDataSO` is the runtime source of truth. CSV files are a normalized batch-edit format; import them back into Unity before testing or building.

Open `Tools > Defenders > Stage > Stage Balance Editor` to edit stages, inspect wave metrics, validate data, or exchange CSV files.

Monster runtime stats are calculated as:

`base monster stats × automatic stage progression × threat preset × stage modifier × wave modifier × spawn modifier`

The progression index is the zero-based position of a `StageDataSO` after sorting all stages by sector and stage. Player level, player XP, and future laboratory progression are not part of this calculation.

## Recommended iteration loop

1. Set reusable base stats and `Threat Cost` on each `MonsterDataSO`.
2. Pick a stage threat preset such as Balanced, Swarm, or Durable.
3. Compose waves with monster counts, spawn intervals, and spawn points.
4. Use stage modifiers for the whole stage, wave modifiers for pacing, and spawn modifiers only for exceptional groups.
5. Check total HP, total DPS, threat, ranged ratio, and warning messages in the editor.
6. Playtest, record clear/fail rates and clear times externally, and adjust the smallest applicable layer.

## CSV layout

- `stages.csv`: stage identity, threat preset, and stage modifiers.
- `waves.csv`: wave type and wave modifiers.
- `subwaves.csv`: sub-wave order and delay.
- `monster_spawns.csv`: monster ID, count, spawn point, cadence, and entry modifiers.

Rows reference stable IDs (`stage_id`, `wave_id`, `subwave_id`, and `monster_id`) rather than Unity GUIDs. Import validates every reference before changing assets and records the operation in Unity Undo.

Keep all four files together. Export a fresh set before adding many rows so headers and IDs remain valid.

## Initial campaign layout

| Stage | Map | Concept | Threat preset |
|---|---|---|---|
| 1-1 | Map_Stage_1 | Slime and orc fundamentals | Balanced |
| 1-2 | Map_Stage_1 | Numerous fast enemies | Swarm |
| 1-3 | Map_Stage_1 | Front line plus ranged pressure | Ranged |
| 1-4 | Map_Stage_1 | Armored enemies and sustained damage | Durable |
| 1-5 | Map_Stage_1 | Sector review with mixed elites | Elite |
| 2-1 | Map_Stage_2 | Fast melee breakthrough | Aggressive |
| 2-2 | Map_Stage_2 | Ranged siege formations | Ranged |
| 2-3 | Map_Stage_2 | Heavy armored formations | Durable |
| 2-4 | Map_Stage_2 | High-density rush waves | Swarm |
| 2-5 | Map_Stage_2 | Mixed elite finale | Elite |

Every stage contains five waves. Wave 3 is an elite wave and wave 5 is a boss wave. Earlier waves introduce the stage concept, while wave 4 combines its main threats before the boss test.
