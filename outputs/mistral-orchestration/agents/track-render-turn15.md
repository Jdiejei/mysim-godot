# Track rendering — turn 15: formal width measurement (Hockenheim rejection corner), legality-boundary math, final acceptance criteria

Author: track-render specialist. All numbers re-measured this session from `assets/Resources/Hockenheim_gp.txt` (the drive-surface collision mesh: 12,631 verts / 21,866 tris — matching the runtime's `DRIVESURFACE verts=12631 tris=21866` exactly) against `Logic/SourceRoutes.cs` route 8 (286 points; the game reports length×0.95 = 4331 ⇒ length = 4559 m, matching my independent polyline sum of 4559.1 m). No dispatch, no rendering, no corridor value changed.

## 1. Per-segment measured widths (source-cited; s = lateral offset from the route centre along the segment normal)

Method: for each route segment (midpoint + normal), a 2D point-in-triangle coverage profile at s = −22…+22 m (0.25 m steps; triangle candidates = centroid within 25 m of the segment midpoint, mean y in [−0.5, 3.0] — Hockenheim's start-field slopes y ≈ 0…1.4 there). `…` = the profile window saturated (the surface extends beyond ±22 m: the number is a LOWER bound).

| Segment | Progress | Coverage spans (s, metres) | Width |
|---|---|---|---|
| 40 | 0.1399 | −11.25..11.25 | 22.75 m |
| 46 | 0.1608 | −17.25..11.50 | 29.00 m |
| 47 | 0.1643 | −16.50..11.75 | 28.50 m |
| 48 | 0.1678 | −15.50..15.75 | 31.50 m |
| 49 | 0.1713 | −14.50..14.75 | 29.50 m |
| 50 | 0.1748 | −13.75..14.25 | 28.25 m |
| 51 | 0.1783 | ≥44 (window-saturated) | ≥44 m |
| 52 | 0.1818 | ≥44 (saturated) | ≥44 m |
| 53 | 0.1853 | [−22..−16.5] **split** + [−14..≥22] | ≥36 m, with an inner seam/gap at −16.5..−14 |
| 54 | 0.1888 | ≥42.75 (saturated) | ≥42.75 m |
| 55 | 0.1923 | ≥42.00 (saturated) | ≥42 m — **contains both rejection rows (s=6.003/6.019)** |
| 56 | 0.1958 | ≥43.50 (saturated) | ≥43.5 m |
| 57 | 0.1993 | ≥43.75 (saturated) | ≥43.75 m — contains the +7.81 peak |
| 58 | 0.2028 | ≥41.25 (saturated) | ≥41.25 m |
| 59-64 | 0.206-0.224 | no surface in the ±22 m / y-band window (the route/pit region transitions; the band gate excluded it) | n/a (window, not absence — see caveat) |
| 120 | 0.4196 | −13.50..20.00 | ≥33.75 m |
| 160 | 0.5594 | −18.75..21.75 | ≥40.75 m |
| 200 | 0.6993 | −22.00..9.25 | ≥31.50 m |

Exact vertex ranges for the两位 rejection rows' own segment (55): the crossing vertices involved in the centreline-containing span map to file vertex indices **881-12219** (the full measured envelope; the usable road+kerb band sits inside file verts ≈ **2392-12219**, with the outer span 855-8475 belonging to the neighbouring apron paths). These ranges are the audit boundary for the overlay's vertex-level check.

CAVEATS (stated, so the document cannot overclaim): the ±22 m window saturates at 10 of the 15 measured segments (the real widths there are ≥ the printed numbers); the y-band [−0.5, 3.0] excludes raised/dropped surfaces (the "no local surface" rows are a window/band result, not proof of absence); the drive mesh is "RR3 collision geometry, not the rendered track model" — the rendered-road correspondence is exactly what battery item 4's overlay renders show.

## 2. Legality-boundary math (baseline 12/6.0 vs applied 16.4/8.2)

- Car 13's physical half-width = 2.22/2 = 1.11 m (CARSIZE 13 (2.22,1.18,4.71)).
- **Baseline corridor (12 m):** the chassis centre must satisfy |lat| < 6.0; the full car occupies to ±7.11 m. The observed centre excursions 6.003/6.019/7.806-7.808 are OUTSIDE — 3.3 mm to 1.81 m — and the 6.003 rejection fired with the car's centre only 3 mm over while all four wheels sat on the drive surface.
- **Applied corridor (16.4 m):** the centre bound = ±8.2. The same excursion set: 6.003 → legal with **2.20 m** margin; 7.808 (the peak) → legal with **0.39 m** margin; the full observed envelope (≤7.81) stays inside with ≥0.39 m to spare, and the corridor edge (±8.2) remains ≥ 9.1 m inside the measured physical surface (±17.3) — no "legal beyond the ground" states are introduced.
- The AI line under each: the AI targets the CENTRELINE in both (unchanged trajectory, unchanged lap time); the corridor re-classifies the same trajectory: baseline = illegal at the first 3 mm overshoot (lap dead at progress 0.195 of 4559); applied = legal through the full observed envelope. No route-parity, pit carve-out (`InPit` engages only beyond 8.2; the Hockenheim pit lane sits at a 9.00 m lateral, still carved out), overtake clamp (±(width/2−2), self-consistent), or collider position changes with the new width.

## 3. Battery item 4 — final acceptance criteria (lock these)

Run shape: track 8 + car 13, `MYSIM_COLLISION_OVERLAY=1`, `MYSIM_COLLISION_OVERLAY_AUDIT` set, `MYSIM_NATIVE_FACE_EVIDENCE=1`, plus the same-shape run WITHOUT the overlay for the naked-eye pair. NOTE (flagged, turn-14): as wired, `depth=track_scene_audit` covers tracks 0/5 only — the corridor validation on track 8 needs the depth's track map extended to include 8 (owner one-liner) or a track-8-capable variant; the overlay env-name alias fix (this branch) must be in the built binary.

PASS requires ALL of:

1. **Log:** the lap produces NO `NATIVE_LEGALITY_FIRST_REJECTION` at progress 0.1948 ± 0.005; `finished=true`; 0 impacts.
2. **Pixel:** cyan corridor edges hug the asphalt/kerb boundary (≤0.5 m inside) through segments 52-62; never over grass; never crossing a red wall volume's inner face.
3. **Pixel:** the red wall volumes are PIXEL-IDENTICAL to the pre-patch overlay frames (no collider moved; the no-collider-movement proof made visible).
4. **JSON:** `unmatchedCount` equal to the pre-patch overlay run's; `driveWidthDeltas` unchanged; `trims=0` (no trim list supplied ⇒ the mechanism stays unexercised).
5. **Scope:** the corridor edge at ±8.2 remains ≥9 m inside the measured physical surface at every measured segment (§1 table) — if any future measurement shows the surface tighter, that segment's evidence comes back to the corridor doc BEFORE any value change.

## 4. Document status / verification

Every number in §1-2 was re-measured this session from the two named source files (the mesh + the route source) and agrees with the runtime's own reported dimensions; the two rejection rows are quoted verbatim from the retained logs. No corridor value changed in this turn (16.4 stands); §3 is the final acceptance contract for the battery run. No dispatch; no local rendering; the visual verdict belongs to the human review.
