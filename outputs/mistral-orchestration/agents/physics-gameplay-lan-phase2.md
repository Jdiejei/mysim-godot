# physics-gameplay — LAN phase-2 (adopted defaults): START handshake, sync-bridge logic half, car picker

## Task

Implement the LAN phase-2 items per my adopted plan (conservative defaults): (1) the explicit `START|token` message in the Wire protocol (host sends on race start; client gates its race entry on it), (2) the `RaceSessionBridge` applying incoming STATE snapshots into PERSISTENT scene objects (never rebuild Race), (3) the join-UI car picker (HELLO `localModel`). Flag-gated/hard-off in single-player. Small verified steps; the LAN acceptance checklist becomes a battery item only after two-instance local testing is possible (owner-side).

## What changed

1. **`src/Logic/Network.cs`** (mine)
   - `Wire.Version` bumped `MSR5` → `MSR6` (the message set changed; nothing deployed depends on the old string — verified the literal existed only in this constant).
   - `GameSession.StartRace()`: host-only, requires a connected session and an unstarted race; arms the REAL 5 s countdown (the lobby's 9999 hold ends → the race-start semantics stay the single-player ones, per the plan's race-start risk row) and sends `START|<sessionToken>`. Sets `startRequested`.
   - Client handling: `START|<token>` accepted only when connected, correctly sized, and carrying the EXACT session token → `startReceived=true` (the gate the client's race-scene entry will use). Wrong token, pre-handshake, or host-received STARTs are ignored (fall-through, consistent with the protocol's reject-quietly pattern for cross-role messages).
2. **`src/Logic/RaceSessionSync.cs`** (NEW, mine) — the logic half of the bridge, pure logic (no Godot types, CoreRunner-testable):
   - `ICarView` (`EnsureCreated(carModel, driver)` / `ApplyPose(...)` incl. the body-pose quaternion fields / `ApplyState(...)`), `IRaceView` (`CreateCarView(id)` / `ApplyRaceState(...)`).
   - `SnapshotApplier.Apply(Race)`: the central-risk mitigation as code — a dictionary keyed by car id creates each view EXACTLY ONCE and updates it in place on every later snapshot (the parsed `Race` is disposable per packet; the views are not). Counters (`AppliedSnapshots`, `CarViews`, `SawRaceState`) for tests/diagnostics.
3. **`src/LanRoomPanel.cs`** (assigned by this task)
   - Join side: a car picker (`CarCatalog.Names`, persisted default via `Settings.Get("car",0)`) wired into `session.localModel` before `Join` — the HELLO already carries it and the host's ACCEPT path already applies it.
   - Host side: a START RACE button, disabled until connected, calling `StartRace()` (error text surfaces the exception message).
   - Status text: `startReceived` shows "Race starting (race-scene launch pending)"; the availability note updated to match reality (handshake + bridge in; scene launch pending). Single-player untouched: the panel only exists on the LAN menu path; no Offline code path changed.
4. **`src/Logic/CoreChecks.cs`** — 13 new assertions:
   - START handshake (through a recording fake channel): valid HELLO accepted; a stale-protocol (`MSR5`) HELLO refused (version gate strict); START carries the session token; the 5 s countdown armed + `startRequested`; the host never sets `startReceived`; client ignores pre-handshake and wrong-token STARTs; gates on its exact token.
   - SnapshotApplier through the REAL client pipeline (`Wire.Snapshot` → `ReadSnapshot` → `Apply`): one view per car across repeated snapshots, in-place updates (pose applied per snapshot, never recreated), a later-added car appends exactly one view, counters consistent.

## Verification

- Clean rebuild (`obj` removed): 0 warnings, 0 errors.
- `dotnet run --project tools/CoreRunner` on the clean build: `PASS 88125 assertions` (was 88112; +13), 0 FAIL, exit 0.
- NOT verified (out of scope this turn, by design): any two-instance LAN behaviour, the Godot scene adapters, the actual race-scene transition. The protocol and applier are logic-verified only; no acceptance claim.

## Next small step (named for the coordinator)

The race-scene adapters: implement `IRaceView`/`ICarView` over Godot nodes + the lobby→race transition. This touches `Main.cs`/the scene setup (coordinator-owned surface) — I propose the diff when asked, or take the task if the surface is granted. After that: two-instance local testing becomes possible → the owner-side acceptance checklist (plan §6) becomes a battery item.

## Risks / open

- `START` is sent unreliably (UDP, like everything else): a lost START leaves the client waiting — the client's existing 5 s silence timeout shows "No host response"; the host UI shows the started state. A retry (re-send START while `startRequested && !ack`) is a cheap phase-3 hardening if two-instance testing shows丢失 — flagged, not built (no speculative machinery).
- `EnsureCreated` records the FIRST carModel per view; a mid-race model change would need view rebuild semantics — out of scope (car models are fixed per session in practice).
- The client's `race` object is still replaced per snapshot inside `GameSession` (unchanged semantics); the applier decouples the VIEW layer from that churn — the scene adapters must hold no per-Race references.

## Commit

Named-files commit on `mistral/physics-gameplay`: `src/Logic/Network.cs`, `src/Logic/RaceSessionSync.cs`, `src/LanRoomPanel.cs`, `src/Logic/CoreChecks.cs`, `outputs/mistral-orchestration/agents/lan-plan.md`, this report.
