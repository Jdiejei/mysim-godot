# track0 (Brands Hatch Indy) standing-rejects review index

One index for the track0 reject families: (A) speckled fences / black-foliage walls,
(B) the forest-wall + ground/road material + rain candidate set. Pointers only - no new
renders were made for this index, no dispatches, nothing accepted. Branch
`mistral/carbody-livery`; evidence roots are the workspace `outputs/` + the merged
integration tree (fork tip `321a620`, integration head `40ddb4e`-era).

## A. Speckled fences / black foliage - the standing reject and the judge pack

### The reject record

| Record | Where |
|---|---|
| "severe black foliage/horizon masses and white/speckled grandstand/fence surfaces persist. Prior cutout candidates remain rejected. Diagnose affected source surfaces/materials/normals/alpha/lighting separately with isolated IDs, no broad policy changes" | `outputs/mistral-orchestration/VISUAL_REVIEW.md` (2026-11-03/04 entries) |
| Baseline fence/grandstand speckle + black foliage masses | `EVIDENCE_INDEX.md` row, verdict **rejected (standing defect; no fix accepted)** |
| Texture A2C attempt | run 41647350933 @`6d2069a`, `outputs/repair-20261029/cloud/fence-a2c-result/` (local) - **REJECTED** (edge density unchanged in 8/9 panels) |
| Source-alpha-blend attempt | run 41632524155 @`5492d90`, `outputs/repair-20261029/cloud/fence-true-blend-result/` (local) - **REJECTED** (no speckle reduction; reviewer: "both fence candidates REJECTED for production. Root cause still unproven") |
| Filtered-pool variant | run 41625218732-era, `FILTERED_FENCE_ACTUAL.json` (recorded in `CONTINUE.md`): source alpha is INTENTIONAL (structured trusses, not noise) - filtered source rejected |
| Mip-repair dispatch row | `EVIDENCE_INDEX.md` `fence_miprepair_review` row - statuses open (the run that fills them is owner-side; this index freezes the review material below) |

### The judge pack (run 41843204651) - already exists, LINKED, not modified

`outputs/repair-20261029/mip3-41843204651/` (decrypted locally; `evidence-publication.json` names
the envelope `game-review-41843204651-1-0.enc`). Contents (`decrypted/review/`):

| Frame / file | Role (one-variable isolation) |
|---|---|
| `day0-filtered-fence-baseline.png` | production baseline (the speckle as shipped) |
| `day0-filtered-fence-family_isolate_fence.png` | the fence surfaces isolated |
| `day0-filtered-fence-family_isolate_foliage.png` | the foliage surfaces isolated |
| `day0-filtered-fence-family_isolate_stands.png` | the grandstand surfaces isolated |
| `day0-filtered-fence-fence_miprepair_candidate.png` | the mip-repair candidate frame |
| `cutout-hatch*.json` (5) | per-mode diagnostics; each declares "one-variable diagnostic; no geometry/UV/source-texture/collision changes; not a promoted rendering fix" |
| `bindings-hatch.json` / `-variation.json` | the layer's texture-resolution records (branding-flag materials resolved by unique-delimited-prefix) |
| `track-route-parity.json` | the bounded geometry parity check (not a fence claim) |

Reviewer asks on this pack exactly three questions: (1) does the mip-repair frame remove
the white speckle at full-frame scale against the baseline; (2) do the isolation frames
hold the speckle to the named surface families (fence vs stands vs foliage); (3) verdict
per the standing rule - pass only with the isolated-ID attribution written back into
`EVIDENCE_INDEX`'s fence rows; otherwise the veto stays and the next candidate must name
source surfaces AGAIN. No acceptance from this index.

## B. The forest-wall / ground-material / rain candidates (merged, env-gated, unreviewed)

All merged on the integration line (fork tip `321a620`); each is opt-in by env, off by
default, self-logged, and self-auditing. NOTE honestly: there is no single knob literally
named "road-material" in the tree - the material/ground item is the
`MYSIM_AUTHORED_GROUND` + `MYSIM_GRASS_BALANCE` pair below.

