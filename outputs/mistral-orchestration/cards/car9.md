# Acceptance card - car9 ACURA NSX GT3

- target: car 2018_acura_nsx_gt3 (index 9) | folder: `2018_acura_nsx_gt3` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car9.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 17 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x8
- defectTaxonomy: none x9
- defectTaxonomy: mirrored-UV x8

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_badges.png | 512x512 | 1c60acbfba00 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_cab.png | 512x512 | 347a3433e132 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_int.png | 1024x1024 | 393eff80a787 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_lights.png | 512x512 | ae0a3b227057 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_mask.png | 1024x1024 | 51253b322313 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_misc.png | 512x512 | 4579ae54a949 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_sha.png | 256x256 | a9de6b4e16a5 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_sw.png | 512x512 | 7dd653038895 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_wheel.png | 512x512 | 400ac91ebd41 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/2018_acura_nsx_gt3_wheel_blur.png | 512x512 | 76c1be1cd8f8 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/livery/2018_acura_nsx_gt3_ext_fictional_40.png | 1024x1024 | a722613e140b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_acura_nsx_gt3/livery/2018_acura_nsx_gt3_ext_fictional_56.png | 1024x1024 | 9c7cc17702d7 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/livery/2018_acura_nsx_gt3_ext_fictional_57.png | 1024x1024 | 8f395ae117a6 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/livery/2018_acura_nsx_gt3_ext_gradient_racing_llc_36.png | 1024x1024 | 1463ef0cdca7 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/livery/2018_acura_nsx_gt3_ext_hart_69.png | 1024x1024 | 7760ee8a5bd6 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/livery/2018_acura_nsx_gt3_ext_msr_86.png | 1024x1024 | 4f63e490dacf | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_acura_nsx_gt3/livery/2018_acura_nsx_gt3_ext_msr_93.png | 1024x1024 | 4a1964165bef | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
