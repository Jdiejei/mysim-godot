# Acceptance card - car21 PORSCHE 919 EVO

- target: car 2018_porsche_919_hybrid_evo (index 21) | folder: `2018_porsche_919_hybrid_evo` | class: 2
- inventory: `outputs/mistral-orchestration/inventory/car21.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 11 inventory rows)
- orientationClass: mirrored-X x7
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x4
- defectTaxonomy: mirrored-UV x6
- defectTaxonomy: none x5

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_badges.png | 256x256 | 5ea306a20bf4 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_cab.png | 512x512 | 1468a5550a86 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_int.png | 1024x1024 | 8e70c1794f5d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_lights.png | 256x256 | 521723c01f97 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_mask.png | 1024x1024 | 17aa3daf0605 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_misc.png | 512x512 | 00cd0226eae3 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_sha.png | 256x256 | d606c45fb08b | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_sw.png | 512x512 | 17aa3daf0605 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_wheel.png | 512x512 | 10d20195c578 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2018_porsche_919_hybrid_evo/2018_porsche_919_hybrid_evo_wheel_blur.png | 512x512 | ce76c9c656f7 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2018_porsche_919_hybrid_evo/livery/2018_porsche_919_hybrid_evo_ext.png | 1024x1024 | cd278ae94644 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: pending-run-<wheel-sweep-batch>

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory
- no secondary (documented, cited) defects recorded for this car beyond the rule-derived rows

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
