# carbody-livery — turn 4

Author: carbody-livery specialist. Branch `mistral/carbody-livery`. Basis: integration head `52937be`-era (fetched; my turn-2/3 work merged at `c7adc01`). **No dispatches; no workflow edits; no shared-file edits.**

Standing context: P1 badges A/B — leg A = run **41847896437** at scratch ref `mistral/p1-badge-baseline` (`adf89ca`; exactly the turn-2 runbook pattern, verified pre-flight by the coordinator); leg B (candidates active at the head) queued via the serial watcher. Fleet evidence run **41848086075** in flight.

## 1. Task 1 — fleet-inventory completion state

Ground truth from the merged `fleet_evidence` code (`cloud_game_audit.py`, session-4):

- **Scope of run 41848086075:** ALL 24 cars across 6 shards (`car % 6 == shard` → shard0: 0/6/12/18, shard1: 1/7/13/19, shard2: 2/8/14/20, shard3: 3/9/15/21, shard4: 4/10/16/22, shard5: 5/11/17/23). Per car: 5 labelled views (front/rear/left/right/top, `MYSIM_FITVIEW`), 4 wheel closeups (`MYSIM_WHEEL_CLOSEUP`+oblique, machine-gated on `PASS complete visible wheel framing`), cockpit (track0, 9 s autodrive), + track0 day/night pair (shard 0). Survey semantics: failing cars record defects and the batch continues; missing captures still fail the run.
- **Render state of the captures:** `car_extra` carries `SOURCE_UV_CANDIDATES`/`MISC_TWIN_HIDE` as the workflow provides them for this depth — the env lines gate both on `depth == 'car_evidence'` only, so the fleet captures are the **production-appearance baseline** (candidates off; twin-hide off; verified on the workflow env lines) — exactly the "before" the truck-cut typing needs.
- **41848086075 does NOT emit assembly JSONs** (no `MYSIM_CAR_ASSEMBLY_AUDIT` in the `FLEET_EVIDENCE` path): the metric fields (`metresPerUv`, `basisDeterminant`, `textOrientation`) come from the **wheel-sweep dispatches** — cars 15–18 already measured (41837315358 SUCCESS no-fit baseline + 41843317303 axle-fit leg; artefacts sit uncommitted in the car-assembly worktree `outputs/wfr-41837315358/`), cars 0–3 queued (41849732286), remaining 16 cars in the three later 4-car batches (6505f16 plan).

Completion state per row (recorded in `fleet.json` as `evidenceState` per car and in `FLEET_MANIFEST.md` §3.0):

| Field (turn-2 QC gap) | Covered by | State now |
|---|---|---|
| fleet visual evidence (10 captures × 24 cars) | 41848086075 | in flight — defect observation waits for human review of captures; the run completes the *visual* field only |
| assembly metrics (metresPerUv/basisDeterminant/textOrientation) cars 15–18 | 41837315358 (SUCCESS) + 41843317303 | **measured, incorporation pending** (artefacts in car-assembly worktree, uncommitted) |
| assembly metrics cars 0–3 | 41849732286 | pending-run |
| assembly metrics cars 4–15, 16–19, 20–23 | three further 4-car batches (6505f16 plan) | pending-run |
| car16 badges A/B pair | leg A 41847896437 (in flight) + leg B (queued) | §2 prep ready |
| per-surface six-field fills for cars ≠ 6/16/20 | assembly metrics join via `--assembly` | pending the metric runs above |

`fleet.json` now carries `evidencePipeline` (the run→field map, dated from the session-4 packet) and per-car `evidenceState` — regenerated this turn; `FLEET_MANIFEST.md` §3.0 records the same map. The generic `pending-run-P5` markers in turn-2 are superseded by these run-specific markers.

## 2. Task 2 — P1 badges verdict table (PREP; to fill when the pair lands)

Preconditions checklist (all must hold before any verdict — from the turn-2 runbook §4):
- [ ] Leg A 41847896437 (`mistral/p1-badge-baseline`, `adf89ca`) SUCCESS, zero `SURFACE_UV_REPAIR_CANDIDATE` lines in logs, three promoted `SURFACE_UV_REPAIR 16/LOD_A_*_mm_ext` lines present.
- [ ] Leg B (`39cdf33`) SUCCESS, exactly three `SURFACE_UV_REPAIR_CANDIDATE 16/LOD_A_{BODY,FRONTBUMPER,REARBUMPER}_mm_badges` lines per capture, same promoted lines, `MISC_TWIN_HIDE_DIAGNOSTIC hidden=` equal in both legs, `CARMAP car=16 bound=` present in both.
- [ ] Both artifact sets complete (18 PNGs each; no missing list in the summaries).

Verdict table (fill on arrival; source reference = `2019_f1_ferrari_sf90_badges.png` opened beside the pair):

| Named region | Frame(s) | Leg A observed (mirrored baseline) | Leg B observed (corrected) | Source-atlas match | Verdict |
|---|---|---|---|---|---|
| Nose number / nose marks | front, top | pending | pending | pending | pending |
| Engine-cover sponsor strip | top, rear | pending | pending | pending | pending |
| Sidepod sponsor text LEFT | left | pending | pending | pending | pending |
| Sidepod sponsor text RIGHT | right | pending | pending | pending | pending |
| Rear-facing marks (bumper/wing endplate) | rear | pending | pending | pending | pending |
| Non-badge regression (livery/carbon/lights unchanged) | all five | pending | pending | n/a (must be IDENTICAL) | pending |

