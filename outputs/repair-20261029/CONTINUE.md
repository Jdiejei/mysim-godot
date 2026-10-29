# Continue MySim cloud rebuild — 2026-10-29

## Start here / owner instructions
- Project: C:/Users/shash/Documents/MySimGodot. Read PROJECT_HANDOFF.md, HANDOFF.md and outputs/repair-20261029/REPAIR_REPORT.md for history; this continuation supersedes their local-compute preference.
- Owner now authorizes rewriting the whole game and importing/reviewing the entire processed mesh catalogue. All further processing, rendering, simulation and builds must run on GitHub. Local work is editing, reading/manual evidence review and necessary encrypted input transport.
- Conserve tokens; no routine progress chatter or questions. Answer direct questions. Never call a pilot, catalogue or automated pass the finished game.
- Preserve originals at D:/MySim-recovered, D:/My Sim, D:/MySim-godot-assets, original UV/collision references and failure history. No paid subscriptions/runners. Do not expose decrypted assets or keys.
- User reported terminal popups. ALL local subprocess.run calls must specify CREATE_NO_WINDOW. The Python service uses pythonw.exe and Win32_Process.Create with ShowWindow=0. Never close other user/app windows. Do not use a global Get-Process MainWindowTitle scan: it can block on unrelated windows.

## Adopt, do not duplicate
- Single-instance input transfer: tools/cloud_transfer_service.py, invoking tools/cloud_transport.py. Read cloud-transport.pid, confirm exact CIM process command line. The latest restart used PID 3596; always use the current PID file, not this historical number.
- Independent cloud game watcher: tools/cloud_game_watch.py. Read cloud-game-watch.pid and exact CIM line. Latest restart used PID 3944. Each service has a D: file lock preventing duplicates.
- State: outputs/repair-20261029/cloud-intake-state.json and cloud-game-state.json.
- Full transport plan: D:/MySim-transport/repair-20261029/plan.json. Do not modify it while its writer is running. It preserves upload attempts and job history.
- Logs: cloud-transfer-service.log, cloud-game-watch.log. Earlier cloud-transport*.log/err record superseded non-detached processes; preserve them.
- Services can survive a turn through WMI parentage. Verify they are alive. If absent, inspect failure logs before restarting. Never start another live copy.
- A three-hour continuation heartbeat was created: continue-mysim-cloud-rebuild-and-visual-review. Keep it active until whole-game completion and real manual coverage; silence routine notifications.

## GitHub and compute
- Repo: instagramshashank8-bot/mysim-godot (already public by prior owner decision). Branch codex/repair-20261029 is pushed to main by fast-forward. No raw assets are committed.
- Authenticate a subprocess with gh auth token -u instagramshashank8-bot captured privately as GH_TOKEN; never print tokens. Do not switch the user's active account globally.
- repair-verification 40901218060 passed complete core/lap/contact checks. 40907166421 passed the later compile/contact scope after wheel/PBR changes. Fetch exact head revisions before attributing results.
- Current cloud workflows: repair.yml; mesh-review.yml (mesh/texture intake); game-review.yml (6 shards of all tracks + 24 cars x 3 views and real race flow); windows-build.yml (export + native player smoke checks).
- Cloud game watcher waits for ALL NINE source asset parts uploaded before dispatching game-review and windows-build. Do not dispatch early or duplicate existing runs. Native Windows smoke is headless logic, not visual acceptance; Linux renders use software Vulkan/Xvfb and need visual inspection.

