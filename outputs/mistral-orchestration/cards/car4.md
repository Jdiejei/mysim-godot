# Acceptance card - car4 BMW M8 GTE

- target: car 2018_bmw_m8_gte (index 4) | folder: `2018_bmw_m8_gte` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car4.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 12 inventory rows)
- orientationClass: mirrored-X x10
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x2
- defectTaxonomy: mirrored-UV x9
- defectTaxonomy: none x3

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_badges.png | 512x512 | ad003b9e2cbf | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_cab.png | 512x512 | 72c79e317957 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_int.png | 1024x1024 | a1bcecaa274a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_lights.png | 512x512 | 37f611962e54 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_mask.png | 1024x1024 | ecb9bea0cbde | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_misc.png | 512x512 | 0e21cb3d7f68 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_sha.png | 256x256 | d0c4cc8e3972 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_sw.png | 512x512 | f802a95da198 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_wheel.png | 512x512 | e3fdb3e11cc1 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_wheel_blur.png | 512x512 | 9385650c2b27 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_bmw_m8_gte/2018_bmw_m8_gte_windows.png | 512x512 | 81750c9ab2ee | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_bmw_m8_gte/livery/2018_bmw_m8_gte_ext_1.png | 1024x1024 | ecb9bea0cbde | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
