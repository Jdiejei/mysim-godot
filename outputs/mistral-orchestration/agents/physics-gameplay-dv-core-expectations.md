# DV-core expectation sheet for 836de96 (merged collider restore) + lapValidity reading rules — 2026-11-04

Sheet for the owner battery command that runs the DV-core case (the merged wheelguard/caliper restore at `836de96`). No dispatches made; results enter charts only as verified terminal states. Companion sheets: the wall battery (my `64b5593`) and the corridor rerun rules (section 3 here).

## 0. Preconditions (fail the item before any physics readout if broken)

| # | Check | Source / expected |
|---|---|---|
| P1 | Backend / rate / assists | `physics-backend-experiment.json` `backend=jolt`; case log `NATIVE_PHYSICS_HZ=240` (or the dispatched value, recorded); `ASSISTS_OFF traction and brake-stability assists disabled` present |
| P2 | Hulls baked for BOTH cases' cars | `NATIVE_SOURCE_BODY_COLLIDERS car=16 parts=N`, `… car=0 parts=M` (N,M>0) — the DV-core manifest is car16+car0; a single-car bake = an older bake lineage (the manifest-reading fix must be present) |
| P3 | Restored collision coverage | `HUB_POSE_RESTORE` lines: car16 → 2 lines (`LOD_A_WHEELGUARD_FRONT_LEFT/RIGHT`, hubs 0/1, shift magnitudes ~1.4–2.1 m); car0 → 4 lines (`LOD_A_BRAKE_CALIPER_FRONT/REAR_LEFT/RIGHT`, hubs 0-3, shifts ~1.2–1.6 m). Missing lines = the restore is not in the binary → infrastructure stop (do not judge physics) |
| P4 | No fixture proxy | `NATIVE_COLLISION_FIXTURE_PROXY` absent in both logs |
| P5 | Damage OFF for core | no `MYSIM_NATIVE_DAMAGE`; the DV contract's impact-flag/manifest match (core has no impact profile) |
| P6 | Session contract | `PASS driving validation scenarios`; exit 0; `driving-validation-summary.json` with `ok:true`; per case: `ok/completed/allPassed=true`, `rotationalTyres=true`, `assistsOff=true`, profile coverage EXACT (car16: 6 profiles; car0: 5 — no edge_excursion), every run `ok+completed, no abortReason, ≥1 assertion, >20 samples, samplesTruncated:false`; `validationResets` = 5 (car16) / 4 (car0); `ruleResets=0`; `simulatedSeconds` ≈ the profiles' authored durations (+ settle overhead); **`gridHoldTraceSeen=false`** |

## 1. The DV_TRACE must show the freed car (no guard-blob rest state)

Read the ~1 s `DV_TRACE` lines in each case log:
1. `started=True` from the FIRST line; `gridHold=False` on every line (the AI boot releases the grid at t=0; a `gridHold=True` anywhere = a boot-flags regression — stop at infrastructure).
2. `thr=0.00` only during the hold phase (~1.2 s); **`thr=1.00` from the launch phase with `spd` RISING on the next lines** — the 41834771421/41840755243 freeze signature (`thr=1.00, spd=0.000` repeated, `bodyContacts=2 tyreContacts=1`) must be **absent everywhere**.
3. The `stage=` column marches through the profile ids in the manifest order; `clock=` advances continuously (no stall).
4. **No guard-blob body contacts**: the report samples' `chassisContacts` during motion read the floor-graze baseline (small counts, never a sustained 2-4 blob rest with `tyreContacts<4` and zero speed); no sample pair shows `spd≈0` for >5 s while `thr=1` in a launch phase.
5. **Driven distance > 0 in every profile**: each run's `metrics["<phase>.pathLength"]` for launch/approach phases > 0, and the session's total simulated motion matches the profile durations (a zero-distance session = the freeze class returning; stop, sample dump to physics).

## 2. The nine target bands (the DV-core assignment per profile; actuals recorded, first native measurements)

Car 16 and car 0 run the same guards except `edge_excursion` (car 16 only). All values come from `runs[].metrics["<phase>.<metric>"]` + `runs[].assertions[]` (id/actual/expected/passed) in the case report:

