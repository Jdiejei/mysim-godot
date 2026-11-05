# Acceptance card - car13 ASTON MARTIN GTE

- target: car 2018_astonmartin_gte (index 13) | folder: `2018_astonmartin_gte` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car13.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 11 inventory rows)
- orientationClass: mirrored-X x8
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x3
- defectTaxonomy: mirrored-UV x7
- defectTaxonomy: none x4

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_badges.png | 256x256 | 9d271f3752e8 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_cab.png | 512x512 | 7e4623d4ce97 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_int.png | 1024x1024 | ab083afa693a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_lights.png | 512x512 | ddde07c4803a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_mask.png | 1024x1024 | ba8a9c501c8a | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_misc.png | 512x512 | ba8a9c501c8a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_sha.png | 256x256 | 49df8c69437e | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_sw.png | 512x512 | 6b1bd5ed1e66 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_wheel.png | 512x512 | a151aabd7c8a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_astonmartin_gte/2018_astonmartin_gte_wheel_blur.png | 512x512 | 0aba40f09eae | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_astonmartin_gte/livery/2018_astonmartin_gte_ext.png | 1024x1024 | 65a416a368c9 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
