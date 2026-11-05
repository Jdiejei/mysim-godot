# physics-gameplay — two-instance LAN test: script + automation hooks

## Task

Prepare the two-instance test for the owner: the exact two-process commands, what each console must show, the PASS criteria from the LAN plan §6 — as an executable doc + the reading-card section.

## What was delivered

1. **`agents/lan-two-instance-test.md`** — the executable script: the two PowerShell commands (host: `MYSIM_LAN_HOST=1 MYSIM_LAN_AUTOSTART=1 MYSIM_AUTODRIVE=1 MYSIM_LAN_QUIT_AFTER=45`; client: `MYSIM_LAN_JOIN=127.0.0.1 MYSIM_LAN_QUIT_AFTER=45`), the expected timeline table (room open → join → auto-start → both race scenes → the AutoDrive motion → the sync proof → auto-exit at 45 s), the six mechanical PASS criteria, the failure-fingerprint table with owners, and the variants (interactive drive for the feel check, two-machine LAN with the firewall note, the disconnect leg).
2. **The automation that makes it a two-command script** (the test was otherwise click-driven):
   - `Main.cs` (proposal hunk #3): the `_Ready` tail opens the LAN panel on `MYSIM_LAN_HOST`/`MYSIM_LAN_JOIN`, guarded to NOT fire when `MYSIM_TRACK` is set (CI runs can never grow a LAN panel).
   - `LanRoomPanel.cs`: `AutoJoinAddress` (pre-fill + auto-join), `MYSIM_LAN_AUTOSTART=1` auto-creates the room and fires START RACE 2 s after a client connects (via the real button signal, so the handler path is the production one).
   - `LanClientRunner`: the `LAN_SYNC` evidence line every 2 s (phase/host position+speed), the `SyncObserved` criterion (the HOST's car seen above 3 m/s ON THE CLIENT = states flowing AND applied), the one-shot `PASS/FAIL LAN_CLIENT ...` summary, and the quit timer (`CleanExit` with exit code 0/1).
   - `NativeRaceController.cs`: the host-side quit timer (`MYSIM_LAN_QUIT_AFTER`, 0–600 s validated fail-closed) with the `LAN_HOST_EXIT connected= inputs= elapsed=` summary — `inputs>0` proves the client→host control channel; exit 0 requires it.
   - `ICarView.ApplyState` gained the `speed` parameter (the sync criterion's input; the applier, `LanCarView` and the test doubles updated).
3. **Reading card §13** — the per-instance required/forbidden lines table, the verdict grammar (`UNVERIFIED(config)` for environment failures), the reading notes (inputs as the reverse-direction proof, the sync marker's meaning, the GRID→RACE cadence check, the disconnect leg, the zero-`LAN_*`-lines hard-off proof).
4. **`lan-plan.md`** — status update 2: LAN is CODE-COMPLETE for the first two-instance test; the automation envs documented.

## Verification

- Clean build: 0 warnings, 0 errors (one compile catch: the test double needed the new `ApplyState` signature).
- `dotnet run --project tools/CoreRunner`: `PASS 88130 assertions`, 0 FAIL.
- NOT verified: the two-instance test itself (owner-side, unblocked by this work); the Godot-side automation paths are compile-verified only. One doc typo caught and fixed (a corrupted phrase in the test doc).

## Risks / open

- The sync threshold (3 m/s) and the 45 s window are test parameters, not product constants — tune them from the first run if the AutoDrive schedule needs more time.
- The autostart's 2 s settle is a UI-side timer; a slow machine that connects late still works (the timer runs from the panel's opening, and START fires only when `connected`).
- Two instances on ONE machine share the GPU — if the second window's render rate collapses, the evidence lines remain valid (they are logic-side), but the visual check should use the two-machine variant.

## Commit

`src/LanRaceView.cs`, `src/LanRoomPanel.cs`, `src/Main.cs` (proposal), `src/NativeRaceController.cs`, `src/Logic/RaceSessionSync.cs`, `src/Logic/CoreChecks.cs`, `outputs/mistral-orchestration/agents/lan-two-instance-test.md`, `outputs/mistral-orchestration/agents/lan-plan.md`, this report — on `mistral/physics-gameplay`.
