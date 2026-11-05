# track-render turn 21 — corridor audit, all 60 tracks (shallow width sweep)

Branch: mistral/track-render. Status: MEASUREMENT COMPLETE, in-tree analysis only, no runs, no dispatches, no corridor value changes made. CorridorWidth proposals are PROPOSALS for the coordinator (adoption decision + dispatch ownership is yours).

## 1. What was measured

Per-track minimum drivable width vs the route corridor, for all 60 TrackList entries, using the turn-15 point-in-triangle methodology generalized to a sweep:

- Route: `src/Logic/SourceRoutes.cs` block i (structure verified: exactly 60 `new Vec[]{` blocks, 16,083 points, file ends at the closing `};}}`; no other Vec arrays). Indexing cross-checked against `Simulation.cs:68-69` (`SourceRoutes.Points[variant]`, `Names[variant]`) and `Simulation.cs:83` (`variant==8?16.4f:12f` current state) — the sweep index = the track index the game uses for the corridor. `baseTrackIndex` is only a material-binding selector (`Main.cs:193/194/200`), it does NOT select the route.
- Surface: `assets/Resources/<map>_<layout>.txt` (TrackList order, all 60 exist; 62 files in the dir, 2 extra untouched). Parsed EXACTLY like `DriveSurfaceLoader.cs:12-13`: file (x,y,z) -> game (x, y, -z); `f` lines take the first 3 indices only (no quad fan — matching the loader, which would also ignore a 4th index). Header note "Axis mapping x,z,-y" describes the native source axes; the loader's actual transform is (x, y, -z).
- Per segment (ALL segments, no subsampling; 16,083 total): candidates = triangles with centroid within 30 m (XZ) of the segment midpoint; roadY = median plane-Y of candidates containing the midpoint; walk outward in 0.25 m steps to +/-22 m; extent = last covered step before the first uncovered step (first-gap); step continuity |y-yPrev|<=2.0 m and |y-roadY|<=8.0 m (handles banked ovals; excludes overpass decks).
- Validation against the turn-15 deep measurements (Hockenheim segs 46-58): sweep reproduces them (seg 50: L=14.25+R=13.75=28.0 vs published 28; seg 55: 19.75+22 capped >= 41.75 vs published >=42; segs 51-54/56-58 all 22-capped on the wide side). Route lengths match reality (Brands Indy 1926.6, Laguna Seca 3601.7, Le Mans Sarthe 13616.2, Monaco 3303.4, Spa 7016.9, Monza 5806.5, Suzuka 5780.7, Interlagos 4292.9, Daytona 4019.0, Indianapolis 4018.7).
- Zero dead segments on all 60 tracks: every route segment midpoint found drive surface. Route<->mesh alignment is globally sound; no systematic coordinate mismatch anywhere.

