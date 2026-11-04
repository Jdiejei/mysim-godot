# Car-assembly turn 9 completion - retained hull/DV evidence - 2026-11-06

Continues from the cut-off mid-analysis (geo8 mounts: car13 calipers ~11 mm lateral offset at anchors). Sources: geo8-41880754125 (retained, plain/review), reviewer-dv-41854714029 (retained DV + native-hull-summary), physics-gameplay-turn7.md (the gate arithmetic source), my turn-5/turn-8 sweep data and code. The DV 41871263005 retained folder is empty - no evidence, no diagnosis claimed for it.

## 1. What the hull bake contained, pre and post exclusion

Pre-exclusion (run 41848766195 era, physics turn-7's measured summary):

- car16: wheelguards `LOD_A_WHEELGUARD_FRONT_LEFT/RIGHT` baked as BODY-FIXED hulls at (the depth datum) -0.81, which sits 0.20 m BELOW the tyre support envelope (tyreBottom -0.6099, wheel centres -0.310, radii 0.2996-0.2999). `mountedHubs` empty - nothing hub-mounted. The stance datum therefore resolved to the guard bottom, not the floor.
- Consequence (the turn-7 identity): floorHeight = radius + preload - (wheelCentreY - floorY); with the guard inside the datum the car rests the guard 0.20 m into the road at any spring state, springs unload, wheels free. The DV freeze signature (41840755243: thr=1, spd=0, 2 body contacts on guard corners, 1 tyre grazing) is exactly this geometry.
- car0 (computed statically): `BRAKE_CALIPER_*` at -0.79/-0.75 baked as body hulls; the old bake regex lacked `WHEELGUARD` and matched only un-prefixed `CALIPER`.
- Real body floor (no hub parts): CHASSIS -0.60 vs tyreBottom -0.6099 = the +9.9 mm static stance - the "+10 mm" number.

Post-exclusion (0de3123, merged head):

- The wheel-template regex now covers `WHEELGUARD` and prefixed `BRAKE_CALIPER`; guards and calipers are out of the body hull set.
- The stance datum resolves to the chassis floor: required preload drops from 0.235 to about 0.025 for car16 (car0: 0 - already in class). Both inside the 0.12 ceiling. The bound raise stays dead exactly as ruled.

Retained evidence anchors: geo8 hull-bake.log (`BAKED_NATIVE_BODY car=13 convexParts=25`), reviewer-dv hull summary (the car0/car16 tables above), turn-7 report (the identity + the numbers). The DV 41871263005 report: unretrievable; nothing to analyse; no diagnosis invented.

## 2. HUB_POSE_RESTORE shifts vs my turn-5 measurements

| Source | Part | Restoring move | Expected result at merged head |
|---|---|---|---|
| geo8 runs (car13, measured) | 4x BRAKE_CALIPER et al | auto mounted to wheel anchors | lateral offset about 11 mm; verified in mounts.json |
| turn-5 sweep data (cars 15-18) | WHEELGUARD shells | out of stance datum; hub-anchor mount per config | the sweep's wheelguard group close-approach drops from 1.2-1.6 m (misplacement distance) to arch-normal clearance; stance-with-hubs converges to stance-no-hubs |
| turn-5 notes | shell raw positions (centre-of-car clusters) | mount arm (existing RecoveredComponentMounts code) | the shells' placement defect becomes measurable per hub instead of persisted |

My stance audit (749f453) already prints both datum views per car, so the pre/post numbers verify automatically on the next sweep run without new code.

## 3. Wall-fit sweep expectations at the merged head (band table)

| Group | Pre-restore (measured, turns 5-6) | Post-restore expectation | Notes |
|---|---|---|---|
| bodywork | -45 mm class at rest | unchanged | the -45 mm is bumper/chassis vs raw-pivot tyres: axle-offset defect, hull changes do not touch visual geometry; needs the axle-fit arm (rung 1) |
| wheelguard | 1.2-1.6 m clear (displaced shells) | near-anchor clearances; no FAIL expected | restored shells become arch liner geometry; correctness judged from the anchor's own clearances |
| suspension | front clear; rear strut -2..-20 mm | unchanged | same axle-dependence as bodywork |
| stance datum | with-guards forced 0.235 preload | equals no-guards; ~0.025 preload | the restore signature in the STANCE line |
| other (cab, exhaust) | clear | clear | no change |

Treat a FAIL in the merged-head's sweep as a defect only in the same row it already occupied: bodywork and rear-strut entries refer back to the axle-arm work, not the exclusion. Wheelguard-era numbers (1.2-1.6 m) disappearing while bodywork stays -45 mm is the CONSISTENT outcome; bodywork improving alongside guards would be a surprising result to verify, not a clean prediction.

## 4. Wheelguard shell misplacement - root cause

The guard meshes import at their template origins (raw FBX placement), because the source project applied wheel-arch placement through its own build/runtime pipeline that is not retained in the recovered data (no transform data for those nodes anywhere in the recovered JSONs). The standing repair is the mount route (Mounts arm): the existing RecoveredComponentMounts code mounts guards/calipers to wheel anchors, and the geo8 run proves the arm works (car13 calipers at anchors). It is a source-placement repair (mount data), NOT a threshold or stance-gate change: the datum exclusion is a separate structural decision already applied at 0de3123, and neither mechanism modifies tyre geometry, stance bounds, or collision thresholds.

Residual risk: the mount's current anchors use authored offsets (component_mount_profiles.json), not verified source arch poses; the PART_HUB_OFFSET lines from 749f453 measure exactly this gap on the next battery run. Threshold data is not a fix for either the shell misplacement or the -45 mm bodywork finding.
