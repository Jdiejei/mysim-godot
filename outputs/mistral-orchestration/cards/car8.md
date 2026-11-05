# Acceptance card - car8 McLAREN 650S GT3

- target: car 2015_mclaren_650s_gt3 (index 8) | folder: `2015_mclaren_650s_gt3` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car8.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 11 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x2
- defectTaxonomy: mirrored-UV x8
- defectTaxonomy: none x3

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_badges.png | 512x512 | 687b1930acc3 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_cab.png | 512x512 | fa2906a4c7ea | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_int.png | 1024x1024 | 9af130245483 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_lights.png | 512x512 | deb6fef1f52b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_mask.png | 1024x1024 | 687b1930acc3 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_misc.png | 512x512 | 47c6f8c5e485 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_sha.png | 256x256 | a6919584d293 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_sw.png | 512x512 | 894461794aba | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_wheel.png | 512x512 | 0f35f497d087 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_650s_gt3/2015_mclaren_650s_gt3_wheel_blur.png | 512x512 | 91e50cbd1b9c | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2015_mclaren_650s_gt3/livery/2015_mclaren_650s_gt3_ext_59.png | 1024x1024 | 8800ac333a50 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
