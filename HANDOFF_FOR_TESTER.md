# MySim (Godot port) - tester handoff (updated 2026-10-29)

Machine: Windows 11, NVIDIA RTX 4050 laptop, Godot 4.7.2 mono (.NET 8).

## What this is
A racing game rebuilt in Godot 4 from the recovered MySim assets (Real Racing 3-style). 24 cars, 60 track layouts (29 circuits), menu with settings and track previews, race with laps/timing/results, pit stops with crew and sound, HUD, four cameras, keyboard/gamepad/touch input, audio. Physics is a new four-wheel model (`src/Logic/Dynamics.cs`: Magic-Formula tyres, load transfer, downforce, ABS/TC) on top of the recovered race rules.

## Where everything is
| What | Path |
|---|---|
| Project (open this) | `C:\Users\shash\Documents\MySimGodot` |
| Playable exe | `...\MySimGodot\build\MySim.exe` (+ `MySim.pck`, `data_MySimGodot_windows_x86_64`) |
| Godot binary | path in `.godotpath` (console build) |
| Code | `src` (Main, DriveController, Contact, CarLoader, UnityMaterialBinder, Hud, TouchControls, Settings) and `src/Logic` (Simulation, Dynamics, ...) |
| Assets | `...\MySimGodot\assets` (4.4 GB, real files); backup copy `D:\MySim-godot-assets\assets` |
| Originals (do not modify) | `D:\MySim-recovered`, `D:\My Sim` |
| Review notes | `REVIEW_LEDGER.md`, `AUDIT.md`, `PORT-CHECKLIST.md`; renders `shots\t0..t59.png`, 4-up sheets `shots4\` |
| Hourly code backups | `D:\MySim-backups\godot-*` |

## Run
Exe: double-click `build\MySim.exe`. Windowless test: `MySim.exe --no-window --resolution 960x540` plus env vars below.
Project: run the Godot binary with `--path C:\Users\shash\Documents\MySimGodot`.

## Controls
W/Up throttle, S/Down brake (hold it when stopped to reverse, up to ~40 km/h), A/D or arrows steer, Space handbrake, P request pit, C camera (chase/far/cockpit/bonnet), Tab/Esc pause, M sound on/off (muted by default). Gamepad: left stick (response curve), RT/LT, A = pit, Y = camera. Touch: on-screen pads; accelerometer tilt steering via the `tilt` setting. Menu settings row: time of day (Noon/Morning/Sunset/Night), weather (Clear/Overcast/Rain), sound, ABS, traction control, tilt.

## Test hooks (environment variables)
`MYSIM_TRACK` 0-59, `MYSIM_CAR` 0-23, `MYSIM_LAPS`, `MYSIM_CAM`, `MYSIM_AUTODRIVE=1` (needed for timed runs), `MYSIM_AI=1` (AI drives; requests a pit each lap unless `MYSIM_NOPIT=1`), `MYSIM_SECONDS`, `MYSIM_SHOT=<png>`, `MYSIM_TRACE=1` (progress/speed every 4 s), `MYSIM_CARVIEW=<car>` (+ `MYSIM_CAMPOS=x,y,z`), `MYSIM_MENUSHOT=<png>`, `MYSIM_CLOSE=1` (side camera), `MYSIM_TOD=0-3`, `MYSIM_WET=0-2`, `MYSIM_STARTP=<progress>`, `MYSIM_WHEELDBG=1`, `MYSIM_LEGACY_PHYS=1` (old physics model).
Headless physics: `tools\LapRunner` (`dotnet run -c Release -- 0 1` = all 1,440 car/track pairs; `-- 0 1 <track> <car>` one pair; probes `trace`, `pit`, `pitroute`, `steptest`).
Render all tracks windowless: `bash tools/shoot_all.sh`, then `python tools/sheet4.py`.

## Status
- Headless lap matrix: 1,438 of 1,440 pairs pass (misses: car 7 on tracks 28 and 51).
- All 60 tracks rendered and reviewed 4-up; signage reads correctly, colours natural, ground textures upscaled (Real-ESRGAN; originals in `upscale_backup`).
- Pit stops work in game (enter, service, tyres reset, rejoin). 13 layouts whose shipped pit data does not overlap their track (29,33,34,36,37,41,42,44,46,47,50,52,58) use an on-track fallback stop beside the start/finish line.
- AI rivals: 5 AI cars start behind the player on the grid (setting `rivals`, env `MYSIM_RIVALS`; 0 for the old solo mode; none in AUTODRIVE/AI test runs). HUD shows race position POS n/6. Rivals use their own car models and the same physics; they are not wall-checked (they follow the centreline).
- Time of day and weather (src/Conditions.cs): sun/sky/fog/ambient presets, rain particles, headlights at night/rain, rain lowers grip (tyre wetness 0.15/0.75). Not tuned per track.
- Audio from recovered assets only: engine idle loop (pitch follows RPM), transmission whine, pit sounds, UI sounds. No recovered tyre-squeal or crash sounds exist.
- Fixed 2026-10-29: in-game physics always used car 0 (now uses the selected car); F1 wheel-guard meshes were spun with the wheels (glassy bubbles); front/rear F1 wheel meshes split by axle.
- Cars are the original recovered LOD_A meshes. The 98,553 "enhanced mesh" candidates in `D:\MySim-processed` are faithful copies (no added detail, none quality-accepted) and are NOT used.

## Known problems
- F1 AI laps are slow on long circuits (Silverstone ~147 s); the power model (from the 0-60 time) understates F1 power. Needs a combined power/drag/gearing retune.
- Pit crew are placeholder capsule figures; no jack/air-gun models.
- A tree-like blob floats near the stand on Silverstone layouts (cosmetic).
- Car rims look near-black (the recovered wheel texture is dark); tyres are plain dark.
- Chassis follows the ground planar from four wheel samples, so a wheel can hover over crests/drops (no suspension).
- Cloud lap/crash campaigns are blocked by GitHub billing on the working accounts; all testing is local.
- Not done: multiplayer UI, championship, weather visuals, Android build, key-rebinding screen, Monoposto side-by-side (emulator removed).
