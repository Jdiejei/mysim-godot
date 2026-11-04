# Owner-battery consolidated reading card — per artifact drop — 2026-11-04

One checklist, applied mechanically per run/artifact; consolidates the DV-core sheet (`d228674`), the wall sheet (`64b5593`) and the track-8 lapValidity rules. No dispatches; no acceptance claims without verified terminal states; forks/head line: corridor fix `b538efd` merged, axle candidate wired, owner-side dispatch only.

## 0. Global rules (apply before EVERY reading)

1. Read ONLY artifacts of a COMPLETED run (`status=completed`); queued/running = "verified wait", never progress.
2. The expected-artifact inventory per run class must be complete BEFORE reading (the wrong-mode lesson): any missing file = the drop fails its inventory gate (see the per-command tables).
3. Never an identical re-dispatch: every red verdict names a fingerprint class + a distinct fix owner before anything re-runs.
4. Charts refresh from VERIFIED TERMINAL STATES only.
5. Any bound breach = the values quoted verbatim into the ledger (actual vs expected); nothing tuned at judgement time.

## 1. Command: DV core (legs: jolt/240 first, then the source/240 pair)

Artifacts (inventory gate): `driving-validation-track-00-car-16.json`, `…car-00…` (or the case set the dispatch names), `driving-validation-summary.json`, both case logs, `physics-backend-experiment.json`.

