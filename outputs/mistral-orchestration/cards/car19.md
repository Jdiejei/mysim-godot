# Acceptance card - car19 McLAREN P1 GTR

- target: car 2015_mclaren_p1_gtr (index 19) | folder: `2015_mclaren_p1_gtr` | class: 3
- inventory: `outputs/mistral-orchestration/inventory/car19.md` (+ `.json`, committed)

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
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_badges.png | 512x512 | 3a811d161a13 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_cab.png | 512x512 | 357d6352bac2 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_int.png | 1024x1024 | 04c8ed443dc7 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_lights.png | 512x512 | e84066777eba | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_mask.png | 1024x1024 | e863f28bb0df | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_misc.png | 512x512 | e863f28bb0df | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_sha.png | 256x256 | 724279addf4c | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_sw.png | 512x512 | 8e4f17632505 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_wheel.png | 512x512 | 1ffcb1a1e0bd | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2015_mclaren_p1_gtr/2015_mclaren_p1_gtr_wheel_blur.png | 512x512 | 5516b6ca9938 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2015_mclaren_p1_gtr/livery/2015_mclaren_p1_gtr_ext_51.png | 1024x1024 | 74688f769f0b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