| Target profile | Metric (phase) | Band (committed, unchanged) | First-native note |
|---|---|---|---|
| input_direction | `meanYawRate:steer_positive / :steer_negative` | ≥ +0.05 / ≤ −0.05, exact left-right symmetry | the native layer's signed-yaw proof; a flip = a real convention finding |
| input_direction | `endSpeed:launch` | [14.5, 16.5] | actuator overshoot window |
| input_direction | `nonSurfaceContactSamples:all` | 0 expected (dry track0, on-corridor scripted) | >0 = investigate the spawn/aim geometry, section 2 note |
| acceleration_0_60 | `duration:launch` | [1.13, 3.91] s (0.55–1.9 × the 2.06 s spec) | the first native 0-60; record for the jolt-vs-source pair |
| braking_60_0 | `pathLength:brake` | [11.73, 24.30] m (0.7–1.45 × 16.76) | the first native full-brake stop; lockup/slip rows recorded (rotational on) |
| braking_60_0 | `longitudinalAccelMin:brake` | [−28, −12] m/s² | watch the transient tick on jolt (the turn-6 note) |
| steering_rate_step | `steerResponseTime` ×3 phases | < 0.2 s | responsiveness at the native actuator |
| steering_rate_step | `steerSettleRatio` ×3 | [0.5, 1.4] | the 3.2 rad/s rate-limit cross-check |
| steering_rate_step | `steerTrackingRatio` (low+high) | [0.9, 1.1] | the SteeringLock curve's achieved authority |
| steering_rate_step | `meanYawRate` signs + `yawRatePeakAbs` | signs correct; peaks < 4 rad/s | no-spin under full lock at both speeds |
| straight_stability | `headingDriftAbs` / `lateralDriftAbs` / `yawRateEndAbs:coast` | < 0.25 rad / < 3 m / < 0.15 rad/s | hands-off at 30 m/s; the paraded floor-scrape damping is unchanged in this configuration (A) |
| edge_excursion | `yawRateEndAbs:recover` / `yawRatePeakAbs:all` / upright / `endLongitudinalSpeed:recover` | < 0.5 / < 2.5 / ≥ 0.9 / > 5 m/s | **the first off-corridor grip measurement**: the recorded `offTrackFraction` + slip deltas either expose the missing off-surface grip reduction (the turn-2 candidate defect) or retire it — either way valuable |
| every profile | `groundedFraction` | ≥ 0.9 (brake: ≥ 0.95; coast: ≥ 0.95) | susp support sanity at the new stance |
| every profile | `positionJumpMax` | < 1.0 (2.0 impact-class) | teleport/rescue detector |

**The verdict line for the owner:** the DV-core item is VERIFIED iff preconditions pass, all committed assertions per profile pass, the trace shows the freed car (section 1), and every recorded actual lands inside its band. Green = chart-eligible. Any red = the fingerprint split per the failure protocol (turn-5/turn-6): profile-geometry (aim/timeout — a distinct attempt required), physics (a real band breach — samples stay attached to the finding, nothing tuned), or timing (`invalidRows==0 && bestLap==0`-class — escalated to me).

## 3. lapValidity reading rules — the track-8 rerun (after the corridor commit)

The rerun runs the geo8 lap case (car13/track8); the report's `lapValidity` block (my `4859ed1`) carries `strictBestLapRecorded / invalidRows / maxAbsLateral / routeHalfWidth / firstInvalidTime / firstInvalidProgress / firstInvalidLateral`. Rules, in order:

1. **`invalidRows==0 && bestLap>0` && finished/laps/distance gates pass** → the corridor fix VERIFIED by the strict gate itself: the lap is now valid end-to-end; chart-eligible PASS. (The `maxAbsLateral` value also documents the corridor margin — record it against `routeHalfWidth=6`.)
2. **`invalidRows>0` → the exit trace**: `firstInvalidProgress/firstInvalidLateral/firstInvalidTime` + `maxAbsLateral` name WHERE and BY HOW MUCH the lap left the band — a corridor finding, for the track team, with the exact coordinates; NOT an acceptance; the next attempt must carry a distinct corridor/data fix (no identical dispatch).
3. **`invalidRows==0 && bestLap==0` (finished=true)** → the timing-defect fingerprint (the pair my evidence block was built to distinguish): escalate to physics immediately — the strict gate should never produce this pair on a completed lap.
4. **`bestLap>0` despite `invalidRows>0`** → legitimate: the stale flag was cleared by a mid-run unordered line crossing ((1)-branch reset) and a later lap segment completed clean — the counted lap is the valid one; judge the VALID segment (the telemetry rows show the restart) and record the pre-restart rows as context.
5. `finished=false` cases never enter this judgement (launch/retirement failures go through their own failure classes — see the earlier sheets).

## 4. Battery context (non-overlapping items)

- The wall battery items keep my `64b5593` sheet ( precondition P4 there now ALSO expects the `HUB_POSE_RESTORE` lines at 836de96+ ).
- The workflow-surface risk (the parse-rejection class seen at `41898934225`) does not affect this sheet: no thresholds or profiles changed; if the bisect changes the workflow text, only the sheet's artifact-path wording would need a review.
- No dispatches by physics; the owner battery (six commands, `OWNER_MERGE_INSTRUCTIONS.md`) remains the only execution path until the 403 clears.
