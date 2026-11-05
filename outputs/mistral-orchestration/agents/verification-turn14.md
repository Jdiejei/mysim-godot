# verification agent — turn 14 report (the fork push-run reconciliation + the post-battery checklist template)

Everything verified first-hand before recording: the fork's tip and run history via the fork's own API (Jdiejei's repo — full read access), the fork's tree via git fetch, PR #1 via `gh pr view`. No dispatches (none needed: the reconciliation reads EXISTING runs); no acceptance claims.

## 1. The fork push-run reconciliation (recorded in EVIDENCE_INDEX)

- **The mechanism (established last turn, confirmed here):** a workflow file that fails GitHub's load validation produces a failed no-jobs run on every push; a VALID dispatch-only workflow is fully silent on pushes. Push-runs are therefore a parse-failure detector, and their ABSENCE is the parse-clean signal.
- **The broken window's record (the fork):** 41895776211 (`9618bd1`-era wall wiring) → 41895855007 (`0e0da0c`) → 41895887541 (`9006a1a`) → 41895922373 (`65bbef6`) → 41897842160 (`c4fb876`) → 41897949436 (`4566853`) → **41905184212 (`4996602`) = the LAST parse failure** — seven consecutive failed push-runs spanning the 26-input window.
- **The clean record:** 35 commits pushed after `4996602` (the fork tip now `46e8f9b73756692b9f994cb8ce2bc266f87423d2`), ZERO push-runs → parse-clean at every head since the 25-input fix. Directly verified at the tip: exactly 25 inputs; the single `COLLISION_OVERLAY` line is my depth-driven form (verbatim match).
- **The integration chain:** both named integration heads (`836de96`, `76c1c43`) verified ANCESTORS of the fork tip (fetch + merge-base). **NO run anywhere has exercised the integrated code** — the owner's six-command battery is the only validation path; parser/parse-clean states say nothing about gameplay.
- **PR #1:** verified still OPEN at `a313492`, MERGEABLE/CLEAN — the merge (owner action) will carry the integration lineage only via the owner's own pull/merge step in the handoff doc.

## 2. The post-battery acceptance checklist template (recorded in EVIDENCE_INDEX as a fill-in section)

A six-row table — one per battery command — with pre-verified expected artifacts (from the workflow sources: the 6-shard runtime/encrypted evidence + the miprepair invariant block for the fence; the persist-summary (tmp=1/rename=2 + win cases) + the userpath summary (9 profiles × 2 passes + determinism) + smoke/render results for windows; the DV report fields (drive distance, no-beach, 9-target contract, DV_TRACE); the wall-battery evidence set; the overlay renders + per-collider JSON; the 6-shard fleet manifest coverage), plus per-row: who reviews (verification / physics-gameplay / track-render / car-assembly / the human reviewer), and EXACTLY which gate each result may move. Filling rules stated: only TERMINAL runs; only artifacts actually READ; no gate moves on a green-but-unproven row. The guards row repeats the three standing opens so nobody "discovers" them as fixed: **tmp=1/rename=2; crash-consistency OPEN; PHYSICS UNACCEPTED** (battery commands 2+3 are prerequisites, not sufficiency).

## 3. Unchanged stands

- Windows assertions: tmp=1/rename=2 (verified unchanged at the fork tip's driver).
- Crash-consistency through the delete-then-rename window: OPEN.
- Physics: UNACCEPTED.

## 4. Updated files

`EVIDENCE_INDEX.md` (the reconciliation section + the checklist template), `REVIEW_PACKET.md` (the merge-block section refreshed: the current fork tip, the reconciliation one-liner, the PR state), this report. All on `mistral/verification` (rebased on the fork's `46e8f9b` line via FETCH_HEAD).
