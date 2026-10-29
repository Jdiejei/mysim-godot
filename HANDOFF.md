# MySim (Godot port) - handoff (updated 2026-10-29, end of session)

Machine: Windows 11, NVIDIA RTX 4050 laptop, Godot 4.7.2 mono (.NET 8). Written for the next person or agent picking this up.

## 1. What this is
A racing game rebuilt in Godot 4 from the recovered MySim assets (a Real Racing 3-style game). 24 cars, 60 track layouts (29 circuits), menu with settings and track previews, races with laps/timing/results, pit stops with crew and sound, HUD with minimap, four cameras, keyboard/gamepad/touch input, audio, time of day and weather, AI rivals. Physics is a new four-wheel model (`src/Logic/Dynamics.cs`: Magic-Formula tyres, load transfer, downforce, ABS/TC) on top of the recovered race rules (`src/Logic/Simulation.cs`).

## 2. How to play it right now (the thing the owner keeps asking for)
- Double-click `C:\Users\shash\Documents\MySimGodot\PLAY_SILVERSTONE_F1.bat`. It opens a normal window: Silverstone, Ferrari F1, 5 AI rivals, 3 laps. It runs the live project source, so it always has the latest code and assets.
- Controls: W/Up throttle, S/Down brake (hold at a standstill to reverse), A/D or arrows steer, Space handbrake, P pit, C camera (chase/far/cockpit/bonnet), Esc/Tab pause, M sound (muted by default). Gamepad: left stick, RT/LT, A pit, Y camera, Start pause.
- The exported exe is `build\MySim.exe` (+ `MySim.pck`, `data_MySimGodot_windows_x86_64`). It is OLDER than the project source: it predates the clean road texture, clearcoat paint and glow. Re-export with `bash tools/export_only.sh` (10+ minutes, writes `export5.log`).

## 3. What the owner reported most recently (all still open)
1. "It froze." The owner launched the game (visible window, 1600x900, Silverstone F1, 5 rivals) and it froze. NOT reproduced: headless runs of the same setup held 140+ fps with no frame spike over 14 s (`MYSIM_FPS=1` prints fps/frame time). Suspects, in order: (a) a connected gamepad or virtual joystick reporting Start pressed, which toggles pause every frame (`DriveController.cs`, `startBtn`), (b) pressing Esc/Tab pauses by design (looks frozen), (c) a real-window-only problem (vsync/GPU). Mitigations applied (untested by the owner): rivals now load one per frame (no start-up stall), and a gamepad Start button that is already held/stuck at launch is ignored. Next step: run the game visibly with `MYSIM_FPS=1` and stdout redirected to a file, and watch `Race.paused`.
2. "The wheels are stretched out and ugly." Not diagnosed. The rims are procedural (`CarLoader.BuildRim`, sized from the tyre mesh bounds); the tyres are the recovered meshes, matte black, no tread or sidewall detail. Check the chase camera field of view and the wheel mesh scale at 16:9; compare in-game against the showroom render (`MYSIM_CARVIEW=16`).
3. "The car is shit / looks like slop / raw machines / unrealistic." Cars are the original recovered LOD_A meshes with simple materials. Some GT cars show engine or mechanical parts through the wheel arches (visible on the BMW M4, Camaro). The 98,553 "enhanced mesh" candidates in `D:\MySim-processed` are exact copies of the originals (none quality-accepted), so there is no better source geometry in hand. The owner wants F1 24/F1 25-level realism (colour, physics, car detail); that is a large art/content gap, not a tuning gap.
4. Car sizes: all 24 were measured. Lengths and wheelbases match real cars within about 3%. Widths are 0.1 to 0.2 m large on six cars (Ferrari 488, Ford GT, Aston GTE, Bugatti, Huracan) and heights are inflated by roof items. No fix applied.

## 4. Where everything is
| What | Path |
|---|---|
| Project | `C:\Users\shash\Documents\MySimGodot` |
| Godot binary | path in `.godotpath` (console build; the non-console one is the same name without `_console`) |
| Code | `src` (Main, DriveController, Contact, CarLoader, UnityMaterialBinder, Hud, TouchControls, Settings, Conditions) and `src/Logic` (Simulation, Dynamics, ...) |
| Assets | `assets` (4.4 GB, real files); backup `D:\MySim-godot-assets\assets` |
| Originals (do not modify) | `D:\MySim-recovered`, `D:\My Sim` |
| Notes | `REVIEW_LEDGER.md` (findings log), `AUDIT.md`, `PORT-CHECKLIST.md`, `DELETION_LIST.md` |
| Backups | hourly code copies in `D:\MySim-backups\godot-*` |
| Git | local repo; also pushed to the three public repos `instagramshashank8-bot`, `Jdiejei`, `blackwingftc-lang` / `mysim-godot` |

