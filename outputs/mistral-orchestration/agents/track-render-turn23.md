# track-render turn 23 — wet-road/night-rain COMBINED validation prep: capture matrix + no-global-change proofs

Branch: mistral/track-render. Doc-only turn (no code changes, no dispatches). Everything below is grounded in the merged integration tree (head 906d60c) by file:line, read today. Coordinates the coordinator's dispatch decisions; I dispatch nothing.

## 1. Flag registry (the measured state; every gate is first-statement, env-read, default = exact)

| flag (game env name) | reads at | default | effect | success log line |
|---|---|---|---|---|
| `MYSIM_TOD` | Conditions.cs:19 | Settings "tod"=0 (noon) | time-of-day: 0 noon, 1 morning, 2 sunset, 3 night | (profiles applied by Apply()) |
| `MYSIM_WET` | Conditions.cs:20 | Settings "wet"=0 (clear) | weather: 0 clear, 1 overcast, 2 rain (rain spawns when profile rainAmount>0) | - |
| `MYSIM_RAIN_VISIBILITY_CANDIDATE=1` | Conditions.cs:57 | off -> base RainVolume | swaps in the RainVisibilityCandidate node | (node swap; render evidence) |
| `MYSIM_RAIN_VOLUME_PROFILE=readable` | RainVolume.cs:13 | "base" | droplet profile: larger drops/longer streaks/opacity (turn-19: 12mm/300mm/0.30 vs base 4mm/120mm/0.22) | (profile line if logged) |
| `MYSIM_WET_REFLECT=<m>` | Conditions.cs:28 | 1 (=exact) | scales WET specular mix-end only; dry end 0.04 untouched; mix weight is 0 at water=0 -> DRY immune by construction | `WET_REFLECT m=.. wet_specular->..` |
| `MYSIM_NIGHT_POOL_GAIN=<g>` | TrackNightLighting.cs:51 (+ audit :234) | 1 (=exact) | multiplies floodlight pool energy + emitter energy (2x g) | `TRACK_FLOODLIGHTS ... poolGain=<g> ...` |
| `MYSIM_NIGHT_AMBIENT_LIFT=<a>` | Conditions.cs:49-50 | 1 (=exact) | multiplies ambient ENERGY only, inside `if(IsNight)` (Conditions.cs:48) - colour stays profile bluish | `NIGHT_LIFT ambientEnergy=.. exposure=..` |
| `MYSIM_NIGHT_EXPOSURE=<e>` | Conditions.cs:49-50 | 1 (=exact) | multiplies the profile's tonemap exposure, same night-only gate as above | (same NIGHT_LIFT line) |
| `MYSIM_VEHICLE_LIGHTS=emissive` | TrackNightLighting.cs:113 | off | per-mesh emissive front/rear surfaces on cars; night-scoped; zero new light nodes | `NIGHT_VEHICLE_LIGHTS emissive vehicles=N ... poolBudgetUntouched=true` |
| `MYSIM_LIGHT_SCHEME=anchored` | TrackNightLighting.cs:46 | "uniform" | switches fixture placement to the route-anchored scheme (pit/brake/apex stations + validator) | `TRACK_FLOODLIGHTS ... scheme=anchored supportedMounts=...` |

Harness note (blocking for EVERY candidate cell below): `tools/cloud_game_audit.py` line 4 strips all `MYSIM_*` from the inherited env and re-adds only a fixed setdefault list; NONE of the ten flags above appears in that list - all ten are currently UNMAPPABLE from the yml. Required coordinator addition (one line each, same pattern as the existing `MYSIM_INTERLAGOS_SEAM_WELD` entry):

