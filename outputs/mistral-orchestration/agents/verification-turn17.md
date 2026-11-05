# Verification — turn 17 report (2026-11-05: fork-head reconciliation, 142-resolution verified, cockpit rung split)

## Task

Coordinator turn-17: fold the turn-16 corrections into the shared docs (the chart's 142-error already corrected at `226686b` — verify no residual; `OWNER_MERGE_INSTRUCTIONS.md` head now `3865d95` with the pre-merge branch option); then the cockpit ledger item — verify hud turn-10's COCKPIT_INT_UV + the seat-contract check landed in the integration merge, and record them.

## What was verified (all first-hand this session)

1. **Where the integration line lives (corrected in the ledger):** the "fork head 3865d95" = the **Jdiejei fork** (`github.com/Jdiejei/mysim-godot`) `codex/repair-20261029` branch, found via ls-remote (full SHA `3865d9512ddd32cfbbc1922103de211756769cf2`); the OWNER repo's branch stays at `a313492` (= PR #1 head, re-verified OPEN / MERGEABLE/CLEAN this turn). The integration commits beyond my previous base (`3cd5aed`) are exactly TWO, both docs-only: `226686b` (chart row-0 correction) and `3865d95` (owner handoff). Because they carry zero code delta, a battery dispatched on the branch ref is code-identical to the merge content — recorded in EVIDENCE_INDEX.
2. **142 residual check: CLEAN.** Grep of the chart at `3865d95`: exactly ONE "142" occurrence — inside the coordinator's own correction note ("the earlier '142 orientationClass rows' figure was a string-count error - the verified count is 104/104 rows across car16(15)/car6(17)/track0(72)"). The count matches my independent turn-16 verification (104/104), so the ledger flag is now marked RESOLVED.
3. **OWNER_MERGE_INSTRUCTIONS.md at `3865d95` verified:** header "updated 2026-11-05 (coordinator)"; the new top paragraph states the current integration/pre-merge SHA `226686b` on the Jdiejei fork branch (API-verified pushed) and documents the pre-merge option (substitute `--ref codex/repair-20261029` per command) next to the post-merge `--ref main` variants. No contradiction found with the workflows' actual inputs.
4. **Cockpit rung — SPLIT verdict (the remaining ledger item):**
   - **Seat-contract check: LANDED.** Introduced by hud `91f665f` ("Product gate: cockpit camera contract asserts (fov/seat/pitch/handedness)") + `527673a` ("Touch layout headroom + cockpit camera contract"), merged via `ce87f1b`. Verified present at `3865d95` (tree-grep): `CockpitSeat` DriveController.cs×3 + ProductControlsScenario.cs×2, `handedness` ProductControlsScenario.cs×1. Status: INTEGRATED-UNVALIDATED.
   - **COCKPIT_INT_UV: NOT LANDED.** Zero occurrences at `3865d95` across src/data/tools. The deliverable exists ONLY as hud worktree commit `7abe886` ("Turn-10: recovery anchored, rebase clean, gap column + cockpit INT-UV emission") on the local `mistral/hud-camera` branch (worktree clean) — **pushed to NO remote** (both forks' heads enumerated). Carbody's mirrored-instrument car16-INT candidates (gated on "the cockpit-audit extension") therefore stay GATED. Action needed (coordinator/hud): push/merge `7abe886` into the integration line, then a run + human review before any cockpit claim moves.

## Work-tree state changes

- Verification branch rebased onto the integration tip: `mistral/verification` = `3865d95` + the turn-replay (`f6b3440`, `9d0f6ba`, the turn-16 ledger as `8284a96`) + this turn's commit on top. One conflict during the replay (`REMAINING_WORK_CHART.md`, at the turn-15 commit): resolved by taking the coordinator's corrected row 0 VERBATIM and restoring my APPLIED-corridor row 1 (`b538efd`); no weakening, no content dropped.
- Ledger writes: EVIDENCE_INDEX.md — `Updated:` line → 3865d95; the INTEGRATED-UNVALIDATED section's fork-head reconciliation paragraph; the 142-flag → RESOLVED; the new `Cockpit rung` section (seat-contract LANDED / COCKPIT_INT_UV NOT LANDED with the exact evidence). REVIEW_PACKET.md — top CURRENT STATUS block refreshed with the same facts.

## Standing gates — UNCHANGED

Windows tmp=1/rename=2; crash-consistency through the delete-then-rename window OPEN; PHYSICS UNACCEPTED until contact + force-driven DV evidence land green and are human-reviewed; greps/manifests stay PRELIMINARY — no run anywhere has exercised `3cd5aed`/`3865d95` code; PR #1 merge = owner's action.
