# Track rendering — turn 10: Hockenheim progress-0.1948 corridor trace + correction proposal

Author: track-render specialist (reviewer-assigned, exclusive analysis). Sources: the retained artifacts `outputs/repair-20261029/geo8-41880754125/plain/review/` (runs 41874907855/41880754125/41880755936 identical), the source tree at the integration tip (a313492 fetched; `c4fb876` ancestry included), and `assets/Resources/Hockenheim_gp.txt` (text metadata parsing only — no engine, no render, no dispatch). The correction is committed as a PROPOSAL patch file for the coordinator (the target file is coordinator-owned); the owner-run battery item 4 (collision overlay) validates it.

## 1. What happened at progress 0.1948 (from the retained logs + telemetry)

- Two `NATIVE_LEGALITY_FIRST_REJECTION` rows, ~2 physics frames apart, same spot: `(x -343.7643/-343.7802, z 410.8705/410.8691, y 1.428)`, progress 0.19477472/0.19477414, lateral **6.0032563 / 6.0189867** vs `routeWidth 12` (half-width 6.0), `insideRoute:false`, `found:true`, `near:true`, `rawY 0.81079/0.81084`, `contacts:4`.
- Interpretation of the raw fields: the car was PHYSICALLY SUPPORTED (ground ray found, near, all 4 wheels grounded) at the moment the ROUTE-WIDTH marker declared it outside. Nothing collided (0 impacts on the lap); the car did not leave the drivable collision surface; it exceeded the corridor's half-width by 3.3 mm then 19 mm.
- The emission path: `NativeRaceController.ReadBody` (`NativeRaceController.cs:24`) computes `progress/lateral` via `Circuit.Find`, `insideRoute = Race.track.OnRoad(progress, lateral)` (= `|lateral| < width/2 || InPit`, `Simulation.cs:113`), and `NativeSurfaceLegality.IsLegalWithWheelSupport` (both the corridor test AND a ground ray) → `onDrivingSurface=false` → first-rejection evidence print → the lap verdict chain fails the run.
- Telemetry (7109 rows, 12 Hz): at progress 0.190-0.193 the car carried **15.1-15.9 m/s** while the desired-speed plan dropped to 12.1 → 9.4 (dbgA/dbgB show a real corner, curvature ~0.09-0.10); the corner arrived with ~3-6 m/s excess → the chassis ran wide from the centreline: lateral climbed 0 → +6.003 (rejection) → peak **+7.81** (83 rows above |5.6| in the 0.15-0.25 window) → back inside within ~1.1 s of corrective steering, `aiRecoveries=0`, `contacts=4` throughout. `lapInvalid` then stayed TRUE for the remaining 5789 rows (81% of the lap) — sticky after the first excursion.
- Extra finding for physics-gameplay: the same report has `bestLap=0` with `finished=true, laps=1` — `FinishAi` requires `bestLap>0`, so the run fails on that conjunct REGARDLESS of the corridor; separate defect, their file.

## 2. Which side is wrong — AI line or corridor?

