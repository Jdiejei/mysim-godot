# Acceptance card - car22 ORECA 07 LMP2

- target: car 2017_oreca_07_lmp2 (index 22) | folder: `2017_oreca_07_lmp2` | class: 2
- inventory: `outputs/mistral-orchestration/inventory/car22.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 12 inventory rows)
- orientationClass: mirrored-X x8
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x4
- defectTaxonomy: mirrored-UV x7
- defectTaxonomy: none x5

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_badges.png | 512x512 | d7d5989e5160 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_badges_alpha.png | 512x512 | 7124f86fd948 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_cab.png | 512x512 | be2d0528172b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_int.png | 1024x1024 | b3dd1a93ee68 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_lights.png | 512x512 | eb1495b685fd | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_mask.png | 2048x2048 | bc382af6a5ca | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_misc.png | 1024x1024 | bc382af6a5ca | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_sha.png | 256x256 | d49568b63c37 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_sw.png | 1024x1024 | 895be1bddf9e | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_wheel.png | 512x512 | a31f56f3a2e8 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2017_oreca_07_lmp2/2017_oreca_07_lmp2_wheel_blur.png | 512x512 | 20b6b53ba57f | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2017_oreca_07_lmp2/livery/2017_oreca_07_lmp2_ext.png | 1024x1024 | d5c3d16760ca | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
