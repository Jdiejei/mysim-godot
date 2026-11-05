# Acceptance card - car5 CAMARO ZL1 1LE

- target: car 2018_chevrolet_camaro_zl1_1le (index 5) | folder: `2018_chevrolet_camaro_zl1_1le` | class: 0
- inventory: `outputs/mistral-orchestration/inventory/car5.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 16 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x7
- defectTaxonomy: mirrored-UV x8
- defectTaxonomy: none x8

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_badges.png | 512x512 | a8c2dd41457c | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_cab.png | 512x512 | 83e7546cdd2b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_int.png | 1024x1024 | b954a371b09a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_lights.png | 512x512 | acaf5ca30b90 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_mask.png | 1024x1024 | 780e8f43f36e | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_misc.png | 512x512 | d352dd5c2d0b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_sha.png | 256x256 | 8194d27b4fac | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_sw.png | 512x512 | 3b4d994f3ad1 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_wheel.png | 512x512 | 9be9de95393b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/2018_chevrolet_camaro_zl1_1le_wheel_blur.png | 512x512 | b27070691c38 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/livery/2018_chevrolet_camaro_zl1_1le_ext_black.png | 1024x1024 | 780e8f43f36e | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/livery/2018_chevrolet_camaro_zl1_1le_ext_bright_yellow.png | 1024x1024 | b0208ee5056a | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/livery/2018_chevrolet_camaro_zl1_1le_ext_hyper_blue.png | 1024x1024 | 96e03827f465 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/livery/2018_chevrolet_camaro_zl1_1le_ext_krypton_green.png | 1024x1024 | 214336fef992 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/livery/2018_chevrolet_camaro_zl1_1le_ext_red_hot.png | 1024x1024 | d3ced2ce6fed | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_chevrolet_camaro_zl1_1le/livery/2018_chevrolet_camaro_zl1_1le_ext_summit_white.png | 1024x1024 | f025cf088898 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
