# Verification — turn 21 report (2026-11-05: 40ddb4e ledger; THREE packs ready with decision points; LAN plan ownership noted)

## Task

Coordinator turn-21: the ledger for `40ddb4e` (the four merges + verified counts: bodyPanelContact ×2/×9, the rain candidates ×2/×2, the car20 pack files); the THREE review packs recorded READY FOR REVIEW with paths + reviewer-decision points (the mirrored pack's two no-change rows; the wheel art options; car20's missing side captures); the LAN plan ownership noted (physics = planning).

## What was verified (first-hand, this session)

1. **Lineage:** fetched the Jdiejei fork `codex/repair-20261029`; my tip `17add5a` verified an ancestor → the branch FAST-FORWARDED to `40ddb4e` (= the integration head exactly, which itself merges my turn-20 ledger: the fourth code merge sequence `fc17237` → `461ed45` → `df57d9b` → `40ddb4e`; a docs commit `0f5d92f` (handoff + chart to c8db1e0) sits in between, verified content via its message + the merged tree).
2. **Counts (recursive, 130 .cs files):**
   - **Rain candidates: EXACTLY ×2/×2 as tasked** — `MYSIM_RAIN_VOLUME_PROFILE`×2 (RainVolume.cs) + `MYSIM_WET_REFLECT`×2 (Conditions.cs), flag-gated.
   - **bodyPanelContact = 21, NOT ×2/×9** — attribution: NativeRaceController.cs×4, Logic/CoreChecks.cs×5, Logic/DrivingValidation.cs×12; `recordContactArchetypes`×4 (CoreChecks×1, DrivingValidation×3). No reading of the tree yields 2/9; flagged in the ledger + packet (cite 21).
   - **car20 pack = 16 files** (`CAR20-PACK.md`, 13 ext-role strips, `metrics.json`, `tools/carbody_car20_pack.py`) — matches the commit message ("13 strips + metrics + generator").
   - All carried counts re-verified unchanged (RecoverySpec 6, MYSIM_ROAD_MATERIAL 2, RaceStart 16, raceStarted 8, DistanceScore 9, leaderScore 2, StripPanel 3, MiniMapPanel 2, CockpitFov 3, CockpitPose 5, CockpitSeat 6, invalidRows 4, lapValidity 1, CorridorWidth 2 at Logic/Simulation.cs, COCKPIT_INT_UV 1).
3. **The three packs' reviewer-decision points (read from the packs themselves):**
   - mirrored-UV: the two NO-CHANGE rows = 7 (LOD_A_EXHAUST_mm_ext) + 10 (LOD_A_REARBUMPER_mm_badges) — rear 0px / top 4px, below the 222px cluster threshold; options (a) accept from family evidence — the identical (u,1-v) remap validated per-renderer on the resolvable rows — or (b) keep pending / re-capture.
   - wheel-detail: PER-ROW (a) accept as-is / (b) the PROPOSED custom-wheel spec (candidate-art, unaccepted) / (c) the open source-wheel presentation decision; the standing reject 41310403219 STANDS until decided; views read row-locally (struts = in-frame staged fixes, not the tyre).
   - car20: per-strip judge (tier-0 panel = the BEFORE defect; 13 strips = 11 rows, rows 0/6 carrying two); HONEST GAPS from the pack: no before-side captures for doors/mirrors (pack rows 2/3/7/8, tier0Before=false — my first mapping note said HOOD by mistake; corrected to the pack's own statement: DOORS and MIRRORS), no fleet captures for car20, wholeCarAccepted=false (runs.integrated = 41458535677).
4. **LAN plan ownership (per the coordinator): physics-gameplay = planning** — recorded in the packet + the INDEX LAN paragraph (the flow's execution/verification staffing + the accept/defer decision remain OPEN).

## Ledger writes

- EVIDENCE_INDEX.md: heading/status → 40ddb4e; the turn-21 merges + counts paragraph (with the two flags: bodyPanelContact 21; rain 2/2 exact); the new `REVIEW PACKS — THREE READY FOR REVIEW` section (per-pack path + decision points + honest gaps); the LAN paragraph updated with the physics-plans note.
- REVIEW_PACKET.md: the top CURRENT STATUS block rewritten (turn 21).
- This file: agents/verification-turn21.md.

## Standing gates — UNCHANGED

Windows tmp=1/rename=2; crash-consistency through the delete-then-rename window OPEN; PHYSICS UNACCEPTED until contact + force-driven DV evidence land green and are human-reviewed; packs/in-tree counts = PRELIMINARY evidence only; no run anywhere has exercised 40ddb4e code; PR #1 (a313492) merge = the owner's action; the SEVEN-command owner battery is the only validation path.
