# Acceptance card - car18 McLAREN MCL34

- target: car 2019_f1_mclaren_mcl34 (index 18) | folder: `2019_f1_mclaren_mcl34` | class: 1
- inventory: `outputs/mistral-orchestration/inventory/car18.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 0 promoted (active in every render) / 0 stage-gated pending (candidates env)

## Orientation / defect summary (over 15 inventory rows)
- orientationClass: mirrored-X x9
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x6
- defectTaxonomy: none x7
- defectTaxonomy: mirrored-UV x6
- defectTaxonomy: authored-detail-missing x1
- defectTaxonomy: source-atlas-fragmentation x1
- secondary: authored-detail-missing -> source-art-fix: FULL_F1_MCL34_WHEEL_REVIEW (run 41310403219, all 8 front/rear 4-compound views): distinct swept spokes + compound colours + wet drainage geometry visi

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_arms.png | 1024x1024 | df8c60d108f7 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_badges.png | 256x256 | 825fdcfe36d1 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_cab.png | 512x512 | 3f22dfb37c8e | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_driver_4.png | 1024x1024 | cafca3a9f411 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_driver_55.png | 1024x1024 | 0cc186b0b89b | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_int.png | 512x512 | 9c5e798bf07b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_lights.png | 256x256 | 3c66c27385e5 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_mask.png | 1024x1024 | b7d4cd0b0c8e | UV0 | mirrored-X | none | none-needed |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_misc.png | 512x512 | 9387e888c58a | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_sha.png | 256x256 | c2b0f8d44951 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_sw.png | 1024x1024 | 604b7bc1ed81 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_wheel.png | 512x512 | b7d4cd0b0c8e | UV0 | mirrored-X | authored-detail-missing | source-art-fix |
| RealRacing/Source/2019_f1_mclaren_mcl34/2019_f1_mclaren_mcl34_wheel_blur.png | 512x512 | af0072ea7d2d | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_mclaren_mcl34/livery/2019_f1_mclaren_mcl34_ext_4.png | 1024x1024 | 138800d89426 | UV0 | mirrored-X | source-atlas-fragmentation | source-art-fix |
| RealRacing/Source/2019_f1_mclaren_mcl34/livery/2019_f1_mclaren_mcl34_ext_55.png | 1024x1024 | a32002cf985d | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- F1 triad pack - car18 section: `outputs/mistral-orchestration/f1-triad-pack/F1-TRIAD-PACK.md`
- car18 main strip: `outputs/mistral-orchestration/f1-triad-pack/images/car18-main.png`
- car18 fleet wheel closeups: `outputs/mistral-orchestration/f1-triad-pack/images/car18-wheels.png`
- fleet visual state: captured-41848086075+41862411919-shard0 (42/42 twice; decrypted locally - visual verdicts with the reviewer)
- P5 metrics state: measured by 41837315358/41843317303 (incorporation pending)

## Open decisions (reviewer's)
- no pending UV entries; the open items are the art/detail verdicts documented in the inventory

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
