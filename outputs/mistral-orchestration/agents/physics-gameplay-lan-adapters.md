# physics-gameplay — LAN phase-2 completion: race-scene adapters + lobby→race transition (Main.cs patch PROPOSAL)

## Task (the granted scope)

The Main.cs race-scene adapters: implement `IRaceView`/`ICarView` over the Godot nodes + the lobby-to-race transition ONLY. Main.cs = coordinator-owned; this commit is the PATCH PROPOSAL for merge review. The adapter must keep single-player byte-identical (the bridge exists only when a session exists).

## What changed

1. **`src/LanRaceView.cs`** (NEW, mine) — the Godot half of the bridge:
   - `LanCarView : Node3D, ICarView` — ONE persistent car visual per car id: `EnsureCreated` loads the car model once (`CarLoader.Load` + `ApplyExact`; the AUDITCARS path proves standalone loading works without pre-cache), `ApplyPose` positions in place (X/Z 1:1 — the same mapping as `Contact.ToWorld`, confirmed against `NativeRaceController.ReadBody` where the body's world X/Z IS the logic position; Y from the body-pose height when the host simulates natively, else ground), full body-pose quaternion when present (norm-guarded), yaw otherwise. `ApplyState` records damage/fuel/laps/pit/servicing for the next step's HUD.
   - `LanRaceView : IRaceView` — creates car views under the world node; carries clock/countdown/started/track/laps for the readout.
   - `LanClientRunner : Node` — the client loop: `session.Tick(dt)` → apply on a NEW sequence number only (`Session.lastSequence` guard) → chase camera over the LOCAL car (`applier.ViewFor(session.localId)`, the new one-line accessor on `SnapshotApplier`) → minimal status line (GRID/RACE + clock + countdown) → ESC = dispose + back to menu. MVP limits per the plan: pure snapshot rendering, no local physics, no prediction.
2. **`src/Main.cs`** (coordinator-owned — PATCH PROPOSAL, two changes, both additive):
   - `ShowMenu`'s `CreateRoom`/`JoinRoom` lambdas now wire `panel.LaunchLan = StartLanRace` (the single-player menu items untouched — `QuickRace`/`Series`/`ResumeSeries` paths byte-identical).
   - New `StartLanRace(GameSession session)`: builds the track world for the session's track variant and adds `LanClientRunner`. The track-loading block is DELIBERATELY DUPLICATED from `StartRace` (documented in the method comment) — zero edits inside `StartRace` itself, so the single-player loading path is untouched byte-for-byte; the duplication unifies only when the LAN race flow merges with the single-player one. Error path mirrors `StartRace`'s (message + BACK TO MENU).
3. **`src/LanRoomPanel.cs`**: `LaunchLan` callback; on `startReceived` the panel transfers the LIVE session (never disposed — nulled first, then `QueueFree`, so `_ExitTree` can't kill it) to `StartLanRace`. The HOST does not transition yet: its lobby keeps ticking and serving STATE (the client sees the grid posed from real network snapshots; movement begins when the host's in-race integration lands — the declared NEXT step, which touches `NativeRaceController` (my file) for the remote-driven car).
4. **`src/Logic/RaceSessionSync.cs`**: `ViewFor(id)` accessor (+ unit check).
5. **`src/Logic/CoreChecks.cs`**: +1 assertion (ViewFor returns the persistent view / null for unknown).

## Single-player byte-identity argument

- No line inside `StartRace`, `_Process`, `ShowResults`, `ShowRaceSetup`, or the boot/_Ready paths changed.
- The only Main.cs edits are the two LAN menu lambdas (previously one-line `AddChild(new LanRoomPanel{...})`) and the NEW `StartLanRace` method — unreachable unless a LAN panel invokes `LaunchLan`, which only happens on `startReceived` (a LAN-only state).
- `LanClientRunner`/`LanCarView`/`LanRaceView` are constructed only by `StartLanRace`.

## Verification

- Clean build: 0 warnings, 0 errors. The build caught a REAL integration miss mid-flight: the `bodyPanelContact` local in `BuildValidationSample` was never written into the returned `Sample` (CS0219) — fixed; the archetype metrics would have read zero on real runs. Both fixes verified in the clean build.
- `dotnet run --project tools/CoreRunner`: `PASS 88126 assertions`, 0 FAIL.
- NOT verified (no local engine runs permitted): the Godot-side adapters and the transition are compile-verified only. First two-instance local test (owner-side, per the plan): host creates room → client joins (car picker → HELLO model) → host START → client lands in the track scene with both cars posed from real snapshots (static until the host integration lands) → ESC both sides clean.

## Merge-review notes for the coordinator

- The only Main.cs hunks: the `modes=` line in `ShowMenu` + the `StartLanRace` method block. Nothing else in the file is touched.
- The deliberate `StartRace` duplication is called out in both the code comment and here — it is the price of byte-identity this step; unification is a later, reviewed change.
- Open follow-ups (next steps, not in this patch): the host's in-race integration (remote-driven car in the simulated race — `NativeRaceController`, mine), LAN results/rematch UI, the START re-send hardening flagged in the phase-2 report.

## Commit

`src/LanRaceView.cs`, `src/Main.cs` (proposal), `src/LanRoomPanel.cs`, `src/Logic/RaceSessionSync.cs`, `src/Logic/CoreChecks.cs`, this report — on `mistral/physics-gameplay`.
