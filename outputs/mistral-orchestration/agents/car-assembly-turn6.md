# Car-assembly turn 6 - fleet clearance + body integrity - 2026-11-06

Runs used: 41837315358 (no-fit baseline, e17fdf5) and 41843317303 (with-fit leg, a996a9d), both SUCCESS. Artifacts of both downloaded read-only into `outputs/wfr-*/` (uncommitted). No dispatches made.

## 1. Delta table: with-fit vs no-fit

Per-car values at the rest state (spring 0.11, steer 0), metres, negative = penetration:

| Car | FL hub z | FL body | FL susp | RL hub z | RL body | RL susp |
|---|---|---|---|---|---|---|
| 15 base | 2.078 | -0.04520 | +0.07185 | -1.6374 | -0.04526 | +0.00413 |
| 15 fit | 2.078 | -0.04520 | +0.07185 | -1.6374 | -0.04526 | +0.00413 |
| 16 base | 2.0083 | -0.04505 | +0.07824 | -1.5888 | -0.04521 | -0.02028 |
| 16 fit | 2.0083 | -0.04505 | +0.07824 | -1.5888 | -0.04521 | -0.02028 |
| 17 base | 2.0639 | -0.04528 | +0.12183 | -1.6184 | -0.04522 | -0.00524 |
| 17 fit | 2.0639 | -0.04528 | +0.12183 | -1.6184 | -0.04522 | -0.00524 |
| 18 base | 2.0555 | -0.04488 | +0.13579 | -1.6201 | -0.04550 | -0.00699 |
| 18 fit | 2.0555 | -0.04488 | +0.13579 | -1.6201 | -0.04550 | -0.00699 |

Delta: exactly zero, every car, every value (bit-identical). No `-source-axle.json` artifacts exist in the with-fit run (count 0): the axle fit did not execute.

Verdict per car: UNCHANGED from turn 5 - the -45 mm bodywork FAILs (front bumper, rear chassis) and the rear-strut FAILs (cars 16/17/18) stand uncorrected. The with-fit leg tested nothing.

## 2. Why the with-fit leg was inert (second pin-defeats-setdefault instance)

Verified chain: the wheel_fit_review python block sets `'MYSIM_SOURCE_AXLE_FIT': os.environ.get('SOURCE_AXLE_REVIEW','0')` inside its per-call values; the workflow defines `SOURCE_AXLE_REVIEW` as '1' only for depth `source_axle_review`; `run()` builds the environment as (os.environ minus MYSIM_*) plus the values dict, so the values entry with '0' overrides everything, and `run()`'s own `setdefault('MYSIM_SOURCE_AXLE_FIT', SOURCE_AXLE_FIT)` never applies because the key already exists with '0'.

Fix (coordinator files, one line, either one):
- Workflow: `SOURCE_AXLE_REVIEW: ${{ (inputs.depth == 'source_axle_review' || inputs.source_axle_fit) && '1' || '0' }}`.
- Or python: delete that one entry from the values dict so `run()`'s setdefault uses `SOURCE_AXLE_FIT` (already driven by the `source_axle_fit` input).

Verification after the fix: the rerun must produce `-source-axle.json` artifacts and move the hub z values back by the fitted amount; only then does the turn-4 protocol produce a real delta table.

## 3. Fleet-scale clearance plan (all 24 cars) - dispatch proposal

Measured wall times (GitHub API, both sweep runs):

| Run | prepare | render shards (min) | wall |
|---|---|---|---|
| 41837315358 (no-fit) | 0.1 | 22.1 / 23.1 / 25.4 / 23.3 | about 26 min |
| 41843317303 (fit leg, inert) | 0.1 | 24.6 / 21.4 / 19.1 / 22.1 | about 25 min |

Per-shard cost is import-dominated (source import + three render passes + audits + upload); the sweep itself is seconds. Plan assumes the same shape holds for road cars.

| Item | Proposal |
|---|---|
| Depth | wheel_fit_review (unchanged); per dispatch: 4 cars, one per shard |
|Cars mapping | the python hardcodes car = 15+shard; one-line fix: take the shard-th entry of the existing `cars` input (comma list), e.g. cars=0,1,2,3 |
| Batches | 20 cars remaining = 5 dispatches: [0,1,2,3], [4,5,6,7], [8,9,10,11], [12,13,14,19], [20,21,22,23] (Chiron last, body-integrity pass alongside) |
| Wall time | about 26 min per dispatch measured; 5 sequential about 2.2 h; 2 in flight (queue check first) about 1.2 h |
| Extra env per batch | `source_axle_fit=true` (blocker: the workflow one-line fix from section 2 - without it the fleet fit arms never run) |
| Per-batch verification | the turn-4 protocol per car: the table, the bands, the ladder; audit and mirror lines ride along |

## 4. Chiron (car 20) loose-parts: current-HEAD check

Archive record (VISUAL_REVIEW.md, the supervisor's own frame review): archived `chiron-focus-visuals` frames (wing side/rear/overhead, run 41429382493 era) show loose blue/black parts below the sill and front-fender area; explicit reject; the archive frame is not evidence about the current HEAD.

Minimal capture set at the current HEAD (one dispatch of the car_evidence shape, car 20):

| Shot | Env (all add MYSIM_CARVIEW=20, MYSIM_FITVIEW=1, MYSIM_SHOT=...) |
|---|---|
| left sill, low | `MYSIM_CAMPOS=-2.6,0.55,0.2` |
| right sill, low | `MYSIM_CAMPOS=2.6,0.55,0.2` |
| left front fender | `MYSIM_CAMPOS=-2.4,0.7,1.5` |
| right front fender | `MYSIM_CAMPOS=2.4,0.7,1.5` |
| 3/4 reference | `MYSIM_CAMPOS=3.4,1.3,3.4` |
| machine list | one pass adds `MYSIM_CAR_ASSEMBLY_AUDIT` |

Difference from the archive: the HEAD is pinned and recorded at dispatch; the assembly audit rides with the frames and lists every part's position and visibility, so a detached part is machine-listed (part below the sill line, no body-shell neighbour), not only eyeballed; the sill angles replicate the archived side frame for a like-for-like comparison.

Pass criteria: the reviewer sees no loose blue/black parts at the sill or fender and the audit lists no detached candidates. Reject: any loose part seen; fix through the attachment/mount path at the named part.

## 5. Not covered

- The full-fleet dispatch needs the coordinator one-liners (the cars-input mapping per shard; the workflow workflow line from section 2). Without them the fleet fit runs stay inert.
- Queue capacity: the standing rule - verify actual queues before piling dispatches.
- The 24-car per-car records remain protocol output; the pass/fail stays with the human review of the frames.
