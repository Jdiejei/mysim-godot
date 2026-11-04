# Car-assembly turn 11 - wheel_fit batch map (dual-state) - 2026-11-06

Commit: `25a8d8b` (axle candidate degrades loudly instead of failing mixed-fleet batches). No dispatches.

## 1. One command shape, five batches

Every batch is the same dispatch; only `cars` changes. Both sweep states ride `source_axle_fit=true` (the merged A/B wiring: raw pivot line + strut-derived candidate line in one run's JSON, rows tagged `axleShift`).

```
gh workflow run game-review.yml -R instagramshashank8-bot/mysim-godot --ref <integrated-head> \
  -f depth=wheel_fit_review -f cars=<BATCH_LIST> -f source_axle_fit=true
```

| Batch | cars | Notes |
|---|---|---|
| B1 | `4,5,6,7` | road cars; axle state may degrade to `unavailable` per car (logged, JSON-recorded) |
| B2 | `8,9,10,11` | same |
| B3 | `12,13,14,15` | car13's mounts already proven at anchors in geo8; car15 = F1, dual-state expected |
| B4 | `16,17,18,19` | the Formulae; dual-state expected for all four; the headline batch (the -45 mm verification) |
| B5 | `20,21,22,23` | Chiron + the body-integrity capture set from turn 6 section 4 rides this batch |
| (done) | `0,1,2,3` | covered by the owner battery's own batch, per its docs |

## 2. Per-batch expectations (the judge-on-arrival values)

For every F1-family car in a batch (the dual-state sweep):

| Field (per wheel group) | axleShift=0 | axleShift=fitted (~-0.476 F1-class) |
|---|---|---|
| front bodywork (FRONTBUMPER) | -0.045 class | +0.20..+0.25; contract >= -0.002 |
| rear bodywork (CHASSIS) | -0.045 class | clear or WATCH; contract >= -0.002 |
| front suspension | +0.048..+0.14 | +0.01..+0.09 clear; hard floor >= -0.002 |
| rear suspension (upper strut) | -0.0203 (car16 class) | clears; contract >= -0.002 |
| stance (no hub parts) | about +0.0099 | unchanged (hull-side) |
| stance (with hub parts) | guard-datum pressure | converges to the no-hubs value when mounts run |

For road cars (single-state after the graceful degradation): the sweep's absolute numbers differ per car; the judge instruments are the same bands applied to their own values, plus the `PART_HUB_OFFSET`/`PART_BELOW_WHEEL_PLANE`/`STANCE` lines from 749f453.

Uniform band rules (turn-4 protocol, unchanged): PASS = within-tolerance (< 2 mm), WATCH = 2-5 mm, FAIL = > 5 mm or any rest-state penetration > 2 mm. PASS is a diagnostic pass, not a physical clearance certificate.

## 3. Coverage invariants (checked in every batch result)

1. `axleCandidate` field present in each sweep (dual/unavailable/off) - a missing field means the run predates 063620d+25a8d8b.
2. wheelguard + brakes categories present in every wheel's `categories` (the lateral coverage guarantee; the hull exclusion must not leak into the audit cloud).
3. `PART_HUB_OFFSET` lines for every guard/caliper part (the misplacement ledger keeps updating).
4. Snapshot restore verification: `pose restored` in the summary line.

## 4. Cadence

About 26 min per dispatch measured (import-dominated); five batches about 2.2 h sequential; two in flight (queue check first) about 1.2 h. B2/B3 and B4/B5 pair well as the concurrent pairs.

## 5. Not covered

- Batch [0,1,2,3] is outside this map (the owner battery covers it).
- Dynamic contact states remain out of scope; static envelope only.
- Car19's axle state is untested data - if its strut roles exist it runs dual; else it degrades like the road cars.
