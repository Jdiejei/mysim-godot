# physics-gameplay — race-start sequence validation (race-flow item)

## Task

Reviewer-directed race-flow game task: extend the standing grid-hold asserts to the FULL start sequence — lights sequence timing, jump-start attempt/detection, the release-to-race transition — as flag-gated code + a reading-card section. Commit only my files; no dispatches; never weaken assertions.

## What changed

1. **`data/native_driving_validation_profiles.json`** — new profile `race_start_sequence` (data-driven, the established `native_*` pattern):
   - `setup.requiresGridStart: true` — the case cannot silently run with the countdown skipped.
   - Phase `grid`: commands FULL THROTTLE through the countdown (a deliberate jump-start attempt), terminates on `until.raceStarted`, `maxSeconds 12` (overrun = race clock never fired), `minSeconds 0.5`; phase `release`: throttle 1 until speed ≥ 5 (the lights-out release bite); phase `settle` + `brake_stop` close the loop.
   - Assertions (wide breakage guards, recorded metrics are the evidence — the established calibration status): `throttleHoldSeconds:grid ≥ 1.0` (proves the jump-start attempt was actually commanded), `maxSpeed:grid < 0.15` + `endSpeed:grid < 0.1` (grid held under throttle; ~20× inside the python grid-reference's measured 32 mm creep ≈ 0.007 m/s), `duration:grid ∈ [4.5, 6.5]` (countdown band against the 5 s default, covering the 1 Hz light-step granularity), `speedRiseTime:release < 0.5`, `raceStartedFraction:release ≥ 0.95`, `endSpeed:release ∈ [4.8, 7.0]`, plus burned-in guards (no teleport, upright cosine ≥ 0.9).
   - Written response: it does NOT assert `chassisContactSamples:all < 1` — the ~1 cm stance floor legitimately grazes the drive surface under full brake-stop compression; that assert would false-fail for a REAL (correct-behaviour) run. `nonSurfaceContactSamples` stays the loss-of-control guard. This is a design decision, not an assertion weakening: the metric is still recorded.
2. **`data/native_driving_validation_start.json`** — new case file: car16/track0/dry, `gridStart: true`, profile list `[race_start_sequence]`, scope note (jump-start PENALTIES out of scope; only hold/hold-off mechanics + timing validated).
3. **`src/Logic/DrivingValidation.cs`** —
   - `Sample` gained `raceStarted/raceClock/countdown` (trace evidence for the lights timing layer).
   - `Profile.RequireGridStart` + parser `setup.requiresGridStart`.
   - `Phase.UntilRaceStarted` (parser, `Terminates`, state machine: done at the `raceStarted` crossing sample once `minSeconds` elapsed).
   - Metrics `maxSpeed`, `throttleHoldSeconds` (samples with throttle > 0.5 summed over delta-t), `raceStartedFraction` added to `KnownMetrics` + `ComputeMetric`.
4. **`src/NativeRaceController.cs`** —
   - `BuildValidationSample` now emits `raceStarted=Race.started, raceClock=Race.clock, countdown=Race.countdown`.
   - Fail-fast guard: profile requiring grid start + `MYSIM_GRID_START != "1"` throws `InvalidOperationException` with the exact fix in the message (`dispatch with MYSIM_GRID_START=1`). The case JSON's `gridStart:true` is intent metadata (same pattern as `requires.damageEnabled` → native_damage dispatch input); the env var configures the boot; the fail-fast enforces consistency, so a mis-dispatch fails LOUDLY instead of validating nothing.

## Verification

- `dotnet build MySimGodot.csproj`: succeeded, 0 warnings, 0 errors.
- `dotnet run --project tools/CoreRunner` (the CI core.yml gate): `PASS 88096 assertions`, 0 FAIL lines.
- Profile JSON re-parsed via python: phases/assertions as documented; `until.raceStarted` parse path (`DrivingValidation.cs:125`) and state-machine trigger (`:444`) confirmed by grep; metric cases (`:299-301`) confirmed against `Sample` fields.

## Proposed dispatch config (coordinator applies; I dispatch nothing)

```
case: data/native_driving_validation_start.json
depth: game (driving_validation runner)
backend: jolt, physics_hz 240 (rotational tyres), then source/240 pair leg
env: MYSIM_DRIVING_VALIDATION=1, DRIVING_VALIDATION_MANIFEST=..., MYSIM_GRID_START=1,
     MYSIM_ASSISTS=0, MYSIM_USERPATH=1 (userpath leg; non-userpath leg optional first)
damage: off (profile requires none)
```
Precondition: run on a head that already frees the car (DV-core PASS on 836de96+ legs) — a frozen car would fail `endSpeed:release` for the wrong (known) reason.

## Reading rules

Appended as **§9** of `physics-gameplay-reading-card.md`: mechanical per-artifact checks incl. the `UNVERIFIED(config)` classification for the thrown fail-fast, the countdown-skip vs clock-stuck vs hold-release fingerprints, and the no-chassis-contact-assert rationale.

## Risks / open

- The countdown band [4.5, 6.5] is tied to the 5 s default; if start-sequencing constants are ever retuned, this profile's band re-baselines BY DESIGN (it is the acceptance spec for exactly that constant).
- Jump-start PENALTY logic (if any exists downstream) is not validated by this profile — hold/hold-off mechanics and timing only, as scoped.
- Grid-creep bound 0.15 m/s is measured-consistent with flat grid spawns; a sloped spawn strip would need its own case (the beach-slope family belongs to the contact investigation, not this profile).

## Commit

Named-files commit on `mistral/physics-gameplay`: `src/Logic/DrivingValidation.cs`, `src/NativeRaceController.cs`, `data/native_driving_validation_profiles.json`, `data/native_driving_validation_start.json`, `outputs/mistral-orchestration/agents/physics-gameplay-reading-card.md`, this report.
