# Wall-battery judgement sheet — owner-run driving_wall battery (item 3: jolt/240/native_damage) — 2026-11-04

Scope: the exact PASS interpretation for the wall battery dispatched from `OWNER_MERGE_INSTRUCTIONS.md` against the merged physics spec (`c059e2c` → `adbd466`, `driving_wall` case_set wired). Nothing here changes thresholds: every bound below is the committed profile's own assertion, restated with its physical justification so the verdict needs no re-derivation at judgement time. No dispatches were made; results enter any chart/dashboard only after this sheet's verification is done against the artifact contents.

## 0. Preconditions (fail the dispatch item before reading physics if any of these break)

| # | Check | Source | Expected |
|---|---|---|---|
| P1 | Backend really is jolt | `outputs/cloud-game/physics-backend-experiment.json` → `backend` | `"jolt"` |
| P2 | Rate really is 240 | case log `NATIVE_PHYSICS_HZ=` | 240 |
| P3 | Assists off | case log | `ASSISTS_OFF traction and brake-stability assists disabled` present |
| P4 | Hulls baked + restored collision coverage | case log | `NATIVE_SOURCE_BODY_COLLIDERS car=16 parts=N` (N>0) AND `NATIVE_SOURCE_BODY_COLLIDERS` NOT followed by `NATIVE_COLLISION_FIXTURE_PROXY` anywhere; **`HUB_POSE_RESTORE car=16 part=LOD_A_WHEELGUARD_FRONT_(LEFT|RIGHT) hub=0/1 shift=(…)` lines present** — their shift should read ~1.4–2.0 m magnitudes (placing the mirrored/misplaced shells at their hubs); `HUB_POSE_RESTORE … metadata hub … overrides` lines, if any, are recorded and quoted |
| P5 | No fixture proxy | case log | `NATIVE_COLLISION_FIXTURE_PROXY` absent |
| P6 | Contract | case log + summary | `PASS driving validation scenarios` present; exit 0; `driving-validation-summary.json` exists with `ok:true` |

## 1. The report file and the four profiles

Artifact: `outputs/cloud-game/driving-validation-track-00-car-16.json` (one for the whole session; the manifest runs `wall_contact_low → wall_contact_high → wall_contact_graze → impact_wall_recovery` in sequence; `validationResets` must read **3** — mechanical resets between the four profiles).

Mechanical pass (all four profiles): `ok:true`, `completed:true`, `allPassed:true`, `rotationalTyres:true`, `assistsOff:true`, `dualBackendNote… none` — and per run: `ok:true && completed:true && abortReason==null && any asserts && sampleCount>20 && samplesTruncated:false`.

### 1.1 wall_contact_low (approach 7.2 m/s ≈ 26 km/h)

| Assertion (id) | Metric | PASS bound | Physical meaning / judge note |
|---|---|---|---|
| wall_touched | `nonSurfaceContactSamples:aim` | ≥ 1 | at least one sample touching a non-drive surface in the aim window; the typical value is several to hundreds after the barrier stop — a bare 1 with the impact also recorded is a blip-graze: judge with `impactContactSeconds` |
| impact_recorded | `impactEventsDelta:aim` | ≥ 1 | `NativeImpactDamage` registered an event in the aim phase; an event itself requires Δv ≥ 2.0 m/s (`native_damage_profile.minimumDeltaV`), so PASS here proves a *real* measured hit, not a brush |
| delta_v_band | `maxDeltaVRise:aim` | [1.5, 20] m/s | judge the actual: plausibly ≤ approach+rebound ≈ 7.2+0.5 — a value above ~8 at this approach speed is a solver explosion flagged by `impulse_identity_bound`/`no_teleport` too |
| impulse_identity_bound | `impactImpulseEstimate:aim` | [1.5, 16] × `vehicleMass` (689.46 kg) | identity, not a threshold: impulse/mass **is** a Δv; it cannot exceed the approach speed plus rebound margin — a breach means non-physical solver output (or a teleport; the next line catches that) |
| no_teleport | `positionJumpMax:all` | < 2.0 m/tick | a 7 m/s car moves 0.03 m per 240 Hz tick; anything ≥2 is a rescue/teleport/explosion |
| drives_after_impact | `endLongitudinalSpeed:recover_fwd` | > 0.5 m/s | the car drove away under its own reverse-out + forward throttle — the recovery proof |
| stayed_upright | `uprightMinCosine:recover_fwd` | ≥ 0.7 | no roll-over during the recovery |
| record-only | `impactTickPeakDeltaV`, `impactPeakForceEstimate`, `impactContactSeconds` | no bound | force estimate = `mass × tickΔv / dt` at the peak tick; expected order of magnitude 0.4–1.2 MN **if the stop resolves in 1–3 ticks** at this approach; dwell expected ≥ ~2/240 s. These are estimates at sample rate, never force certificates |

### 1.2 wall_contact_high (approach 12.5 m/s ≈ 45 km/h)

