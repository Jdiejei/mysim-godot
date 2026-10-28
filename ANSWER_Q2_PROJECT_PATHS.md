# Answer to question A2: where is everything?

> "Where are the source project, assets, build scripts, executable, logs, test results, and recordings? Give absolute paths."

Every path below was checked with `Test-Path` on 2026-10-28 (EXISTS / MISSING shown).

## Source project (Godot 4.7.2 mono, C#)
| What | Absolute path | Status |
|---|---|---|
| Project root | `C:\Users\shash\Documents\MySimGodot` | EXISTS |
| Project settings | `C:\Users\shash\Documents\MySimGodot\project.godot` | EXISTS |
| C# project | `C:\Users\shash\Documents\MySimGodot\MySimGodot.csproj` | EXISTS |
| Game code (Godot side) | `C:\Users\shash\Documents\MySimGodot\src` (Main, DriveController, CarLoader, Hud, UnityMaterialBinder, GroundFixer, WallBuilder, SkyDome, ...) | EXISTS |
| Recovered race/physics logic | `C:\Users\shash\Documents\MySimGodot\src\Logic` (Simulation.cs etc.) | EXISTS |
| Main scene | `C:\Users\shash\Documents\MySimGodot\main.tscn` | EXISTS |
| Godot editor binary used | `C:\Users\shash\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe` (also in `...\MySimGodot\.godotpath`) | EXISTS |
| Git remotes (code only, private) | `github.com/instagramshashank8-bot/mysim-godot`, `github.com/blackwingftc-lang/mysim-godot`, `github.com/Jdiejei/mysim-godot` | EXISTS |

## Assets
| What | Absolute path | Status |
|---|---|---|
| Assets used by the game (link) | `C:\Users\shash\Documents\MySimGodot\assets` (junction) | EXISTS |
| Real location of those assets (34,007 files, 3.89 GB) | `D:\MySim-godot-assets\assets` | EXISTS |
| Godot import cache | `C:\Users\shash\Documents\MySimGodot\.godot` | EXISTS |
| Generated material/pivot maps | `D:\MySim-godot-assets\assets\unity_map.json`, `car_map.json`, `car_pivots.json` | EXISTS |
| Preserved originals (untouched) | `D:\MySim-recovered` and `D:\My Sim` | EXISTS |
| Original Unity project source | `D:\MySim-recovered\full-game-build\project` | EXISTS |
| Old Unity development clone | `D:\MySim-processed\local-unity\full-game-20261025` | EXISTS |

## Build and conversion scripts
| What | Absolute path | Status |
|---|---|---|
| Exact material map from Unity prefabs | `C:\Users\shash\Documents\MySimGodot\tools\build_unity_map.py` | EXISTS |
| Car material map | `C:\Users\shash\Documents\MySimGodot\tools\build_car_map.py` | EXISTS |
| Per-track texture matching (older, name-based) | `C:\Users\shash\Documents\MySimGodot\tools\build_all_assets.py` | EXISTS |
| 60-track screenshot batch | `C:\Users\shash\Documents\MySimGodot\tools\shoot_all.sh` | EXISTS |
| Headless lap/AI test runner | `C:\Users\shash\Documents\MySimGodot\tools\LapRunner` | EXISTS |
| Logic checks runner (153,203 assertions) | `C:\Users\shash\Documents\MySimGodot\tools\CoreRunner` | EXISTS |
| GitHub workflows | `C:\Users\shash\Documents\MySimGodot\.github\workflows\lap.yml`, `core.yml` | EXISTS |

## Executable
- **Godot game build: NONE.** No exported `.exe` or `.pck` exists anywhere under `C:\Users\shash\Documents\MySimGodot`. The game currently runs only by launching the Godot binary above with `--path C:\Users\shash\Documents\MySimGodot`.
- Old Unity candidate (not the current game): `D:\MySim-processed\local-unity\full-game-20261025\Builds\Windows\MySimulator.exe` EXISTS. Packaged old candidate: `C:\Users\shash\Documents\Codex\2026-10-25\master-command-execute-all-build-the\outputs\MySim-Windows` EXISTS.

## Logs
| Path | Content |
|---|---|
| `C:\Users\shash\Documents\MySimGodot\shots\log.txt` | 60-track render log (material binding counts, errors) |
| `C:\Users\shash\Documents\MySimGodot\import_all.log`, `import2.log`, `import3.log` | Godot asset import logs |
| `C:\Users\shash\Documents\MySimGodot\move_assets.log` | copy of assets to D: (34,007 files, 0 failed) |

## Test results
| Path | Content |
|---|---|
| `C:\Users\shash\Documents\MySimGodot\tools\LapRunner\laps-0.jsonl` | latest single-pair/short lap results (the full 1,440-pair run finished 1,416 pass, 24 fail all on Le Mans Sarthe; its raw file was overwritten by later single-pair runs) |
| `C:\Users\shash\Documents\MySimGodot\tools\LapRunner\full900.txt` | full-matrix console output |
| `C:\Users\shash\Documents\MySimGodot\starts.tsv` | start points of all 60 layouts |
| GitHub Actions runs (core-checks, lap-matrix) | in the three repos above |
| `C:\Users\shash\Documents\MySimGodot\AUDIT.md`, `PORT-CHECKLIST.md`, `PORT-STATUS.md` | audit, checklist, status |

## Recordings and renders
- **Video recordings: NONE exist.** No `.mp4/.mkv/.webm/.avi` under the project.
- Still images only: `C:\Users\shash\Documents\MySimGodot\shots\t0.png` ... `t59.png`, contact sheets `shots\sheet0.png` ... `sheet4.png`, `C:\Users\shash\Documents\MySimGodot\renders60` (60 named images), `C:\Users\shash\Documents\MySimGodot\renders60.zip`, comparison stills `compare_*.png`, `latest.png`, `latest2.png`.

## Note
`C:\Android\sdk` and `C:\Android\avd` (the Android emulator) are now **MISSING**: they were deleted after I installed them, so the emulator and Monoposto comparison cannot run until it is reinstalled.
