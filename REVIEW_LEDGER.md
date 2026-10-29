# Review ledger (spec sections 7-14): status as of 2026-10-28

| Spec item | Status | Evidence |
|---|---|---|
| 8: one authoritative sim owner + fixed-step policy | DONE (code) | `Race.Advance` (Simulation.cs): frame deltas accumulate and are consumed in constant 1/120 s substeps; `DriveController` now calls Advance. Render FPS no longer changes physics. |
| 8: timestep matrix gate (<=1%) | PASS for AI laps | LapRunner `FRAMEDT` = 1/30, 1/60, 1/144, 1/240: lap times identical (BH/car 8 154.6 s; Indianapolis/car 3 288.8 s; Catalunya/car 8 378.2 s). NOT yet covered: braking, constant-radius, slalom, kerbs, human-input trajectories. |
| 8: sim result depends on step size | FINDING | Same tracks at the old variable 1/30 step gave BH 136.8-154.7 s, Indianapolis/car 3 89.5 s; at fixed 1/120 Indianapolis/car 3 is 288.8 s. The recovered model/AI behaves very differently at 120 Hz, so the 120 Hz choice itself needs validation against the original step size. |
| 8: yaw/stall oscillation | OPEN | telemetry in tools/LapRunner; cause not isolated |
| 8: wheelbase 2.47 vs 2.70 | OPEN | unreconciled |
| 8: real vehicle model (suspension/rigid body) | OPEN | transform placement only |
| 8: gear/RPM derived from speed | OPEN (labelled cosmetic) | Simulation.cs gear/rpm formulas |
| 8: per-car sweep dimensions, WallBuilder regex, full-track height alignment | OPEN | |
| 9-10: camera obstruction/interpolation/framing, input actions, HUD rework | OPEN | |
| 11: 1440 matrix re-run after fixed step, Le Mans Sarthe 24 failures, pits, audio, multiplayer | OPEN | prior matrix: 1416/1440 at variable step |
| 12: recordings/comparison | OPEN, no video, Android tooling removed (spec: do not reinstall) | |
| Windows export (order 1) | OPEN | no export templates installed, no exe |

## Update: yaw/stall root cause found and fixed (controller-free evidence)
- Test: `LapRunner steptest <car> <steer> <throttle>` (human-model, no AI). Before: a constant steering input of 0.1 (steering angle 0.05 rad, about 3 degrees) at 17-21 m/s produced yaw 1.78-2.47 rad/s, repeated 360-degree spins, speed collapsing to 4-15 m/s. Expected steady yaw about 0.45 rad/s.
- Cause [verified by change]: in `Vehicle.Step` the rear friction circle (`CircleClamp(latR, rearLong, ...)`) used the UNREDUCED drive force (`rearLong = driveForce + ...`) before traction control cut it, so full throttle consumed the whole rear circle and rear lateral force collapsed (about 0 N) -> oversteer snap. Traction control then reduced the force too late.
- Fix: `Simulation.cs` `if(traction) driveForce = Math.Min(driveForce, mu*fzR*.75f)` right after the hybrid term, BEFORE `rearLong`. After: same input gives steady yaw 0.23-0.30 rad/s, no spin, speed reaches 25 m/s.
- Effect on AI laps (fixed 1/120 s): Brands Hatch/car 8 154.6 -> 78.9 s; Catalunya 378 -> 183 s; Le Mans Sarthe now finishes (425.5 s); Bathurst 220.9, Spa 275.0, Monza 188.6.
- New failure: Indianapolis(10)/car 3: AI runs 12 m wide at the 25 percent gate (offTrack 1.46 s) so lap gates are never registered (laps=0). That is an AI braking/cornering problem, not a spin. Full matrix re-run in progress (`tools/LapRunner/full_fixedstep.txt`).

## Update: wheelbase reconciliation and camera fit
- Wheelbase (spec 8): sim table vs Unity WheelPivot spacing: 6 cars equal, **18 of 24 differ by 0.03-1.35 m** (F1 2.9 vs 3.7 m, Oreca 1.66 vs 3.01, P1 GTR 1.48 vs 2.68, Aston GTE 1.89 vs 2.76). Pivot spacing matches real cars, so `RecoveredGeometry.Wheelbase` was replaced by the pivot-derived values (old table kept in a comment). Matrix re-validation pending (LapRunner exe was locked by the running matrix).
- Camera (spec 9): chase distance/height now from measured car bounds (distance = 1.3 x car length, height = 1.35 x car height + 0.25 m, FOV 58-64). Car 8 (4.72 x 1.32 m) fills about 24 percent of image height (`camfit.png`); F1 about 20 percent. NOT yet done: render interpolation, obstruction sweeps, human footage.

