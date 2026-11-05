# car16 wheel-detail visual review pack (standing reject: wheel detail/clearance)

Second pack beside `car16-mirrored-uv-pack/`, same three-panel structure. Generator:
`tools/carbody_wheel_pack.py` (read-only; PIL+numpy). Machine-readable: `metrics.json`.
Branch `mistral/carbody-livery`; local composition only - no run, no dispatch.
**In-tree evidence only; nothing here is cloud-validated and no acceptance is claimed.**

## Row-list provenance (honest scope note)

The car-assembly **wheel-surface list doc is NOT yet landed** (checked against
`outputs/mistral-orchestration/agents/*` this turn: no wheel-surface-list file exists).
The rows below are read from the car-assembly-EMITTED merged artifact instead:
`outputs/repair-20261029/cloud/formula16-density/outputs/cloud-game/car16-channel-0-both-assembly.json`
(`visibleMaterials`, `_BaseMap` rows matching WHEEL/TYRE/CALIPER; the steering-wheel
renderer excluded). Re-cut the pack when the dedicated list lands.

## The standing reject (documented, verbatim)

`FULL_F1_SF90_WHEEL_REVIEW.json` (run 41310403219, accepted:false, 8 closeups viewed,
compounds Soft/Medium/Hard/Wet):
- "correct red/yellow/white/blue identification; dry tread smooth / wet grooves visible; rubber curvature present."
- "Rings look raised/plastic rather than paint; lettering orientation needs refinement. Rims remain bright plain barrels with thin spokes, brakes mostly occluded."
Corroborating: `NEUTRAL_GRADE_W10_SF90_RESULTS.json` (run 41304169011): wet tread channels
+ rubber curvature restored under the neutral grade - overall wheel art still rejected;
`F1_SIDEWALL_SF90_MCL34_REVIEW.json` (run 41302933144): compound rings correct, rubber
black without readable detail. The reject is an ART-DETAIL finding, not a UV-orientation
defect (contrast with the mirrored pack).

## Row table (9 surface rows; images in `images/`)

| # | Renderer | Texture (UV0 island, wrapped) | In-game evidence | legA->legB change | Reading |
|---|---|---|---|---|---|
| 1 | LOD_A_TYRE_REAR_mm_tyre | NONE (geometry-authored rubber) | wheel-rl | 23 px | tyre is a lit mesh, no basemap |
| 2 | LOD_A_TYRE_mm_tyre | NONE (geometry-authored rubber) | wheel-fl | 967 px | same; front view change = strut fix in frame, not the tyre |
| 3 | LOD_A_WHEELGUARD_FRONT_LEFT_mm_chassis | car_chassis.png 1-2 texel sliver (u 0.8018..0.8022, v -0.1699..-0.1689, 9 uv) | wheel-fl | 967 px | the documented `sub-pixel-coverage-collapse` sliver |
| 4 | LOD_A_WHEELGUARD_FRONT_LEFT_mm_misc | 2019_f1_ferrari_sf90_misc.png (u 0.302..0.963, v -0.922..-0.059, 357 uv) | wheel-fl | 967 px | guard shell; hangs below the arch line (see clearance table) |
| 5 | LOD_A_WHEELGUARD_FRONT_RIGHT_mm_chassis | same sliver | wheel-fr | 1018 px | as row 3 |
| 6 | LOD_A_WHEELGUARD_FRONT_RIGHT_mm_misc | same island | wheel-fr | 1018 px | as row 4 |
| 7 | LOD_A_WHEEL_REAR_BLUR_mm | 2019_f1_ferrari_sf90_wheel.png subset island (u 0.371..0.987, v -0.987..-0.013, 1033 uv) | wheel-rl | 23 px | motion-variant rear rim |
| 8 | LOD_A_WHEEL_REAR_mm | 2019_f1_ferrari_sf90_wheel.png (near-full atlas, 2055 uv) | wheel-rl | 23 px | rear rim |
| 9 | LOD_A_WHEEL_mm | 2019_f1_ferrari_sf90_wheel.png (near-full atlas, 2136 uv) | wheel-fl | 967 px | front rim |

