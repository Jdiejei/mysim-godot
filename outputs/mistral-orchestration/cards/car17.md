# Acceptance card - car17 RED BULL RB15

- target: car 2019_f1_red_bull_rb15 (index 17) | folder: `2019_f1_red_bull_rb15` | class: 1
- inventory: `outputs/mistral-orchestration/inventory/car17.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 16 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x7
- defectTaxonomy: none x8
- defectTaxonomy: mirrored-UV x6
- defectTaxonomy: authored-detail-missing x1
- defectTaxonomy: source-atlas-fragmentation x1
- secondary: authored-detail-missing -> source-art-fix: FULL_F1_RB15_WHEEL_REVIEW (run 41310403219, all 8 views): distinct swept 10-spoke rim + compound markings + dry slicks/wet grooves visible, raised rin

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_arms_23.png | 1024x1024 | dcc40be73f67 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_arms_33.png | 1024x1024 | b46acc28f06d | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_badges.png | 256x256 | b63bb2058131 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_cab.png | 512x512 | 2bd19b899fc2 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_driver_23.png | 1024x1024 | de640a2d0887 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_driver_33.png | 1024x1024 | 910ef8e703e2 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_int.png | 512x512 | 400b75bf3d9b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_lights.png | 256x256 | 35fb7c2b4e9a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_mask.png | 1024x1024 | b63bb2058131 | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_misc.png | 512x512 | e56530682fda | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_sha.png | 256x256 | 8f002bb8d188 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_sw.png | 1024x1024 | 75b9f4f8068d | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_wheel.png | 512x512 | 7d3af2f65aaa | UV0 | mirrored-X | authored-detail-missing | source-art-fix |
| RealRacing/Source/2019_f1_red_bull_rb15/2019_f1_red_bull_rb15_wheel_blur.png | 512x512 | c6d2916ac7a4 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_red_bull_rb15/livery/2019_f1_red_bull_rb15_ext_23.png | 1024x1024 | 55e4656673ba | UV0 | mirrored-X | source-atlas-fragmentation | source-art-fix |
| RealRacing/Source/2019_f1_red_bull_rb15/livery/2019_f1_red_bull_rb15_ext_33.png | 1024x1024 | 2a29dec602d4 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- F1 triad pack - car17 section: `outputs/mistral-orchestration/f1-triad-pack/F1-TRIAD-PACK.md`
- car17 main strip: `outputs/mistral-orchestration/f1-triad-pack/images/car17-main.png`
- car17 rubber closeups: `outputs/mistral-orchestration/f1-triad-pack/images/car17-wheels.png`
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: measured by 41837315358/41843317303 (incorporation pending)

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
