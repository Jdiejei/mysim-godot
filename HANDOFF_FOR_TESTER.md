# MySim (Godot port) - tester handoff

Plain summary for another agent/person who wants to run and test the game.
Machine: Windows 11, NVIDIA RTX 4050 laptop, Godot 4.7.2 mono (.NET 8). Written 2026-10-28.

## What this is
A racing game rebuilt in Godot 4 from recovered Real Racing 3-style assets. It has 24 playable cars, 60 track layouts (29 circuits), a menu, a race with laps/timing/results, pit stops, a HUD, four cameras, keyboard and gamepad input. Physics is the recovered C# vehicle model (not Godot rigid bodies). It is NOT finished: see "Known problems".
There is NO exported .exe yet. Run it through the Godot binary.

## Where everything is
| What | Path |
|---|---|
| Project (open this) | `C:\Users\shash\Documents\MySimGodot` |
| Godot binary (console) | `C:\Users\shash\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe` (the same path without `_console` opens a window with no console) |
| Game code | `...\MySimGodot\src` (Main.cs = menu/race flow, DriveController.cs = driving/camera/input, Hud.cs, CarLoader.cs, UnityMaterialBinder.cs, WallBuilder.cs, SkyDome.cs) |
| Physics / race rules | `...\MySimGodot\src\Logic\Simulation.cs` (Vehicle.Step, Autopilot, Race.Advance) |
| Assets (junction) | `...\MySimGodot\assets` -> real folder `D:\MySim-godot-assets\assets` (3.9 GB; drive D: must be connected) |
| Original recovered data (do not modify) | `D:\MySim-recovered`, `D:\My Sim` |
| Import cache | `...\MySimGodot\.godot` (can be rebuilt) |
| Test tools | `...\MySimGodot\tools\LapRunner` (headless AI lap/physics tests), `tools\CoreRunner` (logic checks), `tools\shoot_all.sh` (render all tracks) |
| Status docs | `AUDIT.md`, `REVIEW_LEDGER.md`, `PORT-CHECKLIST.md`, `ANSWER_Q2_PROJECT_PATHS.md` |
| Renders | `...\MySimGodot\renders60` (60 track images), `shots\` |
| Code copies (private) | github.com/instagramshashank8-bot/mysim-godot, blackwingftc-lang/mysim-godot, Jdiejei/mysim-godot (code only, no assets) |

## How to launch
PowerShell (replace nothing; paths are exact):
```
cd C:\Users\shash\Documents\MySimGodot
& "C:\Users\shash\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe" --path . --resolution 1280x720
```
First run after a clean checkout: build and import once:
```
$env:Path += ";C:\Program Files\dotnet"; dotnet build
& "<godot console exe>" --headless --path . --import
```

## How to start a race
1. The menu opens: pick a track (left list, 60 layouts), a car (middle list, 24 cars), laps (right), press START RACE (or double-click an item).
2. The car is placed on the grid and the race starts immediately.
3. When the last lap finishes a RESULTS screen shows: Enter = restart, Esc = menu.

## Controls
Keyboard: W/Up throttle, S/Down brake, A/D or Left/Right steer, Space handbrake, P request pit stop, C change camera (chase, far chase, cockpit, bonnet), Tab pause, Esc back to menu.
Gamepad: left stick steer (0.08 deadzone), right trigger throttle, left trigger brake, A pit request, Y camera. (Gamepad code exists but has not been tested with a real pad.)
Audio is muted by default (no sound implemented yet).

## Skip the menu (automated tests, environment variables)
`MYSIM_TRACK=<0-59>`, `MYSIM_CAR=<0-23>`, `MYSIM_LAPS=<n>`, `MYSIM_CAM=<0-3>`; `MYSIM_AUTODRIVE=1` full throttle, straight; `MYSIM_AI=1` AI drives the player car; `MYSIM_SECONDS=<n>` run length; `MYSIM_SHOT=<png path>` save a screenshot and quit. `MYSIM_CARVIEW=<car>` shows one car in a showroom.
Example render: set `MYSIM_TRACK=0 MYSIM_CAR=8 MYSIM_AUTODRIVE=1 MYSIM_SECONDS=6 MYSIM_SHOT=C:\temp\a.png` then run Godot with `--path .`.
Headless physics/AI tests (no Godot needed): `cd tools\LapRunner; dotnet run -c Release -- <shard> <shards>` (e.g. `0 1` runs all 1,440 car/track pairs) or one pair `-- 0 1 <track> <car>`; `-- steptest <car> <steer> <throttle>` for a controller-free steering test.

## How it works (short)
- `Main.cs` builds the world: loads the track FBX (+ variation layer, sky dome), the recovered drive-surface mesh (the only thing cars drive on), barrier colliders, then the car.
- `DriveController.cs` each physics tick: reads input -> `Race.Advance()` (fixed 1/120 s sub-steps) -> raycasts the drive surface for height/normal -> sets the car transform. Wheels spin/steer from simulation state. Walls use a box sweep.
- Track and car materials come from maps generated from the original Unity prefabs (`assets/unity_map.json`, `car_map.json`, `car_pivots.json`).
- World axis note: tracks are mirrored in Z (scale 100,100,-100) to line up with the drive surface.

## Known problems (what to look for)
- Scenery: some stands/fences render black (transparency/lightmap handling in progress), no baked lightmaps, pale strip beside some barriers.
- Cars: only the 24 menu cars; no damage, brake lights, reflections, driver or pit crew.
- Physics: no suspension; gear/RPM on the HUD are derived from speed; AI is slow on some tracks; wall collision is guessed by material name; kerbs have no collision.
- Missing: audio, multiplayer UI, championship, weather visuals, Android build, Windows .exe.
- Latest numbers: headless lap matrix 1,397/1,440 pass before the last AI fix (final run pending).
- FIXED today: the car used to face backwards (mirrored basis); now nose faces travel direction.
