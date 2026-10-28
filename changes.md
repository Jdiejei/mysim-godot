# MySim Godot — complete correction and quality specification

Reviewed: 2026-10-28. Priority one: correct the current Godot game and produce a credible, tested, coherent racing experience. This is a change specification, not a claim that its changes have been implemented or accepted. “Picture-perfect” means meeting the visual and gameplay gates below; screenshots alone cannot establish handling quality.

## 1. Authoritative context and evidence

- Current project: `C:/Users/shash/Documents/MySimGodot`.
- Reviewed handoff: `ANSWER_Q2_PROJECT_PATHS.md`; corroborating documents: `AUDIT.md`, `PORT-STATUS.md`; source inspected: `project.godot`, `src/Main.cs`, `src/CarLoader.cs`, `src/DriveController.cs`, `src/UnityMaterialBinder.cs`, `src/Hud.cs` and relevant source searches.
- Reported editor: Godot4.7.2 Mono/.NET8. `project.godot` declares4.7/C#/Mobile. Pin and record the exact executable/version, SDK, import settings and source commit for every run.
- Assets are a junction from project `assets` to `D:/MySim-godot-assets/assets`. A code-only clone is not a restored game. Validate maps, asset hashes and link/dependency provisioning.
- Treat the provided screenshot montage as a defect-identification reference, not a release baseline: AUDIT reports mixed working-tree states and later edits. Do not assume the montage depicts the newest source.
- Handoff reports no exported Godot player and no videos. It also reports the full lap JSONL was overwritten. Verify current outputs before treating those reports as current facts; preserve them as outstanding gaps until disproved by evidence.
- Old Unity candidate builds are historical references, not the current executable. Do not resume obsolete Unity build controllers or overwrite the Godot project from old instructions.
- Distinguish **source-confirmed**, **reported by another agent**, **hypothesis**, **proposed change**, and **runtime verified** in all subsequent reports. No physics or visual acceptance is granted by this document.

## 2. Execution policy: GitHub compute controlled through CLI

The user wants computation moved to GitHub and parallelized. Make GitHub Actions the default for all suitable batch computation: source compilation, automated logic/physics tests, asset audits/conversion, scene-load checks, export builds, coverage matrices, report generation and supported offline capture/encoding. Use the CLI locally to submit, inspect, download and verify jobs; do not silently substitute long local batches.

Godot is open source and does not need the Unity licensing workflow. That does not establish available runner minutes, paid capacity, proprietary asset redistribution rights, graphics devices or identical headless/rendered behaviour. Preflight these requirements and report concrete blockers. Do not claim a workflow file or a submitted run is successful compute.

### Required CLI workflow

1. Confirm the intended account and repository explicitly using `gh auth status` and `gh repo view OWNER/REPO --json nameWithOwner,visibility`. Several accounts exist. Use the correct stored account without exposing tokens or globally switching other projects unnecessarily.
2. Preserve the current source, inspect uncommitted changes, secret-scan staged content, and publish an immutable tested commit to the appropriate private repository/branch. Do not force-push or overwrite existing work. Preserve all required asset dependencies separately.
3. Pin Godot Mono4.7.2, matching export templates and compatible .NET SDK. Record download URLs/hashes and license notices. Pin third-party action revisions. Disable destructive deployment steps in audit-only jobs.
4. Fetch the authorized private asset bundle using least-privilege credentials, verify SHA256 manifest, and create a runner-local project asset directory. Windows junctions pointing at D: do not exist on a hosted runner. Cache imports by engine + platform + source asset hash + importer settings.
5. Dispatch an existing appropriate workflow with `gh workflow run WORKFLOW --repo OWNER/REPO --ref BRANCH`; inspect the resulting exact run ID/commit using `gh run list`, `gh run view RUN_ID`, `gh run watch RUN_ID`, and `gh run view RUN_ID --log-failed`. Download evidence with `gh run download RUN_ID --dir OUTPUT` and verify manifests. Do not use only commit success as the result.
6. Before retrying a launch with an uncertain response, reconcile matching runs. Retry an unchanged failed job once; preserve both attempts. Distinguish capacity/authentication failure from source failure. Paid runners remain outside prior authorization.
7. Keep the local device primarily for orchestration, report inspection and necessary interactive validation. Local-only hardware/frame-pacing tests require the real target machine; label them separately. If the user insists every batch run remain remote and suitable remote GPU capacity is unavailable, report that blocker rather than presenting CPU/software renders as target-GPU evidence.

