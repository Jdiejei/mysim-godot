# Whole-game rebuild acceptance gates

Owner authorizes replacing the whole game, track pipeline and physics engine. This is a rebuild charter, not a completion report.

1. Establish a reproducible failing scenario with exact source/build revision, car/layout, inputs, contacts and video. Track rendering, route logic and physical collision are separate checks.
2. Treat the present engine honestly: tyre dynamics are planar and car height is projected onto sampled ground. Full three-dimensional suspension, jumping/landing and body roll/contact response are not implemented by that projection. If the requested behavior requires them, replace the bridge with a native 3D rigid body, measured compound collision, per-wheel suspension/contact and fixed-step tyre forces. Preserve race rules through a tested adapter.
3. Physical acceptance: no barrier/building/vehicle tunneling across the supported speed range, no phantom decal/road walls, no energy gain from depenetration, no body/mesh offset, grounded wheels on grades/kerbs/banked turns, stable reverse/braking/steering, and recovery from resets/initial overlap. Test bridges/underpasses and off-road transitions explicitly.
4. Track acceptance: all 60 layouts get physically valid continuous driving surfaces, correctly assigned obstacle roles and complete live-collision laps. Re-engineer broken collision topology and units from recovered geometry; do not close tunnels with a hull around an entire circuit or conceal holes through arbitrary teleports.
5. Asset acceptance: every reviewed candidate retains identity, source provenance, scale/pivots, UV channels and material roles. Inspect it assembled with its car/track, not just in isolation. Conversion/GLB existence, matching bounds and generated sheets are not quality acceptance.
6. Presentation acceptance: clear menu entry, visible mouse/touch/keyboard controls, unclipped settings/HUD at supported resolutions, correct body/wheel assembly and animation, coherent paint/glass/rubber/road/foliage materials, appropriate normals/alpha, stable readable shadows, and actual in-game recordings.
7. Gameplay acceptance: start, race, lap/checkpoint validity, traffic, pits, penalties, finish/results, restart/menu, save/settings and input focus transitions. Unsupported or unfinished features must remain explicitly recorded.
8. Run substantive import/render/simulation/build work on GitHub. Compare each replacement against the saved candidate and evidence. Promote the new Windows player only after its own checks pass; retain older evidence and failures.

Live status and process adoption: CONTINUE.md, cloud-game-state.json and cloud-intake-state.json. Remaining original scope includes 98,553 candidate meshes; nothing in this charter marks them reviewed or implemented.
