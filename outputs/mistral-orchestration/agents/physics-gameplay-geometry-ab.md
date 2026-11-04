# physics-gameplay — car16 anchor-correction A/B prep (Q1 update (a2), geometry-first) — 2026-11-04

Deliverable per the assignment: the A/B evidence pair prepared on the `7116def` gates; the 0.12 preload bound kept untouched; **no default adoption**; `NativeSuspensionVehicle.cs` is coordinator-owned — the exact diff is proposed here and NOT edited by me. Committed this turn: the data-file proposal block (`data/native_suspension_profile.json`, inert until the coordinator's diff lands) + this document.

## 1. Leg A — already-measured evidence (assembled; no re-run)

| Evidence source | Run(s)/artifact | Configuration A facts |
|---|---|---|
| Clean continuous lap | `41799538756` (jolt/240/CCD/rotational), 87.52 s | floor datum: chassis hull rides the road 99.6% of samples (1785/1792); stance = chassis at ~+1 cm vs the tyre plane; the guard pair (−0.81) never registered in the contact histogram |
| DV thrust freeze | `41834771421` + `41840755243` DV_TRACE (source/120) | `started=true, gridHold=false, thr=1.00 from ~2 s, spd=0.000` whole session; `bodyContacts=2, tyreContacts=1` — the guard-corner wedge; free-rev/power-collapse mechanism (turn-5 analysis) |
| Ride gate block | `41848766195` (DV + ride at `b5ca3e0`) | `Source clearance requires unsupported suspension extension: car=16 clearance=-0.20009995 requiredPreload=0.23509994 maximum=0.12` — measured impossible, gate fired exactly as designed |
| Hull ground truth | that run's `native-hull-summary.json` | car16: wheelCentresY −0.310 (level axles), radii 0.2996–0.2999, tyreBottom −0.6099, guard bottoms −0.81, chassis floor −0.60, `mountedHubs` EMPTY (nothing hub-mounted); car0: calipers −0.79 vs tyreBottom −0.6497 (same defect class; never exercised at runtime because the case-2 label/selection ran car16 twice — coordinator's DV loop note) |

## 2. Leg B — the candidate (ONE dispatch; exact config + exact coordinator diff)

**What gets authorized here (a2):** the stance is fixed GEOMETRICALLY by decoupling the mount-anchor offset from the rest/sag pair, consumed only in ride-height mode. The stance identity (turn 7, re-included in the data ledger):

`staticFloorAboveRoad = radius + preload − anchorOffset − (wheelCentreY − floorPartY)`

At the tied default (`anchorOffset = RestLength − StaticSag = +0.11`) this reduces to the measured impossible form. Moving the anchor from +0.11 to −0.05 (5 cm below the hub centre — still inside the wheel) raises the whole body 0.16 m relative to the road at unchanged spring state. Rest length / static sag / per-axle anchor DYnamics are explicitly **not** touched (they cancel from the identity; the axles are level in the recovered source: −0.31 both, no per-axle correction is authorized or claimed).

### 2.1 The exact coordinator diff (proposed; `src/NativeSuspensionVehicle.cs` — NOT my edit)

```csharp
// 1) BEFORE the wheel/mount loop, parse the anchor (ride-height mode only; the A default keeps the tied offset):
float anchorOffset=RestLength-StaticSag;
if(System.Environment.GetEnvironmentVariable("MYSIM_NATIVE_RIDE_HEIGHT")=="1"){
  using var setup=System.Text.Json.JsonDocument.Parse(FileAccess.GetFileAsString("res://data/native_suspension_profile.json"));var settings=setup.RootElement;   // existing line, relocated above the loop
  // ...existing profile validations and the hull-bottom/tyre-bottom/sourceClearance computation stay...
  if(settings.TryGetProperty("anchorOffset",out var anchorElement)){anchorOffset=anchorElement.GetSingle();if(!float.IsFinite(anchorOffset)||anchorOffset<-.15f||anchorOffset>.2f)throw new Exception("Invalid experimental anchor offset");}
  float sourceClearance=(bottom-tyreBottom)+(RestLength-StaticSag-anchorOffset);   // anchor-aware stance datum
  SuspensionPreload=Mathf.Max(0,clearance-sourceClearance);
  if(!float.IsFinite(SuspensionPreload)||SuspensionPreload>maximum)throw new Exception("Source clearance requires unsupported suspension extension: car="+CarIndex+" clearance="+sourceClearance+" requiredPreload="+SuspensionPreload+" maximum="+maximum);
  GD.Print("NATIVE_STATIC_CLEARANCE preload=",SuspensionPreload," original=",sourceClearance," target=",clearance," anchor=",anchorOffset);
}
// 2) The wheel/mount loop uses the variable (was Vector3.Up*(RestLength-StaticSag)):
mounts[i]=RecoveredWheelCentres==null?new Vector3(i%2==0?-track*.5f:track*.5f,-.32f+anchorOffset,i<2?wheelbase*.5f:-wheelbase*.5f):RecoveredWheelCentres[i]+Vector3.Up*anchorOffset;
```
Everything else in `Configure`/`_IntegrateForces` is untouched: the spring rate, the 0.12 `maximumPreload` bound (the honest-failure branch stays VERBATIM — the bound is not raised), the bump-stop logic, the force application (the contact-point arm never moves with the anchor).

