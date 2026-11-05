# Acceptance card - car15 MERCEDES W10

- target: car 2019_f1_mercedes_w10 (index 15) | folder: `2019_f1_mercedes_w10` | class: 1
- inventory: `outputs/mistral-orchestration/inventory/car15.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 16 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x7
- defectTaxonomy: none x8
- defectTaxonomy: mirrored-UV x6
- defectTaxonomy: authored-detail-missing x1
- defectTaxonomy: source-atlas-fragmentation x1
- secondary: authored-detail-missing -> source-art-fix: FULL_F1_W10_WHEEL_REVIEW (run 41310403219, 8 captures viewed): barrels plain bright, raised rings plastic, brake detail insufficient; NEUTRAL_GRADE_W1

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_arms_44.png | 1024x1024 | 4778b01fc074 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_arms_77.png | 1024x1024 | 377ec42411fb | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_badges.png | 256x256 | 39e529a08331 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_cab.png | 512x512 | 1538e7af4751 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_driver_44.png | 1024x1024 | 0e7dc8332e2c | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_driver_77.png | 1024x1024 | a75e0e2461ad | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_int.png | 512x512 | aed025ec64b6 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_lights.png | 512x512 | b08d43288767 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_mask.png | 1024x1024 | 265345742c65 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_misc.png | 512x512 | 392d3c39e768 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_sha.png | 256x256 | 47ec78a667cc | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_sw.png | 1024x1024 | 7c9b5f32692f | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_wheel.png | 512x512 | 7e148d242bf8 | UV0 | mirrored-X | authored-detail-missing | source-art-fix |
| RealRacing/Source/2019_f1_mercedes_w10/2019_f1_mercedes_w10_wheel_blur.png | 512x512 | 0d8d3f51dba7 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mercedes_w10/livery/2019_f1_mercedes_w10_ext_44.png | 1024x1024 | 265345742c65 | UV0 | mirrored-X | source-atlas-fragmentation | source-art-fix |
| RealRacing/Source/2019_f1_mercedes_w10/livery/2019_f1_mercedes_w10_ext_77.png | 1024x1024 | fe60f35efdee | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- F1 triad pack - car15 section: `outputs/mistral-orchestration/f1-triad-pack/F1-TRIAD-PACK.md`
- car15 main strip: `outputs/mistral-orchestration/f1-triad-pack/images/car15-main.png`
- car15 rubber closeups: `outputs/mistral-orchestration/f1-triad-pack/images/car15-wheels.png`
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: measured by 41837315358/41843317303 (incorporation pending)

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