### Parallel job graph

| Stage | Parallelization | Output / gate |
|---|---|---|
| Source/asset identity + secret/dependency audit | Independent read-only jobs | Commit, engine and asset manifests; no missing dependency |
| Build + CoreRunner + source lint | Separate jobs after dependency resolution | Compiles and records tests against exact source |
| Deterministic physics/timestep tests | Matrix by car and scenario | Telemetry and comparisons at15/30/60/120Hz render configurations with fixed physics step |
| Lap matrix | Shard60layouts ×24cars by stable keys | Exactly1440 unique pair results, no overwritten records |
| Pit/collision regression | Shard track groups | Entry/service/exit and collision coverage; no teleport pass |
| Car visual review | Matrix24cars, fixed camera/light scenes | Exterior/interior/wheels/material captures |
| Track visual review | Matrix60layouts, fixed reference positions | Material, shadow, geometry and visibility evidence |
| Export + standalone smoke | After required tests pass | Windows executable/PCK, checksums and clean restore run |
| Video and benchmark reports | After builds, only supported graphics runners | Recordings labelled actual GPU/software/headless and exact settings |
| Release candidate | Fan-in of gates, not unconditional | Verified artifacts + remaining defects, no false completion |

Choose `max-parallel` from actual quota and memory, not unlimited fan-out. Repeated asset downloads/imports can erase parallel speed gains. Reuse immutable inputs, isolate shard outputs, set timeouts and artifact retention, and never cancel a useful active run merely to resubmit it.

For sharded results, merge by `(commit, engine, asset-manifest, physics-version, track, car, scenario, seed, timestep)`. Reject missing, duplicate, mixed-revision and exception-bearing rows. Save original failed records and traces. Make missing evidence a failing aggregation gate.

**Current constraint:** earlier GitHub attempts failed before useful execution and Unity cloud authentication failed. Those historical failures are not proof that every current Godot workflow/account is blocked. Check current Godot repositories/permissions/quota. Do not assume either free capacity or failure without inspection.

## 3. Hardware and storage plan

Measured target hardware during this review:

| Item | Observed | Required action |
|---|---|---|
| Laptop | MSI Bravo15 C7VEK | Record power mode, AC/battery, display refresh and thermals for reproducible tests |
| CPU | Ryzen7 7735HS,8cores/16threads | Profile main/render/physics thread cost; do not introduce data races to increase thread count |
| RAM |16,328,011,776bytes installed | Limit concurrent heavy applications/importers; measure working set and paging |
| Dedicated GPU | RTX4050 Laptop,6141MiB VRAM, driver610.88 (`nvidia-smi`) | Confirm the Godot process actually uses this adapter, not the integrated or USB virtual display |
| Other adapters | AMD integrated GPU and USB Mobile Monitor virtual display | Record active graphics adapter/backend in run logs; compare without virtual display only when practical |
| Internal drive | KINGSTON512GB NVMe SSD | Prefer active source/import cache here within a measured space budget |
| External storage | TOSHIBA1TB USB device; Windows media type unspecified | Measure sustained and small-file throughput. Use for originals/archives; avoid assuming it behaves like NVMe |
| Disk label H | Currently mounted D: | Use actual volume identity and verified paths; do not hard-code a nonexistent H: drive |

Proposed desktop baseline to validate, not an achieved claim:1920×1080,60FPS, median frame time≤16.7ms, p95≤20ms, p99≤33.3ms in representative full races after warm-up. Record stutter/loading separately. Aim for a steady-state VRAM budget≤4.5GiB with headroom on the6GiB GPU and game working-set budget around5GiB; revise only from measured scene requirements and published quality tiers. Do not recommend hardware purchases as a substitute for fixing defects.

