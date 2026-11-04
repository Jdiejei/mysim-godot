# OWNER MERGE INSTRUCTIONS — MySim rebuild line → main (2026-11-04, prepared by the coordinator)

No secrets in this file. All commands run on the OWNER's machine with the OWNER's credentials.
Current auth on the shared workstation is `Jdiejei` (verified by the reviewer: push to the owner repo = 403, MergePullRequest = denied, collaborator-query = 403 Must have push access). The OWNER performs the merge; nothing here requires sharing tokens or granting admin.

## State being merged (verified this session)

| Item | Value |
|---|---|
| Owner repo | `instagramshashank8-bot/mysim-godot` |
| PR #1 | `codex/repair-20261029` → `main`, state OPEN, MERGEABLE/CLEAN, 243 files |
| PR #1 head (owner repo copy) | `a313492e7458e5733c6933d550b7eee803b4b6f1` |
| Fork with NEWER commits | `https://github.com/Jdiejei/mysim-godot` branch `codex/repair-20261029` |
| Fork tip (UPDATED 2026-11-05 audit) | `c4fb876` — the integration head (verified by ls-remote + fetch; the 2026-11-04 text below said `1f4ba0c`, which is now 5 commits behind) |
| Commits ahead of `a313492` | `1f4ba0c` windows-persist single-source saveErrors count + smoke MYSIM scrub (still NO cloud run at ANY of these heads — UNVERIFIED, do not claim Windows fixed) · `0e0da0c` fence miprepair one-liners (stale a2c/blend block no longer captures the miprepair depth; per-level tolerance max(0.02, 2/sqrt(texels)) with safe fallback) · `9006a1a`/`65bbef6` owner handoff + chart · `c4fb876` shard-matrix fix (the chiron grouping bug had swallowed fleet..f1_wheel_diagnostic and ANDed it with [0]; fleet_evidence restores 6 shards; verified by expression parser across 10 depths) |
| What the branch delivers | The full rebuilt line: `windows-game-build.yml` (persist+userpath+smoke steps), `Main.cs` userpath hook, gate battery, hull/mount fixes, HUD/fence/night evidence tooling |

## Option A (recommended): update PR #1 to the fork tip, then merge once

```bash
# from the owner's clone of instagramshashank8-bot/mysim-godot
git checkout codex/repair-20261029
git pull https://github.com/Jdiejei/mysim-godot.git codex/repair-20261029
git push origin codex/repair-20261029        # PR #1 head moves a313492 -> the fork's CURRENT tip (c4fb876 at the 2026-11-05 audit; confirm with `git rev-parse FETCH_HEAD` right after the pull — this file's older text said 1f4ba0c, now 5 commits behind)
gh pr merge 1 --repo instagramshashank8-bot/mysim-godot --merge
```

## Option B (equivalent): merge PR #1 at a313492 first, then fast-forward main

```bash
gh pr merge 1 --repo instagramshashank8-bot/mysim-godot --merge   # merges a313492
git checkout main && git pull
git merge https://github.com/Jdiejei/mysim-godot.git codex/repair-20261029  # or cherry-pick 1f4ba0c
git push origin main
```

## Post-merge verification the OWNER can run (this is what makes 1f4ba0c "verified")

> **UPDATE 2026-11-04 late:** the workstation's dispatch capability changed server-side — `gh workflow run` now returns `HTTP 403: Must have admin rights to Repository` for Jdiejei (dispatches worked earlier the same day). The validation battery below therefore runs from the OWNER's side (their own credentials), either on the merged main or directly on the branch. Commands are ready as-is.

