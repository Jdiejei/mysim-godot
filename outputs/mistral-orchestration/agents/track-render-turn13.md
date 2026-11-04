# Track rendering — turn 13: vegetation-backdrop ("dark wall") source-bound diagnosis + tone candidate

Author: track-render specialist. Evidence: the run-41843204651 audits (the NEW schema with per-surface `worldOrigin`/`boundsSize` — Brands Hatch, track 0, the same treeline assets the reviewer's frames show), the source textures (measured), and the source tree. No global changes; no dispatch; fence files untouched.

## 1. Which assets the backdrop is (source-bound)

- **The wall itself: `org_brandshatch_tree_line_1.png`** (512×256, the ONLY treeline texture in the project) on **25 strip/block nodes** (`org_brandshatch_treeline_00..24`), materials AlphaScissor 0.35 + AlphaToCoverage + aniso + cull-off + PerPixel (audit rows).
- **Behind it: 462 tree/shrub/bush instances** across 14 textures (oak_a/c/d, pine_fine_a; base + `billboarded/*_type_[a-f]` variants).
- Geometry layout (NEW evidence from the audit's `worldOrigin`/`boundsSize`): these FBX nodes carry **identity transforms with all geometry baked into vertices** (every origin reads (0,0,0); sizes are pre-scale local units ×100):

| Asset class | Count | Per-instance size (m) | Total |
|---|---|---|---|
| Treeline strips/blocks | 25 | length 80-470, height 20-30, depth 10-460 (volumetric forest blocks, not paper strips) | **4.74 km of frontage ≈ the full 4.56 km circuit — a closed ring** |
| Trees/shrubs/bushes | 462 | height 0-30, footprint ~10 | scattered field behind the ring |

## 2. What makes it read as a wall (measured, four axes)

1. **Continuity (the big one):** 4.74 km of strip frontage = the entire circuit perimeter; there are no designed gaps — the horizon is walled by construction of the source layout.
2. **Height continuity:** 20-30 m tall along the whole ring — no skyline variation between segments (my breakup candidate's scale jitter targets exactly this).
3. **Opacity:** the treeline texture is 68.7 % solid alpha + 14.9 % mid-band; at the 0.35 scissor ~76 % of the strip area renders opaque — a near-solid canopy wall by texture design; only 16.4 % reads as holes.
4. **Colour:** measured source RGB — treeline (51,53,45), pine billboard solid (37,43,26), shrub/bush similar — intrinsically dark grey-green at ~20 % luminance against a bright sky = maximal silhouette contrast. The PBR PerPixel response adds nothing on the shaded side. This is the "dark" in "dark wall".
5. (Structural, NOT fixable at my layer:) the single-texture repetition (variant swap unsupported — no second treeline texture exists anywhere on disk, turn-3 finding) and the source placement density.

## 3. Proposal (two flag-gated candidates, both per-material/per-name, default = untouched; plus one correction)

1. **NEW — `MYSIM_FOREST_TONE=<multiplier>`** (`TrackMaterialBinder.cs`, `ForestTonePass`, the GrassBalance/RoadVariation pattern): raises `AlbedoColor` (which multiplies `AlbedoTexture`) on EXACTLY the vegetation-backdrop materials (`org_brandshatch_tree_line_*`, `org_tree_*`, `org_shrub_*`, `org_bush_*`) by the env value (first probe 1.35; clamped acceptance range 0-3; per-material dedup; `FOREST_TONE materials=N tone=...` log + `forest-tone-<layer>.json` audit when `MYSIM_BINDING_AUDIT` set). This is the measured lever for axis 4 (dark): treeline (51,53,45)→≈(69,71,61), pine (37,43,26)→≈(50,58,35) — still foliage-green, not washed. It cannot add colour variety the single texture lacks (structural, item 2.5).
2. **CORRECTED — `MYSIM_TREE_BREAKUP` scale-only (yaw removed this commit):** the audit's identity-transform/baked-vertex finding means a yaw would sweep each 80-470 m strip around the WORLD origin at multi-km radius (catastrophic displacement, not break-up). The pass now applies ONLY the deterministic per-segment height scale (0.94-1.06, AABB-bottom-pinned — the base sits at the local y=0 datum so the pin is exact); it keeps targeting axes 2 (height uniformity) + the hard top edge. The strip LENGTH variation (axis 1) stays un-addressable (geometry-baked).
3. **Validation for both (owner battery):** matched day0 pairs with `MYSIM_FOREST_TONE=1.35` and `MYSIM_TREE_BREAKUP=1` on/off (the fence-run camera family already frames the wall), plus one combined frame; acceptance = human visual: the horizon reads as separated trees/blocks with visible sky gaps and foliage-green (not black) tone; the wall silhouette no longer reads as one continuous band. Fixtures/labels: none of this asserts real-circuit vegetation; the source placement stays as-authored.

## 4. Commit

`src/TrackMaterialBinder.cs` (+37/−6: yaw removal + correction note in `TreeBreakupPass`; new `ForestTonePass` + call) and this file. Balance 61/61 + 288/288; `ForestTonePass` ×2 (call+def); no CJK; fence files untouched; no dispatch; default rendering unchanged (both passes return immediately without their env set).
