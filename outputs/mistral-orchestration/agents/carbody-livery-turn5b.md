# carbody-livery — turn 5b (wheel-band source truths + fleet/P1 state)

Author: carbody-livery specialist. Branch `mistral/carbody-livery`. Basis: integration head `52937be`-era. **No dispatches.** All numbers below were read locally from `outputs/repair-20261029/dvride-41848766195/game-runtime-0-attempt-1/outputs/cloud-game/native-hull-summary.json` (the DV-ride hull manifest: per-car `partBounds` + `wheelCentres` + `wheelRadii`), the car16 assembly-audit JSONs already on disk, and `src/RecoveredComponentMounts.cs` + `data/component_mount_profiles.json`.

## 1. The wheel-band components' SOURCE truths (car16 complete; car0 delivered early — the data was already local)

### 1.1 car16 (SF90) — the wheelguards

Three independent measurements agree:

| Source | WHEELGUARD_FRONT_L/R position (car space) | Wheel/tyre anchor |
|---|---|---|
| Hull manifest `partBounds` (DV-ride bake, `authoredComponentMounts:false`, `mountedHubs:{}`) | y −0.81…−0.50, x ±(0.15…0.49), **z −0.16…+0.16** | wheels at y −0.31, z ±(2.008 / 1.589), radii 0.2996/0.2999 → **tyre bottom −0.61** |
| Assembly audit (post-`ApplyExact`, pre-mount; audit space) | `…_mm_misc` centre (±0.322, −0.005, −0.003), size (0.338, 0.318, 0.317) → bottom −0.164, **z ±0.16** | source tyre centre y +0.340, half-height 0.2995 → **tyre bottom +0.040** |
| car-assembly turn-5 sweep (measured on the assembled car) | shells sit **1.17…1.62 m away from their wheels** ("no contact; separate placement defect recorded since turn 1") | — |

- **Vertical:** the guards hang **0.20 m below the tyre support plane** in both spaces (−0.81 vs −0.61 hull; −0.164 vs +0.040 audit = 0.204). This is the same 0.20 m the physics turn-7 measured when the guards "beached the hull".
- **Horizontal:** the guards sit at the car **centre-line (z≈0)** — ~2.0 m from the front wheels they are named after; laterally they stay INSIDE the body (x max 0.49 vs half-width ~0.98), so with mounts off they are hidden inside the lower body, not visible at the flanks.

### 1.2 car0 (488 GT3) — the brake calipers (the "later" half, delivered now)

From the same hull manifest, car0 section: wheelCentres y −0.3493, radii 0.3007 → **tyre bottom −0.65**; chassis floor −0.60 (5 cm above the plane — sane). The four `LOD_A_BRAKE_CALIPER_*` parts (mounts off) sit at y −0.79…−0.54, x +0.10…+0.20 / z −0.18…−0.08 (FL; mirrored for FR) — i.e. **also small origin-centred templates** (near the car centre, wrong-side coordinate frame), hanging **0.14 m below the wheel plane**. The `LOD_A_STEERING_WHEEL` part likewise (y −0.72…−0.54, z ±0.02) — which is exactly why the mount profiles carry a `steeringWheel.closedCar=[−0.35, 0.66, 0.5]` placement.

### 1.3 Verdict: (c) — source-template geometry, NOT a mount-profile bug

- The wheel-band parts (`WHEELGUARD_*`, `BRAKE_CALIPER_*`, `STEERING_WHEEL`, `STEERING_ARM_*`) are **origin-centred authoring templates in the recovered source FBX set** — the same pattern the mount code already documents for steering arms ("Source links are origin-centred templates"). The real game mounted them per-instance at runtime; our import inherits the template pose.
- **(b) is excluded by evidence, not assumption**: the mount code only ever *raises* guards (`target.Y += radius*aboveHubRadius(0.2)`) and pulls them inboard — it cannot push anything low; and during the DV bake it was OFF anyway (`authoredComponentMounts:false`), so the baked guard hulls were the raw source placement.
- **Visual consequence for the render:** with mounts off, car16's wheel arches show **no guard geometry at all** (the templates sit hidden inside the body centre) — there is nothing visible to fix at the arch; the visible question for the human reviewer is the opposite one: whether the source game's wheel-cover panels are *missing* from our render (the fleet captures `fleet-car-16-left/right.png`, taken with mounts off, are exactly the evidence; same for `fleet-car-00-*` once re-captured in a full-width fleet run).

