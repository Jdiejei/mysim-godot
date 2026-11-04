# carbody-livery — turn 5c (P1 continuation: the Q3 production-state comparison)

Author: carbody-livery specialist. Branch `mistral/carbody-livery`. Basis: integration head `a313492` (fetched). **No dispatches.** shell note applied: all shell work via explicit `cmd`/`powershell.exe` (the note about the WSL `bash` stub is acknowledged; nothing this turn needed POSIX bash).

## 1. Where the P1 pair stands right now (read-only `gh` checks + the watcher log)

- **The watcher re-ran leg A on a rebased baseline and it is GREEN:** run **41870086370**, `car_evidence` at **`26d0191`** — commit message: "P1 leg A baseline (rebased onto the current integration head): ONLY the three `16/*_mm_badges` candidate entries removed; leg B = the integration head itself". Verified: the rebase sits on top of the current integration line (`39cdf33` is an ancestor), and the only diff vs its parent is `data/car_surface_uv_repairs.json` (the rebased-JSON re-serialization + the three deletions — same shape the reviewer already verified for `adf89ca`).
- **Why the re-run was needed:** leg A v1 (`41847896437` at `adf89ca`) died on an **audit-script bug, not on the leg**: `tools/cloud_game_audit.py:389` asserts the HUD design viewport and references an **undefined variable `want`** → `NameError` at the third HUD resolution (1920×1080). The assert exists verbatim at `39cdf33` and `adf89ca`; it is gone at the current head. All five body views + wheels + cockpit + 2 HUDs were captured cleanly in v1 (verified from its downloaded runtime artifact) — but the re-run supersedes it with a full green 18-PNG set.
- **Leg B is in flight:** run **41874911549**, `car_evidence` at **`a313492`** (the integration head; badges entries present, `pending:true`, and the depth forces `SOURCE_UV_CANDIDATES=1` → the badges candidates WILL be active). Expected: the full 18-PNG set (no audit bug at this head) and the log discriminator: leg B shows the 7 ext `SURFACE_UV_REPAIR_CANDIDATE` lines (same as leg A) **plus exactly the 3 `_mm_badges` lines**.
- **Q3 exactness caveat, stated plainly:** the reviewer's condition was "use exact immutable SHAs; no mixing newer integration HEAD into leg B". The rebase, forced by the audit bug, makes the working pair span **`26d0191..a313492`**. That delta is: the mounted-assembly hull bake (`eb3890f` — my turn-5b correction, adopted), collision-overlay compile fixes (`4e11bba`), the product-gate wiring (`0656df7`), the user-path harness (`c3d696a`/`5208a32`/`a313492`) and plan text — **none of it touches the car visual path `car_evidence` exercises**, but it is not byte-identical lineage. Two ways to close this, coordinator's choice:
  1. **Accept the pair as-is** with the per-commit render-neutrality classification above recorded in the verdict table.
  2. **Zero-confound option (recommended if the reviewer wants exact SHAs):** a scratch ref `mistral/p1-badge-candidates` = `26d0191` + the **exact reverse patch** (the three entries re-added — the mirror image of what `adf89ca` removed), then one `car_evidence` dispatch there. Leg A `41870086370` and that leg B then share the base SHA exactly; the in-flight `41874911549` remains as supplementary modern-head evidence.

## 2. The production-state comparison (the Q3 condition — the actual continuation)

**Why it is needed:** both A/B legs run under the `car_evidence` environment: candidates ON (the 7 ext pending entries apply in BOTH legs) and twin-hide ON (`hidden=8`). Neither leg shows production. The Q3 acceptance explicitly said the pair "does not validate production without a subsequent production-state comparison". Promotion must therefore be validated **with no environment at all**.

**The only production-state capture path for car16 is the `fleet_evidence` depth** — by construction it sets `SOURCE_UV_CANDIDATES=0` and `MISC_TWIN_HIDE=0` (verified on the workflow env lines: both gate on `depth == 'car_evidence'` only). `car_evidence` cannot do it (it force-enables both).

**Blocker found and machine-verified this turn: the fleet depth cannot currently reach car16 at all.** Both fleet runs (`41848086075` at `b5ca3e0`, and the re-dispatch `41862411919` at `eb3890f`) executed **exactly one render job (shard 0)** — jobs verified via `gh run view --json jobs` (2 jobs each). The cause is mechanical, not environmental: in the shard expression the fleet term sits inside the parenthesis group opened at `chiron_wing_rebuild` that only closes after `f1_wheel_diagnostic`, and that group is followed by `&& '[0]'`. For `depth=fleet_evidence` the group evaluates to `'[0,1,2,3,4,5]'`, and `'[0,1,2,3,4,5]' && '[0]'` collapses to `'[0]'` (GitHub expressions treat a non-empty string as truthy). **Car16's shard is 4 (`16 % 6`) — car16 has never been fleet-captured; its production-state captures do not exist yet.**

