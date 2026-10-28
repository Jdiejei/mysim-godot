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