### 1.4 The DV attribution correction (for the physics/assembly joint audit)

The ride experiment's gate (run 41848766195, Q1 evidence update) measured **static clearance −0.200 m** and concluded "structural: suspension rest/anchor geometry vs the hull floor extent; 0.235 m preload needed". My measurement says the floor that produced −0.200 was almost certainly the **wheelguard hulls** (−0.81 − (−0.61) = −0.200 exactly; the real chassis floor is −0.60 ≈ the tyre plane → ~0 mm clearance). The 0de3123 exclusion (wheel-band parts out of the body hull set) removed exactly that artefact — and run 41854714029 (guard-excluded bake) still failed the drive at 0.2 m/30 s, consistent with the failure having moved to the (already-identified) thrust/spawn-datum path.

**Honesty bounds on this correction:** the −0.200 match is numeric and three-decimal exact, and the physics turn-7 note independently measured the guards at 0.20 m below the support plane — but I have NOT read `NativeSuspensionVehicle.Configure` to confirm its clearance is measured off the hull floor. The joint audit should confirm on the excluded bake: if the recomputed clearance/preload requirement lands near 0/sane, the earlier "0.235 m preload, structural" reading was the guards; if it still reports a large deficit, my attribution is disproven and the suspension-geometry fix path stands as scoped. Either way the guard ablation was necessary and the remaining drive failure is separately owned.

### 1.5 Mount-proposal status: NO diff required (the mount is not the bug)

None filed. Two proposals for the owning agents (proposals only; both are observability, not behaviour):
1. **hull-bake/DV observability** (car-assembly/physics): the DV static-clearance summary should name the part whose bound sets the floor (`partBounds` already carries everything needed — one line), so a template-contaminated floor can never masquerade as a chassis measurement again.
2. **bake-time guard** (car-assembly): any INCLUDED hull part whose minimum-Y sits below `wheelPlane − 0.05` should fail or flag loudly at bake time (today that class of error surfaces only as a failed drive 30 seconds into a run).

The guard/caliper RENDER decision (mount them as the source game did vs keep them hidden until a source-exact pose exists) stays with car-assembly+carbody after the fleet captures are reviewed; the current mount profile correctly self-describes as "authored experimental pose… not source-exact pose certification", and my recommendation is to keep the parts unmounted/hidden until that evidence exists.

## 2. Task 2 — fleet-baseline enrichment (run 41848086075): what it actually delivered

- **The run executed ONE shard, not six.** `gh run view --json jobs`: exactly 2 jobs (`prepare`, `render (0)`), run conclusion `success`. The `b5ca3e0` shard expression contained the fleet bracket `[0,1,2,3,4,5]` but inside the mis-nested group (the dropped-paren lineage fixed at `e76a1e1`+), so the whole prefix collapsed to `'[0]'` for a fleet dispatch — the 6-shard expansion silently became 1 shard. **The run's green status does NOT mean full-fleet coverage.**
- **Coverage: cars 0/6/12/18 only** — 42/42 captures, `missing: []` (5 views + 4 wheel closeups + cockpit per car + track0 day/night), production render state (candidates off, twin-hide off). Images live in `encrypted-review-0-attempt-1` (12.8 MB, downloaded). Shards 1–5 were never created — **20 cars have no fleet captures yet**.
- **Decryption is blocked locally**: `MYSIM_INTAKE_KEY` is not set in any local environment and the D:-drive key copy is gone (no D: drive on this machine) — the key lives in the CI secret. Decryption + the visual pass therefore run through the coordinator/verification agent, per the standing encrypted-evidence procedure.
- **Bookkeeping updated on my branch:** `fleet.json` now carries `evidencePipeline` (the verified run map incl. this correction) and per-car `evidenceState` (cars 0/6/12/18 = "captured-41848086075-shard0 (envelope; decryption pending)"; the other 20 = "pending-redispatch (…shard-expression collapse; fixed at current head)"); `FLEET_MANIFEST.md` §3.0 carries the same table; both regenerated from the tool (all asserts still pass: 24 cars / 60 tracks / 60 routes).
- **Recommendation for the coordinator queue:** re-dispatch `fleet_evidence` once at the current head (the shard expression is balanced there) to cover the missing 20 cars; no other change needed — the coverage check is the run's own `fleet-evidence-summary.json` per shard (`shardCars` + `missing`), and a 24-car pass = six summaries with no missing entries.