| Candidate | Knob | Merged implementation | What it claims |
|---|---|---|---|
| forest wall (dark backdrop) | `MYSIM_FOREST_TONE=<multiplier>` (e.g. 1.35; 0..3) | `src/TrackMaterialBinder.cs` `ForestTonePass()` | lifts ONLY the foliage-backdrop materials (`org_brandshatch_tree_line_*`, `org_tree_*`, `org_shrub_*`, `org_bush_*`), via AlbedoColor against the measured dark source art (treeline RGB ~(51,53,47), pine ~(37,43,26)); no texture/geometry change; logged + audit JSON on `MYSIM_BINDING_AUDIT` |
| ground/road material (source pool + grass balance) | `MYSIM_AUTHORED_GROUND=1` (+ `MYSIM_GRASS_BALANCE=1` for the `gnd_grass_*` materials only) | `TrackMaterialBinder.cs` `GrassBalancePass()` + the merged ground-pool binding set | swaps the procedural ground for the recovered source pool (the measured-pool table in `outputs/mistral-orchestration/inventory/track0_brands_hatch.*` + `TRACK_UV_DENSITY_ACTUAL.json`, run 41502718760: road 75-87 px/m, worst ground 10.6 px/m); grass = tint/balance only |
| rain visibility | `MYSIM_RAIN=<condition>` + the candidate | `src/RainVisibilityCandidate.cs` + `data/rain_visibility_candidate.json` (same volume/budget as the weather, narrowed near-field + AA'd particles) | readability-of-rain variant; gameplay/weather untouched |

## C. Review-ready capture checklist (the owner dispatches; this agent renders NOTHING)

Per-claim pair requirements - same camera, same light, same resolution, baseline first;
verdict slots stay empty until the frames exist.

1. **forest wall** - `MYSIM_FOREST_TONE=1.35` vs baseline: (a) trackside wide with the
   tree-line filling the horizon (day/dry); (b) the same camera at dusk/night; (c) a
   close parallax pass along the tree-line (walls of drooping alpha must not step). The
   decrypted `ref-car16-*` seat/export views are NOT a valid baseline here - the fence
   pack's baseline rule applies: fresh pairs at the CURRENT head.
2. **ground/road** - `MYSIM_AUTHORED_GROUND=1` (+ `MYSIM_GRASS_BALANCE=1` variant as a
   second pair): day/dry straight-down apex view; long side view (repetition/tile check
   against the 75-87 px/m road measurement); wet variant (the buffers under rain).
3. **rain visibility** - baseline vs `RainVisibilityCandidate`: day/wet and night/wet
   matches. Existing DECRYPTED material usable as PRIOR reference (not as verdicts):
   `outputs/repair-20261029/cloud/rain-focus/decrypted/review/` - 6 PNGs
   (track0, car06/car16, tod 0/3, wet 0/2) - and
   `outputs/repair-20261029/cloud/wet-ground/decrypted/review/` - 7 PNGs (same label
   set + a retry frame). Both captured on the rain-focus visual runs; the verdict pairs
   must still be re-shot at the current head (the terrain/ground candidates changed).
4. **fence family (A, above)** - the 41843204651 pack stands as the review material;
   the only missing step is the owner-side `fence_miprepair_review` dispatch whose
   `EVIDENCE_INDEX` row is open; judgment follows the review-freeze protocol above.

## D. Pointer index (evidence the reviewers will need, all local)

- `outputs/mistral-orchestration/EVIDENCE_INDEX.md` - the fence rows (A2C, source-blend,
  mip-repair open row) + the track0 backend mock. `VISUAL_REVIEW.md` - the rejection
  sentences quoted above + the standing "no blanket policy" rule.
- `outputs/repair-20261029/mip3-41843204651/` - the judge pack (A above).
- `outputs/mistral-orchestration/inventory/track0_brands_hatch.{md,json}` + 
  `TRACK_UV_DENSITY_ACTUAL.json` - the textured-surface/ground-pool numbers (B's
  material claims ride on these).
- `src/TrackMaterialBinder.cs` / `src/RainVisibilityCandidate.cs` /
  `data/rain_visibility_candidate.json` (at the fork tip) - the merged candidate knob set.
- The established verdict protocol everywhere: no gameplay-acceptance is implied by
  greps, inventories, or this index; only the matched-frame reviewer verdicts decide.
