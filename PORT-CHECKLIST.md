# MySim Godot port: master checklist (from the original master prompt)
Legend: DONE verified | PART partly | TODO not started | BLOCK blocked
1 Component inventory ............ PART (this file; PORT-STATUS.md)
2 Physics/driving ................ PART (recovered Race sim runs; AI lap matrix 1440 pairs headless; no tyre/slope PhysX equivalent, ground via raycast)
3 Pits ........................... PART (BH AI pit cycle with walls on; other tracks untested)
4 Controls ....................... PART keyboard + gamepad code (0.08 continuous deadzone, triggers; NOT hand-tested, no pad here); TODO touch, tilt, settings persistence
5 Cameras ........................ PART chase only; TODO cockpit, alt cams, collision/slope review
6 Shadows/lighting ............... PART sun 4 cascades 150 m; TODO weather, quality tiers, acne review
7 Tracks/assets .................. PART exact Unity prefab->material->texture map (38,230 mats, 30,615 textured) bound 100% of BH surfaces, variation layers + sky dome loaded; TODO lightmaps (UV2), other-layout verification, 60-track visual review
8 Race flow ...................... PART menu(60 tracks,24 cars,laps)+race+results+restart verified; TODO championship, penalties UI; DEFECT AI pace very slow (BH lap 134 s, Le Mans 1070 s; raising lateral limit made it slower => AI line-following problem, telemetry in tools/LapRunner)
9 UI/audio/settings .............. PART HUD only; TODO menus, pause, results, audio (muted), settings persistence
10 Multiplayer ................... PART Network.cs core checks pass; TODO Godot wiring + live test
11 Presentation .................. PART cars: 4 wheels at Unity pivots with spin+steer, exact car materials (base+mask), damage/interior duplicates hidden; TODO paint/liveries check per car, reflections, pit crew, drivers
12 Performance .................. TODO frame time, memory, load
13 Platform delivery ............. TODO Windows export, Android export + device test
14 Final release ................. TODO packages, checksums, manifests
CI: GitHub lap-matrix workflow (3 accounts x 12 shards); shawshanker18 blocked by billing.
Cleanup: delete Unity + duplicates ONLY MySim folders, after verification, final list first.

