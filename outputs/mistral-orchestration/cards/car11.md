# Acceptance card - car11 PORSCHE 911 RSR

- target: car 2018_porsche_911_rsr (index 11) | folder: `2018_porsche_911_rsr` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car11.md` (+ `.json`, committed)

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
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_badges.png | 512x512 | ea4ba99a01e7 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_cab.png | 512x512 | 37a14af004b8 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_int.png | 1024x1024 | cb8c1471c895 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_lights.png | 512x512 | 8f4efe8128cb | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_mask.png | 1024x1024 | 37a14af004b8 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_misc.png | 512x512 | 8cf18f03d610 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_sha.png | 256x256 | a4c06b7f5dd4 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_sw.png | 1024x1024 | ae0af4cfa24c | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_wheel.png | 512x512 | 0394818d92dc | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_911_rsr/2018_porsche_911_rsr_wheel_blur.png | 512x512 | c64783d0ea59 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_porsche_911_rsr/livery/2018_porsche_911_rsr_ext.png | 1024x1024 | 3b6e77618219 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
