# physics-gameplay — collision coverage restore + force-driven wall validation — 2026-11-04

Reviewer-assigned, exclusive-ownership turn (`NativeCarColliders.cs` now mine). Commit set: the hub-restore in the bake, the wall-contact force metrics + profiles + manifest, the reference plumbing, the regression guards, and this report. No dispatches; no threshold changes; no physics constants invented.

## 1. Preserve-order review + the restore (task 1)

**Verdict on the `0de3123` exclusion (reviewed, partially accepted):** it fixed the thing it should — the wheelguard/caliper parts stopped defining the stance datum (the ride requirement drops to the measured ~0.025 class, far inside the 0.12 bound, no bound raise — the car-assembly stance data confirmed) — and it preserved source/UV/meshes exactly. **It is still right to keep the parts OUT of the body-fixed hull set.** What it did NOT do: restore their wheel-band collision coverage (the wheel arcs have no lateral collision until it returns), and it left the placement defect (why they were 0.14–0.20 m below the tyre plane) undocumented at the bake level. This turn's restore completes the pair.

**The ground truth (my + car-assembly's data combined):** the misplacement is a TRANSFORM defect, not a low floor: `LOD_A_WHEELGUARD_FRONT_LEFT` bakes at bbox (+0.15..+0.49, −0.81..−0.50, −0.16..+0.16) while its hub sits at (−0.763, −0.31, +2.008) — mirrored X plus ~1.55 m displacement (their turn-5: shells 1.2–1.6 m from their wheels; my reading of the baked bounds shows exactly left-parts-at-plus-X near the body origin). The same mirror+shift holds for car0's `BRAKE_CALIPER_*` (−0.79 vs tyreBottom −0.65). The frozen-car signature is now fully explained: the two misplaced guard blobs (body-fixed hulls) sat under the belly — **`bodyContacts=2` in the DV freeze trace 41840755243 was those two blobs**; with the wheels unloaded (tyreContacts=1) the free-rev/power-collapse loop froze the car (my turn-5 mechanism, confirmed). The clean lap's histogram (CHASSIS/FRONTBUMPER/BODY only) is consistent: its bake predates the guard-hull inclusion.

**The restore (committed in `src/NativeCarColliders.cs`):**
- `WHEELGUARD*` and `BRAKE_CALIPER*` parts are classified as wheel-assembly members (`HubForWheelAssemblyPart`: FRONT→0/2, LEFT→0/1; metadata hub wins over the name-derived one, with an override log line if they disagree).
- They are **excluded from body-fixed hulls (0de3123 semantics kept) and re-baked as HUB-MOUNTED collision members**: the existing hub-mount mechanism (mounted-hubs tag → `MountedShapes` → the per-tick update in the vehicle: steering rotation about the hub for fronts, suspension offset for all) applies with ZERO vehicle-code change.
- **The pose correction is a rigid translation of the baked collision copy only**: bbox X/Z centre → hub centre, bbox bottom → hub height (a guard wraps the wheel's upper half; a caliper clamps the upper disc). Source meshes/UVs/visual assembly are untouched, the shape vertex data is preserved rigidly, and each correction prints `HUB_POSE_RESTORE car=… part=… hub=… shift=(dx,dy,dz)` so the assembly's `PART_HUB_OFFSET` audit should read ≈0 on the next sweep.
- The stance gate (`NativeSuspensionVehicle.Configure`) already skips hub-mounted parts by design, so **the ~0.025 stance requirement survives the coverage restore untouched** — the two fixes coexist by construction.
- Blast radius, stated: wheel-band lateral contacts now COLLIDE (wall/graze evidence becomes more honest — the wheel arcs were pass-through since the exclusion); the source-backend impact history (`41794252920`, 17 impacts) should be re-checked after the next source run — if any of those were the blob hulls' artefacts, the count drops for physics reasons.

## 2. Force-driven wall/contact validation (task 2 — spec first; coordinator dispatches)

**Principle:** the DV wall evidence must come from REAL drive-force contacts (scripted approach + aim phases — never from AI-line passes steering around walls). The hit geometry is not prescribed; the measurements are.

**New committed profiles (`data/native_driving_validation_profiles.json`):**
- `wall_contact_low` — approach to 7.2 m/s (~26 km/h), full-lock aim until the impact event, contact hold, reverse-out, forward recovery, stop.
- `wall_contact_high` — the same at 12.5 m/s (~45 km/h).
- `wall_contact_graze` — approach 8.5 m/s, PARTIAL-lock aim until the contact window itself (a graze may sit under the damage-event threshold), scrub phase, then the recovery phases; light assertions by design.

**New committed metrics (`src/Logic/DrivingValidation.cs`; fail-closed; synthetic-tested in CoreChecks):**
- `impactTickPeakDeltaV` — the largest per-tick body-frame velocity jump inside a contact window (either sample on a non-drive surface); the raw force material, from recorded samples.
- `impactContactSeconds` — total contact dwell.
- `impactImpulseEstimate` — `vehicleMass × maxDeltaVRise` (momentum absorbed; needs and FAILS CLOSED without the new `vehicleMass` reference).
- `impactPeakForceEstimate` — `vehicleMass × tickΔv / dt` at the peak tick — an estimate at sample rate, explicitly not a force certificate (the true force profile inside the solver step is not observed); recorded, never asserted.
- **The identity band (used as the impulse assertion): impulse/mass IS a Δv, so it cannot exceed the approach speed plus rebound margin** — implemented as `impactImpulseEstimate betweenReference [1.5,16] × vehicleMass` for the frontal profiles (the 1.5 floor = below the 2 m/s damage gate so sub-gate hits still show; the 16 ceiling = the 12.5+ approach cap + margin). This is a proven physical bound, not a tuned threshold.

**New manifest `data/native_driving_validation_wall.json`:** car16/track0 with `[wall_contact_low, wall_contact_high, wall_contact_graze, impact_wall_recovery]` — impact recovery runs LAST, which satisfies the existing `DRIVING_VALIDATION_IMPACT`-must-match-manifest contract with zero coordinator python changes (only the case_set option + `DRIVING_VALIDATION_MANIFEST` mapping one-liner).

**Dispatch proposal (coordinator, when queued):** jolt/240/rotational first (`native_ride_height` per the current adopted/fielded configuration — recorded in the report), then the source/240 pair for the backend comparison; expected evidence per case: wall contact ≥1 sample, impact event, Δv in [1.5,20], impulse inside the identity band, no teleport, upright, drive-away recovery, and the restored `HUB_POSE_RESTORE` lines present (the wheel band now participating in the contact).

## 3. Verification (this turn's evidence)

CoreRunner PASS **88092 assertions** (adds: wall-manifest validation incl. impact-last + profile existence; synthetic wall-contact tests for the per-tick jump, the force estimate at 690·√11.25·240, the dwell, and the fail-closed impulse without the reference); full project build 0 warnings/0 errors. Not verified locally (by design): the bake's runtime behaviour on CI (it runs in the hull-bake step) and the wall profiles in-engine — both ride the next coordinator dispatch, with the expected-evidence list above as the acceptance contract.

## 4. Carry-forward

- The `mip2` track-28/30 stalls remain with the `DriveController`/track-build owners (the datum fix, turn-6).
- The steering-feel native measurements ride the next moving-car DV; the turn-8 proposals stay prepared.
- The 35 mm/0.12/identity numbers' labels are unchanged (EXPERIMENTAL where flagged; the guard-exclusion stance ~0.025 stance facts recorded).
