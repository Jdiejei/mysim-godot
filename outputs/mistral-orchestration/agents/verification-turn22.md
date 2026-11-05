# Verification — turn 22 report (2026-11-05: 75bbd78 ledger, the LAN four-decision adoption recorded, the P0 section written)

## Task

Coordinator turn-22: the ledger for `75bbd78` (five merges + the LAN-plan adoption note); the REVIEW_PACKET P0 section (the three player P0s = code-complete in-tree behind flags awaiting the battery, each mapped to its battery command + review); the LAN four-decision adoption recorded (pending reviewer override).

## What was verified (first-hand, this session)

1. **Lineage:** fetched the Jdiejei fork `codex/repair-20261029`; my tip `9434517` verified an ancestor → the branch FAST-FORWARDED to `75bbd78`. The FIVE merges (merge order): `765304c` (physics 674a528: the LAN BOUNDED PLAN — `agents/lan-plan.md` + reading-card §12.1 + the wall-battery judgement sheet) → `f084de5` (track 5321796: the vehicle-lights emissive candidate, night-only, per-mesh role colours, flag-gated) → `f1bce18` (carbody 34465b6: the track0 standing-rejects review INDEX — judge-pack run 41843204651 linked, four prior fence attempts cited with runs/verdicts, the exact candidate knobs indexed incl. the honest "no literal road-material knob exists" note) → `d687399` (hud 4d37f2d: the bindings-display design doc) → `75bbd78` (my turn-21 ledger merged back in).
2. **LAN symbols verified:** `LanChannel` = 9 (LanRoomPanel.cs×5, Logic/CoreChecks.cs×2, Logic/Network.cs×2 — the transport is unit-testable in-tree per the plan: real UDP loopback in CoreChecks); the snapshot/Snapshot footprint across Logic/Network.cs + Logic/RaceSeries.cs (among a broader occurrence set — counted conservatively in the record). The four decisions read from `lan-plan.md` §2 VERBATIM: (1) LanChannel raw UDP (port 44880, broadcast; ENet NOT required by the protocol; IGameChannel keeps the changeover possible); (2) explicit reliable `START|<token>` + a start button — never infer from `STATE.countdown` leaving 9999; (3) client rendering MVP = pure snapshot rendering of both cars (≈1 RTT + snapshot interval ≈ 70-90 ms LAN; no client-side prediction; interpolation only if > 100 ms feels bad); (4) car picker on the join panel (HELLO.localModel rides the protocol; the host ACCEPT path applies it). Scope bounded (2 cars/1 track/results; non-goals listed). **The ADOPTION (coordinator, turn 22) is recorded verbatim with PENDING-REVIEWER-OVERRIDE status.**
3. **The P0 section (task 2):** written in REVIEW_PACKET from the reading card + the tree: P0-1 steering feel (the userpath harness + the human-chain DV + the race-start profile → battery commands 1+2; physics + verification + human), P0-2 contact/collision integrity (the wheelguard restore + the archetype layer §12 + the corridor fix → commands 3+4; physics + verification + human; **the §12 numeric band = TO BE FILLED from the first measured artifacts, no assumed value**), P0-3 floating/floor-ride (the spawn datum + the `native_ride_height` input variant, 35 mm EXPERIMENTAL → command 2 variant, then the re-baseline plan before any default flip; physics + verification + human). Each names its exact battery command(s) + the reviewer(s) + what the human pass contributes.
4. **Counts re-verified unchanged** (all the carried symbols, including the my-flagged figures: RecoverySpec 6, bodyPanelContact 21, rain 2/2, MYSIM_ROAD_MATERIAL 2, RaceStart 16, raceStarted 8, DistanceScore 9, CorridorWidth 2 intact at src/Logic, COCKPIT_INT_UV 1).

## Ledger writes

- EVIDENCE_INDEX.md: heading/status → 75bbd78; the five merges appended; the LAN four-decision adoption paragraph (with the in-tree footprints + the override status + the exact override paths from the plan §2/§6).
- REVIEW_PACKET.md: the top CURRENT STATUS block rewritten (turn 22) including the new `THE THREE PLAYER P0s — CODE-COMPLETE IN-TREE, AWAITING THE OWNER BATTERY` block (each P0 → its battery command(s), its reviewers, its human contribution) + the LAN adoption bullet.
- This file: agents/verification-turn22.md.

## Standing gates — UNCHANGED

Windows tmp=1/rename=2; crash-consistency through the delete-then-rename window OPEN; PHYSICS UNACCEPTED until contact + force-driven DV evidence land green and are human-reviewed; the P0s' code-complete = IN-TREE ONLY until the battery + human review; no run anywhere has exercised 75bbd78 code; PR #1 (a313492) merge = the owner's action; the SEVEN-command owner battery is the only validation path.
