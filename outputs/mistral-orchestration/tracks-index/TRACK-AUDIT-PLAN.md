# Per-track VISUAL AUDIT EXECUTION plan (the battery-attachable plan)

Generated from the same data basis as TRACKS-INDEX (route size = the measured priority signal).
The audit batches + per-track evidence contract; the audit outputs land in the LEDGER via the
verification side (the LEDGER owns the row format; this plan defines the inputs).

## Batching (the POP set first)

- **Batch A (first)**: the tracks indexed 1, 3, 9, 16, 17, 21, 23, 24, 26, 54, 57
  (the 10 largest routes + track 1; the same POP list the tracks-index uses)
- **Batch B (second)**: 27, 32, 28, 51, 6, 33, 19, 46, 25, 5, 8, 31, 35, 4, 12, 7, 10, 40, 42, 53
- **Batch C (third)**: 38, 45, 36, 50, 15, 30, 59, 48, 58, 47, 14, 39, 2, 43, 56, 13, 18, 20, 55, 41, 49, 11, 52, 0, 34, 29, 44, 22, 37

## The per-track capture set (every track, every batch)

- **D0-day-dry** - day/night-matrix D0 row (the day0 baseline capture): baseline dry/day view set - FIRST (the D0-first matrix order)
- **N-night** - night-light-track-<N>-wet-<W>[-candidate-<C>]: the night/wet rows per the battery matrix (wet 0..3; candidate = the night-lighting candidate set where applicable)
- **COV** - night-weather-coverage-track-<N>.json: the coverage manifest row per track (profiles + the capture list)

## The evidence contract (per-track audit output)

- the audit yields per-track: the capture rows (the label sets above) + the defect rows; nothing else
- each capture row = the same track/N + the SAME camera + the same TOD/wet row as its batch-mate (the matrix rows)
- the audit outputs land in the LEDGER via the verification side: the audit JSON + the defect rows fold into the LEDGER's track rows (the ledger owns the schema)
- no defect row may be emitted without the capture rows that back it (absent data never invented)
- the audit = per-track; no multi-track audit row

## Batch computation (the actual point counts)

| batch | tracks (id: points) |
|---|---|
| A | 1:226; 3:389; 9:465; 16:364; 17:488; 21:1079; 23:368; 24:366; 26:441; 54:852; 57:584 |
| B | 27:362; 32:361; 28:348; 51:348; 6:344; 33:338; 19:322; 46:321; 25:317; 5:292; 8:286; 31:280; 35:272; 4:269; 12:262; 7:252; 10:252; 40:252; 42:237; 53:233 |
| C | 38:232; 45:227; 36:225; 50:216; 15:208; 30:194; 59:194; 48:187; 58:169; 47:166; 14:164; 39:163; 2:160; 43:153; 56:151; 13:149; 18:149; 20:149; 55:149; 41:141; 49:141; 11:127; 52:124; 0:121; 34:110; 29:108; 44:94; 22:71; 37:71 |

## Batch-A dispatch (the concrete battery commands)

The batch-A dispatch spec = `BATCH-A-DISPATCH.md` + `batch_a_dispatch.json`
(same directory): the existing capability path (one `game-review.yml`
depth=weather_review dispatch = the full wet/night matrix; the POP D0 rows =
the review-first subset), the proposed minimal two-line addition for a
D0-ONLY quick item (the `weather_rows` input + the audit-tool loop reading it),
the exact gh commands, the expected per-track D0 file names, and the ledger
row templates. Verification owns both the workflow and the ledger; this spec
is what they fold in.

The plan computes from data/TrackList.json only; no other condition enters the
batching (the wet/night SETS stay per-track rows; the batch assignment = the
priority order only).
