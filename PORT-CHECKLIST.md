# MySim Godot port: master checklist (from the original master prompt)
Legend: DONE verified | PART partly | TODO not started | BLOCK blocked
1 Component inventory ............ PART (this file; PORT-STATUS.md)
2 Physics/driving ................ PART (recovered Race sim runs; AI lap matrix 1440 pairs headless; no tyre/slope PhysX equivalent, ground via raycast)
3 Pits ........................... PART (BH AI pit cycle with walls on; other tracks untested)
4 Controls ....................... PART keyboard + gamepad code (0.08 continuous deadzone, triggers; NOT hand-tested, no pad here); TODO touch, tilt, settings persistence
5 Cameras ........................ PART 4 modes (C or pad Y): chase, far chase, cockpit (original interior set), bonnet; verified renders on BH; TODO review in slopes/collisions/pit, obstruction handling, FOV tuning vs Monoposto
6 Shadows/lighting ............... PART sun 4 cascades 150 m; TODO weather, quality tiers, acne review
7 Tracks/assets .................. PART all 60 layouts render with exact Unity materials (shots/sheet0-4.png reviewed); 9 variation FBX are near-empty (5 KB, no meshes) so not an error; DEFECTS: Le Mans Sarthe (54) spawn lands on run-off sand and all 24 AI laps fail; TODO lightmaps (UV2), kerb/wall details, pit lane checks per track
8 Race flow ...................... PART menu+race+results+restart verified; DEFECT AI pace: BH lap 134 s (real ~80). Tried: longer lookahead (worse 176 s), removing curvature feedback cap (much worse 549 s) -- both reverted; root cause likely understeer/heading mismatch in vehicle model at speed, needs per-corner telemetry
9 UI/audio/settings .............. PART HUD only; TODO menus, pause, results, audio (muted), settings persistence
10 Multiplayer ................... PART Network.cs core checks pass; TODO Godot wiring + live test
11 Presentation .................. PART cars: 4 wheels at Unity pivots with spin+steer, exact car materials (base+mask), damage/interior duplicates hidden; TODO paint/liveries check per car, reflections, pit crew, drivers
12 Performance .................. TODO frame time, memory, load
13 Platform delivery ............. TODO Windows export, Android export + device test
14 Final release ................. TODO packages, checksums, manifests
CI: GitHub lap-matrix workflow (3 accounts x 12 shards); shawshanker18 blocked by billing.
Cleanup: delete Unity + duplicates ONLY MySim folders, after verification, final list first.

