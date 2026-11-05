# Acceptance card - car10 BMW M6 GT3

- target: car 2018_bmw_m6_gt3 (index 10) | folder: `2018_bmw_m6_gt3` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car10.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 15 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x6
- defectTaxonomy: mirrored-UV x8
- defectTaxonomy: none x7

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_badges.png | 512x512 | 6f89b6c360b8 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_cab.png | 512x512 | 3c97bfa9d8ea | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_int.png | 1024x1024 | e3d8e2008644 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_lights.png | 512x512 | c5940da9b87d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_mask.png | 1024x1024 | c5940da9b87d | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_misc.png | 512x512 | cb32f45fc221 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_sha.png | 256x256 | 4f3464c26440 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_sw.png | 256x256 | 544a0f26eec5 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_wheel.png | 512x512 | 16e02008d174 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/2018_bmw_m6_gt3_wheel_blur.png | 512x512 | 72031826240c | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m6_gt3/livery/2018_bmw_m6_gt3_ext_fictional_34.png | 1024x1024 | d27c36f18b29 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m6_gt3/livery/2018_bmw_m6_gt3_ext_fictional_35.png | 1024x1024 | 0baedb6ca68f | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m6_gt3/livery/2018_bmw_m6_gt3_ext_fictional_46.png | 1024x1024 | e70a3302abbb | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m6_gt3/livery/2018_bmw_m6_gt3_ext_fictional_47.png | 1024x1024 | 659bd9bf4fcd | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m6_gt3/livery/2018_bmw_m6_gt3_ext_turner_motorsport.png | 1024x1024 | bf9eba8c66ae | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
