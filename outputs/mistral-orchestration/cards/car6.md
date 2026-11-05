# Acceptance card - car6 HURACAN GT3

- target: car 2018_lamborghini_huracan_gt3 (index 6) | folder: `2018_lamborghini_huracan_gt3` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car6.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 1 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 17 inventory rows)
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x8
- orientationClass: mirrored-X x7
- orientationClass: indeterminate (mixed per surface: see bindings[]; reason: different surfaces of this texture sit in different repair states) x2
- defectTaxonomy: none x9
- defectTaxonomy: mirrored-UV x8

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_badges.png | 512x512 | 65f131929dd5 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_badges_02.png | 512x512 | 10fd10f52d20 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_cab.png | 512x512 | da027e0b54ad | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_int.png | 1024x1024 | 7ea24c9c4a05 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_lights.png | 512x512 | 1921a1e0f051 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_mask.png | 1024x1024 | 7ea24c9c4a05 | UV0 | indeterminate (mixed per surface: see bindings[]; reason: different surfaces of this texture sit in different repair states) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_misc.png | 512x512 | 73c5434059a2 | UV0 | indeterminate (mixed per surface: see bindings[]; reason: different surfaces of this texture sit in different repair states) | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_sha.png | 256x256 | 3da7121f7f8c | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_sw.png | 512x512 | f64e38ed5705 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_wheel.png | 512x512 | d2a7dafb5bf9 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/2018_lamborghini_huracan_gt3_wheel_blur.png | 512x512 | 843a87fff5b0 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/livery/2018_lamborghini_huracan_gt3_ext_fictional_08.png | 1024x1024 | cc4e44d77c28 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_lamborghini_huracan_gt3/livery/2018_lamborghini_huracan_gt3_ext_fictional_09.png | 1024x1024 | f35760fc3b99 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/livery/2018_lamborghini_huracan_gt3_ext_fictional_17.png | 1024x1024 | bff240539ed5 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/livery/2018_lamborghini_huracan_gt3_ext_fictional_18.png | 1024x1024 | 0b5a5e590f39 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/livery/2018_lamborghini_huracan_gt3_ext_grt_11.png | 1024x1024 | 83b542b1fb86 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_lamborghini_huracan_gt3/livery/2018_lamborghini_huracan_gt3_ext_grt_19.png | 1024x1024 | a0c61eb595ca | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: captured-41848086075+41862411919-shard0 (42/42 twice; decrypted locally - visual verdicts with the reviewer)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
