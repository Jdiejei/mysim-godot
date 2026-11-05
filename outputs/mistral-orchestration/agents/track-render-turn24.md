# track-render turn 24 — D0 identity-guard EXPECTATION (vs run 41843204651 baseline), executable check

Branch: mistral/track-render. Doc + tooling turn; no game code touched; no dispatches. D0 = the first cell of the turn-23 wet/night matrix and the gate for everything after it.

## 1. What D0 asserts (one sentence)

With every candidate flag unset, re-running the fence review depth on the merged integration line (>= 1f4ccf0) must reproduce run 41843204651's rendered evidence **byte-for-byte** and produce **zero candidate signatures** in its logs - proving the merged wet/night plumbing, the seam welds, and every other flag-gated addition are render-inert at default.

## 2. The baseline (measured today from the local decrypt)

Baseline source: `outputs/repair-20261029/mip3-41843204651/decrypted/review/` (main-tree copy; the run's encrypted artifacts + the owner's `review-20261031.key`). Run 41843204651 = the fence-miprepair validation: it rendered ALL FIVE modes then died at the workflow's stale assertion block (the yml-side gate, fixed since) - the rendered evidence itself is complete and valid, which is exactly why it is the reference.

Manifest (committed next to this file as `track-render-turn24-d0-manifest.csv`; all frames 960x540)

| frame | bytes | sha256 (first 12) |
|---|---|---|
| day0-filtered-fence-baseline.png | 452,845 | 3674a0655401 |
| day0-filtered-fence-family_isolate_fence.png | 430,960 | 1a1bbeba5dd5 |
| day0-filtered-fence-family_isolate_foliage.png | 432,228 | 7ef23c3319d9 |
| day0-filtered-fence-family_isolate_stands.png | 448,861 | 3ba67d31166f |
| day0-filtered-fence-fence_miprepair_candidate.png | 438,357 | d9a9eed691f5 |

Why ALL FIVE frames are in scope, not just `baseline`: the fence-depth harness renders each mode in a fresh game launch at fixed fps with fixed camera state; the fence modes differ only in their own fence-scoped env (baseline/shch = untouched; miprepair = the fence mip rebuild). None of the turn-23 wet/night flags is set by any mode. So the whole 5-frame set is a D0-quality identity set, and every frame doubles as a regression tripwire scoped to its own surface family.

## 3. The D0 dispatch (coordinator; unchanged from the battery's Q1)

```
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref <merged-main-head> -f depth=fence_miprepair_review
```

NO env additions, NO new inputs (the 25-input limit untouched), no yml edits for this cell. The run config IS the identity config: every turn-23 flag defaults to its exact value (`MYSIM_TOD`/`MYSIM_WET` unset -> Settings defaults noon/clear; pool gain/exposure/ambient/reflect multipliers default 1; vehicle lights off; light scheme "uniform"; seam welds off).

## 4. The executable check (committed tool, smoke-tested against the real baseline)

Tool: `check-d0-identity.ps1` (this dir). Usage:

```
powershell -File check-d0-identity.ps1 -Baseline <mip3-41843204651 decrypt dir> -Candidate <new D0 decrypt dir>
```

Steps it performs, in order:
1. Inventory: matches the baseline `.png` set by name against the candidate dir; missing frames = FAIL.
2. **Tier A (pass rule)**: SHA-256 per frame - all five byte-identical = `PASS: all frames byte-identical`, exit 0.
3. **Tier B (investigation aid, opt-in `-Weak`)**: on a byte mismatch, a full-frame pixel diff via LockBits (mean |delta| per channel sample, changed-fraction, max-delta, and the bounding box of all > 2 deltas). Cognitive thresholds ship as: mean <= 0.5, changed-frac <= 0.005, max <= 64 (raster-jitter envelope). Anything with a delta > 64 anywhere = content change = FAIL **even in Weak mode** - a single wrong-but-bright pixel is not jitter.
4. **Signature scan (independent of pixels)**: the candidate's `*.log` must contain NONE of the candidate output lines (`WET_REFLECT `, `NIGHT_LIFT `, `NIGHT_VEHICLE_LIGHTS `, `SEAM_WELD `, `FENCE_MIPREPAIR `) - enforced in ALL modes except the miprepair candidate mode itself where `FENCE_MIPREPAIR` lines are that mode's legitimate job. A hit = the env plumbing leaked a flag into the identity run = the dispatch/config error = FAIL regardless of the pixels (the pixel equality then just got lucky).
5. Exit code = the battery-usable verdict (0 pass / 1 fail / 2 usage).

Smoke results (all against the REAL 41843204651 files, run today):

| smoke | setup | result |
|---|---|---|
| self-compare | baseline vs itself | PASS, exit 0, 5/5 byte-identical |
| 1-px tamper (red pixel) | SHA differs; diff mean 0.0006, frac 6E-06, max 171 | FAIL (strict); FAIL (weak, max > 64 = content) - as designed |
| structural tamper (220x140 rect) | mean 25.5, frac 0.178, bbox x300..519 y200..339 | FAIL both modes, bbox localizes the damage |
| signature leak (injected `WET_REFLECT` line) | pixel set unchanged | FAIL with `SIGNATURE LEAK` named |

## 5. Expected-D0 outcomes and what each failure means

| D0 outcome | reading | action |
|---|---|---|
| 5/5 byte-identical + zero leaks | the wet/night/idle-line merges are render-inert at default | proceed to N0 |
| one frame DIFFs with the fence-area bbox | the fence surfaces changed (wet/night code or a material regression) | STOP the matrix; review the merged wet/night + material changes for a render-side leak |
| ALL frames drift (global low-delta) | infrastructure drift: runner image / renderer / Godot version / display settings changed between the runs | treat as the environment invalidating the comparison; re-baseline with a fresh all-off run, do NOT waive |
| frames missing | run/environment failure (write-side), or the harness's mode list changed | re-run; if a mode disappeared intentionally, this checker's baseline must be re-cut from the surviving valid run |
| any SIGNATURE LEAK | a D0 dispatch carries a candidate flag (yml/env contamination) | fix the dispatch; NOT a code problem |
| pixel equality passes but the leak fires | lucky pass - still FAIL (the log evidence outranks the pixels) | fix the dispatch |

Scope statement: D0 proves inertness ON THE RENDERED VIEWS of the fence run (track 0, its camera set, the day conditions). Off-view behaviour (other tracks, night luminance behaviour, the scaler timing) is covered by the other matrix cells, not by D0; and the flag-gates' source-truth is the code-level pass (the turn-23 registry: every flag = a first-statement env read, the night lift inside the `IsNight` guard at Conditions.cs:48).

## 6. Notes

- The byte-equality bar rests on the ci engine pinning: the same runner image + the same engine binary between 41843204651 and D0. If the infrastructure intentionally changes either, the checker's Tier B + the failure catalog above give the review board a disciplined path: re-baseline, don't waive.
- The turn-23 matrix's other cells (N0..W2) compare CANDIDATE-vs-BASELINE pairs by DESIGN (the content changes = the product); D0 is the only cell where equality = the pass.
- The tool = self-contained (System.Drawing + .NET), no game/engine dependencies; the runs' decrypted dirs = the only inputs.
