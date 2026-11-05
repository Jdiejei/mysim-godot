# physics-gameplay — LAN host-side in-race integration (the last piece before two-instance testing)

## Task

The remote-driven car in the simulated race: the host's NativeRaceController runs the real race; the session adopts it; the remote car is driven by the received INPUT stream and rendered from the simulation. Same discipline: my files direct, Main.cs as a patch proposal.

## Design (grounded in the existing machinery)

- The session ADOPTS the live race: after the race scene is built, `GameSession.AdoptRace(drive.Race)` re-points the session's `race` at the SIMULATED race — the existing INPUT handler (`race.cars[1].input=input`), the 15 Hz STATE broadcast, the input-silence brake, and the disconnect→AI-takeover all then operate on the live race with ZERO protocol changes.
- The remote car is a FULL race citizen built at construction: `NativeRaceController._Ready` consumes a static `PendingLanHost` (set just before the scene builds), adds the remote car from the retained peer identity (`PeerName`/`PeerModel` from HELLO), and forces `rivals=0` (the remote car replaces AI traffic; the experimental-traffic gate is never triggered). Its simulation is the LOGIC path (`Race.Advance` → `Vehicle.Step`), exactly like the AI cars — human=true blocks the autopilot override, so the remote input drives it; on disconnect the existing handler flips human=false and the autopilot takes over seamlessly.
- The remote car's visual: ONE persistent node in NativeRaceController (created on first sight, rebuilt only if the model changed, posed per tick from the logic state — the same 1:1 X/Z mapping; Y on the ground plane for logic-simulated cars, body-pose height/quaternion when present).
- Session tick: `LanSession.Tick(delta)` in `_PhysicsProcess` BEFORE `Race.Advance` — a just-received remote input drives the same tick's simulation.
- Timing hazard solved: the host's track load (~2-5 s) would starve the client's 5 s silence window. The hosting panel now LINGERS after START (it keeps ticking the session) until `RaceAdopted` flips, then closes without disposing; the drive takes over ticking from the adopted state. The CLIENT's own loading gap is safe by construction (its silence timer only advances inside Tick, which is paused while nothing ticks it; the buffered STATE backlog drains on the first ticks).

## What changed

1. **`src/Logic/Network.cs`** (mine): `PeerName`/`PeerModel` retained from HELLO; `RaceAdopted` flag; `AdoptRace(Race)` (host-only, rejects a race without the remote car; throws otherwise — fail-closed).
2. **`src/NativeRaceController.cs`** (mine): `PendingLanHost` static + `LanSession` + the remote-visual fields; the `_Ready` LAN block (remote car from the peer identity, rivals=0, `LAN_REMOTE_CAR` log); the session tick + the remote visual sync in `_PhysicsProcess`; `_ExitTree` disposes the session (BYE on leaving). Null in every non-LAN flow: single-player, CI, and the DV paths never set the static, so all existing behaviour is untouched.
3. **`src/Main.cs`** (coordinator-owned — PATCH PROPOSAL, two additive hunks): `StartLanRace`'s host branch (static handoff + the normal `StartRace` with the lobby's track/laps and the local car from Settings) and the post-`RACE_READY` adoption hook (hands the session to the drive, adopts the live race, `LAN_HOST_ADOPTED` log). Nothing else in the file touched; the single-player paths untouched (verified by the same argument as the adapters turn: the only new code paths require a live LAN session).
4. **`src/LanRoomPanel.cs`**: the host START button now fires `LaunchLan` and the panel lingers until `RaceAdopted` (the timing fix above); the client handoff unchanged.
5. **`src/Logic/CoreChecks.cs`**: +4 assertions — the peer identity retained from HELLO; AdoptRace re-points and flags; a client session can NEVER adopt; a race without the remote car is rejected.

## Verification

- Clean build: 0 warnings, 0 errors (two compile issues caught and fixed en route: the field-name case mismatch peerName/PeerName, and the `Contact` type living in the `MySim` namespace — the vector construction is now inline).
- `dotnet run --project tools/CoreRunner`: `PASS 88130 assertions` (+4), 0 FAIL.
- NOT verified (needs the two-instance test, owner-side): the actual host+client race. The full two-instance checklist from the LAN plan §6 is now UNBLOCKED: host creates → client joins (car picker) → host START → BOTH sides enter the race scene — the host driving its own car with the remote car visible and remote-driven; the client watching the live race through snapshots; ESC/disconnect paths per the checklist.

## Remaining known limits (deliberate, stated)

- The remote car renders on the logic ground plane (y=0) — logic-simulated cars carry no suspension pose; a ground-seeded Y (like DriveController's traffic rendering) is the first polish item if the two-instance test shows it sitting wrong.
- The client's spectating view has no HUD beyond the status line; results/rematch UI on the client = the next step after the two-instance test validates the sync.
- The host's own car choice comes from Settings (the lobby has no host car picker) — cosmetic gap, noted in the plan.

## Commit

`src/Logic/Network.cs`, `src/NativeRaceController.cs`, `src/Main.cs` (proposal), `src/LanRoomPanel.cs`, `src/Logic/CoreChecks.cs`, this report — on `mistral/physics-gameplay`.
