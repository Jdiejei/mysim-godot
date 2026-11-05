# Acceptance card - car14 CORVETTE C7.R

- target: car 2016_chevrolet_c7r (index 14) | folder: `2016_chevrolet_c7r` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car14.md` (+ `.json`, committed)

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
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_badges.png | 512x512 | 95d3d8892941 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_cab.png | 512x512 | 7e45e9965523 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_int.png | 1024x1024 | efbb20ec5956 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_lights.png | 512x512 | 59d84cfc0b4d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_mask.png | 1024x1024 | df795c66e36d | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_misc.png | 512x512 | e73ee0046bdf | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_sha.png | 256x256 | 2ed48e5d809d | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_sw.png | 512x512 | e293af5f2af5 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_wheel.png | 512x512 | 7b97dc95864a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/2016_chevrolet_c7r_wheel_blur.png | 512x512 | c2cf670fc9f8 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2016_chevrolet_c7r/livery/2016_chevrolet_c7r_ext_63.png | 1024x1024 | df795c66e36d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2016_chevrolet_c7r/livery/2016_chevrolet_c7r_ext_64.png | 1024x1024 | ef836ee3298d | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
