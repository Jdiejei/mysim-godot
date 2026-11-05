# Acceptance card - car7 MERCEDES-AMG GT3

- target: car 2016_mercedes_amg_gt3 (index 7) | folder: `2016_mercedes_amg_gt3` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car7.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 16 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x7
- defectTaxonomy: mirrored-UV x8
- defectTaxonomy: none x8

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_badges.png | 512x512 | b6e6bbe879ae | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_cab.png | 512x512 | 1139ad8212ab | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_int.png | 1024x1024 | 17c28d340c44 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_lights.png | 512x512 | c610f4ace6b3 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_mask.png | 1024x1024 | c610f4ace6b3 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_misc.png | 512x512 | a8a375a93107 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_sha.png | 256x256 | fd5366f8ce50 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_sw.png | 512x512 | 2b82256445a9 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_wheel.png | 512x512 | 02ff2249baae | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/2016_mercedes_amg_gt3_wheel_blur.png | 512x512 | 8befa72f57d8 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_mercedes_amg_gt3/livery/2016_mercedes_amg_gt3_ext.png | 1024x1024 | 019ea9c04ad3 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_mercedes_amg_gt3/livery/2016_mercedes_amg_gt3_ext_fictional_2.png | 1024x1024 | 8d44750b219d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_mercedes_amg_gt3/livery/2016_mercedes_amg_gt3_ext_fictional_3.png | 1024x1024 | 2f7255358f67 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_mercedes_amg_gt3/livery/2016_mercedes_amg_gt3_ext_fictional_4.png | 1024x1024 | 0dd8ae373e97 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_mercedes_amg_gt3/livery/2016_mercedes_amg_gt3_ext_fictional_5.png | 1024x1024 | 2eb865ccae04 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_mercedes_amg_gt3/livery/2016_mercedes_amg_gt3_ext_fictional_6.png | 1024x1024 | f671f42ccd7b | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