### 2.2 Leg B dispatch spec (coordinator dispatches; exact run config)

```
workflow: game-art-and-runtime-review (or the DV entry point the coordinator uses)
depth: driving_validation
case_set: driving_validation_core
physics_backend: jolt
physics_hz: 240
rotational_tyres: true
native_ride_height: true            # activates the anchor path + the corrected stance math
native_damage: false
assists_off: true
```
Expected in the B leg (fail the leg otherwise): two `NATIVE_STATIC_CLEARANCE` lines — car16 `preload≈0.0751 original≈-0.0401 target=0.035 anchor=-0.05`; car0 `preload≈0 original≈+0.0497 target=0.035` (car0's stance is already inside the 30–50 mm class; the gate correctly applies no preload; **the case-2/car-0 label bug must be fixed first — this leg is the first runtime test of the car0 gate**). The `maximum=0.12` bound byte-identical: no attempt to raise it; expected preloads sit comfortably inside.

### 2.3 The readout (same gates as the `7116def` pack — mapping)

- **Free-the-car gates (source-class failures must not recur)**: DV_TRACE per the turn-5 table — `spd>0` within ~2 s of the launch phase, `bodyContacts` back to the floor baseline, 4-wheel support in motion; the wedge signature (`thr=1, spd=0, tyreContacts=1`, 30 s) MUST be absent.
- **Group 1 (ride proof)**: underfloor minimum clearance ≥ ~0.03 m at rest and sustained; chassis-vs-DriveSurface contact fraction ≈0 (from the contact-parts rows — the 1785/1792 baseline is the A anchor); `NATIVE_STATIC_CLEARANCE` present/preload ≤0.12/within ~±15% of the expected value (deviation = the anchor-aware gate math vs the actual static equilibrium — investigate before any adoption word).
- **Group 2 (the nine targets)**: the DV core battery judged by the existing bands verbatim (braking [11.73,24.30] m for car16 etc.); travel compared against the run's own static length; **straight-stability's 0.25 rad band is the sharpest watch item** (16 cm of stance and the preload both act on it); no band moved this turn.
- **Wheel-fit/stance visual gate (car-assembly)**: the body rises ~0.16 m; the visual wheel pivots sink by the preload (~7.5 cm) into the arches — the wheel-fit sweep (their clearance evidence) must confirm the arches/wheel relationship reads still correct before ANY adoption word. This is a HARD stop in the gate order, labelled in the data ledger.
- **A/B comparison artifact**: the pair's reports + the readout assembled as the B-evidence appendix (the A side = section 1's table — no re-run).

## 3. Hard commitments

- maximumPreload = 0.12 stays byte-identical (both legs, the gate's honest-failure branch unchanged).
- 35 mm (`minimumStaticClearance=0.035`) stays EXPERIMENTAL-labelled in the data file (scope + status field + this document); nothing here adopts it.
- anchorOffset = −0.05 is labelled EXPERIMENTAL and consumed **only in ride-height mode**; with the flag off, `anchorOffset` falls back to `RestLength − StaticSag` = the exact current fielded behaviour — the A side CANNOT be perturbed by the data patch.
- No workflow/dispatch/imports by me; the coordinator applies the Vehicle diff + dispatches the B leg; I read out on landing.
