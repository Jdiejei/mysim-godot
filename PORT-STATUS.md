# MySim Godot port status (2026-10-28)
Engine: Godot 4.7.2 mono + .NET 8. Run: see .godotpath (console exe). Screenshot: set MYSIM_SHOT=path.
## Verified
- 12 engine-free logic files build under .NET; CoreChecks 153203 assertions PASS (tools/CoreRunner, `dotnet run`).
- McLaren 650S GT3 renders textured (CarLoader.cs), scale x100, LOD_A only, sun shadows (4 cascades,150 m).
- Brands Hatch hatch.fbx loads (2860 meshes, x100 => 1074x1239 m), trimesh colliders, ray hit at spline start (-247.06, y -9.59, 291.44) in same x/z frame as Unity.
- Car spawns on recovered drive-surface mesh (z axis flipped vs spline, track scale (100,100,-100)); chase camera 7.2 m back 2.65 m up; render shows aligned barriers and trees (shot7.png).
- DriveController: recovered Race sim drives the player (WASD/arrows/space), model placed on drive surface by raycast, chase camera. 8 s full-throttle autodrive test: speed 39.6 m/s, progress 7.7 pct, on surface (drive2.png).
- HUD (speed/gear/lap/tyres/fuel/pit status) and P key pit request. AI-driven pit test (MYSIM_AI=1 MYSIM_SECONDS=170): entered pit lane at t=118 s, stopped at box, serviced 7.5 s (tyre wear 0.05 -> 0.00), released and rejoined. CAVEAT: Godot port has NO wall/barrier collision yet, so this does not prove the Unity-era barrier-crossing pit failure is fixed.
## Not done
- Track materials/textures/lightmaps (track is untextured), driving physics, chase camera, controls, HUD/menus, pits, AI, other 29 cars/tracks, Android export, tests on 190 machines.
## Deletion
Not performed. Unity/Hub/original source/duplicates deleted only after port verified; user authorized deleting all incl. original.