- Profile CPU/GPU frame time, draw calls, visible meshes/triangles, shadow cost, transparency overdraw, texture residency, RAM, loading and import time on representative heavy/light tracks and full opponent grids.
- Perform a controlled Mobile vs Forward+ experiment on the same commit, assets, scene and laptop. Choose the Windows renderer from measured features/quality/cost; do not assume renaming to Forward+ makes assets correct.
- Keep deterministic Low/Medium/High presets with explicit shadow, AA, texture, LOD, reflection and effect settings. Quality must actually change those settings and persist across restart.
- Remove Android export/emulator/SDK prerequisites from the active Windows task. The user explicitly requested their removal. Do not reinstall them for reference comparison. Use legitimate existing footage or an available device instead.
- Keep original assets, asset maps, test evidence and current source. Clean generated caches only after reproducibility checks; do not delete `.godot` during a live import/run. Never delete whole projects on code-only backup evidence.
- Use content-hash deduplication/references for identical textures and artifact archives; preserve canonical copies and rollback coverage. Never delete PNGs or extensionless files just by extension.
- For future verified GitHub-backed removals, retain restoration pointers with repository, exact commit/release, hashes and original paths. Text markers are not runtime substitutes for missing assets.

## 4. P0 — truth, repeatability and standalone build

1. Freeze one visual baseline: exact commit including working-tree changes, Godot/.NET versions, driver/GPU, import hashes, track/car/camera, viewport, seed, weather, controls and quality tier. Capture the same reference views before/after each correction.
2. Preserve overwritten-results incident in the audit. Recover original1440-pair results from retained artifacts if available; otherwise rerun. Never reconstruct passing evidence from summary counts alone. All new runs use unique run directories and append-only files.
3. Create export presets and a reproducible Windows build in a fresh output directory. Verify required resources reach the PCK/export; the local junction must not hide dependencies. Run from outside the project and from a clean restored copy without editor paths, D: originals or development environment variables.
4. Add asset/scene startup validation: enumerate missing resources/material slots, unexpected fallback textures, invalid meshes, duplicate IDs and incompatible import data. Fail explicit checks instead of silently continuing with placeholders.
5. Publish a component inventory: implementation, verification status, missing features and evidence for each original requirement. Update conflicting `PORT-STATUS.md`/`AUDIT.md` statements rather than retaining “all tracks load” alongside stale “untextured/29cars” claims without dates.

Acceptance: one pinned baseline is reproducible; clean export launches menus/race/results; missing dependencies are reported; evidence survives later tests. No “done” label before this gate.

## 5. P0 — geometry, transforms and car completeness

Source-confirmed: `CarLoader.cs` scales imported scenes, retains LOD_A, hides damage/broken/glow/blur meshes, uses `cull_disabled` in the custom car shader and flips V in that shader. The audit reports winding/UV changes restored some previously black cars. A few successful car views are not all-car acceptance.

- Define one documented import transform for units, handedness, mesh winding, normals, tangents and UV convention. Track scale `(100,100,-100)` and car scale×100 must be validated against known dimensions; avoid another global flip to correct one family.
- Repair incorrect triangle winding at the import/conversion boundary where possible. Correct corresponding normal/tangent handedness. Keep double-sided rendering only for genuinely double-sided geometry, not as a blanket repair. Test backfaces, interior visibility and shadow casting independently.
- Do not blindly flip V for every texture/channel. Validate each source convention, repeating UVs and mask/lightmap sampling; preserve source data and store conversion provenance.
- Create a renderer/surface/material identity mapping. Duplicate renderer names must not silently select the first record or repeat the last material GUID for extra surfaces. Report ambiguity and require correct surface counts.
- Validate all24cars: body panels, wheels/tyres, glass, mirrors, cockpit, steering wheel, badges, grilles, lights, exhaust and optional driver. Classify intentionally unsupported parts instead of leaving visible holes or shards.
- Correct wheel centres, radii, width, axis, spin direction, axle/steering assignment, arch clearance and interior/camera clipping per car. Do not use one car's chassis/collision dimensions for every car.
- Implement explicit damage and light state activation only after complete undamaged geometry is correct. Hidden damage and brake-light meshes are not equivalent to working damage/lights.
- Use appropriate distant-car LODs with visual transitions, shared resources and deterministic thresholds. Do not render only highest detail for every opponent.

Acceptance:24-car neutral-light turntables + exterior/interior close-ups + moving-wheel footage; no reversed/absent panels, stray shards, incorrect material slots, clipping or wrong wheel scale. Dimensions/pivots verified numerically against the correct source.

## 6. P1 — textures, materials and surface finish

