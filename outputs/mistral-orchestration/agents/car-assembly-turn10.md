# Car-assembly turn 10 - axle-line candidate for car16's front axle - 2026-11-06

Commit: `063620d` (the axle-candidate sweep pass + the ComputeShift extraction). No dispatches.

## 1. The candidate (repair ladder rung 1, now in code)

- `SourceAxleFitCandidate.ComputeShift(car, wheels)`: the strut-derived shared axle translation, extracted from Apply's math with the same guards (roles present, coverage above 0.2 m, delta inside 1.2 m, front/rear spread inside 0.65 m). Returns null when the car is not a supported F1 case.
- `Apply` now calls ComputeShift; its behaviour and messages for existing callers are unchanged in substance (the spread-exception message text is folded into the one ComputeShift-failure message).
- The sweep gains `MYSIM_WHEEL_CLEARANCE_AXLE_CANDIDATE=1`: a SECOND shift state applies the computed translation to the pivots and re-runs the full pose grid. Rows carry `axleShift` (0 = raw pivot line; the fitted value = the strut-derived number, about -0.4765 m for car16 from run 41580145045). Worst tables and the summary are per shift state, so one dispatch yields the raw and the fitted tables side by side. Snapshots and pose-restore still cover the shifted state.
- One dispatch shape (after the two coordinator one-liners from turn 6 land):

```
gh workflow run game-review.yml -R instagramshashank8-bot/mysim-godot --ref <merged-head> \
  -f depth=wheel_fit_review -f cars=16 -f source_axle_fit=true \
  (the sweep envs: MYSIM_WHEEL_CLEARANCE_SWEEP=1, MYSIM_WHEEL_CLEARANCE_AXLE_CANDIDATE=1 via the depth's wiring)
```

## 2. Expected sweep numbers (the acceptance contract for the run)

| Group (car16) | Before (measured, turns 5-6) | After the axle arm (expected) | Band |
|---|---|---|---|
| front bodywork (FRONTBUMPER vs FL/FR tyre) | -0.0450..-0.0455 | about +0.20..+0.25 (the tyre leaves the wing's z-band by about 0.27 m minus the shift spread) | must move from FAIL to clear; contract threshold: >= -0.002 |
| rear bodywork (CHASSIS vs RL/RR tyre) | -0.0452..-0.0453 | clear or WATCH (the rear tub's exact band is unverified; same-direction move) | >= -0.002, verify at the run |
| front suspension (upper/lower arms vs FL/FR) | +0.048..+0.14 | SMALLER but clear: the tyre moves back ONTO the arm zone; the arms pass through the wheel's hollow centre (their surfaces sit inside the bead radius), so expect +0.01..+0.09, not the old values | must stay >= -0.002 (no penetration); the old +48 mm number is NOT the regression gate - see note |
| rear suspension (upper strut) | -0.0203 | clears (the tyre returns to the strut's outer end) | >= -0.002 |
| stance | with-guards -0.52..-0.61 class; no-guards +0.0099 | unchanged by this arm (hull-side separate) | the 0.235 arithmetic stays reproduced only when guards are forced into the datum |

Note on the leader's no-regression wording: "front suspension +48..+132 mm must hold" cannot hold literally - the fit moves the tyres ONTO the arm anchors, so the arm-to-tyre gap necessarily shrinks. The correct no-regression set is: front suspension worse-case stays >= -0.002 (no new penetration), and the front-bodywork worst flips from -0.045 to clear. If the front-suspension numbers go NEGATIVE beyond -0.002 after the fit, that is a real arm-vs-tyre finding (the arm's own geometry, the next ladder rung: mount keys, then the kingpin A/B).

## 3. Ladder continuation if the front-bodywork entry stays negative after the fit

1. Mount keys: the bumper's mounts are not hub-carried; instead re-check the bumper's own source position against the assembly audit values at the fitted stance (its position may have been authored against the raw pivot line).
2. Kingpin A/B: `MYSIM_SOURCE_KINGPIN_VISUAL` 0/1 at the pinned worst state (the bump travel with the wheel turned into the wing).
3. Neither helps - report back with the pinned state's numbers; the axle fit value itself is then suspect (the strut outer-vertex cluster's quality), and the shift's input evidence gets re-audited.

## 4. Not covered

- The axle-shift's own value comes from runtime geometry; the historical -0.4765 is car16's measured reference, not a hardcoded constant in this diff.
- The run depends on the two coordinator one-liners (the cars mapping + the axle-env fix) - unchanged from turn 6.
- Dynamic, moving contact remains out of scope for this fixture; the candidate verification is static-envelope.