Pass criteria per marking (from the turn-2 runbook): leg B reads correctly — not mirrored, not upside-down — matching the source atlas orientation; the mark's position/content must be unchanged vs leg A (only orientation may differ); leg A must show the mirrored reading (this CONFIRMS H1's mechanism leg-by-leg, not just the fix). Non-badge regression rows must be pixel-equivalent in both legs (any difference = the run is not a controlled pair; discard and re-dispatch). Decision: all five regions pass → coordinator promotes the three entries (remove `pending`); any failure → keep `pending`, attach the pair, escalate to the measured H2 output before a further candidate.

My role on arrival: run the log gates, decrypt if the coordinator hands that step over, assemble the A/B contact sheet, and record the verdict table — visual verdicts stay with the human reviewer.

## 3. Task 3 — TRUCK-CUT: next repairs after badges (ranked by user-visible lift; Ferrari first, then closest vehicles per the manifest)

**R1 — car16 badges promotion, then same staging for 15/17/18 and the GT fleet badged surfaces.**
- What: the three staged `_mm_badges` V-only entries (+ the 7 staged ext entries) after the P1 pair passes; then clone the staging per car (15/17/18 next — the "closest vehicles" share the F1 badge-overlay structure; then the badged GTs).
- Why first: correctness (readable sponsor marks/numbers) dominates every sharpness gain; the fleet-evidence run provides the production "before" for every car at the same head.
- Classifier: badges = **albedo+alpha overlay, sRGB, straight alpha** (scissor threshold 0.5); livery ext = **albedo sRGB**. The fix is UV-transform-only — no texture bits change, so no alpha/silhouette risk.
- Gates: per car, one evidence run with per-marking verdicts (the §2 template); promote per surface only on passing.

**R2 — present the SOURCE wheels on the F1 class (and audit the road cars) instead of generated overlays.**
- What: the source wheel meshes+textures bind correctly (car16 alias-fix proven); the rejected "plain rim/ring" appearance in past reviews came from the GENERATED rim/tyre candidates (opt-in envs). The repair = decide, per car, "source wheels suffice" from the fleet run's machine-gated wheel closeups (which run with the generated overlays OFF by default) and keep/remove the candidate paths accordingly — no new art.
- Why: removes the single most-criticised visible element (plain bright rims/rings) using existing source texture detail (`wheel.png` 512², albedo sRGB) at zero asset cost.
- Classifier: `wheel.png` = **albedo sRGB**; source tyre = untextured (flat rubber is correct); masks (if any wheel mask usage) = linear.
- Gates: the fleet run's wheel closeups, per car, compared against the source textures; "PASS complete visible wheel framing" machine gate already in the run; visual verdict per the standing reviewer.

**R3 — badge-atlas upscale candidate (car16 first: 256→1024), CONDITIONAL.**
- Condition: only after R1's verdict (do not upscale a wrongly-oriented atlas) AND a measured need at gameplay distance (the turn-1 blur analysis is borderline: text readable at normal camera distances may not need it; the fleet run gives the gameplay-distance evidence).
- How (cloud-only, per policy): sRGB bicubic/Lanczos-class 4× on `badges.png` (256→1024) with: alpha-silhouette preservation check (per-island edge positions), mip-chain alpha-coverage check, no invented detail (edge-energy delta measured vs original; hallucinated structure = reject), before/after at the same camera/light/resolution (macro + driving distance, dry/wet/night where the depth provides them); jettison on any regression. VRAM delta ~ +0.8 MB decoded — trivial, but the policy asks it measured.
- Classifier: **albedo + alpha, sRGB (straight, non-premultiplied), scissor cutout** — NOT linear treatment; masks stay linear if ever touched.

**Deferred (flagged, not proposed for dispatch):** reflection/material parity (source `_ReflectionMap` dropped by the binders; high visual lift but `CarLoader.cs`/binder edits are shared-car-assembly territory — needs coordinated assignment, and per-surface before/after evidence; the nightly/ambient work belongs to track-render+night owners).

**Coordination (task 4): file-ownership map to avoid conflicts**
- Mine (exclusive): `tools/carbody_texture_inventory.py`, `outputs/mistral-orchestration/manifest/*`, `agents/carbody-livery-*.md`.
- Proposed-only (coordinator applies, no direct edits from me): `data/car_surface_uv_repairs.json` (any promotion = coordinator commit after the P1 verdict), workflows.
- Shared-coordinated (I propose, owners apply): `src/CarLoader.cs`/`CarAssemblyAudit.cs` (car-assembly leads), `src/TrackNightLighting.cs`/`TrackMaterialBinder.cs` (track-render leads — my track rows only read their outputs), Hud/hud-camera files (hud-agent leads). This turn I touched only my own files; R1–R3 above are staged as runbook/proposal, not edits.

## 4. Branch state

One commit this turn: this report + the regenerated manifest files (§1 completion state run-specific markers replace the generic `pending-run-P5` wording wherever a specific in-flight run now covers the row). Turn-1/2/3 commits unchanged.
