# MySim Godot port: master checklist (from the original master prompt)
Legend: DONE verified | PART partly | TODO not started | BLOCK blocked
1 Component inventory ............ PART (this file; PORT-STATUS.md)
2 Physics/driving ................ PART (recovered Race sim runs; AI lap matrix 1440 pairs headless; no tyre/slope PhysX equivalent, ground via raycast)
3 Pits ........................... PART (BH AI pit cycle with walls on; other tracks untested)
4 Controls ....................... PART keyboard + gamepad code (0.08 continuous deadzone, triggers; NOT hand-tested, no pad here); TODO touch, tilt, settings persistence
5 Cameras ........................ PART 4 modes (C or pad Y): chase, far chase, cockpit (original interior set), bonnet; verified renders on BH; TODO review in slopes/collisions/pit, obstruction handling, FOV tuning vs Monoposto
6 Shadows/lighting ............... PART sun 4 cascades 150 m; TODO weather, quality tiers, acne review
7 Tracks/assets .................. PART exact Unity materials; winding fixed (all 2860 surfaces had reversed winding vs normals); fence/chain-link now alpha-scissored from texture alpha (black blocks gone, crowd visible); TODO lightmaps, per-track visual review again, Le Mans spawn check
8 Race flow ...................... PART menu+race+results+restart verified. DEFECT (reproduced): AI/car yaw oscillation limit-cycle: telemetry BH/Le Mans shows yawRate about -1 rad/s at steer 0.08, accel swinging +5.6 to -3.8 m/s2, speed stuck 13 m/s (laps 130-1000 s). Tried: longer lookahead, removing curvature cap, reserving rear grip in TC (all no fix/worse, reverted). Next: isolate vehicle model (human PD-steer test) vs AI pure-pursuit gains
9 UI/audio/settings .............. PART HUD only; TODO menus, pause, results, audio (muted), settings persistence
10 Multiplayer ................... PART Network.cs core checks pass; TODO Godot wiring + live test
11 Presentation .................. PART cars: 4 wheels at Unity pivots with spin+steer, exact car materials (base+mask), damage/interior duplicates hidden; TODO paint/liveries check per car, reflections, pit crew, drivers
12 Performance .................. TODO frame time, memory, load
13 Platform delivery ............. TODO Windows export, Android export + device test
14 Final release ................. TODO packages, checksums, manifests
CI: GitHub lap-matrix workflow (3 accounts x 12 shards); shawshanker18 blocked by billing.
Cleanup: delete Unity + duplicates ONLY MySim folders, after verification, final list first.

