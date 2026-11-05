# Track rendering — turn 18: road material definition — source-bound diagnosis + the profile candidate (applied)

Author: track-render specialist. In-tree only; nothing cloud-validated; no acceptance claims. The road textures/mesh census from my earlier turns is reused; the new code this commit = one env-gated PROFILE selector in `TrackMaterialBinder.cs` (my file).

## 1. What the road material actually is (source-cited; three layers — the terminology the reviewer's item spans)

1. **The DriveSurface** (`DriveSurfaceLoader.cs`, layer 1): the INVISIBLE collision ground built from `assets/Resources/<Track>_<layout>.txt` — plain OBJ (`v/f` lines, RR3 fixed-point geometry; the header states "RR3 collision geometry, not the rendered track model"). Hockenheim GP: 12,631 verts / 21,866 tris. It CARRIES NO MATERIAL AT ALL (a `ConcavePolygonShape3D`+) — any "road material" discussion about it is really about the VISIBLE road below.
2. **The visible road** = the track layers' `gnd_*`/`road*` meshes (the FBX visuals), bound by `UnityMaterialBinder`'s `GroundShader` (`ShaderMaterial`, not my file): `ALBEDO = texture(tex, world-projected UV).rgb * tint + procedural fbm detail/macro variation`; **ROUGHNESS = mix(1.0, 0.18, water)** — constant 1.0 when DRY; **SPECULAR = mix(0.04, 0.45, water)** — 0.04 dry; metallic 0; **no normal-map sampling at all** (the shader has no normal input; vertex normals only). The wet uniforms come from `weather_profiles.json` `surfaceVisual` via `Conditions.ConfigureWetGround`.
3. **The fallback twin** = my `TrackMaterialBinder.cs` `GroundShader` (identical structure) binding the same classes for tracks the unity map does not cover.

Measured texture content (turn-15/earlier census): `gnd_asphalt_01.png` 512×512, mean RGB (54,54,54) — flat neutral grey, low internal contrast, lossless import, mipmapped, aniso-filtered, ~6.25 m world tiling period (world_scale 0.16), no per-rub/polish variation possibilities in a single albedo.

## 2. What it lacks (ranked; with the honest available lever)

1. **Spatial roughness/specular variation** (racing-line polish, patch wear) — the shader's roughness/specular are CODE CONSTANTS; not reachable from parameters. Owner-side follow-up: expose `rough_dry`/`spec_dry` (+ an optional `rough_var` mod uniform) in the GroundShader — a two-line change; my profiles then extend to them. NOT simulated by faking with albedo (that would be cheap and wrong).
2. **Normal detail** — no normal-map sampling in the shader; no road normal texture exists wired anywhere (the only non-albedo road textures on disk are the unwired `_spec` files). Owner/cloud-scope: a detail-normal path.
3. **Albedo content** — the 512² flat grey; a texture-level (cloud) item.
4. **Tiling phase** — world-anchored sampling has no offset/rotation uniform (the uniform repetition stands; documented before).
5. **The procedural variation amplitudes** — authored near-off (detail 0.10/macro 0.07): REACHABLE from my layer — this commit's lever.

## 3. The candidate: `MYSIM_ROAD_MATERIAL=<profile>` (flag-gated, in `TrackMaterialBinder.cs`)

The old `MYSIM_ROAD_VARIATION=1` single-value flag is generalized into a named PROFILE selector (`RoadProfile`); `MYSIM_ROAD_VARIATION=1` maps to `"definition"` for compatibility; when both are set the profile flag wins. Profiles (exact uniform sets, scope = the `gnd_asphalt*` ShaderMaterials only, dedup per material instance, `ROAD_MATERIAL profile=<name> materials=N` log + `road-material-<layer>.json` audit):

| profile | detail_scale | detail_amp | macro_scale | macro_amp | macro_tint | intent |
|---|---|---|---|---|---|---|
| `base` | 90 | 0.10 | 0.18 | 0.07 | (0.96,0.96,0.98) | the authored values, explicit — the in-run A/B baseline |
| `definition` | 110 | 0.16 | 0.26 | 0.11 | (0.95,0.97,1.00) | finer grain + stronger macro contrast = spatial definition (the prior variation flag's values + the grain) |
| `worn` | 55 | 0.10 | 0.40 | 0.14 | (0.97,0.98,1.00) | broader long-wavelength patches = a worn/blotched read |

`MYSIM_AUTHORED_GROUND=1` still skips the pass entirely (unchanged). Defaults untouched when nothing is set. Audit records name the profile + full parameter values; the older `road-variation-<layer>.json` name is superseded by `road-material-<layer>.json`.

## 4. Validation additions for the battery (the road A/B rides the same fence-depth mechanism as the forest A/B)

The same one-liner pattern as turn-14's forest addition: in the fence-depth python block add a third render `day0-filtered-fence-road_definition` with `MYSIM_ROAD_MATERIAL=definition` (and optionally a fourth `..._road_worn`). Matched-pair comparisons:

- **definition vs base:** the road reads finer-grained and less flat at the drive camera; the albedo/kerbs/line paint untouched; the unified sky/kerb contrast may need the reviewer's eye — reject if the road reads noisy or faster-tiling (the period itself unchanged — only the within-tile variation moves).
- **the no-global-change proof (same as turn-14's):** the five unchanged fence modes stay pixel-identical to run 41843204651's set; the log-absence check for `ROAD_MATERIAL`/`FOREST_TONE`/`GRASS_BALANCE`/`TREE_BREAKUP` markers in un-flagged modes; track 0's corridor edges unchanged (6.0 half).

## 5. Commit

`src/TrackMaterialBinder.cs` (+37/−6: the profile selector + the generalized pass) and this file. Balance 64/64 + 309/309; no CJK; no other files touched; no dispatch; in-tree only (labelled: NOTHING cloud-validated — the battery's matched pairs validate).
