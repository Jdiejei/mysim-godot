# P1 production-state comparison — LIVING RUNBOOK (carbody-livery)

Maintained by the carbody-livery specialist on `mistral/carbody-livery`. Supersedes the §2/§4 sequencing in `carbody-livery-turn5c.md` where they differ. **No dispatches made by this agent; the owner executes the battery** (workstation dispatches are blocked server-side: `HTTP 403 Must have admin rights`, per `OWNER_MERGE_INSTRUCTIONS.md`). No identical failed re-dispatches; chart refreshes only with verified results.

## 0. State verified this session (all read-only)

| Fact | Value |
|---|---|
| Integration head | `c4fb876` (= `o_Jdiejei/codex/repair-20261029` tip; commit "shard matrix: fix the fleet-grouping bug…verified by expression parser across 10 depths") |
| Fleet shard fix | **Independently re-verified by this agent's paren-scope check**: at `c4fb876` the fleet term's enclosing paren is the `fromJSON(` itself (top-level; no `&& '[0]'` wrapper), and the wet/`f1_wheel_diagnostic` group opens its own parenthesis — exactly the re-scope the turn-5c analysis prescribed |
| P1 leg A (baseline; badges entries removed) | **`41870086370` @`26d0191` SUCCESS** (full 18-PNG set; v1 `41847896437`@`adf89ca` died on the audit's `want` NameError — fixed heads only) |
| P1 leg B (badges candidates active) | **`41874911549` @`a313492` SUCCESS** (full set; the discriminator to verify in its logs: the three `_mm_badges` `SURFACE_UV_REPAIR_CANDIDATE` lines on top of leg A's seven ext lines) |
| Production-state baseline (cars 0/6/12/18) | **already captured twice** (`41848086075`@`b5ca3e0`, `41862411919`@`eb3890f` — both ran shard 0 only under the old bug) — 42/42 PNGs each, `missing=[]` |
| Local decryption | **WORKS**: the owner key at `C:/Users/shash/.codex/private/mysim-review/review-20261031.key` decrypts the 20261104 envelopes (verified this session on the fleet shard-0 envelope → 42/42 PNGs; the key is used via `MYSIM_INTAKE_KEY` and is never printed) |

Consequence: the "decryption blocked locally" caveat from earlier reports is GONE. Every artefact the production-state comparison needs can be decrypted and inspected locally; only fleet DISPATCHES remain owner-side.

## 1. The battery item to ADD (owner-side; formatted for `OWNER_MERGE_INSTRUCTIONS.md`)

```bash
# N) P1 pre-promotion baseline: fleet_evidence at the FIXED head (6 shards restored).
#    Production state by construction (SOURCE_UV_CANDIDATES=0, MISC_TWIN_HIDE=0) —
#    this captures the mirrored-badges baseline for ALL 24 cars and completes the
#    fleet visual evidence for the 20 cars the two shard-0-only runs never reached.
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref c4fb876 -f depth=fleet_evidence

# EXPECT (machine gates, per shard 0..5):
#   - run jobs: prepare + render (0..5)  (SIX render jobs — if only render (0) appears, the matrix regressed; STOP and re-check the expression)
#   - six fleet-evidence-summary.json artifacts; every shardCars covered; missing=[] in all six
#   - 24 cars x 10 captures + 2 track0 day/night = 242 PNGs across the six envelopes
#   - no SURFACE_UV_REPAIR_CANDIDATE lines anywhere (candidates OFF at production state);
#     SURFACE_UV_REPAIR lines appear only for promoted entries (cars 6/16/20 ext sets)
```

This run is the **pre-promotion mirrored baseline** — do not read it as a defect list by itself; it is the BEFORE side of the comparison and the missing fleet visual evidence.

## 2. The promotion step (after the fleet baseline lands)

One coordinator-owned reviewed commit on the integration branch that flips **only** the three car16 badge entries in `data/car_surface_uv_repairs.json`:

```json
"16/LOD_A_BODY_mm_badges",          "pending": true  ->  false
"16/LOD_A_FRONTBUMPER_mm_badges",   "pending": true  ->  false
"16/LOD_A_REARBUMPER_mm_badges",    "pending": true  ->  false
```

Semantics (verify before merge — these are the load-bearing details):
- `pending:false` ⇒ `SurfaceUV` applies the repair **unconditionally** — the correction is then live in every run, every depth, with **no environment** (this is the point: "mirrored badges stay fixed").
- **Deleting the entries would REVERT to the mirrored default** — deletion is the undo operation, never the promotion.
- The commit must touch nothing else: no asset bits, no UV buffers, no shader, no other entries.
- Rollback = restore `"pending": true` (returns to the opt-in state; production reverts to mirrored).

## 3. The post-promotion fleet run + the comparison

```bash
# N+1) P1 post-promotion validation: fleet_evidence at the PROMOTION head (the commit from step 2)
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref <promotion-commit> -f depth=fleet_evidence
# EXPECT: same machine gates as the baseline run; car16's captures (shard 4) now show the CORRECTED badges
# with NO environment involved.
```

Comparison matrix (fills the final column of the turn-4 verdict table; visual verdicts stay with the human reviewer — this agent prepares the contact sheets):

| Comparison | Images | What it proves |
|---|---|---|
| Post-promotion `fleet-car-16-*` vs pre-promotion `fleet-car-16-*` (both from `fleet_evidence`, production state) | step-1 vs step-3 runs, shard 4 | the promotion's effect at PRODUCTION state, zero confounds: only the badges layers may change; every non-badge region pixel-consistent |
| Post-promotion `fleet-car-16-*` vs leg B `41874911549` (env-controlled) | step-3 vs the P1 leg-B envelope | the env-driven fix and the promoted fix agree (same corrected orientation) |
| Pre-promotion fleet / leg A `41870086370` / the reviewed `41798036351` set (local, decrypted) | as available | the mirrored BEFORE state the reviewer already flagged (09:10Z) |

Decrypt commands (the local key works — use exactly this pattern, never print the key):

```powershell
$env:MYSIM_INTAKE_KEY = (Get-Content "C:\Users\shash\.codex\private\mysim-review\review-20261031.key" -Raw).Trim()
py -3 tools/cloud_decrypt.py <downloaded>\<name>.enc <output-dir>
```

**Pass rule (unchanged from turn-4/turn-5c):** every named marking — nose number, engine-cover strip, sidepod sponsor LEFT/RIGHT, rear-facing marks — reads correctly (not mirrored, not upside-down, matching `2019_f1_ferrari_sf90_badges.png`) in the post-promotion production captures, position/content unchanged, no new seams; non-badge regression rows identical. All pass → the promotion stands and P1 closes. Any failure → rollback (§2) + the failure mode goes back through the measured path (H2's `car_assembly_audit` output) before any further candidate.

**Guardrails:** mirrored badges stay fixed by construction (unconditional data state — no environment or later run can silently revert; any future orientation change must arrive as a new per-surface evidence entry). **No invented liveries** — the entire chain is a UV-transform on the existing source atlas; no asset bits change anywhere in steps 1–3.

## 4. Sequencing note for the owner's battery

The P1 items slot into the existing five-command battery (`OWNER_MERGE_INSTRUCTIONS.md`: fence, windows, DV, wall battery, overlay) as items N and N+1 — the fleet runs are independent of those five (different depth), can run before or after them, but the promotion itself should wait for (a) the fleet baseline run's green gates and (b) the reviewer's confirmation of the P1 pair (leg A/B per-marking table) so the promotion lands with the pair already accepted. No re-dispatch of any failed run without a changed cause.
