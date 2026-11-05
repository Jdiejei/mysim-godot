# F1 TRIAD visual review pack - cars 15 (W10) / 17 (RB15) / 18 (MCL34)

The three cars (beyond car16) with documented defects, in one pack with per-car
sections. Same three-panel structure as the car16 packs: SOURCE (authored livery
island) / the candidate pair from the existing decrypted local sets / the current
in-game state where a current set exists. Generator: `tools/carbody_f1_triad_pack.py`
(read-only). Branch `mistral/carbody-livery`; **in-tree/local evidence only;
cloud-UNVALIDATED; the art decisions stay with the human reviewer.**

Panel sources are the existing decrypted/downloaded artifacts under
`outputs/repair-20261029/cloud/...` and the decrypted fleet set - nothing re-rendered.
Where a panel does not exist (no such local capture), the gap is stated instead of
substituted; the wheel/rubber closeup strips carry the rubber-state evidence for the
"featureless rubber" reject.

---

## car15 - Mercedes W10 - reject: fragmented source atlas

Documented reject (committed in the batch-2 inventory from the L1/L2 reviews):
`FULL_F1_W10_WHEEL_REVIEW.json` (run 41310403219): "source body atlas fragmented";
`FOUR_CAR_PAINT_PARTIAL_REVIEW.json` (run 41335778130 @960e56f): "W10 body paint now
coherent silver/teal instead of scrambled ext atlas; plain body lacks final decals;
wing/misc source surfaces still broken looking"; gloss/reflections unfinished.

Main strip `images/car15-main.png`:
- SOURCE: authored BODY ext island, `2019_f1_mercedes_w10_ext_44.png` (1024x1024, body
  island u 0.006..0.994 / v -0.995..-0.293, 14 UV0 verts; full island table in the
  batch-2 inventory `car15.json`)
- paint variant A (candidate-0), run 41335778130: `cloud/coatedpaint0/review/f1-paint-car-15-candidate-0-view-0.png`
- paint variant B (candidate-1), same run: `...candidate-1-view-0.png` (the variant the
  FOUR_CAR_PAINT review describes as now-coherent silver/teal)
- current generic view: `cloud/car-review-current/review/car-15-view-0.png`

Wheel/rubber strip `images/car15-wheels.png`: `cloud/deepwet0/review/rebuilt-wheel-car-15-compound-{0,1,2}-wheel-{0,2}.png`
(6 closeups, compounds 0/1/2 x wheels 0/2 - the rebuilt-rubber state at the deepwet run)
+ `cloud/axlefit0/review/wheel-fit-car-15-steer{-23,0,+23}.png` (the wheel-fit steer
triplet; fit numbers = the merged WheelFitAudit artifacts, not re-measured here).

## car17 - Red Bull RB15 - reject: featureless rubber / absent brake detail

Documented reject: `RB15_SIDEWALL_CORRECTION_REVIEW.json` (run 41302933144): sidewall
marking PLACEMENT verified (blue front WET / yellow rear MEDIUM rings visible), "rubber
still featureless black, brake detail absent and body livery/UV fragments remain...
issue shared by all four custom F1 models"; `FULL_F1_RB15_WHEEL_REVIEW.json`
(run 41310403219): swept 10-spoke rim + compound colours + wet drainage visible;
raised rings plastic; barrel plain bright; caliper mostly occluded.

Main strip `images/car17-main.png`: SOURCE (BODY ext island of
`2019_f1_red_bull_rb15_ext_23.png`, 1024x1024, body u 0.006..0.994 / v -0.995..-0.293)
+ paint variant A (candidate-0) / variant B (candidate-1) from
`cloud/coatedpaint2/review/` (run 41335778130 family) + current generic view
(`cloud/car-review-current/review/car-17-view-0.png` - note: this set predates the
current head; it is the latest generic single view cached locally, gap noted).

Wheel/rubber strip `images/car17-wheels.png`: `cloud/f1-custom-renders/game-review-41300908861-1-2/review/rebuilt-wheel-car-17-compound-{0,1,2}-wheel-{0,2}.png`
(6 closeups - the rebuilt/custom rim + rubber state the review called featureless)
+ `cloud/axlefit2/review/wheel-fit-car-17-steer{-23,0,+23}.png` (fit triplet; per-pose
clearance numbers live in the 41837315358 CI artifacts - not in-tree, stated).

## car18 - McLaren MCL34 - reject: coated-paint improvement not established

Documented reject: `COATED_PAINT_PARTIAL_REVIEW.json` (run 41337127855 @ff93c22):
"MCL34 still has broad bright body highlights and plain undetailed panels; distinct
improvement from material-only change is not established by these views... avoid
repeated roughness tuning as substitute for surface detail/geometry/UV repair";
body atlas fragmented (`FULL_F1_MCL34_WHEEL_REVIEW.json`, run 41310403219).

Main strip `images/car18-main.png`: SOURCE (BODY ext island of
`2019_f1_mclaren_mcl34_ext_4.png`, 1024x1024, body u 0.007..0.996 / v -0.998..-0.004)
+ paint variant A (candidate-0) / variant B (candidate-1) from
`cloud/coatedpaint3/review/` (the 41337127855 set the review's "not established"
verdict refers to) + **CURRENT production state**:
`fleet-41848086075/decrypted-shard0/review/fleet-car-18-front.png` (960x540, the only
ONE of the triad with decrypted fleet captures; head `b5ca3e0` - older than today's
integration head: production state RELATIVE to that head, stated).

Wheel/rubber strip `images/car18-wheels.png`: the four decrypted fleet closeups
`fleet-car-18-wheel-{fl,fr,rl,rr}.png` from the same production head + the
`FULL_F1_MCL34_WHEEL_REVIEW.json` verdict (all 8 compound views: swept spokes, compound
colours, wet drainage geometrically present; raised plastic rings / bright barrels /
occluded brakes unaccepted).

---

## Cross-car reading + decision space (the reviewer's, documented, not applied)

- The triad shares ONE documented defect family with car16 (the fragmented/liveried
  atlas + the wheel art rejects); the car16 mirror/wheel packs carry the UV-or-livery
  method; these three packs add the per-car candidate pair view.
- The per-inventory documented findings stay authoritative (committed batch-2
  inventories, six-field rows + json citations); this pack changes no finding, adds no
  finding, and pre-decides nothing: the four custom-wheel-plan/spec and any new paint
  direction remain PROPOSED until the reviewer's verdict.
- Verification of the pack build itself: generator ran clean; every referenced file
  existed at build time; per-car file counts: car15 = 7 files used / 6 closeups,
  car17 = 7 / 6, car18 = 7 / 6; no missing-wheel-file warnings.

## Limits (stated plainly)

- car15/car17 have no decrypted FULL-6-view sets at the current integration head, only
  the run/local sets above (head provenance per artifact family: FOUR_CAR_PAINT @960e56f,
  COATED_PAINT @ff93c22, axle-fit sweep artifacts, neutral-grade runs, custom-renders
  41300908861); car18's fleet set is at head `b5ca3e0` (pre-shard-fix; production state
  as of that head).
- The cockpit view exists for car18 only (fleet depth). All strips were resized nearest
  / Lanczos-resized for plate width - judge shape/detail, not sharpness.
- The defect labels in the headings are the DOCUMENTED verdicts, not this pack's
  conclusion; the packs assemble, the reviewer decides.