Source-confirmed: `CarLoader.CarShader` currently uses base/mask/tint, derives crude roughness/specular from a mask and sets metallic0; alpha is treated with a scissor threshold. `UnityMaterialBinder.GroundShader` forces the shading normal toward world-up. These are deliberate substitutions, not faithful recovery of all source shader behaviour.

- Decode each mask channel from actual source shaders/materials before assigning PBR meanings. Keep albedo in its correct colour space; masks/roughness/normal data must not receive inappropriate colour transforms. Check import compression, mipmaps and filtering.
- Replace a single universal car treatment with validated paint, exposed metal, tyre rubber, trim, glass, grille, decals, emissive/brake lights and interior materials. Preserve livery colour/detail without global brightness multipliers that hide broken bindings.
- Add correct normal/detail maps where source data exists, with correct tangent space, handedness and Y convention. Do not fabricate normals from colour indiscriminately.
- Restore a coherent reflection solution: environment/sky lighting and suitable probes or other renderer-supported methods. Port recovered reflection/fresnel semantics where justified. Avoid mirror-like asphalt/rubber and uniform glossy paint.
- Use genuinely transparent glass with appropriate depth/sorting and tint; alpha-cutout is for cutout surfaces, not an automatic replacement for glass. Inspect overlapping glass and cockpit visibility.
- Review exact Unity mapping first, then ensure the subsequent heuristic `TrackMaterialBinder.Apply` does not overwrite already-correct assignments. Produce a binding-coverage report by scene/surface.
- Replace the forced world-up ground normal with verified geometry normals or a physically justified normal reconstruction that preserves banks, slopes, kerbs and normal maps. Isolate wrong winding from wrong light direction before changing normals.
- Restore detail/decal/lightmap information when source coordinate/pages can be faithfully decoded. If rebaking lighting, validate import topology, UVs and transforms first; do not reuse unknown lightmap atlas indices blindly.
- Eliminate stretched/repeating tyre marks, overlarge grain, incorrect texture scale, wrong atlas pages, pale placeholder strips and accidental flat-colour surfaces. Distinguish a missing texture from intentionally painted runoff.

Acceptance: fixed exposure/lighting before/after captures show correct liveries, road detail, rubber/glass/metal distinctions and consistent texel density; logs have no unexpected fallback materials. Review all layouts and representative weather/angles in motion.

## 7. P1 — lighting, shadows, colour and atmosphere

Source-confirmed: `project.godot` has a4096 directional shadow map and MSAA2×. `Main.cs` creates a sun with shadows and an environment using flat-colour ambient, ACES and additional colour adjustments. The audit reports PSSM cascades,150m shadow distance and no reviewed acne/peter-panning. Therefore “no shadow settings exist” is inaccurate; **effective shadow quality is not accepted**.

- Build a neutral material-review scene and separate it from the artistic race lighting. Establish correct materials under neutral illumination before increasing brightness, saturation, contrast, ambient energy or exposure.
- Choose a coherent time-of-day sun, sky/environment, exposure and white point. Remove incompatible duplicate sky/environment assumptions. Match sky orientation and lighting to source scenery references.
- Verify active sun, cast/receive flags and layer masks for car, wheels, track and scenery. Tune cascade splits, distance, map size, bias/normal bias and filtering by measured scene scale. Current numbers are starting values, not universal quality settings.
- Check tyre-ground contact at rest, over banks/kerbs and while moving. Fix detached shadows, acne, stripe patterns, flicker, leaking light and sudden shadow cutoffs. Do not use a blob to conceal incorrect chassis height.
- Make cockpit/interior exposure readable without washing out exterior paint/road. Test glass and alpha-cutout shadow behaviour.
- Introduce environment/reflection lighting and renderer-supported occlusion only after source materials/geometry are correct. Verify any SSAO/SSR/glow option is supported by the selected exact renderer/version. An unsupported setting does not constitute a feature.
- Restore shadow/lightmap information or validated rebakes for large scenery where appropriate. Avoid baking obsolete/dynamic geometry and preserve the chosen time/weather behaviour.
- Keep fog subtle and distance-consistent. Do not use heavy fog/bloom/contrast to hide missing textures, pop-in or bad geometry.

Acceptance: recorded day/overcast/wet scenarios where implemented, slow camera pan and driving at shadow-cascade boundaries; no noticeable acne, detached tyre shadow or distracting flicker. Capture quality tiers and measure their GPU cost.

