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
| Fork tip | `1f4ba0c2d250b16e3b68f3d4ca98a1d9e5416213` (2 commits ahead of `a313492`) |
| The 2 extra commits | `1f4ba0c` windows-persist single-source saveErrors count + smoke-pass MYSIM_* scrub (`tools/cloud_windows_persist.py`, `tools/cloud_windows_smoke.py`; 8 insertions, 4 deletions) — **not yet verified by any cloud run** |
| What the branch delivers | The full rebuilt line: `windows-game-build.yml` (persist+userpath+smoke steps), `Main.cs` userpath hook, gate battery, hull/mount fixes, HUD/fence/night evidence tooling |

## Option A (recommended): update PR #1 to the fork tip, then merge once

```bash
# from the owner's clone of instagramshashank8-bot/mysim-godot
git checkout codex/repair-20261029
git pull https://github.com/Jdiejei/mysim-godot.git codex/repair-20261029
git push origin codex/repair-20261029        # PR #1 head moves a313492 -> 1f4ba0c
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

```bash
# 0) Fence re-run (the two one-liner fixes are applied at 0e0da0c — first run that can exercise the invariants end-to-end):
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref codex/repair-20261029 -f depth=fence_miprepair_review

# 1) Windows build + persistence + userpath + smoke at the merged HEAD:
gh workflow run windows-game-build.yml --repo instagramshashank8-bot/mysim-godot --ref main
# EXPECT: the failures scenario reports tmp=1 / rename=2 (the old double-count was tmp=2/rename=4);
#         all five scenarios green incl. the three win_sharing_violation cases; userpath profiles green.

# 2) Driving validation (collider restore + mounted bake + userpath hook) at the merged head:
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=driving_validation -f cars=16
# EXPECT: the car DRIVES (distance > 0, no beach), the 9-target contract holds.

# 3) Force-driven wall/contact battery (physics c059e2c spec; jolt first, source pair after):
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=driving_validation -f case_set=driving_wall -f cars=16 -f physics_backend=jolt -f physics_hz=240 -f native_damage=true -f rotational_tyres=true

# 4) Collision-overlay evidence (track-render 40c7df5; overlay + per-collider audit JSON):
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=track_scene_audit -f collision_overlay=true
```

Artifacts are encrypted (`encrypted-review-*`); the owner's local key `C:/Users/shash/.codex/private/mysim-review/review-20261031.key` decrypts them via `tools/cloud_decrypt.py` (pattern: `outputs/mistral-orchestration/decrypt-reports.ps1`).

## Explicitly NOT claimed

- `1f4ba0c` is NOT verified until a cloud run at that SHA (or the merged main) passes the Windows persistence suite.
- Physics is NOT accepted: the wheelguard/caliper collision coverage restoration + force-driven contact evidence are still open (physics-gameplay is working them).
- The track-8 legality failure (AI line vs corridor at progress ≈0.195) is diagnosed but NOT fixed; track-render's correction proposal is pending.
- DV 41871263005 failed with cause UNDETERMINED (log retention failed; artifact downloads stalled) — its artifact remains on GitHub for later retrieval.
