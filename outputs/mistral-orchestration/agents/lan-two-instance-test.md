# LAN two-instance test — owner-run script (the first live LAN verification)

Status: the script + automation for the test the LAN plan gated on. Everything is CODE-PRESENT-UNVERIFIED until this run passes. No acceptance claims from in-tree evidence.

## 0. What this test proves (and what it does not)

Proves: discovery-free direct join, the HELLO/ACCEPT handshake, the explicit START gating, the host's in-race integration (the session adopted the live simulated race), the remote-driven car on the host, the client's snapshot pipeline into persistent scene objects, and clean session teardown — with console evidence and exit codes.

Does NOT prove: real-LAN (two-machine) behaviour beyond loopback, the input-latency feel, results/rematch UI, performance. Those are the follow-up runs on real hardware.

## 1. The two commands (same machine first; the loopback proves the full stack)

Run each in its own PowerShell window, from the repo root. `<GAME_EXE>` = the exported player or the Godot binary invocation used by the existing battery (the env vars pass through either):

```powershell
# Window A — HOST (starts ~3 s before the client; auto-creates the room, auto-starts, auto-drives, exits at 45 s)
# MYSIM_NATIVE_GAMEPLAY=1 is REQUIRED: the LAN host integration lives in NativeRaceController;
# without it the run fails loud (LAN_HOST_REQUIRES_NATIVE) instead of silently skipping adoption.
$env:MYSIM_NATIVE_GAMEPLAY="1"; $env:MYSIM_LAN_HOST="1"; $env:MYSIM_LAN_AUTOSTART="1"; $env:MYSIM_AUTODRIVE="1"; $env:MYSIM_LAN_QUIT_AFTER="45"; & "<GAME_EXE>"

# Window B — CLIENT (auto-joins the host on loopback; exits itself with a PASS/FAIL summary)
$env:MYSIM_LAN_JOIN="127.0.0.1"; $env:MYSIM_LAN_QUIT_AFTER="45"; & "<GAME_EXE>"
```

Both windows visible side by side: the expected VISUAL is the client window showing the track with TWO cars — its own at the grid (stationary; nobody drives it) and the HOST's car driving the track from ~t=8 s (the AutoDrive schedule) — the camera follows the client's own car, so the host car must be watched via the console evidence below (or drive the client manually — §4).

## 2. The timeline (expected, seconds from the host launch)

| T | Event | Evidence |
|---|---|---|
| 0 | Host boots → the LAN panel opens → the room auto-creates | A: `ROOM OPEN / <ip>:44880 / waiting for another player` |
| ~3 | Client boots → auto-join | B: `Connecting` then `CONNECTED / 2 players / <track>` |
| ~5 | The host's auto-start fires (2 s after connect) | A: `Race starting...` then the race scene loads: `RACE_READY track=0`, `LAN_REMOTE_CAR model=<n> driver=PLAYER`, `LAN_HOST_ADOPTED cars=2` |
| ~5–8 | The client's panel hands off on START | B: `Race starting` then `LAN_RACE_READY track=0 role=Client` |
| ~8–13 | The 5 s countdown runs; the AutoDrive throttle opens at elapsed 3 | A: normal countdown/race; B: `LAN_SYNC ... phase=GRID ... hostSpeed=0.00` → phase=RACE |
| ~12–20 | The host's car accelerates → the client sees it | B: `PASS LAN_CLIENT sync observed: host car speed=<> at t=<>` — THE sync proof |
| 45 | Both auto-exit | A: `LAN_HOST_EXIT connected=True inputs=<n> elapsed=45.x` (n>0, exit 0); B: `PASS LAN_CLIENT exit: sync observed` (exit 0) |

## 3. PASS criteria (mechanical — all must hold)

1. Host exit code 0 with `LAN_HOST_EXIT connected=True inputs=>0`.
2. Client exit code 0 with `PASS LAN_CLIENT exit: sync observed` AND the `PASS LAN_CLIENT sync observed:` line present.
3. At least one `LAN_SYNC ... hostSpeed=` line with a value > 3 on the client.
4. NO `LAN session error`, NO `Connection lost`, NO `Room connection error` on either console.
5. `LAN_REMOTE_CAR` (A) and `LAN_RACE_READY` (B) both present — the remote car existed from construction and the client entered gated on START.
6. The single-player regression guard (same run or a follow-up): a plain `MYSIM_TRACK=0` run still boots and completes exactly as before — the LAN envs absent ⇒ zero LAN code paths (verified by the absence of every `LAN_*` line).

Failure → fingerprint → owner:

| Symptom | Fingerprint | Owner |
|---|---|---|
| The client times out (`No host response`) | the host's room never opened / firewall / wrong address | run env: re-check the env + the port (44880/UDP; a real two-machine LAN needs the inbound rule) |
| A: `RACE_LOAD_FAILED` after `LAN_HOST_ADOPTED` missing | the track/car load — same as any single-player load failure | physics/environment |
| A: `LAN_HOST_REQUIRES_NATIVE` | the host command missing `MYSIM_NATIVE_GAMEPLAY=1` (the guard dropped the LAN session; no leak into later races) | test procedure |
| B: stuck `Connecting` after the host started | the BUSY race-already-started guard fired (the client joined too late) — re-run with the client earlier | test procedure |
| B: `LAN_SYNC hostSpeed=0.00` forever, `FAIL ... remote motion never observed` | the sync path broken: check A for the `LAN_HOST_ADOPTED` line; if present, the snap/application path — physics (me) with the log | physics (me) |
| Either console: `LAN session error` / malformed-packet text | a protocol regression — physics (me), include both consoles | physics (me) |
| A: `LAN_HOST_EXIT inputs=0` | the client's INPUT stream never landed (the reverse direction broken) | physics (me) |

## 4. Variants

- **Interactive drive (human feel check)**: same commands WITHOUT `MYSIM_AUTODRIVE`/`MYSIM_LAN_AUTOSTART`; drive the host with WASD, press START RACE manually, drive the client from its window (its input streams to the host's simulation of its car). The feel-cost expectation: ~70–90 ms on the client's own car (pure snapshot rendering MVP — the adopted default).
- **Two-machine LAN**: the same commands; the client uses the host's LAN IP (shown in the host's ROOM OPEN line); the firewall note: 44880/UDP inbound on the host.
- **Disconnect leg**: kill the client (close its window) mid-race → the host's console keeps running the race; the remote car continues under AI control (`AI (disconnected)`) — the graceful-degradation proof.

## 5. After the run

Both consoles' full text belongs to the evidence bundle (the reading-card §13 judges it). The result feeds the LAN plan's phase 1→2 exit and the acceptance checklist's first live row. Charts: only after BOTH exit codes are verified terminal states.
