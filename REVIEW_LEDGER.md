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