Each row image: SOURCE (authored island crop) + IN-GAME CURRENT from leg A and leg B.
`wheel-fl`/`wheel-fl-steered`/`wheel-fr` rows: the legA->legB pixel change is the staged
STRUT_UPPER_FRONT correction visible in frame (the strut rows' own evidence lives in the
mirrored pack); the wheel/tyre pixels themselves are IDENTICAL across the legs - the
rear-wheel views at 23 px (noise level) are the direct control.

## Clearance / fit numbers (merged records only; no new measurement)

| Quantity | Value | Source |
|---|---|---|
| car16 wheel radii (front/rear) | 0.2996 / 0.2999 m | DV hull manifest, run 41848766195 (`native-hull-summary.json`, `outputs/repair-20261029/dvride-41848766195/game-runtime-0-attempt-1/outputs/cloud-game/`) |
| car16 wheel-centre plane / tyre support plane | y -0.31 / y -0.61 (hull space) | same manifest; analysis in `carbody-livery-turn5b.md` (committed on this branch) |
| car16 wheel-guard hull extent | y -0.81 = **0.20 m below the tyre support plane** (hull) / -0.164 vs +0.040 (assembly-audit space = 0.204) | turn-5b measurement (both spaces agree) |
| DV static clearance -0.200 m | attributed to the guard hulls; guard-excluded "mounted bake" adopted upstream (`eb3890f`); the guard-excluded bake (run 41854714029 @`0de3123`) STILL failed the drive (~0.2 m in 30 s) - residual cause = suspension rest/anchor geometry (physics/assembly joint audit continues) | `REPAIR_REPORT.md` + turn-5b |
| car0 sanity reference | tyre bottom -0.65 vs chassis floor -0.60 (5 cm above the plane - sane) | same hull manifest |
| Swept-clearance fixture | `src/WheelFitAudit.cs`, `MYSIM_WHEEL_CLEARANCE_SWEEP=1`: steers {-28,-14,0,14,28 deg} x travel {-80,-40,+40,+80 mm} = 20 poses/wheel; per-pose clearance + penetrating-part names; sampling caveat (15 points/triangle, static cloud - complements, not replaces, in-motion evidence) | merged car-assembly report `car-report.md` sec 3 |
| Swept-clearance RUN | 41837315358 @`e17fdf5` completed (v2 poses); per-car `WHEEL_CLEARANCE_WORST` lines + `sweep.perWheel` worsts live in CI artifacts - **numbers not yet in-tree**; the joined assembly JSONs from the same run feed the P5 metrics | `EVIDENCE_INDEX.md` (Clearance sweep v2, completed) |
| F1 tyre/rim dimensions | measured radius/width/hub/offset per F1 car, "no guessed dimensional replacement" | `data/f1_wheel_profiles.json` + `F1_TYRE_DIMENSION_REFERENCE.json` + `F1_CUSTOM_WHEEL_SPEC.json` |

## What the reviewer is asked to judge (per row)

1. Wheel/tyre rows: does the in-game state still match the reject (plastic rings, plain
   barrels, occluded brakes)? The fix candidates are NOT in this pack - art side only:
   (a) accept as-is, (b) proceed with the PROPOSED custom wheel spec (labelled
   candidate-art, unaccepted), or (c) the open source-wheel presentation decision from
   the fleet captures (open turn-4 item). The pack gives the BEFORE evidence for any of
   the three; no AFTER exists yet for wheel detail.
2. Guard rows: the shells visibly hang below the arch line in the crops - consistent
   with the 0.20 m hull finding; geometry follow-up belongs to car-assembly/physics
   (their joint audit), the pack only documents the visible consequence.
3. Tyre rows: no texture bound - any "tread lettering" on the tyres comes from the
   geometry/masks, not the livery atlas; relevant to the "lettering orientation"
   reject item.

## Limits

- 960x540 crops: judge FORM and DETAIL PRESENCE, not sharpness/colour.
- The wheel-closeup views include neighbouring staged-fix content (struts); row-local
  changes are asserted per RENDERER (no wheel-texture candidates staged), not per pixel.
- The car-assembly wheel-surface list pending; the assembly-audit rows used here may
  differ from that list when it lands (re-cut planned).
- No acceptance claims; the reject stands until the reviewer decides.