## 8. P0 — physics, timestep and collision correctness

The audit reports recovered `Vehicle.Step` dynamics with Godot ray/shape queries and direct transforms, no vehicle suspension, default60Hz physics and headless tests at30Hz. It reports materially different AI lap times at those timesteps. Confirm each claim in the current revision before correction.

- Establish one authoritative simulation owner and fixed-step policy. Separate render delta, physics delta and test timestep. Use deterministic stepping/substeps where justified; do not make render framerate change physical grip, acceleration, damping, steering or lap logic.
- Build a timestep test matrix covering starting, braking, constant-radius cornering, slalom, kerbs and complete laps. Proposed initial gate: comparable fixed-input trajectories/lap metrics within a documented tolerance (e.g.≤1% for suitable timing/speed metrics) across render framerates; define spatial/error tolerances from the scenario, not after seeing failures.
- Reconcile wheelbase/profile values with actual imported pivot geometry; the reported2.47m vs2.70m discrepancy is unresolved. Verify mass, units, grip, aero, braking, gearing and centre-of-mass assumptions per car.
- Distinguish driver-controller oscillation from tyre/chassis instability using fixed-input human-model tests and controller-disabled comparisons. Record steering command, achieved steering/yaw, slip, speed, target speed, acceleration, road contact, normal and recovery state.
- Fix yaw/stall oscillation in `Simulation.cs`/`Autopilot` from measured causes. Do not count a loop as passed after automatic resets, reverse recovery or forced speed correction.
- Decide and implement the intended vehicle model. Transform-driven ray placement is not suspension or rigid-body collision. Add consistent multi-wheel support, pitch/roll, contact transitions, kerb response and grounded/airborne behaviour appropriate to that model. Do not combine independent transform writers and physics bodies without a defined ownership boundary.
- If realistic transmission is required, replace speed-derived display gear/RPM with calibrated engine/ratio/shift behaviour. Do not invent “physical gearbox verified” from a changing gear label.
- Replace hard-coded chassis sweep dimensions with per-car validated dimensions. Sweep full motion with consistent heading, collision mask and recovery when initially overlapping. Test large timestep impacts, corners, thin barriers and off-track surfaces.
- Replace guessed barrier regex coverage in `WallBuilder.cs` with audited source collision identity plus reviewed supplemental geometry. Include kerbs and required scenery contacts. Keep non-solid decorations and transparency out of unintended collision sets.
- Validate road/visible geometry height alignment across the entire track, not three start-region points. Check banked sections, bridges, stacked surfaces and pit lanes. Ground queries need the intended surface/mask rather than any ray hit.
- Reproduce each old failure in the new engine or document why the code path differs. Unity runtime evidence is not Godot evidence.

Acceptance: measured acceleration/braking/turn tests, zero non-finite states, no artificial launches/teleport passes, no unresolved stalls/spins, correct impacts and reproducible full laps. Headless lap success cannot alone establish visual contact, human handling or real collision completeness.

## 9. P1 — camera and input feel

Source-confirmed: chase camera in `DriveController.cs` uses fixed7.2/11m offsets,2.65/4.2m height and speed-varying FOV. Camera moves in `_PhysicsProcess`; the audit reports no render interpolation or obstruction handling. Do not copy old Unity camera values as a substitute for fitting Godot cars.

- Frame the player from measured car bounds, aspect ratio and selected FOV. Initial art target for review: player car roughly18–28% of image height in the normal chase view, with adequate road visibility; adjust through recorded cornering/braking trials rather than treating this range as a physics requirement.
- Keep FOV stable by default; expose bounded speed effects as an option. Do not combine excessive distance and wide FOV that make the car tiny.
- Separate interpolated presentation/camera updates from deterministic physics. Validate60/120/144Hz display behaviour where available, including pause/resume and frame spikes.
- Add camera obstruction sweeps, smooth return and safe minimum clearance. Inspect trackside walls, tunnels, bridges, close opponents and cockpit/bonnet clipping. Exclude the player appropriately from camera queries.
- Fit cockpit and bonnet views per car; use real interior visibility and correct near plane. Fix shards and wrong eye position before presenting cockpit as complete.
- Define reverse, crash, airborne, pit and camera-switch behaviour. Avoid nausea-inducing roll/shake; expose comfort settings and remember user choices.
- Build explicit Godot input actions for keyboard/gamepad and rebinding. Handle device loss/reconnection, simultaneous input arbitration, focus loss, pause, deadzones, sensitivity, throttle/brake priority and reverse safely.
- Validate continuous deadzone response and separate digital steering slew from analog input response. Add speed-sensitive steering only with measured predictable behaviour; assists must be labelled and disableable.

