# physics-gameplay — guard-contact force characterization (archetype layer)

## Task

Mounted-wheel-collision / human-input item per the reviewer: spec + implement guard-contact force metrics — which deltaV/impulse signatures a hub-mounted guard contact produces vs a body-panel contact — as the archetype table for the reading card, so the battery's wall runs distinguish guard contacts from body contacts mechanically. Flag-gated; defaults untouched.

## What changed

1. **`src/Logic/DrivingValidation.cs`**
   - `Sample.bodyPanelContact` (a contacted part that is neither the drive surface nor the WHEELGUARD/CALIPER band). Mixed ticks (both bands) set BOTH flags and feed both signatures; `mixedContactSamples` makes that ambiguity measurable instead of hidden.
   - `Profile.RecordContactArchetypes` parsed from `setup.recordContactArchetypes` (fail-closed pattern identical to `recordRecovery`).
   - Six metrics (`ContactArchetypeMetricNames`, also in KnownMetrics so assertions can use them phase-scoped): `bodyPanelContactSamples`, `mixedContactSamples`, `wheelBandContactSeconds`, `bodyPanelContactSeconds`, `wheelBandTickPeakDeltaV`, `bodyPanelTickPeakDeltaV`. All NULL unless the profile sets the flag — asserting them unflagged fails with "metric not observed", never a silent zero.
   - DeltaV semantics mirror `impactTickPeakDeltaV` exactly (per-tick velocity jump while the archetype was in contact on either side of the tick). The §2 impulse identity applies to whole-event numbers; per-archetype impulse is deliberately NOT derived (attribution across the cumulative MaximumDeltaV would be dishonest).
2. **`src/NativeRaceController.cs`**
   - `BuildValidationSample` classifies `bodyPanelContact` from `ContactParts` (excluding DriveSurface/WHEELGUARD/CALIPER, case-insensitive, same pattern as the existing band check).
   - Serialized samples gain `bodyPanelContact` per tick; flagged runs emit a per-run `contactArchetypes` map (unflagged report JSON unchanged).
3. **`data/native_driving_validation_profiles.json`** — the four wall profiles' setup now `{ "recordRecovery": true, "recordContactArchetypes": true }`. No assertion text changed; every other profile untouched.
4. **`src/Logic/CoreChecks.cs`** — 6 new assertions: per-archetype per-tick deltaV correctness on synthetic contact data (guard spike √(3.4²+1.9²), panel 0.65), panel sample count including the mixed tick, mixed count, contact-seconds accumulation, and the flag-off null (both metrics) fail-closed check.
5. **Reading card §12** — the archetype layer: classification rules, metric table, the ARCHETYPE TABLE (guard vs body-panel structural expectations — duration/deltaV-shape/count — with the numeric band column explicitly TO BE FILLED from measured artifacts), the integrity rule (no numeric archetype thresholds before a measured artifact exists — that would be invented physics), and the battery reading rules (archetype parity across the source/jolt pair as a divergence fingerprint; guard-label ⊕ §6 position cross-check; mixed-as-information).

## Verification

- `dotnet build MySimGodot.csproj`: 0 warnings, 0 errors.
- `dotnet run --project tools/CoreRunner`: `PASS 88112 assertions` (was 88106; +6 archetype checks), 0 FAIL.

## Dispatch position (nothing dispatched by me)

No new dispatch: the archetype layer rides the existing wall-battery config (same four-profile sequence, jolt/240/native_damage then the source pair). Reading order per artifact: §2 → §6 → §12. The first measured wall artifacts FILL the archetype table's numeric column; until then the metrics label contacts only.

## Risks / open

- The structural expectations (guard = shorter/sharper, panel = longer/distributed) are hypotheses from geometry, explicitly marked to-verify in §12 — the first battery run may correct them; the table is the recording frame, not a claim.
- `bodyPanelContact` treats every non-drive, non-wheel-band contacted part as "body panel" (the everything-else-solid class); if ContactParts ever carries non-structural names (_markers etc.) the class needs a refinement — flagged for the first artifact review.
- Per-sample `bodyPanelContact` is serialized for ALL runs (tiny width increase); the aggregate map stays flag-gated.

## Commit

Named-files commit on `mistral/physics-gameplay`: `src/Logic/DrivingValidation.cs`, `src/Logic/CoreChecks.cs`, `src/NativeRaceController.cs`, `data/native_driving_validation_profiles.json`, `outputs/mistral-orchestration/agents/physics-gameplay-reading-card.md`, this report.
