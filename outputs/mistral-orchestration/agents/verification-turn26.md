# Verification — turn 26 report (2026-11-05: 4179e30 ledger; the three caveat closures verified; the matrix's last-mile wiring gap identified)

## Task

Coordinator turn-26: the ledger for `4179e30` (the four merges + the three caveat closures); fold carbody's BATCH-A commands into the handoff's matrix section when they land; the packet refresh (the matrix "end-to-end executable: prerequisites closed").

## What was verified (first-hand, this session)

1. **Lineage:** my turn-25 tip was NOT an ancestor (the integration line built past it) → the verification branch REBASED onto `4179e30` cleanly. The FOUR merges (merge order): `0e2d952` (physics b7c5138: the two-instance LAN test — the script + the hooks `LAN_HOST`/`LAN_JOIN`/`AUTOSTART`/`QUIT_AFTER` + the sync-observed marker + the host exit summary + reading-card §13) → `91457d1` (track 882d3b2: the D0 identity checker + the baseline manifest — the D0-first gate becomes executable) → `8a48a69` (carbody 8e3123c: the per-track visual audit plan — `TRACK-AUDIT-PLAN.md` + `audit_contracts.json`; batch A = tracks 1,3,9,16,17,21,23,24,26,54,57; the capture set per track; the per-track evidence contract: capture-backed rows only, LEDGER-folding via the verification side) → `a5bd221` (my turn-25 ledger) → `4179e30` (the prerequisites).
2. **The three turn-25 caveats — closure verified one by one:**
   - **(1) The 4-case manifest pair LANDED:** `data/native_wetnight_matrix_dry.json` = 4 cases (cars 16/0 × tracks 0/8, NO weather keys) + `data/native_wetnight_matrix_wet.json` = the SAME 4 cases with weather=2 (both parsed). The tool's weather handling turned conditional (`if 'weather' in case:`) while the wet-name assert (weather>0 for wet-named manifests) is INTACT — the dry file passes, no assertion weakened.
   - **(2) The 11th env port LANDED:** `MYSIM_NIGHT_FIXTURE_VIEW` setdefault (tool line 9) AND the fixture-view execution (line 281: `source-night-fixtures-<track>` runs with `MYSIM_NIGHT_FIXTURE_AUDIT` + `MYSIM_NIGHT_FIXTURE_VIEW`) — N2's station frames unblocked.
   - **(3) The py_compile gate LANDED in windows-build.yml** (the compile line covering the audit + the four windows tools).
3. **THE LAST-MILE GAP FOUND (the turn-context's "end-to-end executable" is one edit short):** the matrix pair is committed but NOT SELECTABLE — `case_set`'s 32 choices (line 28 read verbatim) contain no wetnight option; the `DRIVING_VALIDATION_MANIFEST` chain (line 208, read verbatim) maps driving_impact/driving_wet(=the OLD 2-case wet)/driving_wall/core only; `native_wetnight_matrix_{dry,wet}.json` appears NOWHERE in the yml. CONSEQUENCE: my turn-25 commands (`case_set=driving_core`/`driving_wet`) run the OLD manifests — a DEGRADED matrix (core = the 9-target contract, not the 4-case cells; wet = track-0-only). **The exact closing edit proposed in the handoff:** the two case_set options + the two line-208 chain entries (choices cost no input-count; 25 stays).
4. **The BATCH-A fold: PENDING** (the plan defines the batching + the capture set but NO dispatch strings exist yet) — the handoff's matrix section now carries the fold-note: the batch-A track list, the label convention (`night-light-track-N-wet-W[-candidate-C]`), the same prerequisite shape per batch (the per-track cases missing from the pair — the pair covers tracks 0+8 only), and the evidence contract (capture-backed per-track rows folding into the LEDGER my side owns).

## Ledger writes

- OWNER_MERGE_INSTRUCTIONS.md: the matrix section header + the prereq lines updated to the VERIFIED closures; the LAST-MILE GAP paragraph + the exact closing edit; the BATCH-A fold note; the D0/N0/N1/N2/W1/W2/W0 commands retained (marked degraded-until-wired, with the substitution rule AFTER the case_set edit).
- EVIDENCE_INDEX.md: heading/status → 4179e30; the turn-26 merges + the three closures first-hand + the last-mile gap.
- REVIEW_PACKET.md: the top CURRENT STATUS block (turn 26) — includes the honest correction: "ONE YML EDIT AWAY", not "executable now".
- This file: agents/verification-turn26.md.

## Standing gates — UNCHANGED

Windows tmp=1/rename=2; crash-consistency through the delete-then-rename window OPEN; PHYSICS UNACCEPTED until contact + force-driven DV evidence land green and are human-reviewed; everything CODE-PRESENT-UNVERIFIED until runs + human review; no run anywhere has exercised 4179e30 code; PR #1 (a313492) merge = the owner's action.
