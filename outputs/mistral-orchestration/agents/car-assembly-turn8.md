# Car-assembly turn 8 - axis-line runs for the remaining batches - 2026-11-06

Committed this turn: `749f453` (StanceAudit wired into the wheel-fit audit; hub lateral/longitudinal deltas with `PART_HUB_OFFSET` lines for guards/calipers; bounds read from `data/native_suspension_profile.json`). No dispatches.

## 1. New evidence channel (rides in EVERY candidate run)

The wheel-fit JSON now carries a `stance` block; console adds three line classes:

| Line | Meaning |
|---|---|
| `STANCE car=N stance=X floorPart=P stanceNoHubParts=Y floorPartNoHub=Q preloadRequired=A preloadRequiredNoHub=B maximumPreload=0.12 bound=V` | stance including all stationary parts vs excluding hub geometry; the with-guards number reproduces the 0.235/0.12 gate arithmetic (guard datum below the wheel plane), the no-guards number is the real floor stance; bound verdict per car |
| `PART_BELOW_WHEEL_PLANE car=N part=P axle=A lowestY=X delta=D` | per part hanging below the wheel plane |
| `PART_HUB_OFFSET car=N part=P hub=H lateralDelta=DL longitudinalDelta=DN` | guards/calipers only: the part's placement offset from its hub (the lateral coverage) |

The `lateralDelta`/`longitudinalDelta` fields also land in the JSON `stance.parts[]` rows for every part.

## 2. Wheelguard/caliper lateral coverage - policy

- The 0de3123 hull exclusion removes wheelguards/calipers from the COLLISION hull bake only. Audits keep them: BodySampleCloud and PartLowPoints still include them (categories `wheelguard`/`brakes`), so every sweep candidate keeps their lateral clearance coverage.
- The new PART_HUB_OFFSET lines quantify their placement per hub in every run - the known shell misplacement (shells ~1.2-1.6 m from their wheels, turn-5 data) stays tracked until fixed.
- No audit excludes them; no config may drop them from candidate sweeps.

## 3. Axis-line run configs for the remaining batches

Prerequisites (coordinator one-liners, from turn 6 section 2 - still blocking):

1. cars-mapping: the wheel_fit_review python takes the shard-th entry of the `cars` input instead of hardcoded 15+shard.
2. axle arm: `SOURCE_AXLE_REVIEW` workflow line becomes `${{ (inputs.depth == 'source_axle_review' || inputs.source_axle_fit) && '1' || '0' }}` (or delete the python's pinning entry so `run()`'s setdefault sees `SOURCE_AXLE_FIT`).

Per batch (five remaining; ref = the merged head containing 749f453):

```
gh workflow run game-review.yml -R instagramshashank8-bot/mysim-godot --ref <head> -f depth=wheel_fit_review -f cars=0,1,2,3  -f source_axle_fit=true
# same shape: cars=4,5,6,7 | cars=8,9,10,11 | cars=12,13,14,19 | cars=20,21,22,23
```

Per batch expectations:
- artifacts: wheel-fit + assembly JSONs per steer pass, `-source-axle.json` per pass (the fit proof), PNGs.
- verification: the turn-4 protocol per car (per-wheel worst table, PASS/WATCH/FAIL bands), plus the new `STANCE`/`PART_HUB_OFFSET` lines: the stance no-guards value is the car's static stance (compare against ~+10 mm class), the with-guards value reproduces the guard-datum pressure on the bound.
- wall time: about 26 min per dispatch (measured); 5 sequential about 2.2 h; queue check before running two in flight.

Batch 5 (cars 20-23) additionally carries the Chiron body-integrity capture set from turn 6 section 4 (sill/fender stills + assembly audit) at the same head.

## 4. Not covered

- The two prerequisite one-liners are coordinator files; until they land the axis-line arm stays inert in wheel_fit_review dispatches, and batch runs would repeat the turn-6 null delta.
- Dynamic contact (transients, kerb strikes) remains future work; the sweeps cover the static envelope only.
- The guard shells' correct placement is unresolved - tracked by PART_HUB_OFFSET until a source-justified mount repair is approved.
