# Track rendering — turn 16: source-anchored night fixture PLACEMENT scheme (pit/brake/apex) + placement validator

Author: track-render specialist. `TrackNightLighting.cs` + `Conditions.cs` are my files; this turn touches `TrackNightLighting.cs` only (+97/−7). No dispatches. Default behaviour bit-identical: the scheme env is unset in every existing flow, and ALL prior flags (pool gain / ambient lift / exposure) are unchanged and independent.

## 1. What was added: `MYSIM_LIGHT_SCHEME=anchored`

The procedural floodlight candidate previously placed every pole on ONE generic pattern: a uniform 38 m grid along the route at lateral 9-14 m — no relationship to where a real circuit's lighting actually goes (pitlane, braking zones, apexes). The new scheme (opt-in env; `uniform` = the exact historical path):

- **PIT family:** poles follow the track's REAL pit route (`Circuit.PitRoute` — for track 0/8 these are the source pit data; measured Hockenheim pit lane = a uniform 9.00 m lateral), one pole per ~1/10 of the pit route, mounted 3.5 m BEYOND the pit lane (outside the pit wall), **8.5 m tall** (lower than racing poles — pit-scale lighting), aimed at the pit lane surface.
- **APEX family:** at each corner (per-segment curvature peaks; |turn| ≥ 0.007 rad/m grouped to their worst segment), a 15 m pole just OUTSIDE the apex point (10.5 m past the apex, corridor-side rejected by the validator), aimed at the exit road (4 segments downstream).
- **BRAKE family:** at each corner's approach: the mount at the physics braking distance before the apex (`v_apex = √(8.5/κ)`; `brakeDist = v²/16`, clamped 25-110 m), aimed 35 % back into the braking zone.
- All three families run through ONE placement validator; failures are recorded with per-zone reasons, exactly like the uniform scheme's rejections.

## 2. The placement validator (hooked to the existing audit)

Each anchored candidate must pass: outside the corridor (`|lat| ≥ width/2 + 2.5` — note this reads the LIVE corridor width, so the Hockenheim 16.4 m corridor automatically widens the pole standoff there); ground support (`Contact.Ground` found); level ground (normal.Y ≥ 0.85); no obstruction (the same 0.7 m-radius cylinder sweep the uniform scheme uses); ≥14 m separation from every accepted pole; sane aim declination (12°…70° — no flat beams, no vertical posts). Accepted poles build the SAME visible mast/head/emitter meshes (`pole()` helper — extracted from the uniform path, shared by both schemes) and join the SAME bounded 8-light pool with its per-tick dynamic assignment — the anchored scheme changes WHERE stations are, not how the pool behaves.

Audit: `MYSIM_TRACK_LIGHT_AUDIT` JSON now carries `scheme`, per-station `zone` (`pit`/`brake`/`apex`/`uniform`), `poolGain`, all placement attempts with `anchored/<zone>` reasons, and the corner count (`TRACK_FLOODLIGHTS ... scheme=anchored ... rejectReasons=...` + the per-zone tally line). The scope line keeps the standing label: provisional art, not real-circuit photometry.

## 3. Proposal with evidence (what the review should check)

- The reviewer's complaint was a generic uniform grid (the "fixture grid defects" line). The anchored scheme answers it structurally: poles at pit/brake/apex = where lighting is expected, with per-zone heights (8.5 m pit vs 15 m track) and aims.
- Evidence already in-tree: the uniform scheme's rejection reasons + the route geometry (the curvature peaks) are computed, not authored; every accepted/rejected placement is auditable per zone.
- Provisional labelling preserved everywhere (the audit's scope line + the data file's note); no real-circuit claims.
- Not done (stated): changing light COUNTS beyond the bounded pool; touching `SourceFixtureLighting` (owner file) — the anchored scheme composes with it or replaces it per dispatch; adding real survey data (none exists in-tree).

## 4. Verification status

Full re-read of the modified region; brace/paren balance (86/86, 315/315); `BuildAnchoredLamps` call+definition pair; the uniform path's code is untouched except the scheme gate (the `else for` restructure — verified). No compile exists yet (cloud-owned); the scheme has never run — the first flight must be the battery's night run with `MYSIM_LIGHT_SCHEME=anchored` + `MYSIM_TRACK_LIGHT_AUDIT` set, and the reviewer reads the audit's per-zone rejection tally against the visible nightly frame.
