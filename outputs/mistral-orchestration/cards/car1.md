# Acceptance card - car1 AUDI R8 GT4

- target: car 2018_audi_r8_gt4 (index 1) | folder: `2018_audi_r8_gt4` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car1.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 12 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x3
- defectTaxonomy: mirrored-UV x8
- defectTaxonomy: none x4

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_badges.png | 512x512 | 960c278a1f6a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_cab.png | 512x512 | b8e61acb0dea | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_int.png | 1024x1024 | 09a9cce592e9 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_lights.png | 512x512 | bd53f10d2740 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_mask.png | 1024x1024 | 2f29c3767ced | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_misc.png | 512x512 | cf64e5b9a57d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_sha.png | 256x256 | 241e20ad9c6f | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_sw.png | 256x256 | 2483616da459 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_wheel.png | 512x512 | a98a4207c84d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_audi_r8_gt4/2018_audi_r8_gt4_wheel_blur.png | 512x512 | 431e2f22e2ab | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_audi_r8_gt4/livery/2018_audi_r8_gt4_ext.png | 1024x1024 | 111f7ea9fd58 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_audi_r8_gt4/livery/2018_audi_r8_gt4_ext_fictional_23.png | 1024x1024 | 2f29c3767ced | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: 41849732286 FAILED; retry 41854428291 also FAILED (at 6263c51); re-run pending

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