Acceptance: actual human-driven footage for acceleration, braking, slalom, corners, reverse, collision, pit and camera switching; no unexplained input latency, jitter, camera clipping or deadzone jump. Autodrive footage is labelled separately.

## 10. P1 — HUD, text, menus and accessibility

`Hud.cs` currently builds three outlined labels with fixed pixel positioning. This is a functional debug HUD, not a finished visual hierarchy. Rework the interface without hiding useful telemetry.

- Introduce a coherent visual system: spacing grid, restrained typography, scale hierarchy, consistent panels/colours and controller-focus states. Validate1920×1080,1280×720, ultrawide and Windows DPI scaling.
- Make speed/gear readable without dominating the lower-right scene; give lap, position, timing deltas, flags and pit prompts meaningful hierarchy. Use safe margins and avoid obscuring apexes/other cars.
- Example copy specification: `Lap 1 / 3`; `Lap time 00:01.400`; `Best —` before a valid lap; `Tyre condition 99%`; `Fuel 45 L` only if the model unit truly is litres; `Pit: request [P]`, `Pit requested`, `Pit lane`, `Servicing · 7.5 s`, `Rejoining`. These are proposed replacements, not verified units.
- Clamp completed-race display logically (do not showLap4/3), while fixing underlying finish-state errors rather than concealing them. Record invalid laps, penalties and disqualification distinctly.
- Show bindings for the active input device, not keyboard-only prompts on gamepad. Use clear pause/settings/results actions and consistent confirm/back behaviour.
- Verify tyre/fuel/damage units, update rates and rounding. A number appearing on a label does not validate its simulated subsystem.
- Provide settings descriptions for steering/assists/camera/graphics/audio, persistent values, reset-to-default and readable non-colour-only warnings. Honour mute; do not automatically unmute when starting a test.
- Add a readable car/track selector with thumbnails, real names, layout information, race configuration and loading feedback. Remove debug IDs/technical build details from player flows except a separate diagnostics page.
- Verify start/countdown, lap detection, pause, finishing, classification, results, restart, championship progression and return-to-menu with screenshots AND interaction recordings. The screenshot collage is not UI acceptance.

## 11. P1/P2 — full game systems and content

- Inventory all60layouts and24cars against authoritative data, variations, spawn/grid positions, routes, collision, textures, pits and scenery. Do not equate scene-load success with finished tracks.
- Recreate and verify the1440 track/car lap matrix with immutable results. Investigate the24 reported Le Mans Sarthe failures before claiming full coverage.
- Implement/test opponents on full grids: racecraft, braking/avoidance, starts, passing, blue flags/rules where required and collision behaviour. Preserve telemetry and detect reset-assisted completion.
- Complete pits with collision-clear real-lane routes, speed limit, accurate box stop, timed service, occupied-box handling, safe release and rejoin. A no-wall pit demonstration does not clear wall-crossing failures.
- Verify tyres/compounds/wear, weather/wet grip, fuel, damage/repairs, ERS, race flags/penalties and championship standings against the original requirement inventory. Explicitly mark missing features rather than inheriting Unity documentation as proof.
- Restore required engine/tyre/skid/impact/UI audio using proper loops, pitch/volume curves and mixing. Avoid clipped/abrupt sound and respect user mute. Add controls before enabling sound in review sessions.
- Implement required multiplayer wiring and actual two-client host/join/synchronization/disconnect/finish tests. Retained engine-free networking code is not functioning multiplayer UI or transport integration.
- Complete presentation states: brake lights, damage variants, wheel blur/LOD, cockpit steering, driver/pit crew where required and effects. Prioritize correct complete base assets over decorative effects.

## 12. Recording, comparison and evidence

