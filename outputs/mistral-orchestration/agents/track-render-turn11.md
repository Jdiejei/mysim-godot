# Track rendering — turn 11: corridor-proposal cross-check (wall geometry), overlay validation checklist (battery item 4)

Author: track-render specialist. Read-only source verification at the fetched tip (`a313492`; fork tip `4566853` lineage included). NO dispatches; NO workflow edits; the `bestLap=0` defect untouched (physics-gameplay's). Companion to the accepted corridor proposal (`agents/track-render-turn10.md`, commit `6d34560`).

## 1. Cross-check: does widening the corridor (12 → 16.4, variant 8) move any collider? — NO (proven from source)

Complete census of every `Circuit.width` consumer in the tree:

| Consumer | File:Line | Effect of width 12→16.4 (variant 8 only) | Verdict |
|---|---|---|---|
| Legality boundary `OnRoad = \|lateral\| < width/2 ‖ InPit` | `Logic/Simulation.cs:113` | boundary 6.0 → 8.2 | THE intended change |
| Pit carve-out `InPit = \|lat\|>width/2 && dist<5` | `Logic/Simulation.cs:124` | carve-out engages only beyond 8.2 | consistent; see pit note below |
| Non-external-physics `onRoad` fallback | `Logic/Simulation.cs:242` | same boundary semantics | consistent |
| Autopilot overtake-dodge clamp `±(width/2−2)` | `Logic/Simulation.cs:287` | ±4.0 → ±6.2 | stays INSIDE the corridor by construction (`width/2−2 < width/2`); the 2 m relative margin is preserved; dodge targets may sit near the apron edge — same relative safety as designed |
| Speed planning (`SpeedAhead`/`latAcc`/`desired`) | `Simulation.cs:296-315` | **NO width reference anywhere in the speed path** | no AI-speed/handling side effect |
| **Wall colliders** (`IsSolid`/`WallFaces`/`AddFaces`/`Build`) | `WallBuilder.cs` | **ZERO width references** — colliders are the barrier MESHES' own triangles (`mesh.Mesh.GetFaces()`), positioned by the meshes themselves | **NO COLLIDER MOVES. The wall ring at that corner (and everywhere) stays exactly where the visible barrier geometry is.** |
| Drive surface | `DriveSurfaceLoader.cs` (OBJ `v/f` verbatim; `Hockenheim_gp.txt` 12,631 verts = the runtime's `DRIVESURFACE verts=12631`) | no width reference | collision ground unchanged |
| Legality predicate | `NativeSurfaceLegality.cs` | consumes `insideRoute` + physics only | unchanged |
| Floodlight placement clearance (mine) | `TrackNightLighting.cs:64` (`\|lateral\| < route.width*.5+2` rejects masts) | clearance band 8 → 10.2 m | automatic and desirable: with the wider corridor, masts also stay out of the 9.0 m pit-lane band (see pit note); lighting behaviour otherwise unchanged |
| Corridor overlay drawing (mine) | `WallBuilder.cs:120/202` (`DrawCorridor` renders `route.width` edges) | the drawn cyan edges follow the patch (8.2 half) automatically | the validation instrument updates itself |

**Conclusion:** the corridor is a LEGALITY-ONLY quantity; the physics walls (WallBuilder mesh-face colliders) and the collision ground (DriveSurface OBJ) are geometry-derived and carry no width dependency. The corridor widening cannot move, grow or remove any collider; the wall ring at segments 52-62 still stops the car exactly at the visible barrier. The only behavioural side-effects are the intended legality boundary, the self-consistent overtake clamp, and my own floodlight clearance band.

**Pit interaction (measured, not assumed):** Hockenheim's `pitRoute` = 200 points at EXACTLY **9.00 m** lateral (real source pit data; endpoints 9.00/9.00 → `PitValid` true; `pitServiceIndex=100`). At the proposed 8.2 half-width the pit lane REMAINS outside the corridor (9.0 > 8.2), so the `InPit` carve-out keeps governing pit-lane legality exactly as before; cars crossing the 6-8.2 m band near the pit entry become plain on-road (they are on the visible apron — intended); the pit box/service geometry is width-independent. No pit regression. (With the OLD 6.0 half-width the same band was carved out by `InPit` only within 5 m of the pit route — the new corridor is strictly more permissive exactly where the surface is real.)

## 2. Overlay validation checklist — owner-run battery item 4 (segments 52-62, the progress-0.1948 corner)

One run at the branch with the corridor patch applied: `-f depth=... ` (owner's choice) with env `MYSIM_COLLISION_OVERLAY=1`, `MYSIM_COLLISION_OVERLAY_AUDIT=<path>`, `MYSIM_NATIVE_FACE_EVIDENCE=1`; camera set below; track 8, car 13 (the failing pairing).

Frames to capture (each 960x540 + one 1920x1080 set for the fine print):

1. **Chase-cam stills at progress 0.1900 / 0.1948 / 0.1960 / 0.1990** (car placed/autodrive paused at each): the corridor cyan edges vs the asphalt/kerb line vs the red wall volumes, with the car at the rejection spot.
2. **Overhead still** (fixed `MYSIM_CAMPOS` above x −343.8 z 410.9, looking down): the single frame that shows corridor-vs-asphalt vs wall-ring in one image; segments 52-62 visible in frame.
3. **Driving frames:** the force-driven lap passing segments 52-62 (chase cam) — 3 frames across the corner — plus the same lap REPLAYED without the overlay for the naked-eye pair.
4. **Pit-entry frame** at the pit entry (where the 9.0 m pit lane merges) — confirm the corridor edge does not cut the pit entry visual.

What CORRECT (post-patch) looks like:

- **Cyan edges sit at the asphalt/kerb boundary** through segments 52-62: inside the visible tarmac+kerb envelope by ≤ ~0.5 m, NOT floating over grass/run-off proper, NOT crossing into any red wall volume's inner face.
- The **red wall volumes still touch the visible barrier** exactly as before the patch (compare any pre-patch frame: wall positions must be pixel-identical — the no-collider-movement proof, visible not just argued).
- The **green drive surface** still covers every cyan-corridor interior (no(corridor-over-void): the corridor must never extend past collidable ground at this corner; measured surface half-extent ~17 m vs corridor 8.2 — ample).
- The car at the formerly-rejecting spot (lateral 6.003-6.019) now reads INSIDE the cyan corridor with visible margin to the edge.
- Numeric cross-check from the audit JSON: `unmatchedCount` unchanged vs a pre-patch overlay run; `driveWidthDeltas` identical; `trims=0` (no trim list supplied).
- Failure mode to reject: cyan edges over grass, cyan edges crossing red volumes, or wall visuals moved by even one pixel (any of those = the patch is wrong and is rejected).

## 3. bestLap=0 (untouched, physics-gameplay's)

Restating for the record: `mounted-native-lap-track-08-car-13.json` has `finished=true, laps=1, bestLap=0`; `FinishAi`'s `ok` requires `bestLap>0` (`NativeRaceController.cs:52` chain), so that run fails on this conjunct INDEPENDENT of the corridor. Not touched, not re-diagnosed here — physics-gameplay's file; the corridor fix alone will not green that specific historical run's verdict rule.

## Commit

This turn: `outputs/mistral-orchestration/agents/track-render-turn11.md` only (documentation turn — no code change required: the cross-check PROVED the proposal is collision-neutral from source, and the corridor-drawing instrument already exists from `40c7df5`). Verification: full source reads cited per line; measurements reproducible from the named artifacts; no dispatch, no local engine/exec, no shared-file edits.
