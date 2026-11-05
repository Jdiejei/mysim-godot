# car20 (Bugatti Chiron) carbody visual review pack

Third pack beside the car16 mirrored-UV and wheel packs; same panel logic:
**SOURCE (authored island) / BEFORE / AFTER-or-current**. Generator:
`tools/carbody_car20_pack.py` (read-only; fail-closed: the 11 pack rows are asserted
equal to the 11 `20/*` entries in `data/car_surface_uv_repairs.json`). Machine-readable:
`metrics.json`. Branch `mistral/carbody-livery`; no run, no dispatch.
**In-tree/local evidence only; cloud-UNVALIDATED; no acceptance claims.**

## Scope: the 11 promoted repairs (the largest per-role staged set after car16)

All 11 are the visible exterior `_mm_ext` roles of car20: BODY, BOOT, DOOR_LEFT,
DOOR_RIGHT, EXHAUST, FRONTBUMPER, HOOD, MIRROR_LEFT, MIRROR_RIGHT, REARBUMPER, WING —
one atlas: `assets/RealRacing/Source/2017_bugatti_chiron/livery/2017_bugatti_chiron_ext_blue.png`
(`res://assets/RealRacing/Source/2017_bugatti_chiron/livery/2017_bugatti_chiron_ext_blue.png`,
guid `6cbf28e8030d8558fb2926454456387e`), per-renderer `uv_scale=(1,-1) uv_offset=(0,1)`
(V-only), `pending` absent/false = ACTIVE in every production render.

## Evidence material (all locally decrypted/downloaded, runs + reviews verbatim)

| Set | Run · review JSON | What it shows |
|---|---|---|
| tier 0, 8 PNGs (`cloud/chiron-atlas-baseline-visuals/review/chiron-atlas-transpose-0-*`) | 41453316712 · CHIRON_ATLAS_BASELINE_PARTIAL_REVIEW.json | the same atlas rendered at FOUR mapping modes x front/rear. `none` = the pre-repair mapping ("both-axis baseline mapping remains visibly wrong: bright blue mismatched door/rear panels, white mechanical patch on front bumper, red line artefacts and yellow wheel outlines"); `v` = V-only ("V-only coherent two-tone panels/front+rear details") = the mode the per-role entries encode. `u`/`both` = the rejected flip variants |
| tier 1, 8 PNGs (`cloud/chiron-transpose-visuals/review/chiron-atlas-transpose-1-*`) | 41457447642 · CHIRON_TRANSPOSE_ACTUAL_REJECTION.json | the re-check: "Reject transpose; non-transposed V-only remains coherent mapping selected for exact 11 visible exterior roles" — why the fix is per-renderer, not an atlas/shader-level flip |
| integrated, 4 PNGs (`cloud/chiron-integrated-visuals/review/chiron-reconstructed-*`) | 41458535677 · CHIRON_INTEGRATED_ATLAS_ACTUAL_REVIEW.json | production-path re-verification (repairs active + reconstructed wing): "continuous blue front/doors and dark rear/roof, front badge and rear CHIRON plate correctly placed. Prior unrelated mechanical/taillamp islands on body panels are removed in four actual views." `exteriorAtlasCorrectionVerified: true`, `wholeCarAccepted: false` |

## Row strips (13, in `images/`)

| Row | Renderer | UV0 island (atlas space, wrapped) | uvCount | Strip panels | Gap |
|---|---|---|---|---|---|
| 1a/1b | LOD_A_BODY_mm_ext | u 0.003..0.995, v -0.993..-0.057 | 5965 | FRONT: source, tier-0 none, tier-0 v, integrated-front — REAR: same with rear views | - |
| 2a | LOD_A_BOOT_mm_ext | u 0.533..0.996, v -0.994..-0.125 | 990 | REAR strip | - |
| 3a/4a | LOD_A_DOOR_LEFT / RIGHT_mm_ext | L: u 0.260..0.538 v -0.437..-0.232; R: u 0.262..0.538 v -0.888..-0.683 | 632 each | SOURCE + integrated-side only | **no before-side capture exists in the tier sets (front/rear only)** — stated on the image |
| 5a | LOD_A_EXHAUST_mm_ext | u 0.012..0.050, v -0.014..-0.002 (tiny corner island, wraps at v=0/1) | 1145 | REAR strip | - |
| 6a | LOD_A_FRONTBUMPER_mm_ext | u 0.009..0.472, v -0.053..-0.012 | 787 | FRONT strip | - |
| 7a/7b | LOD_A_HOOD_mm_ext | u 0.004..0.224, v -0.702..-0.417 | 337 | FRONT strip + integrated-overhead | - |
| 8a/9a | LOD_A_MIRROR_LEFT / RIGHT_mm_ext | L: u 0.208..0.295 v -0.553..-0.442; R: u 0.209..0.297 v -0.677..-0.566 | 714 each | SOURCE + integrated-side only | **no before-side capture in the tier sets** — stated on the image |
| 10a | LOD_A_REARBUMPER_mm_ext | u 0.581..0.990, v -0.074..-0.012 | 636 | REAR strip | - |
| 11a | LOD_A_WING_mm_ext | u 0.827..0.984, v -0.710..-0.410 | 731 | REAR strip | wing MESH is a reconstructed design (run 41449769834), not the recovered pose |

Reviewer reading per strip: the tier-0 `none` panel = the documented BEFORE defect
(mechanical/taillamp islands on unrelated panels, mismatched doors); the tier-0 `v`
panel = the mapping the promoted entries encode; the integrated panel = the production
path today. PASS per row = the marking/detail placement in the integrated panel reads
correctly against the SOURCE island crop.

## Honest gaps

- **Side-view BEFORE**: the tier sets contain only front/rear; DOOR and MIRROR rows
  therefore have no before-side crop (gaps drawn on the image; the door/mirror lettering
  defect was documented front-view in the reviews, the placement correction verified in
  the integrated side view).
- **No fleet captures for car20**: the car_evidence 41458535677 views are the current
  local imagery; shard-based fleet coverage for car20 awaits the owner battery runs
  (post-fix shard matrix).
- **Tier-0 mechanism note**: tier-0 images were produced by the run's atlas-transpose
  env; production carries the effect as the 11 per-renderer entries. Same resulting
  mapping `(u,v)->(u,1-v)` on the 11 roles (documented family); the production-path
  evidence is the integrated run, not the tier-0 sweep.
- Wheels/interior/wing-art remain rejected items (wholeCarAccepted false): yellow wheel
  outlines and rim/tyre quality live in the wheel-pack's scope; no work has been done
  on them in this pack.

## Sources (files)

- `data/car_surface_uv_repairs.json` — the 11 `20/*` entries (verbatim in metrics.json)
- `outputs/repair-20261029/cloud/car-material-bindings/.../geometry.json` — car20 per-renderer UV0 bounds
- the four review JSONs + three decrypted PNG dirs under `outputs/repair-20261029/` and `outputs/repair-20261029/cloud/`
- `CHIRON_RECONSTRUCTED_ACTUAL_REVIEW.json` (run 41449769834) — the reconstructed-wing provenance note
