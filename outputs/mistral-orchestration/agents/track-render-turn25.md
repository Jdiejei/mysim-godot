# track-render turn 25 — D0 runner-hook spec: where the identity check runs

One-page operational note for the owner battery. No code changes this turn; the yml suggestion below is COORDINATOR-OWNED (I edit nothing).

## VERDICT (the placement decision)

**The D0 checker runs POST-DOWNLOAD on the owner's workstation. The workflow itself gets at most one 2-line pre-encryption hash hook (optional but recommended). Nothing runs the checker in-workflow.**

Why not in-workflow:
1. **The baseline is unreachable there.** The reference frames live inside run 41843204651's `encrypted-review-*` artifact; only the owner's `review-20261031.key` decrypts it. The runner has neither the key nor any decrypted baseline.
2. **The checker is Windows-bound by design.** Tier B (the pixel diff) uses System.Drawing/GDI+ (Windows-only since .NET 6); the runner is Ubuntu-`xvfb`. Tier A (SHA-256) is portable, but a runner-side check without the decrypted baseline can only compare against *itself* - worthless for identity.
3. **The yml discipline.** The 25-input cap is spent; the workflow edits should stay one-liners. The hook below is two shell lines, no inputs, no new artifacts.

## RECOMMENDED WORKFLOW HOOK (coordinator edit; placement is exact)

In `game-review.yml`, insert BETWEEN the render step (`run: xvfb-run -a python tools/cloud_game_audit.py`, yml ~line 294) and the `Preserve encrypted visual evidence` step (~line 295) - inside the shard matrix, so every shard emits it:

```yaml
- name: Frame hash manifest (identity evidence, pre-encryption)
  run: cd outputs/cloud-game && for f in *.png; do sha256sum "$f"; done | tee frame-hashes.log
```

Effect: `frame-hashes.log` (a) joins the encrypted bundle like any frame, (b) matches the public runtime artifact's `*.log` pattern (yml lines ~316-320), so **the D0 verdict input ships unencrypted** - the owner (or any reviewer) can byte-verify the run against the turn-24 manifest without downloading the encrypted artifact or touching the key. The hash list contains no render content; nothing about the evidence-privacy model changes.

## OWNER RUNBOOK (the local, mandatory part)

Prereq: `track-render-turn24-d0-manifest.csv` (committed, has name/bytes/SHA-256 for the 5 baseline frames) and the local copy of run 41843204651's decrypt (`outputs/repair-20261029/mip3-41843204651/decrypted/review/`).

```powershell
# 1. fast path - if the workflow hook is merged (from the public runtime artifact):
#    gh run download <run-id> --repo instagramshashank8-bot/mysim-godot -n game-runtime-0-attempt-1 -D out/d0-runtime
$ref   = (Import-Csv <path>/track-render-turn24-d0-manifest.csv).sha256
$frame = (Get-Content out/d0-runtime/outputs/cloud-game/frame-hashes.log | ForEach-Object { ($_ -split '\s+')[0] })
if (($frame | Sort-Object) -join ',' -eq ($ref | Sort-Object) -join ',') { 'D0 HASH PASS - matrix may proceed to N0' } else { 'MISMATCH - run the full check below' }

# 2. full check (always valid, required whenever step 1 is unavailable or mismatches):
#    gh run download <run-id> ... -n encrypted-review-0-attempt-1 -D out/d0-enc
#    python tools/cloud_decrypt.py out/d0-enc/game-review-<id>-0.enc out/d0   (owner key flow, existing pattern)
powershell -File <agents>/check-d0-identity.ps1 `
  -Baseline 'C:/Users/shash/Documents/MySimGodot/outputs/repair-20261029/mip3-41843204651/decrypted/review' `
  -Candidate '<out/d0 decrypted review dir>'
# exit 0 = D0 PASS (byte-identical, no leaked signatures) - proceed to N0
# exit 1 = FAIL - read the verdict lines: bbox/leak classification decides per promo/turn24 failure catalog
```

## DECISION TREE

```
Q1 dispatch -> workflow renders (all flags default) -> frame hashes logged
  -> [hook merged?] yes: compare the hashes vs the turn-24 manifest
                        '-- match: D0 PASS -> start N0
                        '-- mismatch: go to the full check
     '-- no hook: go directly to the full check
full check (post-download + post-decrypt, owner side) -> checker exit code decides
  -> PASS : record the verdict (run id + the checker output) in the matrix thread -> N0
  '-- FAIL: apply the turn-24 failure catalog (fence-bbox = matrix STOP; global-drift =
     re-baseline; missing = re-run; signature leak = fix the dispatch, not the code)
```

## RULES THAT DO NOT CHANGE

- The dispatch stays exactly the Q1 command: `gh workflow run game-review.yml ... -f depth=fence_miprepair_review` - no env, no new inputs; the identity config = the flags' exact defaults.
- No acceptance claim until the checker (or the hash match) says PASS **and** the logs are leak-free; partial runs prove nothing.
- The re-run rule: a replacement D0 = a NEW run id; the verdicts are always attached to one specific run id.
- If the runner image / engine binary is intentionally changed, the byte-identity regime must be re-baselined (the turn-24 catalog's drift row) - never waived.
