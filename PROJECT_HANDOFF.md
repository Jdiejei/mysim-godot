# MySim - whole-project handoff (2026-10-29)

Read `HANDOFF.md` (the game) after this. This file covers the project as a whole: history, where every piece of data lives, accounts and compute, decisions already made, and what is still open.

## 1. What the project is
Recover and finish "MySim", a Real Racing 3-style racing game (24 cars, 60 track layouts, recovered vehicle physics), with Monoposto (Android) as a quality reference and the Real Racing 3 videos in `D:\MySim-recovered\design-references` as the feel reference. The owner wants a polished, playable, realistic game (they now cite the recent F1 games as the standard), tested at huge scale, with cars, tracks, physics, colours and pits all at a high bar.

## 2. History in one paragraph
Started as completing a Unity project. Unity builds were slow (HDD), then the Unity licence token failed, so on the owner's instruction ("no we scrap unity") the game was ported to Godot 4.7.2 mono (C#/.NET 8) at `C:\Users\shash\Documents\MySimGodot`. The port reuses the recovered vehicle model, race rules, AI and data, plus the owner's own recovered assets (nothing from personal folders). The physics was then rewritten from scratch as a four-wheel Pacejka model. An earlier 5-day cloud compute run produced 98,553 "enhanced mesh" candidates; they were checked and are exact copies of the originals, so they are not used.

## 3. Where everything is
| Thing | Location |
|---|---|
| Godot game project | `C:\Users\shash\Documents\MySimGodot` (code in `src`, logic in `src/Logic`, tools in `tools`) |
| Game assets (4.4 GB, used by the game) | `...\MySimGodot\assets`; backup `D:\MySim-godot-assets\assets` |
| Original recovered data (KEEP, only copy) | `D:\MySim-recovered` (63 GB full-game-build, car-tuning, driving-physics, design-references, cars-fbx, ...) and `D:\My Sim` (62 GB) |
| Enhanced-mesh candidates (not used) | `D:\MySim-processed` (manifest `final\manifest\selected_candidates.csv`, per-wave zips of `.blend`, about 94 GB; needs Blender, not installed) |
| Hourly code backups | `D:\MySim-backups\godot-*` |
| Playable exe | `...\MySimGodot\build\MySim.exe` (older than source; re-export with `tools/export_only.sh`) |
| One-click play | `...\MySimGodot\PLAY_SILVERSTONE_F1.bat` |
| Docs | `HANDOFF.md` (game), this file, `REVIEW_LEDGER.md`, `AUDIT.md`, `PORT-CHECKLIST.md`, `DELETION_LIST.md` |
| Memory notes (Claude) | `C:\Users\shash\.claude\projects\C--Users-shash-Desktop-big-turbo\memory\` |

## 4. Accounts and compute
- GitHub: five accounts were logged in via `gh` device codes. In use: `instagramshashank8-bot`, `Jdiejei`, `blackwingftc-lang`. `shawshanker18` has a billing block. Always pass `GH_TOKEN=$(gh auth token -u <account>)`.
- Repos: `<account>/mysim-godot` on the three accounts. They were PRIVATE until the owner chose to make them PUBLIC (2026-10-29) so Actions minutes are free. They contain game-derived data (track routes, baked collision, renders, asset IDs) and full history. Not undoable.
- Workflows: `lap.yml` (1,440-pair lap matrix; completed successfully in the cloud), `core.yml`, `crash.yml` (crash campaign: inputs `spacing`, `layout`; 20 layouts x 4 car groups per account; 1-day artifact retention).
- Crash campaign running: spacing 100 m, about 1.3 million crashes, 240 jobs across three accounts, 0 failures so far (about 23 finished at last check). Plan: download `crash-*` artifacts, review with `tools/crash_report.py`, change the game from findings, delete the downloads. Owner's target is 25 m left/right and 10 m on bends (about 5.2 million crashes).
- Local compute only for light work; the owner does not want heavy jobs on the PC.

## 5. Decisions already made (do not re-litigate)
- Engine: Godot, not Unity. Unity was deleted on the owner's approval: `D:\MySim-processed\local-unity`, `Documents\Codex\2026-10-25`, the Unity settings folders. NOT deleted (Windows needs admin): `C:\Program Files\Unity` (23 GB) and `...\Unity Hub` (0.6 GB); uninstall from Windows Settings > Apps.
- Use only the owner's own recovered assets and the recovered physics/data; never touch personal folders.
- Keep `D:\MySim-recovered`, `D:\My Sim`, `D:\MySim-godot-assets`.
- Sponsor-slot placeholder banners are replaced by plain grey (the original filled them at runtime from content that is not recovered).
- Audio uses only recovered sounds; game starts muted (M toggles).
- Do not add Claude as co-author on commits (global CLAUDE.md).
- No pop-up windows from automated runs; never close the owner's open windows; prefer bulk review scripts.

## 6. Current state
- Physics/AI: 1,440 of 1,440 car-track pairs pass headless; six-car packs race cleanly; F1 Silverstone about 112 s (real about 85 s).
- Tracks: all 60 reviewed; signage, foliage, road texture and colour fixes applied; bulk re-render of all 60 was 47/60 done.
- Cars: all 24 render with liveries; procedural spoked rims, matte tyres, clearcoat paint. Visual quality is the weakest area (see `HANDOFF.md` section 3).
- Systems done: menu, settings, pits and crew, rivals, results, weather and time of day, audio, minimap, reverse, controls.
- Hours spent on pace, pits, tyres and AI are in `REVIEW_LEDGER.md`.

## 7. Open items, in the owner's priority order
1. The game froze when they launched it and the wheels look stretched/ugly (undiagnosed; see `HANDOFF.md`). They want to launch and drive Silverstone with an F1 car themselves.
2. Car realism to the standard of the recent F1 games: paint, tyres, wheel and body detail, driving feel. Needs better source geometry than the recovered meshes, or substantial procedural work.
3. Finish the crash campaign review and the finer-spacing run.
4. Remove the two Program Files Unity folders (admin needed).
5. Optional: install Blender to try individual enhanced-mesh candidates (they were identical copies in the sample checked); Android build and a Monoposto comparison (emulator was removed).