**The comparison runbook (all coordinator-owned steps; I dispatch nothing):**

1. **Fix the paren re-scope** (one line, `game-review.yml`): close the group after `night_evidence))`; give `recovered_wet_review … f1_wheel_diagnostic` their own parentheses; keep `inputs.depth == 'fleet_evidence' && '[0,1,2,3,4,5]'` top-level. Mechanical check after the edit: the fleet term's enclosing group must close immediately before `|| night_light_review`-class terms with NO trailing `&& '[0]'` on the fleet's own bracket.
2. **Pre-promotion baseline:** dispatch `fleet_evidence` at the current head → 6 shards × 24 cars, the mirrored-badges production state captured (`fleet-car-16-{front,left,right,rear,top}.png` among them). Success criterion: six `fleet-evidence-summary.json` artifacts, `missing=[]` in each, and render jobs `render (0..5)` in the run's job list.
3. **Promotion commit:** flip the three `16/*_mm_badges` entries from `"pending": true` to `"pending": false` in `data/car_surface_uv_repairs.json`. **Semantics matter: `pending:false` makes the repair apply UNCONDITIONALLY (no env can un-apply it) — that is the promotion. Deleting the entries would REVERT to the mirrored default; deletion is the undo, not the promotion.** The promotion commit touches nothing else (no asset bits, no UV buffers, no shader).
4. **Post-promotion production captures:** dispatch `fleet_evidence` again at the promotion head → the same 24-car set, now with corrected badges under zero environment.
5. **The comparison matrix (fills the §turn-4 verdict table's final column):**
   - Promoted fleet-captures vs the pre-promotion fleet-captures: the five named markings change ONLY in orientation (mirrored → correct per the source atlas); every non-badge region pixel-consistent (the regression rows).
   - Promoted fleet-captures vs leg B (`41874911549`): the badge markings agree (same corrected orientation); the difference is confined to the twin-hide/ext-candidate states the environments control — listed, not a failure.
   - Promoted fleet-captures vs the locally-available decrypted `41798036351` set (`e8b80bb`, the reviewed pre-fix baseline): the before/after the human reviewer already anchored when flagging the mirrored logos.
6. **Pass rule (unchanged from turn-4):** every named marking reads correctly (not mirrored, not upside-down, matching `2019_f1_ferrari_sf90_badges.png`) in the promoted production captures, position/content unchanged, no new seams; any regression → the promotion is reverted (`pending` restored) and the failure mode goes back through the measured-H2 path.

**Guardrails (as instructed):** mirrored badges stay fixed — the promotion is unconditional by construction, so no environment or later run can silently revert the correction; any future change to those surfaces' orientation must arrive as a new per-surface evidence entry, never as an environment toggle. **No invented liveries** — the entire chain is a UV-transform change on existing source atlases; no asset bits are touched, no art is generated, and the R3 upscale candidate remains conditional on measured need (per the turn-4 truck-cut).

## 3. Fleet-baseline bookkeeping updated this turn

- `fleet.json` / `fleet-tables.md` regenerated: `evidencePipeline` now records both fleet runs + the mechanically-confirmed paren cause + the fix; `p1Badges` records the v1 failure cause (the audit `NameError`), the v2 green leg A at `26d0191`, the in-flight leg B at `a313492` with the exactness caveat and the zero-confound option; a new `productionStateComparison` block carries the §2 sequence.
- Per-car `evidenceState`: cars 0/6/12/18 = captured twice at shard 0 (envelopes, decryption pending); the other 20 (incl. car16) = pending the paren fix + re-dispatch.
- `FLEET_MANIFEST.md` §3.0 updated to the same state.

## 4. Branch state

One commit this turn: this report + the regenerated manifest files + the tool's annotation update. No dispatches, no workflow edits, no asset or data changes; the only network operations were read-only (`gh run list/view/api` metadata + the two earlier artifact downloads already on disk).

## 5. Open items

1. Leg B (`41874911549`) completion + its log discriminator (the 3 `_mm_badges` candidate lines) — next turn verifies from its runtime artifact.
2. The coordinator's decision: accept the `26d0191..a313492` pair or dispatch the zero-confound `mistral/p1-badge-candidates` ref.
3. The fleet paren fix + the two `fleet_evidence` runs (pre/post promotion) — the Q3 condition's execution.
4. The promotion decision itself stays with the reviewer until the §turn-4 verdict table is filled from real images.
5. Shards 1–5 of both fleet runs never existed — nothing to download for them; do not hunt for missing envelopes.
