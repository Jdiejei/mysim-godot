# Acceptance card - car3 CAMARO GT4.R

- target: car 2018_chevrolet_camaro_gt4r (index 3) | folder: `2018_chevrolet_camaro_gt4r` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car3.md` (+ `.json`, committed)

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
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_badges.png | 256x256 | 6dedb250f27d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_cab.png | 512x512 | 62675095eba7 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_int.png | 1024x1024 | 6fddc2adf018 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_lights.png | 512x512 | 4e3da82f4b5f | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_mask.png | 1024x1024 | 0ce2d3d4dd49 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_misc.png | 512x512 | 4e902fd31670 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_sha.png | 256x256 | e72e30e595d4 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_sw.png | 512x512 | 276144bfae91 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_wheel.png | 512x512 | c6bf61c2ea82 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/2018_chevrolet_camaro_gt4r_wheel_blur.png | 512x512 | 77f687e8f576 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/livery/2018_chevrolet_camaro_gt4r_ext.png | 1024x1024 | bddfa7fafbd9 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_gt4r/livery/2018_chevrolet_camaro_gt4r_ext_fictional_12.png | 1024x1024 | 0ce2d3d4dd49 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: 41849732286 FAILED; retry 41854428291 also FAILED (at 6263c51); re-run pending

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