- Produce actual gameplay videos showing menu → configuration → start → driving → pit → finish → results. Include HUD and relevant audio/mute state. Do not substitute60 five-second screenshots or frame captures for a recording.
- Capture a neutral car turntable per car, controlled material/shadow comparisons and representative full-lap recordings. Store resolution, FPS, codec, GPU/backend, renderer, quality, seed, inputs and commit alongside each file.
- Use Monoposto as the requested quality benchmark, not as a source of proprietary assets. Use legitimate reference footage or a physical device without reinstalling the removed Android SDK. Match scenario/camera/speed/lighting as closely as possible; disclose mismatches.
- Compare camera framing/comfort, car finish, shadow contact, road/scenery readability, control predictability, UI hierarchy and frame pacing. Define observable gaps and prioritize fixes. Do not assert equal quality from dissimilar screenshots.
- Record input and telemetry synchronization for handling investigations. A software-rendered cloud video is valid visual evidence only for the stated backend; it does not benchmark the RTX4050.
- Preserve failed videos/screenshots as well as selected successes. Link every accepted correction to exact before/after evidence. Protect privacy in recordings and avoid capturing unrelated desktop content.

## 13. Execution order and acceptance ledger

| Order | Deliverable | Main source / tools | Required evidence |
|---|---|---|---|
| 0 | Pinned baseline + private asset provisioning + CI account/quota check | project.godot, manifests, .github/workflows | Reproducible source/asset/engine identity |
| 1 | Complete standalone Windows export | export presets, build workflow | Clean restore and exported-player smoke |
| 2 | Correct import geometry/material identity and complete cars | CarLoader, mapping tools |24-car review and mapping/no-fallback report |
| 3 | Deterministic physics, collision and AI stability | Logic/Simulation, DriveController, WallBuilder | Telemetry, timestep regressions, repaired lap failures |
| 4 | Controlled lighting, shadows, textures/reflections | Main, material binders, project settings | Fixed before/after scenes + GPU timings |
| 5 | Playable camera/controls and production HUD/text | DriveController, Hud, Settings, Main | Human interaction videos + display/device coverage |
| 6 | Complete tracks and race/pit/system parity | Track/asset/race/network code | Coverage matrix and end-to-end races |
| 7 | Performance tiers, long-session stability, comparisons | Profilers, CI exports/capture tools | Frame-time/memory reports + representative recordings |
| 8 | Final release | CI fan-in + package manifests | All required gates green and no known unresolved critical defect |

Independent audits/tests may run in parallel. Do not mutate shared input trees or overwrite evidence while a run uses them. Do not hide core defects by pursuing atmosphere/polish first. Keep an issue ledger with symptom, cause confidence, source, correction, regression risk and acceptance evidence.

Minimum release gates:

- All required components inventoried; no unresolved required “missing/untested” items concealed.
- Export runs from a verified clean restoration with assets, without editor-only paths.
- All applicable logic, physics, lap, pit, controls, UI and multiplayer gates pass on the final revision. Original failures and current failures remain traceable.
- All24cars/60layouts have appropriate coverage; representative worst cases have full runtime/playability review.
- No missing panels/textures, obvious placeholder HUD, shadow/contact defects, camera clipping, severe input/physics bug or unbounded recovery-dependent pass.
- Performance target established on actual target hardware with stable quality tiers; cloud tests labelled accurately.
- Gameplay recordings reviewed against reference; limitations and intentionally unsupported scope explicitly documented.
- Final executable/PCK, manifests, checksums, restore guide and concise verified-results report are delivered. No claim of literal flawlessness or “finished” from job completion alone.

## 14. Changes this review does NOT make

This document does not change game code, renderer, drivers, assets, quality settings, Git remotes or release state. It does not authorize deleting originals, restoring Unity as the active engine, reinstalling Android tooling, or enabling paid compute. User-authorized project backups and system-storage work remain separate tasks; source-only archives do not justify deleting the asset junction target.

## Official technical references

- Godot renderer feature comparison: https://docs.godotengine.org/en/stable/tutorials/rendering/renderers.html
- Godot4.7 rendering architecture: https://docs.godotengine.org/en/4.7/engine_details/architecture/internal_rendering_architecture.html
- Godot environment/post-processing: https://docs.godotengine.org/en/latest/tutorials/3d/environment_and_post_processing.html
- Official mesh LOD documentation source: https://github.com/godotengine/godot-docs/blob/master/tutorials/3d/mesh_lod.rst

Use documentation matching the pinned engine version when implementing. Newer documentation is supporting context, not proof an option exists in this build. Recheck runtime support and measure effects.