## Update (evening): orientation, export, UI
- [V] Car was mirrored: `Car.Basis = new Basis(-right, n, -f2).Orthonormalized()` was left-handed (det -1) and Orthonormalized() kept the mirrored Z, so the nose faced the camera (external audit was right). Fixed: `new Basis(n x f2, n, f2)`; chase view now shows tail lights/diffuser, cockpit shows steering wheel/cage. Wheel spin sign flipped to match. Evidence: orient.png, pause1.png.
- [V] Livery decals read backwards with U not flipped; shader now samples (1-U, 1-V). Evidence: ucompare.png.
- [V] Pause is a state (Esc/Tab): freezes race + camera + wheels, overlay Resume/Restart/Main menu (pause1.png). UI scales via content scale 1280x720 expand (menu_scale.png: 960x540 and 1920x1080 identical layout).
- [V] Windows export: exe runs the C# code (needs MySimGodot.sln; added). First two exports could not load FBX/PNG: Godot cannot export files reached through the D: junction (592 "Can't open file" errors in export3.log). Fix in progress: real asset copy under the project (tools/export_real.sh) then re-export.
- [V] LapRunner exits non-zero on failure and fails reset/recovery-assisted laps: 1440/1440 clean passes (laps-full-assist.jsonl).
- [OPEN] audit items not started: pause for pad focus, selection previews/search, settings schema, audio, network catalogue bounds (R04), time-trial vs race flow (U04), clean-machine export test, performance numbers.

## Update: AI braking and valid-lap gate
- [V] Core check "Invalid lap timing" regressed after the grip fix: Laguna Seca AI ran 6.9 m off track at the corkscrew on both laps, so bestLap stayed 0 (lap validity protection worked). Cause: AI braked at 100% leaving no front lateral grip (car went straight while steering -0.26). Fix: trail braking in `Autopilot` (brake scaled down with steering). Laguna worst lateral 6.9 -> 2.4 m.
- [V] Gate now: every one of 1,440 pairs must FINISH, with zero resets/recoveries, AND have a valid (on-track) lap (`bestLap>0`). Result 1440/1440 (`laps-full-valid.jsonl`, laps 47-389 s). LapRunner exits non-zero on any failure.
- [V] CoreChecks PASS (87,884 assertions; lower than before only because the AI now finishes in fewer ticks). Workflow `core.yml` now checks `^PASS n assertions` and no FAIL line.
- [V] Network.cs bounds changed to full catalogue (60 tracks, 24 cars). [U] No network integration test on non-legacy IDs beyond core loopback.

## Update: standalone export + car materials
- [V] Standalone Windows export works (build/MySim.exe + MySim.pck 2.24 GB + data_MySimGodot_windows_x86_64): from a clean run it loads Brands Hatch (2860/2860 surfaces bound), sky, McLaren 650S with 92 car materials, wheels, HUD; 0 ERROR lines (build_smoke2.png). Requirements found: a .sln beside the csproj; assets must be real files (not a junction) under the project; directory listings must strip .import/.remap suffixes.
- [V] 24-car showroom (carshots/sheet0-1.png): 20 correct liveries; 4 black cars (BMW M4 GT4, 911 RSR, Aston GTE, 919) had Unity materials with empty _BaseMap -> now fall back to the car's own livery; fixed in editor (fb_sheet.png). Camaro ZL1, Chiron and Oreca are legitimately dark liveries (livery brightness measured).
- [OPEN] Not yet tested in the exported player: the livery fallback (needs re-export). Not done: clean-machine test (no D: / no dev tools), exe size/packaging manifest, checksums.

## Update: cloud crash campaign (run 1, old code)
- [V] Built a headless crash harness (`CrashBatch.cs`, shared `Contact.cs` ground+wall logic) and baked barrier collision for 29 circuits (`data/col/*.gz`, 21 MB) + drive surfaces for 60 layouts (`data/surfaces`). Runs on GitHub Linux runners with Godot 4.7.2 mono (no art assets needed).
- [V] Run 1: 1,213,920 crashes (60 layouts x 24 cars x every 100 m x 3 speeds x 8 angles), 53.2% with >=1 flag: ESCAPED_TRACK 31.1%, STUCK_AT_WALL 21.7%, VOID_GROUND 12.9%, SPEED_GAIN 7.6%, NO_WALL_CONTACT_AT_HIGH_ANGLE 1.7%, GROUND_JUMP 1.7%. Worst layouts 64-76% flagged (Hockenheim National, Suzuka East, Singapore, Silverstone National/Southern, Daytona Roadcourse, Le Mans). Report: crashmap/CRASH_REPORT_run1_before_fixes.md.
- [V] Cause analysis: car pinned nose-first against walls under throttle (no glancing response); barrier coverage gaps (regex by material name) let cars leave the track; ground ray picked barrier tops / far decks earlier (fixed before the campaign).
- [U] Fixes written, NOT yet verified by data: wall sliding (heading rotates toward wall tangent), invisible containment (put back + bounce when no drive surface under car or >30 m from centreline), SPEED_GAIN now counts only gains at wall impacts. Run 2 (300 m spacing) in progress to measure.
- [V] Limits: free Actions minutes (2,000/account/month on private repos) are the cap. blackwingftc-lang is exhausted; shawshanker18 has a billing block; the 5.2M-crash plan (25 m left+right, 10 m on bends) needs ~240 runner-hours, far above ~100 free hours.
- Not changed: JSON flags are heuristics (ESCAPED_TRACK = >25 m lateral; STUCK = speed<0.5 and >6 m lateral). They do not prove visual correctness.