| Step | Check | PASS | Fail path |
|---|---|---|---|
| 1.1 | `backend=jolt` / `NATIVE_PHYSICS_HZ=240` / `ASSISTS_OFF` line | per dispatch inputs | infrastructure |
| 1.2 | Hulls + `HUB_POSE_RESTORE`: car16 2 wheelguard lines, car0 4 caliper lines; no `NATIVE_COLLISION_FIXTURE_PROXY` | present | infrastructure (older bake lineage) |
| 1.3 | DV contract | `PASS driving validation scenarios`, exit 0, summary `ok:true`, per case `ok/completed/allPassed`, coverage exact (car16: 6, car0: 5), every run ok/no-abort/≥1 assert/>20 samples/not truncated, `validationResets` 5/4, `ruleResets=0`, `gridHoldTraceSeen=false` | infrastructure / contract |
| 1.4 | Freed car | trace: `started=True` line one, `gridHold=False` throughout, `thr=1 → spd rising` at launch, stage column marches, **no sustained spd=0/`tyreContacts<4` while thr=1**; `chassisContacts` at the floor-graze baseline; distance > 0 per profile | the freeze class returned → STOP, samples to physics |
| 1.5 | Nine bands | accel [1.13,3.91] s; brake [11.73,24.30] m + decel [−28,−12]; steer response <0.2 s, settle [0.5,1.4], tracking [0.9,1.1], yaw signs + peaks <4, recovery <0.35; stability 0.25 rad / 3 m / 0.15 rad/s; edge (car16) 0.5/2.5/0.9/>5; grounded ≥0.9 (0.95 braked/coasted); jump <1.0 | profile-geometry (mobile aimless) / physics (band breach, samples attached) |
| 1.6 | Verdict writing | jolt leg GREEN = chart-eligible; record every actual as the jolt baseline | — |
| 1.7 | Source pair | same bands VERBATIM; tendencies recorded (impulse [0.8,1.25]×, force [0.5,2.0]× = guidance); source-only breach = a backend finding (the choice is the coordinator's) | same fingerprints, source-labelled |

## 2. Command: wall battery (jolt leg, then the source pair)

Artifacts: the DV harness files for the `driving_wall` set (`wall_contact_low/high/graze` + `impact_wall_recovery`), the summary, case logs, backend JSON + `MYSIM_NATIVE_DAMAGE=1` evidence.

| Step | Check | PASS | Fail path |
|---|---|---|---|
| 2.1 | 1.1–1.4 preconditions + freed car (the wall profiles run the same session mechanics) | as above | infrastructure |
| 2.2 | Per wall profile: `wall_touched ≥1`; `impact_recorded ≥1` (low/high/impact profiles); `maxDeltaVRise` [1.5,20] (impact profile [0.5,30]); **impulse identity [1.5,16]×mass (graze [0,16])**; jump <2; drive-away >0.5; upright ≥0.7 | all rows | X-class breach (identity/jump) = CRITICAL, samples verbatim to the coordinator; G-class aim miss (mobile, no contact) = a NEW aim geometry attempt; S-class Δv<2 graze = a higher-approach profile version; R-class = recovery forensics |
| 2.3 | Record-only reads | `impactTickPeakDeltaV` / `impactPeakForceEstimate` (order 0.2–2.1 MN at these approaches) / `impactContactSeconds` / the restored-guard participation (wheel-band contact part names now include WHEELGUARD only if genuinely at that band — the audit lines decide) | — |
| 2.4 | Source pair | same bands verbatim; event-count nuance (>20 in one aim phase flags the backend decision); agreement guidance as in 1.7; inputs identical (manifest revision + dispatch recorded) | source-labelled findings |
| 2.5 | Verdict | all four profiles' rows green on jolt (+ source rows compared) → VERIFIED; charts refresh with the verified terminal state | — |

## 3. Command: track-8 rerun (corridor fix verification, car13/track8 lap case)

Artifact: `mounted-native-lap-track-08-car-13.json` (+ log) — the `lapValidity` block decides:

| Case | Reading | Verdict / next action |
|---|---|---|
| `invalidRows==0 && bestLap>0` (+ finished/laps/distance gates pass) | the corridor fix VERIFIED by the strict gate itself; `maxAbsLateral` recorded vs `routeHalfWidth=6` (the corridor margin) | VERIFIED; chart-eligible |
| `invalidRows>0` | the exit trace (`firstInvalidProgress/Lateral/Time` + `maxAbsLateral`) = the corridor finding with exact coordinates | NOT verified; a distinct corridor/data fix, then a new rerun |
| `invalidRows==0 && bestLap==0` on a finished lap | the timing fingerprint (by construction impossible from validity) | escalate to physics immediately |
| `bestLap>0` despite `invalidRows>0` | a mid-run restart: a clean counted segment exists | judge the VALID segment; the pre-restart rows recorded as context |
| `finished=false` | launch/retirement failure class (not this sheet's scope) | infrastructure / the launch-failure evidence path |

## 4. Command: axle arms (wheel_fit_review with `source_axle_fit=true`)

- Artifact inventory: `-source-axle.json` per pass present = the fit actually ran (the turn-5 lesson: without it the sweep measures the raw pivots and the verdicts are void).
- The verdicts are car-assembly's (their per-wheel worst tables + `STANCE`/`PART_HUB_OFFSET` lines); the physics intersection: the `STANCE` no-hubs value should hold near the measured ~0.025 class (the ride gate's datum integrity while the axle candidate moves anchors) — a large drift = a physics intersection finding before any acceptance.
- No re-baselining without a verified fit: if the fit moves the recovered anchors, ALL stance/geometry numbers re-baseline (the corridor/lap/standards sheets re-checked on the next runs).

## 5. Escalation map (who takes which fingerprint)

| Fingerprint | Owner |
|---|---|
| Missing artifacts / older bake / wrong backend / parse | coordinator (pipeline) |
| Guard-blob rest, band breach, timing pair (`invalidRows==0 && bestLap==0`) | physics (me) |
| Exit-trace corridor coordinates | corridor/track |
| Wheel-fit/stance/`PART_HUB_OFFSET` drifts | car-assembly |
| Workflow parse/`403` dispatch blockers | coordinator + owner child-dispatch path |

Verdicts recorded per run as: `PASS / FAIL(<class>, <assert id>, <actual vs expected>) / UNVERIFIED(<reason>)` — the ledger line the charts consume.