> **AUDIT 2026-11-05 (verification agent) — battery is SIX commands; three corrections applied:**
> (1) command 1's workflow name was `windows-game-build.yml` — that string matches NEITHER the workflow file (`windows-build.yml`) NOR its name (`windows-game-build`, no `.yml`): **this was the real cause of the Q4 "404 workflow not found"**, not merely the stale default branch;
> (2) command 1 was missing the REQUIRED `publication_tag` input (the workflow's first step hard-fails without it; create the tag at the post-merge main head first);
> (3) the sixth command added: the fleet_evidence re-run, unblocked by the shard-matrix fix at `c4fb876` (expression-parser-verified across 10 depths). Commands 1-5 are POST-MERGE-ONLY: main's October-era workflows lack the new depth/case_set choices and reject the inputs with HTTP 422 before the merge; command 0 runs on the branch AFTER the Option-A push (which carries `0e0da0c`).

```bash
# 0) Fence re-run — pre-merge OK only AFTER the Option-A push (the branch must carry 0e0da0c's one-liners);
#    all flags verified against the c4fb876 workflow:
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref codex/repair-20261029 -f depth=fence_miprepair_review
# EXPECT (artifacts): game-runtime-<shard>-attempt-1 + encrypted-review-<shard>-attempt-1 (6 shards — 41830995246's
#        shard 0/4 failures were the GENERIC track block, not these invariants; this is the first run that can
#        exercise the miprepair invariants end-to-end at the fixed tolerance).

# 1) Windows build + persistence + userpath + smoke at the merged HEAD — POST-MERGE ONLY; CORRECTED name + tag:
gh workflow run windows-build.yml --repo instagramshashank8-bot/mysim-godot --ref main -f publication_tag=windows-candidate-<40-hex-of-post-merge-main-head>
#   (create the tag first, pointing at the post-merge main head: git tag windows-candidate-<sha> <sha> && git push origin windows-candidate-<sha>)
# EXPECT (artifacts): windows-build-diagnostics-<run>-attempt-1 → windows-settings-persist-summary.json (all five
#        scenarios ok; failures scenario tmp=1 / rename=2 — the UNCHANGED expectations; the old double-count showed
#        tmp=2/rename=4), the three win_sharing_violation cases present; userpath-summary.json (9 profiles x 2 passes
#        incl. the determinism re-injection check) green; windows-smoke-results.json + windows-render-results.json green.

# 2) Driving validation (collider restore + mounted bake + userpath hook) at the merged head — POST-MERGE ONLY
#    (main's old workflow lacks the driving_validation depth choice → HTTP 422 before the merge):
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=driving_validation -f cars=16
# EXPECT: car DRIVES (distance > 0, no beach) — the prior DV runs failed no-travel/beach; DV_TRACE telemetry in the
#        report; encrypted-report-*/runtime artifacts as for the depth.

# 3) Force-driven wall/contact battery (physics c059e2c spec; jolt first, source pair after) — POST-MERGE ONLY
#    (case_set driving_wall verified present in the choices + the DRIVING_VALIDATION_IMPACT wiring; manifest
#    data/native_driving_validation_wall.json verified present in the tree):
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=driving_validation -f case_set=driving_wall -f cars=16 -f physics_backend=jolt -f physics_hz=240 -f native_damage=true -f rotational_tyres=true
# EXPECT: encrypted-review-* + the DV report covering the wall cases (contact, impulse identity, wall hits, recovery;
#        no bound relaxation); the source-backend pair runs after this one with the same case_set.

# 4) Collision-overlay evidence (track-render 40c7df5; overlay + per-collider audit JSON) — POST-MERGE ONLY
#    (collision_overlay input verified present in the c4fb876 workflow):
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=track_scene_audit -f collision_overlay=true
# EXPECT: encrypted-review-* carrying the overlay renders + the per-collider audit JSON.

# 5) NEW (2026-11-05 audit): fleet_evidence re-run — unblocked by the shard-matrix fix at c4fb876 (the chiron grouping
#    bug had swallowed fleet..f1_wheel_diagnostic and ANDed the shard set with [0]; the fix restores 6 shards;
#    expression-parser-verified across 10 depths) — POST-MERGE ONLY (not in main's old depth choices):
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=fleet_evidence -f cars=16
# EXPECT: SIX shards in the run's matrix (the fix's visible effect); per-shard encrypted-review-* + the fleet manifest
#        coverage from the authoritative 24-car/60-track manifest (74017c6); per-car evidence dirs.
```

Artifacts are encrypted (`encrypted-review-*`); the owner's local key `C:/Users/shash/.codex/private/mysim-review/review-20261031.key` decrypts them via `tools/cloud_decrypt.py` (pattern: `outputs/mistral-orchestration/decrypt-reports.ps1`).

## Explicitly NOT claimed

- `1f4ba0c` is NOT verified until a cloud run at that SHA (or the merged main) passes the Windows persistence suite.
- Physics is NOT accepted: the wheelguard/caliper collision coverage restoration + force-driven contact evidence are still open (physics-gameplay is working them).
- The track-8 legality failure (AI line vs corridor at progress ≈0.195) is diagnosed but NOT fixed; track-render's correction proposal is pending.
- DV 41871263005 failed with cause UNDETERMINED (log retention failed; artifact downloads stalled) — its artifact remains on GitHub for later retrieval.
