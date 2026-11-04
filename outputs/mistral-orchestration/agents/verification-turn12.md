# verification agent — turn 12 report (handoff audit: OWNER_MERGE_INSTRUCTIONS verified + finished)

Scope: the reviewer's directive — verify the handoff's commands against the actual workflow inputs, finish the battery (the sixth command), confirm the chart rows carry only verified facts, keep the Windows/consistency opens. All audit claims verified against the c4fb876 tree (my worktree rebased onto it) and live gh state; corrections committed here. No dispatches; the owner runs the battery.

## 1. Command-by-command audit of OWNER_MERGE_INSTRUCTIONS.md

| # | Command (as found) | Verdict | Evidence + action taken |
|---|---|---|---|
| 0 | `game-review.yml --ref codex/repair-20261029 -f depth=fence_miprepair_review` | VALID with a precondition | flags verified (the depth exists); the fixes it exercises are `0e0da0c` — verified an ANCESTOR of c4fb876 (so the Option-A push carries them); the doc now says explicitly it must run AFTER the Option-A push, and names the expected artifacts (game-runtime-*/encrypted-review-* × 6 shards) + why this is the first decidable attempt (41830995246's failures were the GENERIC track block, not the invariants — verified last turn) |
| 1 | `windows-game-build.yml --ref main` (no flags) | **BROKEN ×2 — fixed in the doc** | (a) the NAME: no workflow file or name matches `windows-game-build.yml` (the file is `windows-build.yml`; the workflow name is `windows-game-build` without .yml) — this was the real Q4 "404 workflow not found" cause, now proven from the workflow list; (b) `publication_tag` is a REQUIRED input (required:true + the first step hard-fails without it) — the command dispatched nothing. The corrected command: `windows-build.yml ... --ref main -f publication_tag=windows-candidate-<40-hex-of-post-merge-main-head>` with the tag-creation precondition line |
| 2 | `game-review.yml --ref main -f depth=driving_validation -f cars=16` | VALID POST-MERGE ONLY | flags verified (the depth exists at c4fb876; cars exists); main's October workflow lacks the choice → 422 pre-merge; marked; expected evidence noted (drive distance > 0, DV_TRACE) |
| 3 | `... depth=driving_validation -f case_set=driving_wall -f cars=16 -f physics_backend=jolt -f physics_hz=240 -f native_damage=true -f rotational_tyres=true` | VALID POST-MERGE ONLY | `driving_wall` verified IN the case_set choices (line 28) + the DRIVING_VALIDATION_IMPACT env wiring mentions it; the manifest `data/native_driving_validation_wall.json` verified present; the source-pair note kept |
| 4 | `... depth=track_scene_audit -f collision_overlay=true` | VALID POST-MERGE ONLY | `collision_overlay` input verified present (2 workflow mentions); `40c7df5` (the referenced track-render commit) verified to exist |
| 5 | (no command existed) | **ADDED — the sixth entry** | `game-review.yml --ref main -f depth=fleet_evidence -f cars=16`, POST-MERGE-ONLY; the unblock verified DIRECTLY in the workflow: line 77 `inputs.depth == 'fleet_evidence' && '[0,1,2,3,4,5]'` — the chiron-group bug's swallow + AND-with-[0] is gone; expected: 6 shards + per-shard encrypted evidence + the 74017c6 manifest coverage |

The c4fb876 fix itself: a ONE-LINE change to the shard expression (verified via `git show c4fb876`); the "parser across 10 depths" claim is the coordinator's out-of-band verification (the parser is not a committed file) — the direct expression read confirms the FLEET part of the claim, which is what the sixth command depends on.

## 2. Chart audit (REMAINING_WORK_CHART.md) — corrections applied

- Row 0: fork tip `9006a1a` → **`c4fb876`** (verified via ls-remote + fetch; the stale hash predated three handoff commits + the fence fixes + the shard fix); "five-command battery" → **six-command** (with the two window-command corrections noted); the exit gate now includes the fence tolerance + the 6-shard fleet re-run.
- Row 1: verified the cited runs exist with matching results (41880754125 / 41880755936 — both completed/failure at a313492, consistent with the shard-8 legality diagnosis record); no change needed beyond the row-0 context.
- Row 2: verified `c059e2c` + merge `adbd466` exist and `adbd466` ∈ c4fb876; the wall-manifest file verified present; the 41871263005 cause-undetermined record matches my turn-11 entry. No correction needed.
- Row 3: corrected the profile count to the delivered set (9 = probe + 8 patterns) and recorded that the harness is wired (a313492) with NO green cloud run yet.
- Rows 4-9: audits of record only (no verifiable run claims cited) — left as written.

## 3. Unchanged, as directed

- Windows persistence: the expected saveErrors stay **tmp=1 / rename=2** (the fork fix corrected the COUNTING, not the expectations); no Windows run exists at any fixed-head SHA — Windows stays OPEN-verified-as-of-41828819144 and UNVERIFIED at the fix head.
- **Crash-consistency through the delete-then-rename window: OPEN** (no crash-injection test exists).
- **Physics: UNACCEPTED** until contact + force-driven evidence land (battery items 2/3 are the path).
- No identical failed re-dispatches proposed anywhere; the battery's failed-run corrections are all distinct-fix based.

## 4. Committed

`OWNER_MERGE_INSTRUCTIONS.md` (state table + Option A hash refresh + the six-command battery with per-command artifacts and pre/post-merge validity), `REMAINING_WORK_CHART.md` (rows 0/3), `EVIDENCE_INDEX.md` (the audit record + the geometry-run verification), this report. All on `mistral/verification` (rebased onto c4fb876 first).