## Full mesh and texture intake
- Manifest D:/MySim-processed/final/manifest/selected_candidates.csv: 98,553 selected candidates, 9,226 archives, 150.35 GiB. 19 originally missing from 98,572 requested. Every quality_accepted is False; 48,703 rows have NEEDS_REPAIR archive labels.
- Transport plan: 307 mesh parts + 9 source asset parts (316 total). Assets are prioritized after the first mesh part. Current assets are 17,413 files / 397 FBXs / 4.166 GiB, not the full recovered D: tree.
- Encrypted inputs: release repair-inputs-20261029; encrypted outputs: repair-review-20261029. Key is stored outside Git at D:/MySim-transport/repair-20261029/intake.key and as the Actions secret MYSIM_INTAKE_KEY. Never display it. Keep prereleases distinct from finished game releases.
- Every converted .blend is SHA-verified. The files are Blender 4.5 LIBRARIES, not scenes. Ubuntu apt Blender is too old and loads empty scenes. Fixed cloud workflow downloads Blender 4.5.3 and links the chosen datablock into the active scene before GLB export.
- Original failed conversion runs are preserved. Named lookup failed because objects have __program_N suffixes; use explicit provenance, unique manifest topology, or a single SHA-verified object. Never silently choose an arbitrary object.
- Corrected part 00000 run 40903767442: 362 requested, 362 converted, zero intake failures; manual_reviewed=0 and accepted=0. Earlier successful-looking jobs converted zero: do NOT count those as imported. The service upgrades known old-loader revisions and retries a distinct failure once.
- GLBs are staged candidates. They are not integrated into complete cars/tracks yet. Applying them must preserve source transforms, unit scale, materials, UVs, collision and assembly roles. Some car-category rows are visibly fences/buildings: repair categorization and never spawn every component as a car.
- Release objects sheets-<part>.enc contain small labelled review sheets; review-<part>.enc includes full conversion data, GLBs and images. tools/cloud_decrypt.py authenticates and extracts. Local decryption is evidence reading, not new rendering.

## Real manual coverage, not generated-sheet counts
- MANUAL_REVIEW_LEDGER.json and MANUAL_REVIEW_NOTES.md are authoritative.
- Inspected texture thumbnails 0–399 in assets-part-00000 sheets 0000–0003. There are 4,263 textures in this FIRST part alone.
- Inspected mesh-part-00000 sheets 0000–0005 (600 views = first 150 meshes); ledger was initially first 75 and needs the additional 75 saved before extending. No final quality acceptance.
- Continue mesh sheets 0006 onward and texture sheets 0004 onward. Record exact IDs/paths/tile ranges and findings after each viewed batch. Full-resolution, assembled-scene, UV/physics and animation checks remain separate. Never mark unseen objects manually reviewed.
- Main findings: literal AAA/BBB/CCC branding placeholders; directional sign/font/foliage atlases must be judged with original UV transforms; black foliage rectangles/backfaces require alpha/sidedness checks; many candidate previews are isolated source components with clipped/overexposed details, not polished new complete assets.

## Engineering/build evidence
- Local engineering checks and recording were completed BEFORE owner switched to cloud-only. See REPAIR_REPORT.md. Local candidate D:/MySim-builds/repair-20261029/MySim.exe is older than subsequent hub-centering/PBR changes. Do not present it as the final current game.
- Source fixes include sustained reverse, oriented mass-dependent contacts, initial-overlap wall recovery, wheel-template visibility, hub centering/radius spin, loading feedback/threaded requests, progress-based standings, matched-class rivals, source PBR scalar/color and untextured glass recovery, and respecting crash spacing.
- Latest source must get cloud geometry/render/Windows checks after input readiness. Inspect actual images and recordings. Original 1,440 lap successes do not prove current full mesh contacts, user hardware, full art quality or all game components.
- Required remaining work includes complete candidate review/integration, all track/car/weather/camera/render acceptance, long-session stability, suspension/contact fidelity, production car and pit art, real controller/audio/mobile checks and the still-unbuilt components listed in HANDOFF.md.

## Immediate next steps
1. Verify hidden transfer and watcher are alive; reconcile uploads and cloud runs, and repair concrete failures without duplicating jobs or deleting history.
2. Commit the console-suppression/service/watcher/continuation changes still pending locally; push to the existing authorized repo. Avoid staging unrelated pre-existing modifications such as shots/log.txt or massive asset folders.
3. Continue labelled manual review, persist exact coverage, inspect incoming cloud game diagnostics and fix actual bugs/art defects. Follow the whole game through to verification; keep the continuation active.
