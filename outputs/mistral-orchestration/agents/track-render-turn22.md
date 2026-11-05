# track-render turn 22 — seam welds: 2 implemented (flag-gated), 3 inventory items resolved as non-defects

Branch: mistral/track-render. In-tree work only: source reading, mesh-data measurement, patch authoring, local (non-engine) verification. NO dispatches, NO corridor value changes (turn-21 proposal list untouched), no source-asset files touched on disk.

## 1. What was delivered

- `src/TrackSeamWelds.cs` (NEW, mine): measured seam welds for the two confirmed drive-mesh holes, following the `MeasuredInterlagosSeam` precedent: OFF by default (`MYSIM_SEAM_WELDS=1` enables), exact-resource-path gate, exact source triangle-count gate, every bridge vertex resolved against the source mesh by exact float match, bridge face area bounds (0.02-5.0 m2), loud exception on any identity mismatch (never welds against unexpected geometry), `SEAM_WELD ...` log line on success.
- `src/DriveSurfaceLoader.cs` (ONE-LINE hook, SHARED FILE - ownership flag for the coordinator): the weld chain inserted before the existing Interlagos chain:
  `NativeSpinDiagonalExperiment.ApplyCandidate(MeasuredInterlagosSeam.Apply(TrackSeamWelds.Apply(tris.ToArray(),resPath),resPath),resPath)`
  My weld runs first (on raw file data; its count gates expect the raw file counts). The existing Interlagos/Spin gates are untouched; for non-listed paths every stage is a pass-through no-op.
- Evidence: `track-render-turn22-seams.txt` (this dir): boundary loops, station maps, before/after walks, the runtime dress rehearsal.

## 2. Inventory resolution (turn-21 list -> verdicts)

| item | verdict | evidence |
|---|---|---|
| Hockenheim_gp seg 214 R: gap lateral 4.75-6.75, surface resumes to 18 | REAL hole; WELDED | boundary = closed lens loop, 18 inner + 21 outer verts (v12614/v12595 shared tips, span al -11.4..+7.3); walks below |
| Dubai_Autodrome_club seg 55 R: gap 8.0-10.3, surface resumes to 30 | REAL hole; WELDED | closed loop, 19 inner + 17 outer verts (v4839/v4833 tips, al -17.3..+2.6); walks below |
| Daytona_speedway seg 93 R `[15.25-18.5]` | RETRACTED - probe artifact | full-mesh re-walk = [0-26] on every station -14..+14; the turn-21 gap was my probe's 30 m centroid prefilter starving on the banking's long thin triangles (centroids >30 m away). Turn-21 inventory corrected by this report; the sweep's small-lateral minima are unaffected (patch-edge triangles are small) |
| monaco_gp seg 188 L min 1.50 | NOT A SEAM | smooth V-narrowing to a clean connected 1.13-1.41 m boundary edge = real corner-apex pavement edge; the 1.50 stands as Monaco's real minimum |
| Suzuka_west seg 97 L min 1.00 | NOT A SEAM | route sits on the figure-8 crossover's upper road: mid hit at y=-5.04 (road) AND y=-12.12 (underpass road 7 m below); the left void = the crossover opening. Correct topology. (Also explains the sweep's L/R alternation at segs 95-99.) |

## 3. Weld geometry (measured, not guessed)

Both holes are lens-shaped splits between two mesh patches with PARALLEL boundary arcs ~2 m apart and near-identical surface heights (max dy across the seam 0.16 m). The weld = a fan strip between the two boundary arcs using EXISTING vertices only (no new positions - watertight to the patch edges, no T-junction vertices created):

- Hockenheim: 35 bridge triangles (two-pointer merge across the 18/21-vertex arc mismatch, degenerate shared-tip faces skipped); faces 0.057-1.913 m2; max edge 2.648 m; arc y -0.267..-0.405.
- Dubai Club: 32 bridge triangles (19/29 arcs after closing the -end tip the naive pass had missed: verts 4831/4838/4849/4850; one 0.5 mm pinch at v4860 deduped); faces 0.114-2.847 m2; max edge 3.719 m (two straddle faces across the inner arc's sparse 4846->4845 stretch - flat, healthy).

## 4. Verification (all local, all reproducible)

- BEFORE/AFTER coverage walks (0.25 m steps, same method as turn 21): Hockenheim se214R `[0-4.75],[6.75-18]` -> `[0-18]`; Dubai se55R `[0-8],[10-30]` -> `[0-30]`; ALL neighbor segments (Hockenheim 212/213/215/216, Dubai 53/54/56/57) byte-identical before/after - the weld is strictly local.
- Runtime dress rehearsal: the emitted .cs compiled (V3-adapted only for the local harness - no Godot assembly here) and executed against the REAL mesh arrays:
  - flag off: returns the source array reference unchanged (both tracks);
  - flag on: Hockenheim 21866 -> 21901 tris (+35 as designed), Dubai 8780 -> 8812 (+32);
  - truncated source array: throws the count gate; wrong path: pass-through.
- The remaining verification the local environment CANNOT do: render/physics reality of the welded strips (below).

## 5. Open items / risks

- SEMANTIC QUESTION (deliberately left to evidence, not to me): a histogram of "hole vs feature" - the drive-mesh mirror of the two strips is LANNES between tarmac patches, but I cannot see the RENDERED world. If the source game PUT something there (grating/garden/wall base), the weld adds ground where the original had none - which would be WRONG. Mitigations in order: (a) the welds are OFF by default - adoption requires the battery evidence; (b) the flag-gated run + the collision overlay on track 8 will show the strip as drive surface exactly where the rendered world shows tarmac (or not).
- HOCKENHEIM 16.4 residual (turn-21, unchanged): segs 211-212 right edge at 7.25/7.75 m still caps the useful corridor at ~14.5 there; the weld does NOT change that finding (different mechanism: real pavement edge vs hole).
- Road-surface patch cracks (NEW discovery, cosmetic only): Hockenheim's road strip carries zero-width patch seams (boundary edges with no index-sharing at lateral ~3.4 near seg 213 and along ~+15) - invisible to 0.25 m walks, no gameplay effect, listed so nobody re-discovers them as a bug.
- `src/DriveSurfaceLoader.cs` is outside my standing file list; the one-line chain edit is the minimal integration point and is called out here for the coordinator's ownership bookkeeping.

## 6. Proposed validation run (for the coordinator - I dispatch nothing)

Existing depth + one env addition; the run() helper strips MYSIM_*, so map to the bare name as with my other passes:

- Battery item (lap legality/ground, the closest existing depth): `MYSIM_SEAM_WELDS=1` mapped to bare `SEAM_WELDS=1`, tracks [8, 2], expect in logs: `SEAM_WELD ... Hockenheim ... bridgeTriangles=35 ...` + `... Dubai ... bridgeTriangles=32 ...`, 0 ground-loss events in the two weld zones (progress neighborhoods of the seg-214/seg-55 stretches), and unchanged lap legality outcomes.
- The overlay pass (item 4 pattern) on track 8 with the same env renders the corridor + colliders over the welded strip - the visually decisive frame for the adopt/reject call.

If the battery shows ANY anomaly attributable to the welds, the revert = the flag (default off); no source files change either way.
