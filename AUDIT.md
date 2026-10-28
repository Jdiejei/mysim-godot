# MySim engine migration and quality audit

Labels: [V] verified fact (command/render/log exists), [S] suspected cause, [U] implemented but untested, [M] missing.
Everything below was checked on 2026-10-28 against the tree at `C:\Users\shash\Documents\MySimGodot` (HEAD d5403d9 plus uncommitted edits listed in A3).
The question list I received stops mid-sentence at item 41, so items after 41 are NOT answered.

## A. Engine and project

1. [V] Godot 4.7.2.stable.mono (`Godot_v4.7.2-stable_mono_win64.exe --version`), C# on .NET 8, renderer "Forward Mobile" (project.godot `rendering_method="mobile"`), Vulkan 1.4 on RTX 4050 (run log), MSAA 2x, 4096 directional shadow map. Physics: Godot Physics 3D is used ONLY for ray/shape queries (ground height, walls). Vehicle dynamics are NOT Godot physics: they are the recovered C# model in `src/Logic/Simulation.cs` (`Vehicle.Step`), stepped from `DriveController._PhysicsProcess`.
2. [V] Project `C:\Users\shash\Documents\MySimGodot`; raw assets `D:\MySim-godot-assets\assets` (junction `assets`); import cache `...\MySimGodot\.godot`; originals untouched at `D:\MySim-recovered` and `D:\My Sim`. Tests: `tools\LapRunner` (headless lap matrix, `laps-*.jsonl`), `tools\CoreRunner` (153,203 assertions). Logs: `shots\log.txt`, `import*.log`, `move_assets.log`. Stills: `shots\t0..t59.png`, `renders60\`, `renders60.zip`, `compare_*.png`. [M] NO exported executable exists (the game only runs through the Godot binary with `--path .`). [M] NO video recordings exist, only still PNGs.
3. [V] The 60-track montage is NOT from one clean commit. Timestamps: 51 images rendered 13:44-14:30, 9 re-rendered about 14:31-14:42, from the working tree around commits 735e937/f0aec7d; later edits (car shader, culling) are newer than the montage. Visual code was the same family but the build was not pinned. [S] Treat it as a snapshot, not a release.
4. [V] Unity was replaced because: the Unity build stalled twice on a slow HDD (log silent 45 min, then 90-min limit) and later the Unity license token failed (`build-attempt-3..9` logs). [U] There is no measured performance comparison Unity vs Godot; I have no frame-time data for either. Regressed: no packaged build, no touch/tilt, no audio, no multiplayer wiring, no pit crew. Improved: runs and renders here; all 60 tracks load.
5. [V] A rebuild, not a migration. Retained unchanged: recovered race logic (12 files in `src/Logic`: Simulation, Network, routes, profiles, CoreChecks) which pass 153,203 checks. Rewritten: rendering, cameras, HUD, menu, input, materials, loaders (Godot). Approximated: car shader (see B12), ground shader, lighting. Omitted: audio, pit crew/drivers, multiplayer wiring in Godot, championship, weather visuals, ERS/damage visuals, touch/tilt, Android export, 452 of 476 car models (only the 24 menu cars).
6. Applicable from Unity era: recovered logic and its checks; the slope-sign finding in `World.PlaceOnGround` is not applicable (Godot placement is a different code path). Need fresh verification: everything driving-related in the new engine (see E).
7. [V] Running now (process list at audit time): `dotnet build` only. Emulator not running (no qemu process). Earlier jobs finished. Planned only: Monoposto comparison (not installed), full car catalogue, lightmaps.

## B. Car import and completeness

8. [V] Player car uses LOD_A (highest detail). Vertex totals per LOD group, car 8 (McLaren 650S): LOD_A 164 meshes/84,525 verts for car 7, 109,493 for car 15; LOD_H is 146-189 verts. `CarLoader.Load` hides everything not named LOD_A. Distant LODs are not selected. [M] No LOD switching exists for other cars on track.
9. [V] Present: body, doors, bumpers, glass, interior set (cockpit camera only), mirrors, 4 wheel assemblies at the Unity wheel pivots, exhaust. [V] Deliberately hidden: DAMAGE, BROKEN, CRACKED, SHATTERED, BONNETCAM, LIT_ glow quads, WHEEL_BLUR, SCRATCH. [M] No damage swap, no brake-light toggle, no blur swap at speed, no contact-shadow quad, no driver model, no headlight glow at night.
10. [V] Not preserved correctly until today: the recovered meshes keep reversed triangle winding (`cars-fbx/README`: "reverse-facing triangles are retained"). Godot culled them, so we saw the inside of the body. [V] UV0 V values are negative (e.g. BODY_mm_ext V -1.00..-0.06), a flipped convention. [V] UV1 absent in these car meshes (UV1=none). Normals/tangents/axes not audited beyond visible results. Units: the FBX are in cm, scaled x100.
11. [U] "Encoded UV pages" were not examined for cars; textures are single 1024 atlases. For tracks, lightmap UV2 pages (sprites_N.png) are NOT used. [M] Lightmaps missing.
12. [V] Car shader: Unity `fba824ff...` replaced by `CarLoader.CarShader` using only `_BaseMap`, `_MaskMap`, `_AmbientColour`. LOSSES: `_ReflectionMap` (env reflection), `_FresnelFeedbackScale`, `_ReflectionContrast`, `_SpecBase/_SpecTop`, `_RotorGlow` are ignored. Track: Unity materials -> StandardMaterial3D (albedo/colour/alpha clip) or a ground shader; detail/lightmap slots ignored.
13. [V] ROOT CAUSE FOUND: black cars were reversed-winding culling, not lighting. Evidence: `compare_lit_vs_albedo.png` (albedo-only render was equally black, so not lighting), `livery_rgb.png` (source texture is bright orange, mean RGB 141,73,26), then `compare_cull.png` (culling disabled: orange 650S, yellow/green P1, silver W10 all correct). Fix applied in `CarLoader.cs` (`render_mode cull_disabled`, V flipped). [U] Applied to the shader path only; other cars not individually inspected.
14. [V] Roughness/specular are crude: `ROUGHNESS=mix(.65,.22,mask)`, `SPECULAR=mix(.35,.9,mask)`, metallic 0. [U] Normal maps: none used. Alpha: badges/lights use scissor, glass uses `queue>2450` scissor not true blending. Emissive lights: [M] none.
15. [U] Wheels: positions come from Unity `WheelPivotN` local positions (`assets/car_pivots.json`, extracted from `Car{n}.prefab`), spin and front steering code exist in `DriveController`. [M] No close-up footage of moving wheels, no suspension travel, no radius check per car.
16. [M] No neutral-lighting turntable footage. [V] Stills exist: `carsheet*.png`, `compare_*.png`. Known defects now: wheels/arches proportion not validated, stray interior shards in cockpit view (cam2b.png), no reflections, dark tyres, glass opaque-ish.

## C. Lighting, shadows, rendering

17. [V] `Main.cs`: DirectionalLight3D (energy 1.7), ProceduralSky background, AMBIENT = flat colour (0.62,0.66,0.72) energy 1.0 (not sky/IBL), ACES tonemap (exposure 1, white 6), contrast 1.12, saturation 1.1, exponential fog density 0.00025. Reflections: [M] none (no ReflectionProbe, the car reflection texture is unused).
18. [V] Real dynamic shadows are enabled: `ShadowEnabled=true`, PSSM 4 splits, max distance 150 m, blend splits, bias 0.03, normal bias 1.0, blur 1.0, shadow map 4096 (`project.godot`). Casters: all meshes default to cast shadows except the sky dome (off). Receivers: all lit materials; the ground uses a custom shader that also receives. [U] Not reviewed for acne/peter-panning.
19. [V] Car shadow = real dynamic shadow from the sun (visible in renders). No contact-shadow quad, no baked shadows.
20. [V] See 18. [M] No per-quality tiers.
21. [S] Flat vs dark: the dark scenery faces are lit only by the sun + flat ambient and have no lightmap, so faces away from the sun go dark. Reversed winding on track meshes is a second suspect (see D below). [U] Not separated by experiment yet.
22. [U] Colour space: Godot imports PNG as sRGB colour textures with `source_color` in shaders; the car shader declares `source_color`. [U] Not checked on masks. Pipeline is Mobile renderer, linear lighting by default; HDR/glow not enabled.
23. [V] Settings present: MSAA 2x, anisotropic filtering on track materials, mipmaps (Godot import default). [M] No SSAO/SSR (not supported on Mobile renderer), no reflections.
24. [V] Seen: inverted faces (fixed for cars; ground needed a shader), dark scenery blocks, a pale strip beside the Brands Hatch left barrier. [U] No z-fighting, flicker, or LOD pop checked: no moving footage.
25. [V] Before/after pairs saved: `latest.png` vs `latest2.png` (track cull), `compare_lit_vs_albedo.png` vs `compare_cull.png` (car). I did not use extra brightness to hide material defects.

## D. Camera and presentation

26. [V] Chase camera: 7.2 m behind, 2.65 m up, look-ahead 3..7 m by speed, FOV 63..71 with speed, position smoothing exp(-6 dt). A 4.7 m car at 7.2 m with 63 degrees FOV occupies a small area; this is a choice copied from the Unity revision, not fitted to vehicle size. [S] Too far for a Monoposto-style feel; not compared yet.
27. [V] No: distance is fixed, not scaled by car dimensions or aspect ratio.
28. [V] The camera is moved inside `_PhysicsProcess` (60 Hz tick) toward the physics pose with smoothing; there is no render-pose interpolation. [S] Visible jitter possible at high monitor refresh rates; untested.
29. [V] Camera is NOT rolled with the chassis (it looks at a point with world up). Slopes: car pitch comes from the ground normal, the camera height follows car position + fixed offset. Jumps/reversing/collisions: [M] no special handling.
30. [M] No obstruction handling.
31. [V] Modes: chase, far chase, cockpit (original interior set), bonnet; switch on C / pad Y. [V] Stills exist for cockpit and bonnet. [M] No continuous footage of switching.
32. [V] The montage images were taken by the normal player camera path with an automated driver (`MYSIM_AUTODRIVE`), not a human session; the real player sees the same camera. They were NOT hand-driven.

## E. Driving and physics

33. [V] Cars are simulated by the recovered kinematic-dynamic model (`Vehicle.Step`: friction-circle tyres, downforce, drag, ABS/TC, steering limit). Loop: `DriveController._PhysicsProcess` reads input -> `Race.Step(dt)` -> raycast onto `DriveSurface` mesh for height/normal -> sets the car transform; wheels spin and front wheels steer from `Player.steering`. There is no suspension and no PhysX/Godot rigid body for the car.
34. [V] The ~100-101 km/h, gear 3 in the montage is an automated test: `shoot_all.sh` sets full throttle for 5 s (`MYSIM_SECONDS=5`, `MYSIM_AUTODRIVE`). 28 m/s at 5 s matches the headless straight-line test (28.5 m/s). Gear and RPM are DERIVED from speed in `Vehicle.Step` (`gear = 1 + speed/(vmax/gears*.92)`), so they are display values, not a transmission.
35. [V] HUD reads real state: speed, gear, lap, lap time, best lap, tyre wear, fuel, damage, pit status (`Hud.cs` from `Player`). Gear/RPM are the derived values above. Fuel/damage are simulated values but the HUD has no fuel-consumption verification.
36. [V] Tyre forces, drivetrain, braking, aero (downforce/drag), weight split: all in `Vehicle.Step`. Weight transfer: [U] only front/rear static split `fzF/fzR`; no dynamic transfer. Suspension: [M]. Collision with walls: `DriveController.ResolveWalls` (box sweep, ported from Unity), car-vs-car: `Race.Collisions`.
37. [V] Physics tick: Godot default 60 Hz; sim dt = physics delta (clamped to 0.1 s in `Race.Step`). No substeps. Headless lap matrix uses dt=1/30. [U] Frame-rate dependence not measured; results differ between dt=1/30 and 1/60 (BH AI lap 154.7 s vs 136.8 s).
38. [V] Mass, power, grip, brakes: `RecoveredVehicleProfiles.cs` arrays (24 cars). Wheelbase: `RecoveredGeometry.cs`. Wheel pivots: from Unity prefabs. [S] Wheelbase in the sim (2.47 m for car 8) differs from the pivot spacing in the prefab (2.70 m); not reconciled. Centre of mass, suspension travel, contact patches: [M]/not used.
39. [V] Reproduced in the new code path: stalled acceleration / spinning. Evidence: LapRunner telemetry, Brands Hatch and Le Mans AI: yaw rate about -1 rad/s at steer 0.08, acceleration swinging +5.6 to -3.8 m/s^2, speed stuck about 13 m/s (target 40+). Human straight-line: accelerates to 45 m/s in 10 s (model itself is not stalled). Vertical launch and slope placement: [U] not reproduced; Godot placement is by raycast and uses ground normal only. Pit failures: [U] AI pit cycle completes on Brands Hatch once; NOT verified against the Unity barrier-crossing failure.
40. [V] Masking features: (a) `Race.Step` stuck detector calls `Reset(v)` after 3 s below 1.5 m/s while off the road; (b) AI `recoveryRemaining` reverse when stationary > 3 s; (c) wall response scrubs speed (`into*1.1`, damage); (d) `Wrap01` rounding fix (a real bug fix, not a mask); (e) `newLong = max(0, ...)` forbids negative speed except in reverse mode; (f) steering rate limit 3.2 rad/s; (g) traction/ABS assists on by default. No teleports in the Godot layer except initial placement.
41. [V] Road collision = recovered drive-surface mesh (separate from visible geometry); barriers = 731 render meshes selected by material-name regex on Brands Hatch. [S] The regex is a guess: some barriers may be missing and some non-barriers solid; kerbs and scenery have no collision. [V] Only the BH start region was checked: drive surface height matched render terrain at 3 sample points. Chassis penetration: [U] not tested.

## Corrective plan (ordered by evidence)

| Problem | Root cause | Files | Fix | Verification | Effort |
|---|---|---|---|---|---|
| Black cars | reversed winding + V convention [V] | CarLoader.cs | applied; re-check all 24 | 24-car turntable at neutral light | small |
| Dark scenery blocks | winding/lightmaps [S] | UnityMaterialBinder.cs | test cull-disabled vs flip; add lightmap UV2 | before/after stills per track | medium |
| Car looks small/far | fixed camera offsets [V] | DriveController.cs | scale by car length/FOV, compare Monoposto | side-by-side stills + footage | small |
| Spin/stall AI and car | sustained yaw oscillation [V] | Simulation.cs `Vehicle.Step`, `Autopilot` | isolate with human PD-steer test, then fix | LapRunner matrix + telemetry | large, unknown |
| No reflections/lighting fidelity | shader losses [V] | CarLoader.cs | port reflection map, fresnel | close-ups | medium |
| No packaged build, recordings | missing [V] | export presets | Windows export + screen capture | run exe | small-medium |
| Collision completeness | regex guess [S] | WallBuilder.cs | derive from Unity layer-8 set | pit + wall tests per track | medium |
