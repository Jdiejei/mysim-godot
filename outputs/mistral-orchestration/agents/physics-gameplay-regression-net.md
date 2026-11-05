# physics-gameplay — the single-player regression net for the LAN/bridge work

## Task

Regression tracing per the reviewer: enumerate the single-player regression risks of the LAN/bridge work (Main.cs StartLanRace + the NRC LAN fields + the userpath routing) as CONCRETE pre-launch assertions for the reading card — what a REGRESSED single-player run looks like in the logs — so the battery's DV runs double as the single-player regression check.

## Two real holes found and closed while tracing

1. **The LAN-host-without-native-controller hole:** my two-instance test doc's host command did NOT set `MYSIM_NATIVE_GAMEPLAY=1` — the drive would be the `DriveController`, the adoption hook's `is` check would silently skip, the session would never adopt, AND `PendingLanHost` would leak into the NEXT race. Fixed three ways: the test doc's host command now requires the env (with the explanation), the adoption hook gained a fail-loud else-branch (`LAN_HOST_REQUIRES_NATIVE` — the session disposed and dropped, no leak), and the test doc's fingerprint table gained the row.
2. **The failed-load leak:** `StartRace` catches its own load failures internally — a LAN host load failure left `PendingLanHost` set, so the NEXT (unrelated) race would inherit the LAN session (`LAN_REMOTE_CAR` in a run that never asked for it). Fixed with the one-line `PendingLanHost=null` in `StartRace`'s catch (proposal hunk #4; until it merges, card A1 is the detector).

## Deliverables

1. **Reading card §14 — the regression net:** eight concrete assertions, ALL log-reading rules on runs the battery already produces (no new runs):
   - A1 the master absence proof: zero `LAN_*` lines in any non-LAN run; a violation invalidates the run as evidence REGARDLESS of its own section's outcome (class S).
   - A2 the DV report shape: pre-existing fields/assertions unchanged; the new fields additive only.
   - A3 the traffic gate: rivals-without-the-flag must still throw; the LAN rivals-zeroing only fires on a real adoption.
   - A4 the userpath chain: `userPath:true` + the §5 bands (CoreRunner-enforced + run-level metrics).
   - A5 the menu flows: FLOWCHECKS green (the ShowMenu lambda edit must not have moved the single-player buttons).
   - A6 the start sequence: the §9 profile unchanged (the LAN countdown arming lives in the session, not the Race boot).
   - A7 the new-env inertness: each new env is absent-inert by construction, with the guard location named.
   - A8 the LAN-hosts-without-native guard: loud failure, no silent skip.
   Plus the "what a REGRESSED run looks like" summary list (the log fingerprints a reader scans for) and the verdict-integration rule (the checks ride every existing battery reading).
2. **Main.cs** (proposal hunks, both additive to/adjacent to the previously approved hunks): the `LAN_HOST_REQUIRES_NATIVE` guard + the catch-clear. Merge review required as usual.
3. **`lan-two-instance-test.md`**: the host command fixed (`MYSIM_NATIVE_GAMEPLAY=1` + the explanation), the new fingerprint row.

## Verification

- Clean build: 0 warnings, 0 errors; CoreRunner `PASS 88130 assertions`, 0 FAIL.
- The A-assertions themselves are documentation (reading rules) — their SUBSTANCE is verified by construction: each traces to a specific code path read this session (the guard branches, the null-conditional guards, the env-parse validation).

## Risks / open

- The catch-clear (`A1`'s fix) needs your merge; until then the pre-fix window's fingerprint is documented (A1/A6 catch it in evidence).
- A4's run-level bands depend on the userpath DV leg actually being dispatched (the battery item) — the net cannot prove what never runs.

## Commit

`src/Main.cs` (proposal hunks), `outputs/mistral-orchestration/agents/physics-gameplay-reading-card.md`, `outputs/mistral-orchestration/agents/lan-two-instance-test.md`, this report — on `mistral/physics-gameplay`.
