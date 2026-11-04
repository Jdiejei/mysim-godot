# EVIDENCE INDEX — MySim acceptance-claim register

Maintained by the verification agent (branch `mistral/verification`). One row per acceptance claim covering: settings persistence, continuous lap, HUD bounds, wheel closeups, cockpit, fences, UV candidates. This is the reviewer's index — every claim points to an exact run ID, commit and artifact path with an honest status.

Status vocabulary (nothing here implies visual acceptance or whole-game acceptance):
- **machine-verified** — an automated gate in the named run passed (green run + assertion); artifact content checked by machine, not necessarily seen by a human
- **human-pending** — machine evidence exists; human review not yet done
- **rejected** — explicitly rejected by review/metrics; evidence preserved
- **failed** — the run failed; preserved for history; proves nothing in either direction
- **superseded** — a newer run/claim replaces this one

Updated: 2026-11-04, session 3 (addendum refresh). Run statuses via `gh run view --json` only. Artifact paths marked `✓disk` were verified to exist on the local disk this session; `(CI artifact)` = exists as a run artifact, not downloaded locally. Content figures quoted from the packet/JOB_REGISTRY and not independently re-read this session are marked `(content per packet)`.

## Settings persistence

| Claim | Run | Commit | Artifact path | Status |
|---|---|---|---|---|
| Atomic save + typed fallback + legacy/foreign/corrupt/malformed + failure preservation (28 base cases, Linux, isolated per-scenario HOME) | 41710482854 | `dfe0ed3` | `outputs/repair-20261029/cloud/settings-persist-result/` ✓disk | machine-verified — SUPERSEDED: 2 of 28 (`temp_live_kept`,`rename_live_kept`) passed vacuously (un-globalized path, `""==""`); fixed and re-proven by the rows below |
| Same 28, non-vacuous re-proof + `contract_sync_case_names` live (fixture case names == audit `required_cases`, both directions) | 41798040383 | `e8b80bb` | CI artifact (not downloaded locally) | machine-verified |
| 29-case Linux basis incl. `legacy_long_roundtrip` (long-typed `Get` fix at `9ff443c`; regression found by 41795630650's controls probe) | 41818941902 | `9ff443c` | CI artifact (not downloaded locally) | machine-verified |
| Windows packaged-player persist: build + smoke + persist assertions on real `%APPDATA%\Godot\app_userdata\MySim\settings.cfg` | 41819498045 | `d3629e8` (`d3629e8430fcddcd5544153221e07467f7499120`, tag `windows-candidate-d3629e8430fcddcd5544153221e07467f7499120`) | `build-cloud/windows-settings-persist-*.{json,log}` (CI artifact) | **failed — in the persist step itself** (build and native-smoke steps PASSED; verified via `gh run view --json jobs`). First real Windows persist datum: the persist assertions failed on Windows. Which assertion = unknown (run logs outside permitted read scope); candidates: the predicted `atomic_replace_*` delete-then-rename defect, the temp-fault/`FileShare.None` mechanics, or a harness issue — NO root cause claimed. ADDITIONALLY ran the PRE-platform driver (`d3629e8`): even on success it could only have proven the 8-case persist contract — NOT `win_sharing_violation_*` presence, NOT `platform=windows`. Must not be cited as Windows persistence evidence of any passing kind |
| Windows decisive run: full 32-assertion contract incl. the 3 `win_sharing_violation_*` cases (driver-required) + `platform=windows` report proof, at a head that also carries the 41819498045 failure diagnosis/fix | REQUIRED at `2862b04`+ | `2862b04` (driver enforcement + fixture `platform` field) | `build-cloud/windows-settings-persist-summary.json` (expected) | open — not yet dispatched |
| Three-way contract sync: fixture `BaseCases` = `cloud_game_audit.py required_cases` = `cloud_windows_persist.py required` (29 base / 32 with win cases; verified by set comparison at `2862b04`) | static verification (turns 2-3, this agent) | `2862b04` | `src/SettingsPersistChecks.cs`, `tools/cloud_game_audit.py`, `tools/cloud_windows_persist.py` | machine-verified (static) + runtime-enforced on every repo run by `contract_sync_case_names` |
| Prior Windows dispatch attempts (misconfigured; never evidence) | 41818937383 (ran at stale `a6416fb`), 41819339436 (cancelled; tag/checkout mismatch) | — | — | failed, preserved; never cite as Windows evidence |

## Continuous lap

| Claim | Run | Commit | Artifact path | Status |
|---|---|---|---|---|
| Clean continuous lap, jolt/240 Hz/CCD/rotational: `finished=true`, `laps=1`, `bestLap=87.52s`, `damage=0`, `impactEvents=0`, `ruleResets=0`, 4 tyre contacts, no chassis contacts (content per packet — metrics not re-read this session) | 41799538756 | `5d21c56` | `outputs/repair-20261029/lap-jolt-41799538756/encrypted-review-0-attempt-1/game-review-41799538756-1-0/review/` ✓disk (89.6s video + telemetry) | machine-verified metrics; video human-pending — NOT yet watched by the human reviewer |
| Earlier source-solver lap attempt: 17 impacts, damage 0.27 | 41794252920 | `71dfc6c` | (per JOB_REGISTRY) | failed, preserved — superseded by the jolt run above |

## HUD bounds (per-resolution)

| Claim | Run | Commit | Artifact path | Status |
|---|---|---|---|---|
| Objective HUD bounds: `Hud.EmitRects()` at capture time + `reference_evidence` asserts speed/gear/info/minimap/standings/hybrid rects inside viewport at 640x360…2560x1080 | first run 41820839687 | `076363b` | CI artifact | failed (shard 0, audit step) — superseded by the row below; no bounds result produced |
| Same claim at the fixed head (`5bf41d7` CS0103 fix in the merged mirrored-text detector) | 41825439700 | `5bf41d7` | CI artifact | in flight — human-pending (no result yet); bounds claim OPEN |
| HUD visual improvements at 5 resolutions — EXISTENCE-ONLY, no bounds asserts | 41798036351 | `e8b80bb` | `outputs/repair-20261029/car-ev-41798036351/` ✓disk | machine-verified (existence only) — superseded for the bounds claim by the rows above |

## Wheel closeups

| Claim | Run | Commit | Artifact path | Status |
|---|---|---|---|---|
| All 4 wheels + steered-FL real closeups, engine gates pass (`corners=32`, not whole-car) | 41798036351 | `e8b80bb` | `outputs/repair-20261029/car-ev-41798036351/` ✓disk | machine-verified; human-pending (wheel/wing clearance unproven from occluding perspectives per VISUAL_REVIEW 2026-11-04T09:10Z) |
| Earlier `wheel-fl` capture was a whole-car three-quarter image, not a closeup | 41788844369 | `e41e29b` | `outputs/repair-20261029/cloud/reference-evidence-41788844369/` ✓disk | rejected (VISUAL_REVIEW 2026-11-04 item 4) |

## Cockpit

| Claim | Run | Commit | Artifact path | Status |
|---|---|---|---|---|
| Genuine cockpit view: `COCKPIT_VIEW mode=2 fov=75 seatLocal=(0,0.88,0.05) seatOffsetFromAuthored=0.000 wheelsHidden=True` (authored seat, not overhead) | 41798036351 | `e8b80bb` | `outputs/repair-20261029/car-ev-41798036351/` ✓disk | machine-verified; human-pending (halo dominance / instrument readability / driveability with motion open) |
| Earlier cockpit frame was an overhead whole-car image, not cockpit evidence | 41788844369 | `e41e29b` | `outputs/repair-20261029/cloud/reference-evidence-41788844369/` ✓disk | rejected (VISUAL_REVIEW 2026-11-04 item 5) |

## Fences / foliage / alpha

| Claim | Run | Commit | Artifact path | Status |
|---|---|---|---|---|
| true-Alpha blend candidate reduces fence speckle | 41632524155 | `5492d90` | `outputs/repair-20261029/cloud/fence-true-blend-result/` ✓disk | rejected (no speckle reduction; evidence kept) |
| A2C candidate (alpha-to-coverage + alpha-to-one over 4x MSAA) reduces fence speckle | 41647350933 | `6d2069a` | `outputs/repair-20261029/cloud/fence-a2c-result/` ✓disk | rejected (edge density unchanged in 8/9 cells; VISUAL_REVIEW rejection stands) |
| mip-repair candidate (`fence_miprepair_review`) | first run 41795630650 (`5288329`): failed on the controls probe — fence evidence never produced; retry 41818948189 (`9ff443c`): FAILED mixed (shards 0/3/4 failed in the audit step; 1/2/5 succeeded; invariant schema fix `3061afa`/`49ece2c` postdates the run — per-shard causes unverified, logs outside scope) | `9ff443c` | CI artifacts | failed — NO fence conclusion possible yet; rerun at `49ece2c`+ required |
| Baseline fence/grandstand speckle + black foliage masses | visual reviews 2026-11-03 and 2026-11-04 | — | see `VISUAL_REVIEW.md` | rejected (standing defect; no fix accepted) |

## UV candidates / liveries

| Claim | Run | Commit | Artifact path | Status |
|---|---|---|---|---|
| car16 misc twin-hide A/B (implemented on the car-assembly branch, merged; runtime-3 in the evidence run is the twin-hide-off variant) | 41798036351 | `e8b80bb` (impl `09a3d16`/`ca7ce66`) | `outputs/repair-20261029/car-ev-41798036351/` ✓disk | machine-run only; human-pending — VISUAL_REVIEW 2026-11-04T09:10Z: do not promote on cleaner silhouette alone |
| car16 V-only UV-candidate A/B (the ten pending entries) | NONE — the leg was INERT in every run so far: `cloud_game_audit.py` wired `SOURCE_UV_CANDIDATES` but the `CarLoader` gate read `MYSIM_SURFACE_UV_CANDIDATES` (env mismatch; verified by car-assembly-turn2 against the e8b80bb blobs; fix `e41d4f4` accepts both spellings, merged) | fix `e41d4f4` + detector `80ceb22` (both merged) | — | **never run** — the "pending UV candidate NOT accepted" verdict is NOT evidence against the candidate; a run at a head with `e41d4f4`+`80ceb22` must show `SURFACE_UV_REPAIR_CANDIDATE` lines (leg live) + `MIRRORED_TEXT_RISK_SUMMARY` before any UV-candidate verdict |
| Mirrored logos/numbers root cause (UV orientation/handedness per surface) | none yet — detector `80ceb22` emits the per-surface taxonomy (`ok/mirrored_text_risk/rotated_text_risk/flipped_text_risk/mixed_risk/...`) for the next run | — | — | open — awaiting the candidate-enabled run + human review before any livery change |

## TEXTURE-INVENTORY QC (addendum, turn 3)

Applies to the texture-inventory deliverables of the carbody-livery and track-render agents (`agents/*turn*` files) when they land. Every inventory ROW claiming a texture-level finding must carry ALL SIX fields below; the QC flags every empty/hand-waved field. Until an inventory passes this QC, none of its rows may be cited as acceptance evidence.

Required per-row schema:

| # | Field | Valid form | Hand-waved (flag these) |
|---|---|---|---|
| 1 | atlas GUID + file | BOTH the source GUID (as resolvable in `guid2path.json`/`car_map.json`) AND the resolved file path under `assets/` | file name only (`ext_16.png`), "the livery atlas", "misc atlas", no GUID |
| 2 | dimensions | WxH pixels of the actual imported texture (e.g. `512x512`; the `512²` shorthand is accepted) | blank, `various`, `see atlas` |
| 3 | UV channel | which channel the finding applies to: `UV0` / `UV2` / `none-bound`, plus whether the material uses the UV-flip policy | unqualified `UV`, blank |
| 4 | orientation class | controlled set: `as-authored` / `mirrored-X` / `rotated-90` / `transposed` / `mixed-per-island` / `indeterminate (+why)` | prose ("looks flipped"), blank |
| 5 | defect taxonomy value | controlled set: `black-card` / `alpha-bleed-dark-RGB` / `mid-alpha-scissor-loss` / `sub-pixel-coverage-collapse` / `mirrored-UV` / `wrong-scale` / `compression-artifact` / `low-res-source` / `none` | adjectives ("ugly", "broken", "bad"), blank |
| 6 | correction family | controlled set: `none-needed` / `mip-rebuild` / `uv-remap-per-island` / `uv-remap-per-surface` / `threshold-change` / `material-rebinding` / `geometry-fix` / `source-art-fix` | `TBD`, "fix later", blank |

Cross-checks the QC performs when an inventory lands: (1) every GUID resolves to the stated file and vice versa; (2) stated dimensions match the audit/manifest metadata where available; (3) rows classed `as-authored` must state the UV-flip-policy exception (per track-render-report §1.1, `uv_scale=(-1,-1)`/`offset=(1,1)` applies to non-`org_`/non-crowd names); (4) every row with a defect value != `none` names exactly one correction family (or `none-needed` with a reason); (5) every row cites the run/commit whose evidence produced it — uncited rows are flagged; (6) no row may contradict a REJECTED entry above without a new run ID.

QC status (pre-checks against already-landed material; formal inventories not yet delivered):

| Inventory | Status | Pre-QC finding |
|---|---|---|
| carbody-livery inventory | NOT DELIVERED | QC pending. Note: the related car-assembly surface inventory has the same gaps as below |
| track-render inventory (formal turn deliverable) | NOT DELIVERED | Pre-QC of the existing `track-render-report.md`: §1.2 texture table has texture file + dimensions (`512²` form) and excellent measured alpha/RGB/keep-fraction data, but NO GUID, NO per-row UV channel, NO orientation class, NO defect-taxonomy values (prose only: "pure-black RGB in transparent texels" → `alpha-bleed-dark-RGB`; "alpha-binary ... white speckle" → `mid-alpha-scissor-loss`; mip-collapse analysis → `sub-pixel-coverage-collapse`), and correction family stated per-FAMILY only (§2: mip-rebuild for the 3 fence textures). The measurements are reuseable; the rows must be upgraded to the six-field schema when the formal inventory lands |
| car-assembly surface inventory (`car-assembly-turn2.md` §1) | LANDED (partial scope: car16 text-bearing surfaces) | Has texture file (+2 GUIDs cited: `wheel.png` 78fcd2c9, tyre 8fe5c99) and per-row UV-transform state (V-only/candidates/default); MISSING: dimensions (none), UV channel per row (the detector reads UV0 but rows do not state it), orientation class per row (H1-H5 are mechanisms, not classes — note the detector itself emits a valid per-surface taxonomy: `ok/mirrored_text_risk/rotated_text_risk/flipped_text_risk/mixed_risk/no_texture/no_uv/no_geometry/degenerate_uv`, which can satisfy field 5 for its own output rows), correction family per row (H1-H5 imply fix shapes; not stated per row) |

## Scope and honesty notes

- This index covers ONLY the claim families listed in the coordinator's tasking (persist, lap, HUD bounds, wheel closeups, cockpit, fences, UV). The full 15-stage scope (all cars/tracks, weather/night, racing AI, pits, menus, championship, LAN, audio, performance, asset acceptance, final Windows e2e) has far more open claims — see `outputs/repair-20261029/TASK_BOARD.json` and `CONTINUE.md`.
- "machine-verified" reflects the named run's automated gates only. No entry in this table constitutes visual acceptance, motion acceptance, or stage-10/15 closure.
- Zero-assertion passes, self-comparisons and content-free greens are excluded by construction (see the fixture's contract rules and the packet's history).
- Update procedure: re-verify run states with `gh run view --json`, re-check artifact paths on disk, add the row, never delete rejected/failed rows.