Identical assertion set and PASS bounds. Judge notes: Δv plausibly up to ~12.5–13 (a frontal stop); force estimate order 0.4–2.1 MN; if the aim phase's angled approach keeps Δv below the 2.0 event gate the profile FAILS honestly at `impact_recorded` — that is a geometry miss (section 2, class G), not a metrics failure.

### 1.3 wall_contact_graze (approach 8.5 m/s, partial lock, until contact)

| Assertion | Bound | Judge note |
|---|---|---|
| wall_touched | ≥ 1 | the contact itself ends the aim phase (`until: nonSurfaceContact`) |
| impulse_identity_bound | [0, 16] × vehicleMass | lower bound 0 by design: a graze may stay sub-threshold; `impactEventsDelta:aim` (0 or ≥1) is the recorded informer, not a gate |
| no_teleport / stayed_upright (all) | < 2 / ≥ 0.7 | as above |
| record-only | endLongitudinalSpeed at scrub/recover, dwell | did the car keep rolling through the graze? recorded, no bound (a wall pocket may legitimately stop it) |

### 1.4 impact_wall_recovery (approach 12 m/s)

Same shape as 1.2 with its own committed bands: `maxDeltaVRise:aim ∈ [0.5, 30]`, `impactEventsDelta:aim ≥ 1`, drive-away + upright + no-teleport identical. `maxDeltaVRise:aim ≥ 0.5` additionally documents the impact was MEASURED (the damage system's `MaximumDeltaV` rose), regardless of the event gate.

## 2. Failure fingerprints (what each failure looks like in the outputs — and the distinctive next attempt it demands)

The run-level failure surfaces as exit 1 + the case log line `FAIL driving validation: <firstFailure>` + the python message `Driving validation failed for driving-validation-track-00-car-16: <firstFailure> / <abortReason>`; the report JSON carries the failing run's `assertions[]` row (id, actual, expected) plus the FULL per-tick samples for that profile.

| Class | Fingerprint in the report | Owner's next step (never an identical re-dispatch) |
|---|---|---|
| G — aim geometry miss | `abortReason="phase aim exceeded maxSeconds without reaching its condition"`; samples show `spd>0` throughout (a mobile, aimless failure) | the wall was not where the seek looked; a NEW profile version with a different aim geometry (steer sign/lock/progress window) is required after checking the track's barrier coordinates; chart stays un-refreshed |
| S — sub-threshold graze | `impactEventsDelta:aim == 0` while `nonSurfaceContactSamples:aim ≥ 1` and tick jumps < 2 m/s | the approach was too gentle for the event gate; distinct attempt: a higher approach (my profile edit), NOT any threshold change |
| X — explosion/teleport | `impulse_identity_bound` or `no_teleport` fails; `positionJumpMax` ≥ 2 or Δv > 16/20 | CRITICAL: solver non-physicality or a rescue path; the samples' position trace locates the tick; no battery item may be marked green; the affected profile's samples go to the coordinator verbatim |
| R — recovery failure | `drives_after_impact` fails (endLongitudinalSpeed ≤ 0.5) with finite contact numbers | the car could not leave the barrier under its own drive — check the sample window for fuel/history/damage state and the reverse-out behaviour; distinct attempt only with a named fix |
| I — infrastructure | P1–P6 break (missing hulls, missing `HUB_POSE_RESTORE`, wrong backend/rate, report missing) | stop; fix the pipeline; the physics readout does not start |

**Global rule (unchanged):** any red item means the wall battery reports NOT-verified; chart refreshes only with verified results; the next attempt must carry a distinct fix or profile version, never identical inputs.

## 3. The source-backend pair (what must MATCH when the source legs run)

Same manifest, same gates, no loosened bands — the source pair answers the backend question, so the bounds apply verbatim:

1. **The gates:** every assertion above passes on source too — with one judge nuance: source may register MORE impact events within the aim window (contact resolver can emit multiple Δv≥2 blips in a grind). Recorded per profile; a source count > 20 inside one aim phase is flagged for the backend decision (the turn-3 protocol's instability criterion), it is not itself a battery failure.
2. **Band parity:** `delta_v_band` and the identity bound hold on source with the SAME numbers range — a source-only breach is a backend physics deviation finding (feeds the ship/backend decision; the pair compares, the choice is the coordinator's).
3. **Estimate agreement:** `impactImpulseEstimate` (the momentum integral) should agree between backends within ~[0.8, 1.25]×; `impactPeakForceEstimate` may differ more (per-tick resolution differs) — reference ratio [0.5, 2.0]× as judge guidance, never as a gate.
4. **Recovery parity:** `drives_after_impact` + `stayed_upright` must pass on both — the recovery behaviour is part of what ships.
5. **Identical inputs 艺术:** the source legs must run the SAME manifest revision and the same dispatch inputs as the jolt legs (recorded in `physics-backend-experiment.json` + the summary; no in-flight config drift).

## 4. Verified-with queue for me

When the battery artifacts land (downloaded by the owner/verification), the readout = this sheet executed literally: the preconditions, the 4×7-ish assertion rows with actuals, the record-only metrics, the source-pair deltas — appended to my agents report chain and marked verified/unverified per item. Commit reference for the criteria: this document.