## 3. P1 badges pair — state and corrected gates (supersedes the turn-2 numbers where they differ)

- **Leg A (41847896437, `adf89ca`) = FAILURE — but the failure is the audit script, not the leg:** `cloud_game_audit.py:389` asserts the HUD design viewport and references an undefined variable `want` → `NameError` at the **third** HUD resolution (1920×1080). The assert exists verbatim at `39cdf33` (leg B's pinned head) and was removed at the current head. Per the Q3 constraints leg B stays at `39cdf33`, so **leg B will fail the same way after the same captures** — the pair remains decidable.
- **What leg A actually captured (from its downloaded runtime artifact, now at `outputs/repair-20261029/p1legA-41847896437/`):** all five body views + 5 wheel closeups + cockpit + HUDs 640×360/1280×720 — every one `returncode 0, screenshot_exists true, error_lines []` (`PASS complete visible car framing, corners=376`; saves `error=Ok`). Only the 3 remaining HUD resolutions + the 2 lap frames were not reached. The five body views — the only images the badge verdict needs — are inside the uploaded encrypted envelope (4.6 MB).
- **Corrected log gates (observed on leg A, supersedes the turn-2 table's numbers):**
  - `MISC_TWIN_HIDE_DIAGNOSTIC hidden=8` (the matcher also hides the DAMAGE/BONNETCAM `_mm_misc` twins — 8 pairs, not the 3 visible-shell pairs my turn-2 table listed; identical in both legs, so the pairing holds).
  - Promoted trio: 3 × `SURFACE_UV_REPAIR 16/LOD_A_{BODY,FRONTBUMPER,REARBUMPER}_mm_ext` (both legs).
  - Candidates: leg A shows **7** `SURFACE_UV_REPAIR_CANDIDATE` lines (the 7 pending ext entries — doors/exhaust/mirrors/strut-uppers — they activate under `car_evidence`'s candidates env in BOTH legs); **leg B must show those 7 PLUS exactly the 3 `_mm_badges` lines** — the discrimination is the 3-line delta, not "zero candidates in leg A" as my turn-2 table wrongly said.
  - `CARMAP_SOURCE_ROLE_ALIAS` both wheels; `CARMAP car=16 bound=96`; `WINDING surfaces=250 flipped=0`.
- **Verdict path:** both envelopes (leg A in hand, leg B when it lands) are decrypted CI-side (key in CI secrets), the five body-view pairs + the two run's log gates make the §turn-4 verdict table fillable; the audit `want` bug should be fixed independently by the coordinator (it will keep turning legitimate evidence runs red after their captures complete).

## 4. Branch state

One commit this turn: this report + the regenerated `fleet.json`/`fleet-tables.md` + the `FLEET_MANIFEST.md` §3.0 update + the tool's annotation update (`evidencePipeline` now carries the observed outcomes: the fleet shard collapse, the leg-A failure cause, corrected per-car states). No source/data/workflow changes; no dispatches; downloads were read-only retrievals of already-existing artifacts (`fleet-41848086075` shard-0 runtime was already local; leg-A `game-runtime-0-attempt-1` fetched to `outputs/repair-20261029/p1legA-41847896437/`).

## 5. Open items

1. Recompute the ride/preload numbers on the guard-excluded bake (physics/assembly; my §1.4 correction makes the earlier −0.200/0.235 reading suspect).
2. Fleet re-dispatch at the current head for the missing 20 cars (coordinator).
3. Decrypt both P1 envelopes + the fleet shard-0 envelope (CI-side key) → fill the §turn-4 verdict table + the wheel-arch/wheel-cover visual question.
4. The audit `want` fix (coordinator-owned file) — it will keep reddening legitimate runs otherwise.
5. Cars 15–18's measured assembly metrics still sit uncommitted in the car-assembly worktree — incorporation into the inventories awaits that handoff.
