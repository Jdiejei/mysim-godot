# physics-gameplay addendum — geo8 `bestLap=0` diagnosis — 2026-11-04

Assignment: diagnose from source why `bestLap` stays 0 on a completed AI lap (`finished=true, laps=1`) in the geo8 run (track-render's `6d34560` finding at their run); propose the minimal fix; no corridor files, no WallBuilder, no rule weakening. Committed this turn: the lap-validity evidence block in my controller (`src/NativeRaceController.cs`) + this document. No dispatches.

## 1. Source diagnosis — it is NOT timing

The lap-completion path (`src/Logic/Simulation.cs`, `RegisterProgress`): a line crossing completes a lap only through the **ordered gate sequence** (gates at 0.25/0.50/0.75 then the wrap with `nextGate==4`); on completion it assigns, in order:
`completedLaps++; lastLap=lapElapsed; if(!lapInvalid&&…) bestLap=lastLap; lapElapsed=0; nextGate=1; lapInvalid=false; …; if(completedLaps>=laps) finished=true;`

So on a completed crossing `lastLap` is assigned WITHOUT condition; `bestLap` is the STRICT-lap gate: it stays 0 whenever `lapInvalid` was set at the completion instant. `lapInvalid` is set by **any sample outside the route corridor** (`if(!onRoad){offTrackTime+=dt; lapInvalid=true;}` — one tick suffices) and clears only at a line crossing — i.e. an off-corridor excursion invalidates the WHOLE lap by design (real-racing semantics; the CoreChecks test suite pins this: "Invalid lap entered best-lap records" expects `bestLap==0` for laps crossing with the flag).

For the native path `onRoad` = `onDrivingSurface` = `NativeSurfaceLegality.IsLegalWithWheelSupport(space, position, track.OnRoad(progress,lateral), grounded)` — the route-corridor bound (`|lateral| ≤ routeWidth/2`) AND the physical wheel-support check.

## 2. The measured fingerprint (the geo8 artifact, read-only)

`geo8-41880754125/plain/review/mounted-native-lap-track-08-car-13.json` (car13/track8, dry, Medium, rotational, jolt-class run): 

| Field | Value |
|---|---|
| finished / laps / ruleResets / impactEvents / damage | true / 1 / 0 / 0 / 0 |
| time / distance / requiredDistance | 355.74 s / 4646.0 m / 4331.1 m (the lap distance gates PASSED) |
| **bestLap** | **0** ← the flagged failure |
| lap-completion wrap | PRESENT: the progress trace wraps 0.9→0.0 at t=355.74 — the lap DID complete through the ordered crossing (also an earlier wrap at t=4.85: the spawn-line crossing that started the lap clock) |
| **invalid samples** | **5789 of 7109 (81% of the run)** |
| first invalid | **t=66.0, progress 0.1948, lateral 6.02 → 7.0 climbing, speed 3.8 m/s** — the car crawled across the corridor edge inside the first lap quarter |
| invalid segment shape | one continuous span 65.99 → 355.69 (progress 0.195 → 1.0): the flag never cleared because the ENTRY (not timing) buried it and the semantics hold it until the line |
| route width | `routeWidth=12` → the legal band is ±6 m; the car's lateral crossed 6.0 at progress 0.195 |

**Conclusion: nothing in the timing or crossing detection failed.** The lap completed; the strict-lap rule suppressed bestLap because the car genuinely left the 12 m route band at progress 0.195 — crawling (3.8 m/s) out to lateral 6.0→7.0 — inside the *repaired* geo8 corridor geometry. The "fails regardless of the corridor fix" observation is explained: the corridor repair changed the SURFACE; the route-band datum (`routeWidth=12`, the `Track.OnRoad`/lane corridor the legality test uses) still reads the original 12 m band, so the AI's line at that section sits outside the regulatory band while being physically on the repaired road. That registry mismatch lives in the corridor/track data (track-render's files) — untouched by me, as instructed.

## 3. Minimal fix — committed (evidence, not behaviour)

`src/NativeRaceController.cs`:
1. Per-tick accumulation of lap-validity evidence after `Race.Advance` (`invalidRows`, `maxAbsLateral`, and the FIRST invalid sample's time/progress/lateral).
2. The lap report (`FinishAi`) gains a `lapValidity` block: `strictBestLapRecorded`, the row counts above, and the explicit self-check: **`invalidRows==0` with `bestLap==0` would indicate a timing defect instead; with `invalidRows>0` the exit trace names where the lap went out and by how much**.

Effects: the strict-lap gate is untouched (same `!lapInvalid` rule, same FinishAi conjunct); every future geo8-类 attempt self-documents the invalidity cause (progress + lateral + half-width), so the corridor team's width-datum fix can be judged from the run report alone; and a genuine timing regression, if one ever occurs, becomes distinguishable (its fingerprint = completed lap + zero invalid rows + bestLap 0).

The corridor team's minimal fix (proposed, their files, not mine): sync the route-band width datum with the repaired corridor measurement at the affected section (a data change), then one rerun — the report's `lapValidity` block must read `invalidRows==0` (then `bestLap>0` follows mechanically from the strict gate).

## 4. Notes

- The 41898934225 workflow-parse failure and the c4fb876 workflow surface: no interaction with this addendum — my judgement sheet (64b5593) needs no change while the workflow text re-lands.
- The geo8 report carried three byte-identical copies of the 19.8 MB lap report (main/attempt-1/attempt-2) — evidence duplication, not a physics item; noted for the artifact-size watchers.
- No dispatches; no corridor/WallBuilder touches; the strict-lap semantics verified unchanged (CoreRunner PASS 88092, build 0 warnings after the edit).
