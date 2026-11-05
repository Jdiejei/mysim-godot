# Acceptance card - car2 BMW M4 GT4

- target: car 2018_bmw_m4_gt4 (index 2) | folder: `2018_bmw_m4_gt4` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car2.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 11 inventory rows)
- orientationClass: mirrored-X x7
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x4
- defectTaxonomy: mirrored-UV x7
- defectTaxonomy: none x4

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_badges.png | 512x512 | 630c7f2fe001 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_cab.png | 512x512 | 36183aaf9fc7 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_int.png | 1024x1024 | 848f30018b6d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_lights.png | 512x512 | 4db18308dd67 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_mask.png | 1024x1024 | aa9f4dddfd4b | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_misc.png | 512x512 | d9ef7a086380 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_sha.png | 256x256 | 588cda5cc4ba | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_sw.png | 512x512 | 015e8cab3094 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_wheel.png | 512x512 | 196eacd85edd | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m4_gt4/2018_bmw_m4_gt4_wheel_blur.png | 512x512 | 8f3ddf8d4ea2 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m4_gt4/livery/2018_bmw_m4_gt4_ext.png | 1024x1024 | 932fed5da214 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: 41849732286 FAILED; retry 41854428291 also FAILED (at 6263c51); re-run pending

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