## 2026-10-29 sheets s00-s06 (tracks 0-27), new physics build
- Mirrored/upside-down sponsor text: Berlin (Julius Bar), New York/2021 (ABB, Formula E), Nurburgring (header banner), Sebring (REAL RACING), Richmond. Source PNG is stored V-flipped; V-flip alone gives horizontally mirrored text, so the mapping needs a per-mesh orientation check (not a global texture flip). Experiment reverted.
- Start gantries on Monaco/Suzuka/Hockenheim show low-res checker texture (upscale candidate).
- Dubai/Bathurst kerbs over-saturated; Sebring has a stray dark wedge beside the car; Brands Hatch has a thin black wire line across scenery.
- Laguna variation FBX is a 5 KB empty placeholder in the original: load error harmless.
- s07-s08 (tracks 28-35): Daytona wall lettering mirrored (same text-orientation fault); Catalunya start gantry checker blurry/mirrored; Yas Marina has black rectangles at left stand and a hard dark shadow band; tracks 29/33/34 show "no pit lane" (13 layouts lack pit data); Daytona road course car 32 ends up angled at wall at 5 s (autodrive, expected).
- 2026-10-29 UV fix: all non-ground track materials now use 180-degree UV; signage (nurburgring, Julius Bar, MOTUL, FIA, ABB) reads correctly on re-render of all 60 tracks. Open: floating bush blob on Silverstone Southern, repeated-A gantry placeholders, kerb saturation, Sebring dark wedge, Yas Marina black rectangles.
- 2026-10-29 after upscale+saturation+UV: sheet s00 re-reviewed: signage right, kerbs natural, Brands Hatch black wire line gone. Open: BMW M4 livery has grey smudge near rear lower body; Bathurst thin green wing-height ribbon; Sebring wedge; Yas black rects; bush blob Silverstone Southern.
- Sebring dark wedge = long sun shadow of a floodlight pole (not a defect). Placeholder A/B/C sponsor strips hidden on gantries/flags (re-rendered, verified Silverstone).
- Pace note: F1 sustained lateral accel measured 1.1-1.5g (expected ~2g with downforce); steady-state corner shows a yaw wobble at full lock. Tyre model tuning item: raise peak slip-angle stiffness / check rear traction share. AI cornering plan is not the limiter.
- Yas Marina black rectangles gone after UV/placeholder fixes (verified t28). Remaining visual items: Silverstone Southern bush blob, BMW livery smudge.
- BMW M4 'smudge' is the baked AO islands in the livery texture (by design). Pit crew: 5 placeholder figures during service.
- 2026-10-29 BUG FIXED: in-game Player.carModel was never set, so every car drove with car 0 (Ferrari 488) mass/power/downforce/brakes. Now set from the selected car; in-game lap trace matches headless sim exactly (Brands Hatch F1 p=.274 at t=16).
- Cars: F1 wheel-guard meshes were duplicated onto every wheel (glassy bubbles) and rear tyre/wheel meshes onto all four wheels; fixed: guards stay on body, front/rear wheel meshes split by axle.
- Local crash run (F1, Brands Hatch, 250 m spacing, 3408 crashes): 1092 flagged. ESCAPED_TRACK/NO_WALL clusters at p=.15-.2,.45-.5,.65-.75 are open grass/gravel run-off (verified p=.15 both sides: grass bank left, gravel + tyre wall ~25 m right); car slows to ~24 km/h and is contained at 30 m. STUCK_AT_WALL 460 = head-on 90-degree hits that stop dead (recovery by reverse). New env MYSIM_STARTP=<progress>.
- 2026-10-29 TREES FIXED: the global 180-degree UV rotation (signage fix) also rotated foliage atlases, turning leaf cards into brown bark planks and the Silverstone 'floating bush'. Foliage (org_*) and crowd meshes now skip the rotation. Verified Brands Hatch, Silverstone, Laguna, Berlin signage.
- 2026-10-29 F1 pace: physics+AI plan now share Dynamics.GripMul (slope 1.2, max 2.8). Silverstone 147->120 s, Brands Hatch 54->45 s. Raising grip further (1.6/3.4) or power (x1.7) fails on track 54: at ~72 m/s the AI plan has no corner limit once downforce-adjusted curvature <= 0 (downforce term cancels curvature) and the car runs wide at a kink in the 16 m centreline. Fix needs a speed cap from the unadjusted curvature or a smoother line; not done.
- Fix: the AI corner plan now also caps speed from the unadjusted curvature (MYSIM_LATCAP=50 m/s2) so downforce can no longer cancel a corner; with wing-car downforce x2 (MYSIM_DOWNM) F1 Silverstone 120->112 s, matrix 1438/1440.
- 2026-10-29: AI planned braking factor 0.45 -> 0.38 (MYSIM_PDK); lap matrix 1440/1440, the two car-7 misses (tracks 28, 51: arrived at the hairpin ~20 m/s too fast) are gone; F1 Silverstone 112.6 s.
