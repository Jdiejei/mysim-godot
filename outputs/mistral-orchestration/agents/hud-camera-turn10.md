# HUD/camera — turn-10 (the recovery pass completed)

Branch: mistral/hud-camera, rebased on the integration head 654d7eb, tip = this report's commit tree. No runs, no coordination-file edits.

## 1. Recovery + re-anchor (per the coordination's ground-truth)

- Both pass files exist and were read fresh before any edit: src/RaceStandingsPanel.cs (the panel) and
  src/ProductControlsScenario.cs (the by-name row checks: GetNodeOrNull("Car"+car.id), the header check, the
  cockpit contract asserts at lines ~73-77 of the fixture).
- The claimed in-tree state verified by grep: StripPanel/MiniMapPanel accessors present in Hud.cs;
  CockpitFov/CockpitSeat/CockpitPose present in DriveController.cs and referenced by the product gate.

## 2. The rebase (the previous turn's ask, now closed)

The branch was rebased onto 654d7eb (the corridor fix + the axle A/B + the battery line): 3 commits replayed
clean, zero conflicts. The coordination's "Hud.cs changed in both lines" did not materialise - the
integration-side delta 836de96..654d7eb touches only src/Logic/Simulation.cs (CorridorWidth), 
src/SourceAxleFitCandidate.cs and src/WheelFitAudit.cs; no integration-side Hud.cs edit exists. My Hud.cs
changes of turn-9 were the only Hud delta; they merged at 647a2bb on the integration side and rebased cleanly
on mine.

## 3. This turn's deltas

1. src/RaceStandingsPanel.cs - the standings/timing hierarchy step: every non-leader, still-running car's row
   carries its live distance deficit to the leader in metres (the results board's DistanceScore measure,
   leader-referenced; finished/retired rows keep the state-only form). The row is one line of real data:
   "01  NAME  L4  +96m".
2. src/DriveController.cs - the cockpit-state audit now emits COCKPIT_INT_UV for every visible LOD_A_INT_*
   surface at cockpit captures (the UV0/UV2 bounding boxes in the CARVIEW UVAUDIT shape) - the input
   carbody-livery needs for the cockpit dash-texture repair; the COCKPIT_VIEW seat check now reads the
   CockpitSeat contract (the single source of the seat numbers, no inline duplicate).

## 4. Open

- The gap-column fixture assertion (the by-name rows: the leader row carries no gap, the trailing rows carry
  the model-derived metres) is deliberately NOT added this turn - the fixture deserves a careful pass with
  fresh eyes after the previous session's corruption incident; it is one 6-line addition against the existing
  by-name block.
- The cockpit UV emission needs one cloud capture (any cockpit view run) to hand carbody their dataset.