Tool: `track-render-corridor-sweep.ps1` in this directory (Add-Type C#, ~10 s for all 60 tracks, regenerates both CSVs). Evidence: `track-render-turn21-sweep.csv` (per-track), per-segment detail regenerable; spot probes in `track-render-turn21-probe.txt` (Hockenheim 205-220 with run structure + Daytona 88-96 + Monaco 186-190 + Dubai Club 52-58 + COTA Club 26-32).

## 2. Column meanings

- `minHalf`: min over segments of min(left,right) first-gap extent (m). Conservative: a mesh seam truncates it.
- `p10Half`: 10th percentile of the same per-segment metric. Seam-resistant robust narrow-width estimate.
- `medHalf`: median. Wide-track character.
- `nB6` / `nB82`: segments with half-width below 6.0 (current corridor edge) / below 8.2 (proposed 16.4 corridor edge).
- `nSat`: segments at the 22 m measurement cap (wide areas).

## 3. Per-track table (60 rows, sorted by track index; full CSV in evidence file)

| # | track | minHalf | p10 | med | nB6 | nB82 |
|---|---|---|---|---|---|---|
| 0 | BRANDS HATCH INDY | 7.75 | 8.25 | 13.25 | 0 | 12 |
| 1 | LAGUNA SECA | 5.25 | 7.25 | 13.75 | 4 | 26 |
| 2 | DUBAI AUTODROME CLUB | 8.00* | 9.25 | 13.50 | 0 | 1 |
| 3 | BATHURST | 5.50 | 6.25 | 7.50 | 9 | 258 |
| 4 | INTERLAGOS | 6.00 | 7.75 | 11.00 | 0 | 53 |
| 5 | CIRCUIT DE CATALUNYA | 6.25 | 8.50 | 12.75 | 0 | 10 |
| 6 | CIRCUIT OF THE AMERICAS | 9.50 | 11.00 | 16.25 | 0 | 0 |
| 7 | DAYTONA SPEEDWAY | 3.75 | 5.75 | 6.25 | 42 | 252 |
| 8 | HOCKENHEIM | 4.75* | 11.00 | 15.50 | 1 | 3 |
| 9 | HONG KONG | 3.75 | 5.00 | 5.25 | 414 | 459 |
| 10 | INDIANAPOLIS SPEEDWAY | 7.00 | 7.25 | 7.25 | 0 | 176 |
| 11 | LEIPZIG KLEINERKURS | 4.50 | 8.25 | 12.75 | 2 | 5 |
| 12 | LEMANS | 6.25 | 7.50 | 19.00 | 0 | 35 |
| 13 | LIME ROCK | 5.50 | 7.25 | 14.00 | 3 | 31 |
| 14 | MELBOURNE | 3.25 | 4.50 | 5.75 | 88 | 155 |
| 15 | MONACO | 1.50* | 3.50 | 4.25 | 193 | 204 |
| 16 | MONZA | 6.50 | 7.50 | 9.50 | 0 | 61 |
| 17 | NEW YORK | 3.25 | 4.00 | 5.00 | 425 | 473 |
| 18 | NEW YORK 2021 | 2.50 | 3.75 | 5.25 | 121 | 147 |
| 19 | NUERBURGRING | 7.00 | 9.25 | 17.75 | 0 | 5 |
| 20 | BERLIN FORMULAE | 3.75 | 5.50 | 6.25 | 49 | 134 |
| 21 | RED BULL RING | 6.75 | 8.50 | 11.00 | 0 | 19 |
| 22 | RICHMOND | 7.75 | 8.25 | 8.50 | 0 | 14 |
| 23 | SEBRING | 7.00 | 8.25 | 15.25 | 0 | 13 |
| 24 | SILVERSTONE | 6.50 | 8.75 | 20.00 | 0 | 27 |
| 25 | SINGAPORE | 3.00 | 5.75 | 6.75 | 33 | 233 |
| 26 | SPA | 4.75 | 7.75 | 9.75 | 19 | 85 |
| 27 | SUZUKA | 5.50 | 6.25 | 11.50 | 2 | 118 |
| 28 | YAS MARINA | 7.50 | 9.25 | 10.00 | 0 | 1 |
| 29 | CATALUNYA CLUB | 5.00 | 8.00 | 14.50 | 4 | 12 |
| 30 | CATALUNYA NATIONAL | 6.00 | 8.75 | 12.75 | 0 | 5 |
| 31 | DAYTONA MOTOGP | 4.50 | 6.00 | 6.75 | 13 | 152 |
| 32 | DAYTONA ROADCOURSE | 3.75 | 5.75 | 6.50 | 47 | 229 |
| 33 | DUBAI AUTODROME GP | 8.75 | 9.75 | 14.00 | 0 | 0 |
| 34 | DUBAI AUTODROME HILL | 9.25 | 10.00 | 13.25 | 0 | 0 |
| 35 | DUBAI AUTODROME INTERNATIONAL | 7.75 | 9.50 | 12.75 | 0 | 1 |
| 36 | DUBAI AUTODROME NATIONAL | 8.50 | 9.75 | 15.75 | 0 | 0 |
| 37 | DUBAI AUTODROME OVAL | 9.50 | 10.00 | 19.50 | 0 | 0 |
| 38 | HOCKENHEIM NATIONAL | 5.50 | 9.00 | 16.50 | 1 | 16 |
| 39 | HOCKENHEIM SHORT | 4.50 | 10.50 | 14.25 | 2 | 5 |
| 40 | INDIANAPOLIS ROADCOURSE | 6.50 | 7.25 | 11.25 | 0 | 65 |
| 41 | LEIPZIG DYNAMISCHKURS | 3.25 | 5.25 | 8.25 | 38 | 70 |
| 42 | LEIPZIG GROBERKURS | 7.75 | 8.50 | 10.25 | 0 | 2 |
| 43 | MONZA JNR | 5.75 | 7.00 | 9.75 | 1 | 30 |
| 44 | NUERBURGRING MULLENBACH | 5.25 | 13.50 | 22.00 | 3 | 3 |
| 45 | NUERBURGRING SPRINT | 7.25 | 9.00 | 16.00 | 0 | 5 |
| 46 | SILVERSTONE BRIDGE | 6.75 | 8.75 | 16.50 | 0 | 27 |
| 47 | SILVERSTONE NATIONAL | 6.75 | 7.00 | 16.50 | 0 | 26 |
| 48 | SILVERSTONE SOUTHERN | 8.75 | 8.75 | 17.00 | 0 | 0 |
| 49 | SUZUKA EAST | 6.25 | 7.25 | 14.00 | 0 | 31 |
| 50 | SUZUKA WEST | 1.00* | 6.25 | 9.25 | 5 | 86 |
| 51 | YASMARINA TUTE | 7.50 | 9.25 | 10.00 | 0 | 1 |
| 52 | COTA CLUB | 11.50 | 11.75 | 13.00 | 0 | 0 |
| 53 | COTA SHORT | 9.50 | 9.75 | 18.00 | 0 | 0 |
| 54 | LEMANS SARTHE | 4.50 | 7.00 | 9.00 | 12 | 345 |
| 55 | LIMEROCK SOUTH | 5.75 | 7.25 | 14.00 | 3 | 26 |
| 56 | LIMEROCK WEST | 5.00 | 7.25 | 14.00 | 5 | 33 |
| 57 | REDBULL SOUTH | 5.75 | 8.25 | 10.75 | 2 | 31 |
| 58 | SEBRING JOHNSON | 8.50 | 11.25 | 17.50 | 0 | 0 |
| 59 | SEBRING SCHOOL | 5.00 | 8.00 | 10.50 | 2 | 21 |

`*` = min is a confirmed/suspected mesh seam, not a pavement edge (see 4.2).

## 4. Findings

### 4.1 Class A — 16.4-eligible (measured wide everywhere): CorridorWidth proposals

Surface supports a half-width of 8.2 m on every measured segment (minHalf >= 8.2 AND p10 >= 8.2):

| # | track | minHalf | p10 | proposal |
|---|---|---|---|---|
| 6 | CIRCUIT OF THE AMERICAS | 9.50 | 11.00 | CorridorWidth(6)=16.4f |
| 33 | DUBAI AUTODROME GP | 8.75 | 9.75 | CorridorWidth(33)=16.4f |
| 34 | DUBAI AUTODROME HILL | 9.25 | 10.00 | CorridorWidth(34)=16.4f |
| 36 | DUBAI AUTODROME NATIONAL | 8.50 | 9.75 | CorridorWidth(36)=16.4f |
| 37 | DUBAI AUTODROME OVAL | 9.50 | 10.00 | CorridorWidth(37)=16.4f |
| 48 | SILVERSTONE SOUTHERN | 8.75 | 8.75 | CorridorWidth(48)=16.4f |
| 52 | COTA CLUB | 11.50 | 11.75 | CorridorWidth(52)=16.4f |
| 53 | COTA SHORT | 9.50 | 9.75 | CorridorWidth(53)=16.4f |
| 58 | SEBRING JOHNSON | 8.50 | 11.25 | CorridorWidth(58)=16.4f |

Conditional 10th (not applied in any table above; only if you accept a seam-adjusted reading): track 2 DUBAI AUTODROME CLUB — raw min 8.00 is a seam at R=8.0-10.0 with pavement to 20 beyond (probe, seg 55); adjusting to the post-seam extent puts it at >= 10. Ship it with the seam caveat or leave at 12; both defensible.

ADOPTION STATUS: these are MEASUREMENT-BASED PROPOSALS, not applied. Hockenheim's 16.4 was incident-driven (observed 6.003/6.019/7.81 m excursions, two hard rejections). These ten have NO observed incidents; widening them is prophylactic. Two coherent policies:
- conservative: adopt only incident-driven entries (current state of the code);
- pre-emptive: apply the ten (surface-safe per this sweep), accept that the fairness benefit is unproven until an incident or telemetry shows excursions past 6.0 there.
Either is consistent with the no-blanket-policy rule because each entry is individually measured. The battery overlay run on any adopted track is the verification hook (corridor edge vs surface, same protocol as track 8).

### 4.2 Hockenheim residual risk from THIS sweep (refines my merged change)

- seg 214 R: `R=[0-4.75],[6.75-18]` — a 2.0 m SEAM (hole) at lateral 4.75-6.75 with pavement continuing to 18 m. The 4.75-6.0 portion was ALREADY legal under the old 12 m corridor; 16.4 extends the legal span across the seam to 8.2. Pre-existing hazard, slightly widened by my change.
- seg 211/212 R: pavement genuinely ENDS at 7.25 / 7.75 (no resume — first-gap = final). Under 16.4, lateral 7.25-8.2 there is legal-but-no-ground (0.45-0.95 m sliver, 2 segments, inside of a medium-speed corner). Under 12 m it was illegal-in-pavement (rejects) instead. This is the only place the merged 16.4 creates new off-surface legal positions on track 8.
- Observed failure behaviour is unaffected (rejections were at segs 54-56, pavement 28-44 m wide); no excursion was ever observed near 211-212.
- NOT fixing unilaterally (no corridor value changes without your decision): options are (a) keep 16.4 and document, (b) per-segment corridor cap (new mechanism, bigger change), (c) verify via battery item 4 overlay on track 8 — the corridor edge lines and collision overlay make the sliver directly visible. Recommend (c) first; decide ((a) vs (b)) on that evidence.

### 4.3 Class B — wide tracks with isolated pinch segments (cannot take global 16.4)

p10 >= 8.2 but min < 8.2, and the min is NOT a confirmed seam: 0 BRANDS HATCH (7.75), 22 RICHMOND (7.75), 35 DUBAI INTERNATIONAL (7.75), 42 LEIPZIG GROBERKURS (7.75), 45 NUERBURGRING SPRINT (7.25), 24 SILVERSTONE (6.50, p10 8.75, med 20.0 — the widest median in the set), 46/47 SILVERSTONE BRIDGE/NATIONAL (6.75). These would need per-segment corridors or seam triage to widen; recommend leave at 12.

### 4.4 Class C — cliff-risk present, corridor currently surface-safe

min >= 6.0 but p10 < 8.2: 4 INTERLAGOS, 5 CATALUNYA, 10 INDIANAPOLIS SPEEDWAY, 12 LEMANS, 16 MONZA, 19 NUERBURGRING, 21 RED BULL RING, 23 SEBRING, 28 YAS MARINA, 51 TUTE (dupe of 28), 40 INDY ROADCOURSE, 49 SUZUKA EAST, 19-sprint family. These have the Hockenheim geometry-type (pavement beyond 6.0 somewhere, zero-tolerance cliff at 6.0) but not the pavement margin to take 16.4 globally. No change proposed; incidents here would be per-case.

### 4.5 Class D — corridor EXCEEDS pavement (legal-but-no-ground zones, pre-existing)

min < 6.0: every street circuit (17 NEW YORK 425/488 segments, 9 HONG KONG 414/465, 15 MONACO 193/208, 18 NEW YORK 2021 121/149, 25 SINGAPORE 33/317, 20 BERLIN 49/149, 14 MELBOURNE 88/164) and the ovals' inner side (7 DAYTONA 42/252, 10/40 INDY speedway-side segments, 32 DAYTONA ROADCOURSE 47/361, 31 DAYTONA MOTOGP 13/280), plus 3 BATHURST (9), 11/41 LEIPZIG, 13/55/56 LIMEROCK, 26 SPA (19), 27 SUZUKA (2), 29/54/57/59/43/44/37/38/39 partials.

Reading: on street circuits the 12 m corridor is wider than the roadway for most of the lap (Monaco median half-width 4.25 m vs the 6.0 corridor edge). This is PRE-EXISTING under the 12 m corridor on every track (not introduced by any of my changes); barriers physically confine cars there, and no failure has been observed, so the risk is latent, not active. The oval case (Daytona/Indy): the inner apron edge sits at 3.75-6.5 m from the racing line for long stretches (sustained, real — probe confirms the surface ends; roadY descends on the banking with no resumption). Flag for the coordinator: IF any incident ever shows an off-surface rejection on a street/oval track, the fix direction is corridor NARROWING (per-track, measured) — the opposite of Hockenheim; do not widen those. No proposals made from Class D because narrowing is a gameplay change with no observed failure to justify it.

### 4.6 Mesh-seam inventory (source-data defects, physics/data-owner territory)

Confirmed 2.0-3.25 m holes in drive-surface meshes, pavement continuing beyond:
- Hockenheim_gp seg 214 R: gap 4.75-6.75, pavement to 18 (inside the current 12 m corridor — live hazard today: a car straddling 4.75-6.75 R loses support on that side)
- Dubai club seg 55 R: gap 8.0-10.0, pavement to 20 (outside current corridor)
- Daytona speedway seg 93 R: gap 15.25-18.5, surface to 26.25 (outside)
- Monaco seg 188 L: 1.5-? (cause not probed; one-segment dip, neighbors 4.5-5.75)
- Suzuka West seg 97 L: one-segment dip to 1.00 (neighbors 4-11; likely crossover/overlap region — the segs 95-99 walk alternates sides implausibly)
These are candidates for a surgical seam weld like the existing `MeasuredInterlagosSeam` precedent (brazil_circuit.txt pattern), owned by whoever holds the collision-mesh files — I own none of them; listing for the coordinator.

### 4.7 Data observation

YasMarina_tute.txt is content-identical to YasMarina_gp.txt (same 16,508 verts / 28,900 tris / route 348 pts / identical stats) — the tutorial variant reuses the GP collision mesh. Harmless; noted for asset accounting.

## 5. Caveats (shallow sweep)

- One measurement point per segment midpoint; 0.25 m lateral steps; first-gap semantics (a seam truncates the reading — seams marked `*`; p10 is the robust cross-check).
- No AI-telemetry input: "cliff risk" here = geometry exposure only (pavement margin beyond the corridor edge), not observed excursion behaviour. Hockenheim remains the only incident-backed entry.
- Ovals: sides identified geometrically (short side = inner apron); no wall layout analysis — the apron edge vs inner wall relationship is unverified.
- Elevation: step-continuity band handles banked ovals and standard kerbs; a genuine >2 m/0.25 m step would read as a gap (none of the confirmed seams is of this type — each was probed).
- The sweep reads the SOURCE assets cold; it does not exercise any build. Verification = owner battery (item 4 overlay pattern) on whatever you adopt.

## 6. Suggested next steps (all yours)

1. Decide policy for the Class A ten (adopt all / incident-only). If adopting: one-line `CorridorWidth` extension, same shape as the track-8 entry, `src/Logic/Simulation.cs:83`; verify via battery overlay on 2-3 adopted tracks (COTA CLUB + Dubai oval + Silverstone Southern cover the range).
2. Route 4.6 seam inventory to the collision-mesh owner (potential surgical welds; the Hockenheim 214 seam is the only one inside a live corridor today).
3. Track-8 battery item 4 (already dispatched pattern) doubles as the 211-212 sliver check — corridor edge lines make it visible; if the overlay shows AI/pit traffic near that sliver, revisit (a) vs (b) from 4.2.
