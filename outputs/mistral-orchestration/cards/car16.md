# Acceptance card - car16 FERRARI SF90

- target: car 2019_f1_ferrari_sf90 (index 16) | folder: `2019_f1_ferrari_sf90` | class: 1
- inventory: `outputs/mistral-orchestration/inventory/car16.md` (+ `.json`, committed)

## Repair state (data/car_surface_uv_repairs.json)
- 3 promoted (active in every render) / 10 stage-gated pending (candidates env)
  - `16/LOD_A_DOOR_LEFT_mm_ext` pending
  - `16/LOD_A_DOOR_RIGHT_mm_ext` pending
  - `16/LOD_A_EXHAUST_mm_ext` pending
  - `16/LOD_A_MIRROR_LEFT_mm_ext` pending
  - `16/LOD_A_MIRROR_RIGHT_mm_ext` pending
  - `16/LOD_A_STRUT_UPPER_FRONT_LEFT_mm_ext` pending
  - `16/LOD_A_STRUT_UPPER_FRONT_RIGHT_mm_ext` pending
  - `16/LOD_A_BODY_mm_badges` pending
  - `16/LOD_A_FRONTBUMPER_mm_badges` pending
  - `16/LOD_A_REARBUMPER_mm_badges` pending

## Orientation / defect summary (over 15 inventory rows)
- orientationClass: mirrored-X x7
- orientationClass: as-authored (file on disk; not bound by any runtime material in the local maps) x6
- orientationClass: indeterminate (mixed per surface: see bindings[]; reason: different surfaces of this texture sit in different repair states) x2
- defectTaxonomy: mirrored-UV x8
- defectTaxonomy: none x7
- secondary: low-res-source -> source-art-fix: 256x256 atlas carries all sponsor marks (turn-1 sec 1.2/1.5: no higher-res original exists in the recovery)
- secondary: low-res-source -> source-art-fix: 512x512 carries dash+cabin+wheel faces; instrument glyphs land at a few px at seated-eye distance (the cockpit readability complaint); conditional clo
- secondary: low-res-source -> source-art-fix: 512x512 for the full interior cage; same conditional-upscale policy; no higher-res original local (turn-1 sec 1.5)
- secondary: low-res-source -> source-art-fix: 1024x1024 for the whole body livery (~4-5 mm/texel; turn-1 sec 1.3); source ceiling - no higher-res original

## Atlas table
| file | px | guid | uv | orientationClass | defect | correction |
|---|---|---|---|---|---|---|
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_arms.png | 1024x1024 | 5593202caff1 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_badges.png | 256x256 | a903631edbed | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_cab.png | 512x512 | 3f4b5473ee12 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_driver_16.png | 1024x1024 | 1e7cd890130a | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_driver_5.png | 1024x1024 | 0671e16dab65 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_int.png | 512x512 | c829acec3cfa | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_lights.png | 512x512 | f491358d555b | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_mask.png | 1024x1024 | 449e4db78ecc | UV0 | indeterminate (mixed per surface: see bindings[]; reason: different surfaces of this texture sit in different repair states) | none | none-needed |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_misc.png | 512x512 | aed61c8bdfb9 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_sha.png | 256x256 | 158f1499a010 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_sw.png | 1024x1024 | 4734243d4ddc | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_wheel.png | 512x512 | 78fcd2c95e54 | UV0 | mirrored-X | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/2019_f1_ferrari_sf90_wheel_blur.png | 512x512 | 486488712cf4 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |
| RealRacing/Source/2019_f1_ferrari_sf90/livery/2019_f1_ferrari_sf90_ext_16.png | 1024x1024 | 449e4db78ecc | UV0 | indeterminate (mixed per surface: see bindings[]; reason: different surfaces of this texture sit in different repair states) | mirrored-UV | uv-remap-per-surface |
| RealRacing/Source/2019_f1_ferrari_sf90/livery/2019_f1_ferrari_sf90_ext_5.png | 1024x1024 | 4efe1add8f19 | none-bound | as-authored (file on disk; not bound by any runtime material in the local maps) | none | none-needed |

## Evidence + packs
- car16 mirrored-UV pack: `outputs/mistral-orchestration/visual-review/car16-mirrored-uv-pack/PACK.md`
- car16 wheel-detail pack: `outputs/mistral-orchestration/visual-review/car16-wheel-pack/WHEEL-PACK.md`
- fleet visual state: pending the post-fix fleet run (owner battery item = pre-promotion baseline for all 24 cars; shard-matrix fix landed at c4fb876, merged into later heads)
- P5 metrics state: measured by 41837315358/41843317303 (incorporation pending)

## Open decisions (reviewer's)
- 10 pending entries need the per-surface confirm run before promotion

## Verdict

**ACCEPTANCE VERDICT (human reviewer):** ____________________

_cloud-UNVALIDATED; no acceptance from in-tree evidence._