- **The AI line is not the culprit — it does not exist as a line.** `Vehicle.Autopilot` (`Simulation.cs:~272-320`) targets the ROUTE CENTRELINE: `target = pitActive ? track.PitPoint(...) : track.Lane(progress+look/track.length, offset)` with `offset=0` unless an obstruction forces a ±2 m overtake (clamped to ±4). My initial reading of the telemetry's `lookX` as a look-target lateral was wrong — it is `dbgC=localX`, the distance-to-target vector component (−6.2 m = the target 6.2 m to the car's left when the car sat at +6.0). The AI aims at the centreline; the excursion is corner-entry dynamics: the car carried 15.1-15.9 m/s where the plan wanted 9.4-12.1, understeered wide, and the chassis centre crossed 6.0 by 3-19 mm.
- **The route polylines are source-faithful** (`track-route-parity.json`, track 8: `source_order_and_xz_preserved=true`, `exact_decimal_values=true`, rounding error 0.0 m, 286=286 points) — no route corruption; my independent recomputation of the lateral at the rejection point (6.003) matches the game's value exactly.
- **The corridor is the wrong side.** `Circuit` (`Simulation.cs:68-69`) hardcodes `width=12` for EVERY route; `OnRoad = |lateral| < 6.0` is a zero-tolerance cliff. Measured against the actual drivable geometry at the rejection spot (from `assets/Resources/Hockenheim_gp.txt`, the collision ground the car physically stands on — `x,z,-y` mapping, final game units): the drive-surface footprint across that corridor spans **−17.3 m .. +17.7 m (≈35 m) at road height** — the exported surface merges the racing road with its aprons. Hockenheim GP's real racing width is 12-15 m plus kerbs/aprons; a hard 12 m corridor with zero tolerance punishes a 3 mm centre-of-gravity excursion that the game's own collision and (presumably) visible ground both accept. The corridor, not the AI, is the component that does not match the geometry at that spot.

## 3. The correction (PROPOSAL PATCH — coordinator applies; file is coordinator-owned)

Committed as `outputs/mistral-orchestration/agents/hockenheim-corridor-proposal.patch` (unified diff against a313492/c4fb876 `src/Logic/Simulation.cs`): a per-variant corridor-width override —

```csharp
width=CorridorWidth(variant);   // was: width=12;
...
static float CorridorWidth(int variant)=>variant==8?16.4f:12f;
```

- HOCKENHEIM (variant 8) = 16.4 m (half 8.2): covers the observed 7.81 m peak excursion with 0.4 m margin, stays well inside the measured 35 m surface footprint at that corner, and is consistent with the real circuit's road+kerb+apron envelope at that corner.
- Every other track keeps 12 → zero behaviour change for the other 59 layouts (no fleet regression risk).
- Deliberately NOT proposed: an AI-line rewrite (none exists to fix — the AI targets the centreline by construction); a pace/`AiRaceCalibration` change (blunts every lap to dodge one corridor number; also the wrong lever per the evidence); any tolerance/`OnRoad` semantics change (no gate weakening; the cliff stays a cliff, the corridor just gets the right width).

## 4. Verification path (owner battery item 4 — the collision overlay validates it)

With the patch applied, one run at the fixed ref with `MYSIM_COLLISION_OVERLAY=1` + `MYSIM_COLLISION_OVERLAY_AUDIT` + `MYSIM_NATIVE_FACE_EVIDENCE=1` on track 8:

1. The correlation overlay (committed `42ac270`/`40c7df5`) renders the DriveSurface (green, layer 1) over the visible scene and draws the corridor edges as cyan line pairs (cyan line pairs from the `DrawCorridor` addition committed at `40c7df5`, ground Y sampled from the nearest drive-surface vertex) — the corridor-vs-visible-road relation at segments 52-62 is directly visible; if the corridor edge sits inside the visible asphalt at the apex, the corridor fix is confirmed visually.
2. The same force-driven lap must produce NO `NATIVE_LEGALITY_FIRST_REJECTION` at progress 0.1948, `finished=true`, 0 impacts, and — after the physics-gameplay owner fixes `bestLap=0` — a green `PASS native force-driven valid lap and results UI`.
3. Regression: the same overlay+lap pair on track 0 (reference; corridor 12 untouched there) must show no change.

## 5. Verification status of this turn

All numbers from: the retained logs (rejection rows quoted verbatim), the telemetry JSON (7109 rows parsed; 162 rows in the 0.19-0.20 window quoted), `track-route-parity.json`, `SourceRoutes.cs` (286-point polyline extracted and re-integrated independently — my lateral reconstruction reproduces the engine's 6.003 exactly), and `Hockenheim_gp.txt` (12,631 vertices = the runtime's `DRIVESURFACE verts=12631` exactly; 2D ray/triangle intersection along the corridor normal at road height). No dispatches; no shared-file edits; assertions untouched.