```
env.setdefault('MYSIM_TOD',os.environ.get('TOD','0'))
env.setdefault('MYSIM_WET',os.environ.get('WEATHER','0'))
env.setdefault('MYSIM_RAIN_VISIBILITY_CANDIDATE',os.environ.get('RAIN_VISIBILITY_CANDIDATE','0'))
env.setdefault('MYSIM_RAIN_VOLUME_PROFILE',os.environ.get('RAIN_VOLUME_PROFILE','base'))
env.setdefault('MYSIM_WET_REFLECT',os.environ.get('WET_REFLECT','1'))
env.setdefault('MYSIM_NIGHT_POOL_GAIN',os.environ.get('NIGHT_POOL_GAIN','1'))
env.setdefault('MYSIM_NIGHT_AMBIENT_LIFT',os.environ.get('NIGHT_AMBIENT_LIFT','1'))
env.setdefault('MYSIM_NIGHT_EXPOSURE',os.environ.get('NIGHT_EXPOSURE','1'))
env.setdefault('MYSIM_VEHICLE_LIGHTS',os.environ.get('VEHICLE_LIGHTS','0'))
env.setdefault('MYSIM_LIGHT_SCHEME',os.environ.get('LIGHT_SCHEME','uniform'))
```

No workflow input needed (the 25-input limit stays respected; these are env-port mappings, not inputs).

## 2. Capture mechanism (why the pairs are trustworthy)

The driving-validation harness (cloud_game_audit.py:101-121) runs a MANIFEST of cases; each case = {car, track, weather, compound, drivingValidation:[profiles]}, and the scenario runner **resets the car to the same spawn between profiles**. Therefore: the same scenario profile in two dispatches that differ ONLY in env = the same spawn, same camera, same scripted action -> frame pairs with exactly one variable. A wet manifest already exists (`data/native_driving_validation_wet.json` + the wet family: wet_comparison_matrix / wet_slick_comparison / wet_support_reference; harness line 108 asserts wet manifests carry weather>0). Video evidence = fixed-fps 960x540 records; static frames via MYSIM_SHOT; the night fixture view (Main.cs:193 `MYSIM_NIGHT_FIXTURE_VIEW`) renders the fixture stations directly.

RECOMMENDED MANIFEST FOR ALL MATRIX CELLS: one 4-case manifest, car 16 (open-wheel ref) + car 0 (road-car ref), tracks 0 and 8, each case = [input_direction, acceleration_0_60, braking_60_0, straight_stability, edge_excursion]. Same file reused by every cell = the spawns/shots align across the whole matrix. (Track choice rationale: 0 = the night/fixture heritage track; 8 = the corridor/overlay/seam-weld heritage track; both are base grids, no layout surprises.)

## 3. THE MATRIX (6 dispatches)

Labels: TOD n / WET w; candidate flags in brackets. Baseline = the cell with ALL candidate flags off.

| cell | env block (bare names) | manifest | frames required |
|---|---|---|---|
| **D0 identity guard** (dry day, all off) | TOD=0, WEATHER=0 | the 4-case manifest | full set; PLUS diff each frame against the pre-merge baseline run of the same depth (pin the SHA = 906d60c) |
| **N0 night base** (all off) | TOD=3, WEATHER=0 | same | same |
| **N1 night light package** | TOD=3, WEATHER=0, NIGHT_POOL_GAIN=1.6, NIGHT_AMBIENT_LIFT=1.5, NIGHT_EXPOSURE=1.15 | same | same |
| **N2 night + vehicles + anchored** | = N1 + VEHICLE_LIGHTS=emissive, LIGHT_SCHEME=anchored | same | same + the fixture-view frames (MYSIM_NIGHT_FIXTURE_VIEW) at 3 stations (pit/brake/apex) |
| **W1 wet day, rain+reflect** | TOD=0, WEATHER=2, RAIN_VISIBILITY_CANDIDATE=1, RAIN_VOLUME_PROFILE=readable, WET_REFLECT=1.6 | same | same + a chase-frame through the braking zone for spray/pool reads |
| **W2 wet night, FULL package** | = N2 + W1 (both halves ON) | same | same; the money cell |

Wet-baseline note: W0 (wet, flags off) is derivable from the WET pair inside W1's run ONLY if the harness passes the candidate flags per-case - it cannot; so IF the budget allows, add W0 as a 7th dispatch for a clean wet baseline; otherwise the N0/D0 pair + the W1-vs-D0 contrast carries the argument at reduced strength. The review board should pick: strict (7 runs) vs economical (6).

## 4. Acceptance criteria per pair (checkable, not vibes)

