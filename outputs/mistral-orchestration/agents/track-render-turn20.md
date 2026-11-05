# Track rendering — turn 20: vehicle light emissions at night — diagnosis + the emissive candidate

Author: track-render specialist. `TrackNightLighting.cs` (+51) + this file. No dispatch; in-tree only; defaults untouched.

## 1. Source-bound diagnosis

- **The root cause, cited:** `CarLoader.cs:161` — `if (suf == "lights") { mat.EmissionEnabled = false; }` — every car's light glass is force-disabled at build. At night the lamps render as dead glass against a 0.18-energy ambient (the turn-6 night profile numbers) — the reviewer's "other cars invisible at night" is this line plus the ambient level.
- **The materials are per-CAR, not per-light:** `CarLoader.Build(dir, car, suf)` builds ONE `StandardMaterial3D` per suffix from the texture `<carId>_lights.png`; every light mesh of the car shares that single instance. Material-level emission would therefore light ALL lamps one colour — head and tail mixed. The correct unit is the MESH: duplicate the material per lights-mesh and give it a role colour.
- **The role split:** the body convention is forward = +Z (`NativeRaceController.cs:24`, `forward=body.GlobalBasis.Z`). The pass classifies each lights-mesh by its world-centre z against the car's mesh-mean z: front = white headlight (0.85,0.88,0.92), rear = red taillight (0.85,0.08,0.06). This is a centroid heuristic (long vehicles split at the mesh mean) — good enough for tier-1; name-based hints (`headlight/taillight` in the mesh ids) are the later refinement.
- **What the night renders show (the existing evidence):** the merged night runs (the night25-cutout family + the turn-6 profile numbers) show the cars as dark bodies with no lamps; the floodlight pool aims at the ROAD (the reviewer: road lit, cars dark). A car without visible lamps loses its strongest night cue to following players.

## 2. The candidate: `MYSIM_VEHICLE_LIGHTS=emissive` (in `TrackNightLighting.cs` — night-only scoping for free)

When the flag is set, the node's night-only build sweeps the world for MeshInstance3D surfaces whose active albedo ends `_lights.png` (the car texture convention — the track's fixture textures use other names and are naturally excluded), identifies the owning car as the depth-one ancestor whose mesh count/size is car-scale (≤400 meshes, ≤30 m — the track layers are rejected structurally), and re-enables emission per MESH with the role colour at `EmissionEnergyMultiplier=2.5`. Copies are per-mesh (`Duplicate()`), the original shared material is never mutated (my standing pattern), and the day rendering is untouched BY CONSTRUCTION: the node exists only when `Conditions.IsNight`.

**The light-budget cooperation (the brief's requirement):** tier-1 emission adds ZERO light nodes — the floodlight pool (the anchored scheme's bounded 2-12) and its per-tick assignment are untouched; `NIGHT_VEHICLE_LIGHTS emissive vehicles=N frontSurfaces=… rearSurfaces=… poolBudgetUntouched=true` is the audit line. Brake modulation (the tail energy up on brake input) and real beam spotlights (2 per car for the player + the nearest rivals) are the named later tiers — each will re-audit the light budget before existing; this turn deliberately ships only the zero-cost tier.

**Wiring note for the coordinator (owner, one line):** the pass lives inside `TrackNightLighting`, so it requires the night node's own gates (`IsNight` + `MYSIM_TRACK_FLOODLIGHTS=1`) — for a night battery this is already set; if vehicle lights should exist WITHOUT floodlights, that is a separate `Main.cs` hook (their file).

## 3. Validation (the wet/night discipline)

- Matched night pairs at one commit: {flag off, flag on} × {track 0, track 8} × {chase cam behind a rival, oncoming-traffic view, the pit-exit line-up}: the OTHER cars' lamps read as white-front/red-rear at distance; the car pans correctly (no red-facing illusion at close range); the day stills byte-identical (the node absent by construction — the scoping proof).
- The numbers to check: `NIGHT_VEHICLE_LIGHTS emissive vehicles=…` (vehicles>0 — zero would mean my `carId`-root heuristic missed the prefabs: reject and re-diagnose, do not promote); `frontSurfaces+rearSurfaces>0`; the pool metrics unchanged.
- Rejected alternatives stated: per-material emission (wrong colour domain — see §1); real beam lights per car (the budget rule — later tier); brake-state modulation (needs the drive/input plumbing — later tier).

## 4. Verification status of this turn

Full re-read of the method; balance 95/95 + 374/374; no CJK leaks; the override-aware material access (`GetActiveMaterial` — the exact pattern `RoadContactEvidence` uses) so both CarLoader's override path and raw mesh materials are seen. No compile, no run, no dispatch — in-tree only, owner battery validates.
