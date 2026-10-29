# Proposed deletion list (nothing has been deleted)

Written 2026-10-29. The Godot port runs without any of the items below: it uses only `C:\Users\shash\Documents\MySimGodot` (code + 4.4 GB `assets`) and, as a backup copy, `D:\MySim-godot-assets\assets`. Verified by running the exported `build\MySim.exe` and the headless lap matrix (1,438/1,440 pass) with Unity untouched.

## A. Safe to delete after your OK (Unity itself and its clones, about 47 GB)
| Item | Size | What it is |
|---|---|---|
| `C:\Program Files\Unity` | 23.0 GB | Unity editor install |
| `C:\Program Files\Unity Hub` | 0.6 GB | Unity Hub |
| `C:\Users\shash\AppData\Local\Unity`, `...\Roaming\UnityHub` | ~0 GB | Unity settings/caches |
| `D:\MySim-processed\local-unity` | 23.2 GB | old Unity development clone of the game |
| `C:\Users\shash\Documents\Codex\2026-10-25` | 0.4 GB | packaged old Unity Windows candidate |

## B. Originals: I recommend KEEPING for now
| Item | Size | Why keep |
|---|---|---|
| `D:\MySim-recovered` (incl. `full-game-build`, 63.1 GB) | 63+ GB | the only copy of the recovered source game; the Godot `assets` folder holds just the 4.4 GB the port uses. Audio, GUI sounds and pit sounds were copied from here. Deleting it means anything not yet ported is gone for good. |
| `D:\My Sim` | 62.2 GB | original recovered folder (similar size to full-game-build; likely overlapping) |
| `D:\MySim-processed` (final manifest, 94 GB of candidate `.blend` meshes in zips) | large | the "enhanced mesh" candidates; not used by the game (exact copies of the originals, none quality-accepted). Safe to delete if you do not want them, but they took 5 days of compute to make. |

## C. Keep
`C:\Users\shash\Documents\MySimGodot`, `D:\MySim-godot-assets`, `D:\MySim-backups` (hourly code copies).

## How to approve
Reply with the letters you want removed (for example "A") and I will delete exactly those, list what was removed, and leave everything else alone.
