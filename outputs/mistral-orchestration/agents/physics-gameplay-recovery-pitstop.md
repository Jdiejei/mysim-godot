# physics-gameplay — recovery-phase telemetry (R-window) + pit-stop scope spec

## Task

Wall-recovery item per the reviewer: (1) implement the post-impact R-window recovery telemetry (what the car does during recovery, the inputs applied, the path back) as recorded metrics + reading-card section, flag-gated, defaults untouched; (2) sketch the pit-stop sequence validation scope (pitCompleteness: entry/box/serve/exit states — existing coverage vs what needs validation) as a spec-only section.

## What changed

1. **`src/Logic/DrivingValidation.cs`**
   - `RecoverySpec` (Enabled, MotionSpeed 1.0, HoldSeconds 1.0, MaxSeconds 20) + `RecoveryWindow` (startT/endT/startSpeed, Recovered, motionTime, pathLength, maxReverseSpeed, offTrackFraction, throttleFraction, brakeFraction, steerPeakAbs) + `RecoveryWindows()` — a pure function over recorded samples: every rise of the cumulative impact counter opens a window; it closes when the speed magnitude holds at MotionSpeed for HoldSeconds (motionTime = the moment the sustained interval completes), else records unresolved at the MaxSeconds cap. Motion is speed MAGNITUDE, so a scripted reverse-out counts as recovered motion (maxReverseSpeed records it); forward drive-away stays the recover-phase assertions' job.
   - Flag-gating: `setup.recordRecovery` (bool or object with motionSpeed/holdSeconds/maxSeconds, range-validated fail-closed). Profiles without the flag yield NULL recovery metrics — an assertion on them fails with "metric not observed", never a silent zero; flag-on with no impacts yields an observed 0.
   - Nine metrics in KnownMetrics + a separate `RecoveryMetricNames` list (the host emits them once per run, not per phase — windows span phases): `recoveryWindowCount`, `recoveryRecoveredFraction`, `recoveryMotionTimeMax/Mean`, `recoveryPathLengthMax`, `recoveryOffTrackFractionMax`, `recoveryThrottleFractionMean`, `recoveryBrakeFractionMean`, `recoverySteerPeakAbsMax`. All read the FULL stream regardless of the declared scope (declare `:all`).
2. **`src/NativeRaceController.cs`** (`FinishValidation`)
   - Per-phase metric loop skips the recovery family; when `profile.Recovery` is set the run entry gains `recoveryMetrics` + `recoveryWindows[]`. Run entries are now built as dictionaries (same keys as before, byte-compatible JSON for unflagged profiles; the two new keys exist only when the flag is on).
   - Serialized samples gain `nonSurfaceContact` and `wheelBandContact` per tick (additive) — per-sample contact composition is now auditable against reading-card §6 without re-deriving.
3. **`data/native_driving_validation_profiles.json`** — `"setup": {"recordRecovery": true}` added to exactly the four wall profiles (`impact_wall_recovery`, `wall_contact_low`, `wall_contact_high`, `wall_contact_graze`). No assertion changed; all other profiles untouched (defaults untouched).
4. **`src/Logic/CoreChecks.cs`** — 10 new assertions in the scaffolding block: window opens exactly on the impact rise; motionTime correct on a synthetic stall→reverse-out series; maxReverseSpeed/pathLength/fractions honest; `recoveryMotionTimeMax` maths; unresolved window (stalled car) records Recovered=false with motionTime null capped at maxSeconds; flag-off returns null (fail-closed); flag-on with no impacts returns an observed zero. The scaffolding PASS line now names recovery windows.
5. **Reading card**: new §10 (R-window readout: field table, parity rules for the jolt/source pair, reading rules incl. the commanded-vs-applied input caveat) and §11 (the pit-stop scope spec, below).

## Pit-stop scope (spec only — no code, per the task)

Recorded as card §11. Existing coverage (unit-tested, keep): full state machine `pitRequested→pitActive→servicing→pitDone`, service freeze before start, pit speed limit 16.67 m/s + 1 s overspeed penalty latch, occupied-box refusal, unsafe-release hold + clearance release, explicit repair duration, tyre-only damage preservation, staged tyre swap, safe release; entry gate 16 m/pit-valid, box gate 1.2 m & 1.2 m/s. The gap (needs flag-gated DV-runner extensions before ANY game claim): the DRIVEN entry (with the open design question of who steers a human-driven car while pitActive — resolve before implementation, no silent assist), box stop accuracy evidence, service timing against the wall clock under physics load, work-performed verification on the live car, release under real traffic, and the driven exit/rejoin with lap validity intact. Proposed future metrics listed (pitEntrySpeedMax, boxStopDistance, boxStopSpeed, serviceWallClockSeconds, pitOverspeedSeconds, rejoinOffTrackSeconds, tyresReplaced, compoundAfter). Until then pitCompleteness stays "logic-unit-covered, not game-validated".

## Verification

- `dotnet build MySimGodot.csproj`: 0 warnings, 0 errors.
- `dotnet run --project tools/CoreRunner`: `PASS 88106 assertions` (was 88096; +10 recovery checks), 0 FAIL.
- Profiles JSON re-parsed: `recordRecovery` active on exactly the four wall profiles.

## Proposed dispatch config (coordinator applies; nothing dispatched by me)

No NEW dispatch needed: the next wall-battery run (already spec'd: jolt/240/native_damage, then the source pair) now carries the R-window evidence automatically via the flagged profiles. Reading: apply card §10 on top of §2. If a dedicated first exercise is wanted before the full battery, re-run ONLY the wall manifest with the four-profile sequence unchanged — no new case files needed.

## Risks / open

- `motionTime` close times are interpolated between samples (physics-rate granularity); comparisons across backends are descriptive only (stated in §10).
- The recovery metrics treat a fast drive-through scrape as near-instant recovery (speed-magnitude semantics) — deliberate; the forward-drive proof remains with `endLongitudinalSpeed:recover_fwd`.
- Report files grow slightly (per-sample nonSurfaceContact/wheelBandContact + per-window arrays on the four wall profiles only).
- Pit spec is a scope draft: the pitActive steering question for human-driven cars is flagged as a pre-implementation decision for the coordinator.

## Commit

Named-files commit on `mistral/physics-gameplay`: `src/Logic/DrivingValidation.cs`, `src/Logic/CoreChecks.cs`, `src/NativeRaceController.cs`, `data/native_driving_validation_profiles.json`, `outputs/mistral-orchestration/agents/physics-gameplay-reading-card.md`, this report.