## 5. Tools (use these instead of one-off manual runs)
- `tools/bulk_render.py [car] [seconds]`: renders all 60 tracks (AI drives the chosen car) into `bulk/` and a mosaic `bulk/mosaic_all60.png`.
- `tools/bulk_cars.py`: all 24 cars, front/side/rear mosaics in `bulk/`.
- `tools/bulk_audit.py`: contact sheets of all 527 unique ground textures in `audit/`.
- `tools/bulk_declean.py`: removes baked tyre-skid streaks from 78 road textures (originals in `audit/orig/`). Already applied.
- `tools/hidden.py`: runs Godot with no window/console and low priority. Use it for every automated run so nothing pops over the owner's windows. NEVER kill or relaunch the owner's own game window.
- `tools/LapRunner`: headless physics/AI (`dotnet run -c Release -- 0 1` = all 1,440 car/track pairs; probes `trace`, `pit`, `pitroute`, `pack`, `steptest`).
- Cloud: `.github/workflows/lap.yml`, `core.yml`, `crash.yml`.
- Env test hooks: `MYSIM_TRACK`, `MYSIM_CAR`, `MYSIM_LAPS`, `MYSIM_CAM`, `MYSIM_RIVALS`, `MYSIM_AUTODRIVE`, `MYSIM_AI`, `MYSIM_NOPIT`, `MYSIM_SECONDS`, `MYSIM_SHOT`, `MYSIM_TRACE`, `MYSIM_FPS`, `MYSIM_CARVIEW`/`MYSIM_CAMPOS`, `MYSIM_TOD`, `MYSIM_WET`, `MYSIM_STARTP`, `MYSIM_CONTRAST`/`MYSIM_SAT`, `MYSIM_NOGLOW`, `MYSIM_UVROAD`, `MYSIM_OLDRIMS`, `MYSIM_LEGACY_PHYS`, tuning `MYSIM_GSLOPE/GMAX/DOWNM/LATCAP/PDK`.

## 6. Status (verified this session)
- Headless lap matrix: 1,440 / 1,440 pairs pass.
- Six-car AI packs on 12 layouts: all classified, no resets.
- All 60 tracks and all 24 cars rendered and reviewed (some reviewed before the later road fix; the full bulk re-render was in progress, 47 of 60 done, results in `bulk/`).
- Fixed this session: in-game physics used car 0 for every car; F1 wheel bubbles; sponsor placeholder banners; text mirrored on signs; foliage rotated onto bark; pit lane wall/containment bug; reverse; results-screen controls; skid-streaked road texture (`gnd_road_dark.png` replaced by clean asphalt, 78 road textures de-streaked); car paint now clearcoat; scene glow; contrast 1.14 / saturation 1.1.
- F1 pace: Silverstone about 112 s (real about 85 s), Brands Hatch about 42 s.
- Not verified: any control on real hardware (gamepad, touch, tilt), the freeze above, night headlight beam, audio by ear.

## 7. Known problems / not done
- The freeze and the "stretched wheels" complaint (section 3).
- Car visual quality: no tyre tread/sidewall art, no carbon/vent/wing detail, sponsor banners are plain grey (original filled them at runtime from content not in the recovered files).
- Wheels use simple per-wheel droop; no real suspension.
- Pit crew are placeholder capsule figures (no jack or air gun models).
- Not built: multiplayer UI, championship, Android build, key-rebinding screen, Monoposto side-by-side (emulator removed).
- Windows blocked deleting `C:\Program Files\Unity` (23 GB) and `...\Unity Hub` (0.6 GB): they need administrator rights; uninstall via Windows Settings > Apps.

## 8. Cloud and repos
- The three mysim-godot repos were made PUBLIC on the owner's instruction (this exposes game-derived data: track routes, baked collision, renders, asset IDs). Public repos get free Actions minutes.
- Crash campaign (100 m spacing, about 1.3 million crashes, current physics) was started on all three accounts: 240 jobs; about 23 had finished with 0 failures when last checked. When done: download the `crash-*` artifacts (1-day retention), review flagged crashes with `tools/crash_report.py`, then delete the downloads. The owner's earlier wish was 25 m / 10 m-bend spacing (about 5.2 million crashes).

## 9. Owner preferences (important)
- Terse replies; short bullet points. Do not stop to ask; decide and proceed, but ask before anything destructive or public.
- Never show pop-up windows from automated runs; use `--no-window` and `tools/hidden.py`. Do not close or relaunch the owner's open windows.
- Do not add Claude as co-author on commits.
- Prefer bulk scripts and reviewing many renders at once over one-by-one manual checks.
- Deleted so far (approved): `D:\MySim-processed\local-unity`, `Documents\Codex\2026-10-25`, Unity settings folders. Everything in `D:\MySim-recovered`, `D:\My Sim`, `D:\MySim-godot-assets` is kept.
