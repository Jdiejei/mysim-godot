# physics-gameplay — the wet/night matrix reading card (§15) + the execution blockers found

## Task

The per-cell reading rules for the D0/N/W cells (what each cell proves, the D0-first gate, the failure fingerprints per cell) so the owner runs the matrix mechanically.

## CRITICAL FINDING — the matrix as wired at c677943 is NOT executable (three blockers, verified against the integration's own artifacts)

1. **B1 — the dispatch dies at boot (FIXED on my branch):** the wetnight case files' cases carried NO `drivingValidation` lists; the wetnight case_sets are wired into the `DRIVING_VALIDATION_MANIFEST` chain ONLY (game-review.yml:208), and `ParseCaseProfiles` (src/Logic/DrivingValidation.cs) throws `"Driving validation case ... has no drivingValidation profiles"` for exactly that shape. Every `depth=driving_validation case_set=wetnight_matrix_*` dispatch would die before the first tick. FIX DELIVERED: the corrected pair on my branch (`data/native_wetnight_matrix_{dry,wet}.json`) — the per-case `drivingValidation` lists added, mirroring the core/wet manifests exactly (car16 carries `edge_excursion`, car0 does not — the established asymmetry); everything else byte-faithful to the integration's design (the dry file keeps its no-weather-keys convention; the wet file keeps weather 2 + compound 3). **The coordinator merges the pair before the first matrix dispatch.**
2. **B2 — the tool kills the DRY file (proposal; not my file):** `cloud_game_audit.py:114` asserts `all(weather>0)` for any manifest whose NAME contains "wet" — `"wetnight_matrix_dry"` contains "wet", so the DRY file dies at the tool's assert REGARDLESS of content (dry cases cannot carry weather>0; explicit `weather:0` still fails `0>0`). Proposed one-line fix honouring an explicit marker: `if spec.get('requiresWetWeather', 'wet' in manifest.name and 'wetnight' not in manifest.name):` — backwards compatible; my wet file already carries `"requiresWetWeather": true`. The verification agent's "the tool treats missing weather as the 0-default" note covers the game-side env path (line 124) but missed the name-based assert — the same py_compile-passes/semantics-fail class as the workflow-parse incident.
3. **B3 — track 8 silently never runs (dispatch design; coordinator/verification):** the `driving_validation` depth runs shard `[0]` ONLY (the workflow's strategy matrix) and the tool filters `case['track']%12==shard` — the matrix's track-8 cases (8%12=8) match NO shard: the workflow goes GREEN with half the matrix missing. Options: (a) extend the shard list for these dispatches (yml), (b) move the second track to one with `track%12<=5` (the verification agent's matrix-design call — no code change), (c) a shard-filter exemption for the wetnight manifests. Until one lands, only the track-0 half of every cell is real.

## Deliverables

1. **Reading card §15** — the per-cell reading rules: the blocker box (B1-B3, prominent), the dispatch shape (which case_set serves which cells; the env-block cell identity; the owner doc stays the operational source of truth), the D0-first gate (no cell verdicts without a verified D0; failures collected, `UNVERIFIED(no-baseline)` issued), the per-cell table (D0/N0/N1/N2/W1/W2/W0: what each proves, the DV read, the visual read — incorporating track-render turn-23's visual criteria verbatim where they exist: rain streaks ≥2px at 10-40m on ≥5 frames, the zero-delta dry cells, the pool-budget/anchored checks, the fence-wire BLOCKING watch — and the fingerprints per cell), and the cross-cell invariants (physics-invariance-under-lighting: N*≈D0, W2≈W1 — the cosmetic envs must not move the DV metrics; the direction expectations as analyst checks; §14 riding every cell; PARTIAL recording for a partially-run matrix — never green).
2. **The corrected matrix pair** (my `data/native_*.json` charter) — B1's fix + the `requiresWetWeather` marker.
3. **CoreChecks** — the pair's manifest validation added to the scaffolding block (+49 assertions): the lists present, the profile references resolve, the dry file stays dry-default, the wet file keeps weather 2 + compound 3 + the pin. This locks B1's fix against regression at the CI gate.

## Verification

- Clean build: 0 warnings, 0 errors; CoreRunner `PASS 88179 assertions` (+49), 0 FAIL.
- Every blocker claim cites read code this session: `ParseCaseProfiles` (the throw), game-review.yml:208 (the DV-chain wiring) and :76 (the shard `[0]` strategy) and the tool's line 114/124 (the name assert + the env translation). The B2/B3 fixes are NOT implemented (not my files) — proposals only.

## Risks / open

- B2/B3 await the owners' fixes; until BOTH land (plus B1's merge), any matrix dispatch either dies (B1/B2) or green-lies about track 8 (B3). The card's BLOCKERS box states this so a green matrix run is not mistaken for a executed matrix.
- The physics-invariance tolerance ("a few %") is an analyst heuristic until the first D0-vs-N0 pair exists to calibrate the actual run-to-run noise; the hard rule (the bands hold in every cell) needs no calibration.

## Commit

`data/native_wetnight_matrix_dry.json`, `data/native_wetnight_matrix_wet.json`, `src/Logic/CoreChecks.cs`, `outputs/mistral-orchestration/agents/physics-gameplay-reading-card.md`, this report — on `mistral/physics-gameplay`.
