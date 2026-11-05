# physics-gameplay — LAN bounded plan + archetype prep continuation

## Task

(1) LAN bounded implementation plan (spec only): `agents/lan-plan.md` — ENet scope, discovery, lobby, sync model, minimal viable race, migration risks, phase plan. (2) Continue the archetype-table prep (c526235 stands).

## Deliverable 1 — LAN plan (`agents/lan-plan.md`)

Grounded in what EXISTS (verified this session): the full logic-layer stack is already built and unit-tested — `IGameChannel`, `LanChannel` (UDP 44880 + broadcast DISCOVER/OFFER), `Wire` v MSR5 (host-authoritative `STATE` ~15 Hz with full car state incl. body pose/damage/pit, token-authed seq-guarded `INPUT`, HELLO/ACCEPT/BUSY/BYE/RESET_ACK), `GameSession` (lobby via countdown 9999, disconnect → AI takeover, input-silence safety), and the Godot lobby UI `LanRoomPanel.cs` which deliberately stops at CONNECTED. The missing piece is exactly the flagged gate: the race-scene integration.

Key positions the plan takes (decisions for the coordinator):
- The sync model is ALREADY DECIDED by the shipped protocol: host-authoritative state replication + client input replication; no client physics; everything gameplay-relevant is state-replicated, only controls are input-replicated.
- Transport: ship phases 1-2 on the tested `LanChannel`; an ENet-backed `IGameChannel` is a phase-3 swap only if real-LAN testing shows defects (the protocol semantics are the tested asset; `IGameChannel` makes ENet a drop-in). Rewriting the protocol onto Godot RPCs would discard test coverage — advised against.
- The central engineering risk named: `Wire.ReadSnapshot` builds a NEW `Race` per packet — the new `RaceSessionBridge` must apply state into PERSISTENT scene objects (car-id → node map), never rebuild the scene per packet.
- Other risks tabulated: single-player regression (bridge exists only when a session exists; Offline paths untouched), input-path divergence (LAN must feed the SAME human chain the userpath work validated), race-start semantics (LAN start routes through the same 5 s countdown so card §9 stays valid), version skew (strict Wire.Version), firewall/port note.
- Phases: 0 spec (this doc) → 1 minimal sync (bridge + 2 cars moving, exit = two instances driving on one LAN, suites green) → 2 race flow (start handshake, results, teardown/rematch, disconnect→AI) → 3 hardening (ENet if needed, interpolation if >100 ms, LAN race-start profile, acceptance checklist as a battery item). Mechanical acceptance checklist included. LAN stays flagged unstaffed until phase 1 has a named dev and the plan is signed.

## Deliverable 2 — archetype prep continuation

- Reading card **§12.1**: the measurement protocol that fills §12's numeric column — mechanical cluster extraction from serialized samples (maximal nonSurfaceContact runs → per-cluster archetype counts/peak Δv/duration + §6 cross-check), the qualification rules (completed terminal runs; source/jolt label agreement required BEFORE a run contributes numbers), the descriptive-then-promoted adoption criteria (envelopes never gate until the coordinator promotes with source runs named; contradiction = class-S, not a silent re-baseline), and a ready-to-paste fill-in template for the first artifacts.
- Wall judgement sheet: added readings 4/6 — R-window evidence folded into the parity rule; archetype labelling added to the source-pair section (label disagreement = divergence finding feeding the backend decision).

No code, no physics constants, no dispatches. c526235 unchanged.

## Verification

- Plan claims about existing code verified by reading `src/Logic/Network.cs` (175 lines, full), `src/LanRoomPanel.cs` (full), and the CoreChecks network/pit test lines this session.
- No build changes this turn (docs only) — the last verified build state stands (88112 assertions, 0 FAIL, from c526235's turn).

## Commit

Named-files commit on `mistral/physics-gameplay`: `outputs/mistral-orchestration/agents/lan-plan.md`, `outputs/mistral-orchestration/agents/physics-gameplay-reading-card.md`, `outputs/mistral-orchestration/agents/physics-gameplay-wall-battery-judgement.md`, this report.
