# BATCH-A dispatch spec - the 11 POP tracks' D0-day-dry baselines

The concrete battery commands for audit batch A (per `TRACK-AUDIT-PLAN.md` = the POP
set first + the evidence contract). The dispatch is owner-side; verification folds
the patch rows in; this agent renders nothing. Machine-readable companion:
`batch_a_dispatch.json` (same dir). Everything here derives from the MERGED code
paths (`cloud_game_audit.py` + `game-review.yml`) - no invented capability.

## Path 1 - dispatch with the CURRENT workflow (zero workflow changes)

One dispatch runs the full wet/night matrix per track (5 rows incl. D0); the plan's
D0-first = the REVIEW ORDER, not a separate run:

```bash
gh workflow run game-review.yml --repo Jdiejei/mysim-godot --ref codex/repair-20261029 \
  -f depth=weather_review
gh run watch <run-id>
gh run download <run-id> --repo Jdiejei/mysim-godot -n "encrypted-review-<shard>-attempt-1"   # shards 0..5
# decrypt with the established review-key flow (MYSIM_INTAKE_KEY; key never printed)
```

- The shard matrix for `weather_review` = `[0,1,2,3,4,5]` (the depth falls through
  the strategy filter's final branch); every POP track rides one shard:
  task shards: 21/57/9/3 = 3, 54/24 = 0, 26 = 2, 16 = 4, 17/23 = 5, 1 = 1.
- The D0 row per track = `weather-track-<NN>-tod-0-wet-0.png` (the capture =
  `MYSIM_TRACK=<N>`, car 16, `MYSIM_TOD=0`, `MYSIM_WET=0`, AI on/autodrive, no pits
  or rivals, 3 s drive-by - the tool's weather branch verbatim).
- The other four rows per track (night/dry, day/wet-2, night/wet-3, day/heavy) land
  in the same run - the matrix comes free; review them AFTER the D0 rows.

## Path 2 - the D0-ONLY quick battery item (a two-line addition; recommended)

For the owner's quick-battery order (the D0 verdicts before the full matrix):

```yaml
# game-review.yml, inputs:
weather_rows: {description: 'csv of tod_wet pairs for the matrix', type: string, default: '0_0,3_0,0_2,3_3,0_4'}
# env block:
WEATHER_ROWS: ${{ inputs.weather_rows }}
```

```python
# cloud_game_audit.py, the WEATHER_REVIEW branch:
rows = [tuple(int(v) for v in p.split("_")) for p in os.environ.get("WEATHER_ROWS", "0_0,3_0,0_2,3_3,0_4").split(",")]
...for tod, wet in rows:
```

Then batch A =
`gh workflow run game-review.yml --repo Jdiejei/mysim-godot --ref codex/repair-20261029 -f depth=weather_review -f weather_rows=0_0`
(60 D0 frames; the POP-11 is the review-first subset).

## The D0 target list (the 11 POP tracks; names verified against the per-track files)

| Track | Name | Shard | Expected D0 file |
|---|---|---|---|
| 21 | RED BULL RING | 3 | weather-track-21-tod-0-wet-0.png |
| 54 | LEMANS SARTHE | 0 | weather-track-54-tod-0-wet-0.png |
| 57 | REDBULL SOUTH | 3 | weather-track-57-tod-0-wet-0.png |
| 17 | NEW YORK | 5 | weather-track-17-tod-0-wet-0.png |
| 9 | HONG KONG | 3 | weather-track-09-tod-0-wet-0.png |
| 26 | SPA | 2 | weather-track-26-tod-0-wet-0.png |
| 3 | BATHURST | 3 | weather-track-03-tod-0-wet-0.png |
| 23 | SEBRING | 5 | weather-track-23-tod-0-wet-0.png |
| 24 | SILVERSTONE | 0 | weather-track-24-tod-0-wet-0.png |
| 16 | MONZA | 4 | weather-track-16-tod-0-wet-0.png |
| 1 | LAGUNA SECA | 1 | weather-track-01-tod-0-wet-0.png |

## Ledger filing (verification owns it; the row content proposed here)

`outputs/mistral-orchestration/EVIDENCE_INDEX.md` - one row per track, the index's
existing claim/run/head/evidence/verdict shape:

```
| track<N> D0 day/dry baseline captures (batch A, audit plan) | <run> | <dispatch head> | <decrypted weather-track-<NN>-tod-0-wet-0.png> | open |
```

Verdict rule: the reviewer's D0-first pass = these 11 before any night/wet row; every
defect row needs its capture attached; nothing accepted from the dispatch itself.

## Honesty notes

- The depth list, the shard mapping and the capture labels above are read from the
  merged workflow + tool at the current integration head - re-verify before wiring.
- The reference car in the matrix = car16 by the tool's fixed value; the track audit
  does not vary the car (that is the car-cards' domain).
- No dispatches from this agent; no acceptance; everything stays
  code-verified-only until the owner's run + the reviewer's verdicts.