- RAIN READABILITY (W1 vs D0, and the W2 night variant): streaks >= 2 px wide at 10-40 m camera range on >= 5 frames (the base profile = sub-pixel at those ranges = the turn-19 measurement); NO full-screen veil: the sky/horizon mean-luma at the horizon band must not drop > 15% vs the same frame's non-rain region read.
- WET ROAD (W1 vs D0): the road shows specular response under the light sources (night) / sky sheen (day); GUARD: the DRY cells must show ZERO road appearance delta (the mix-end scaling touches the wet term only; constructively guaranteed + verified by the D0 identity diff).
- NIGHT READABILITY (N1/N2 vs N0): the car body + the near barriers + the kerb line = discernible in every car-close frame; the ambient lift keeps the hue: no channel inversion towards white (the "not white wash" rule: the sampled tint hue of the lit road must stay within the profile's bluish family); no highlight clipping: the histogram 99th pct < 0.98 on the full matrix.
- VEHICLE LIGHTS (N2 vs N1): the front white / rear red points visible at 30-100 m in the chase frames; `poolBudgetUntouched=true` line present (the pool budget floor holds).
- ANCHORED SCHEME (N2 vs N1): the fixture pools = placed at the pitting/braking/apex station sets per the audit JSON (MYSIM_TRACK_LIGHT_AUDIT output cross-checks the poolGain + the scheme + the station zones; the placements are PROVISIONAL - no real-circuit claims, per the turn-16 label).
- COMBINED (W2): the wet specular + the wet darker + the night lift TOGETHER must not black-hole the road: the road surface under the fixture pools remains >= the visibility of N1's pools (the reflection is the point); the rain streaks must remain visible against the lit road (the contrast read).

## 5. No-global-change proofs (three independent lines)

1. **D0 IDENTITY**: with every flag unset, the frame set must be pixel-identical to the pre-merge baseline run at the same depth (the diff = the acceptance gate of D0 itself). This catches any accidentally-unconditional change from the merged wet/night code.
2. **CODE-ANCHORED GATES** (the verified guard lines - all measured today in the integrated tree): the wet-reflect = the DRY immunity by construction (Conditions.cs:25-27 comment + the mix term); the night lift = inside `if(IsNight)` (Conditions.cs:48-51) so the day cells are IMMUNE BY CODE; the rain node = spawned only when rainAmount>0 (Conditions.cs:57); every candidate = the first-statement env read with the exact-default fallback (the registry table's "default = exact" column). Any future edit that breaks one of these = a re-run of D0 fails = the gate.
3. **SCOPED-DELTA RULE** (per candidate cell): the frame diff vs ITS baseline must be localized to the flag's domain (the rain streaks =/only rain; the pool circles =/only the floodlight spots; the emissive =/only the car surfaces; the exposure/ambient = the global brightness shift = expected and bounded by the clip criterion). A diff appearing OUTSIDE the domain = the isolation failure = REJECT the cell, fix the flag interaction, re-run.

## 6. Regression watch (not matrix variables - but every frame gets checked)

- The FENCE (the turn-1 heritage stays REJECTED: no fence candidate re-enters through a wet/night flag): the fence wire must remain visible (not black-crushed) in the W2 night frames - the wet/night pass MUST NOT change the fence materials.
- The CORRIDOR/OVERLAY (the item-4 framing): the overlay runs are unaffected by the wet/night flags (separate depth); no interaction expected - the watch = the audit JSON's `scheme` field does not leak into non-night runs.
- The known string artifact: Conditions.cs:34 log line contains a mojibake arrow ("->" garbled) - cosmetic only, in a log string, no behavioural effect; note for a future cleanup commit, not this turn.

## 7. Proposed dispatch commands (for the coordinator; I run nothing)

Same shape as the battery's existing six:

```
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref <merged-main-head> -f depth=driving_validation -f cars=16   # + the yml env block per the cell table
```

Six (or seven with the strict W0) dispatches total, each differing ONLY in the env block of its yml edit; the manifest shared. Artifacts: the encrypted-review videos/frames + the wet/night report JSONs + (for N2) the light audit JSON; decrypted via the owner key per the standard flow.

THE ORDER RECOMMENDATION: D0 first (the identity guard - if it fails, STOP and review the merge), then N0/N1/N2 (the night chain), then W1, then W2 (the combined), each accepted before the next launches (one-change-per-cycle preserved per cell).
