# Track rendering — turn 12: corridor proposal APPLIED as code

The Hockenheim corridor correction (accepted proposal, commit `6d34560`) is now applied for real in `src/Logic/Simulation.cs` on this branch, per the coordinator's direction (the file is otherwise coordinator-owned; this specific change was explicitly authorized): the `Circuit` constructor now sets `width=CorridorWidth(variant)` with `CorridorWidth(int variant)=>variant==8?16.4f:12f` — HOCKENHEIM only; every other track keeps the historical 12 m exactly. The evidence rationale is embedded as the method's comment (rejection rows, the 35 m measured footprint, the collider-neutrality census, the pit-lane 9.0 m position, the validation path).

Unchanged and still attached on this branch:

- Evidence table + decision rationale: `agents/track-render-turn10.md` (and the original proposal snapshot `agents/hockenheim-corridor-proposal.patch` — now historical; the applied state is the live code here).
- Overlay validation checklist for the owner-run battery item 4: `agents/track-render-turn11.md` — unchanged and directly applicable: with this commit the drawn corridor edges render at the NEW width automatically (`DrawCorridor` reads `route.width`), so one track-8 overlay run validates corridor-vs-asphalt AND the disappearance of the two progress-0.1948 rejections in the same run.
- `bestLap=0` (finished=true, laps=1) remains flagged for physics-gameplay; note the corridor fix does NOT fix that conjunct — a green track-8 verdict needs both.

Scope note for the merger: this commit touches `src/Logic/Simulation.cs` (+12/−1: one line changed, the annotated static method added) — outside my standing file list, applied under the coordinator's explicit turn instruction, mirroring the accepted proposal byte-for-byte. Verification: full re-read of the edited region; brace/paren balance (144/144, 672/672); `CorridorWidth` referenced exactly twice (call + definition); no other behaviour touched; no dispatch (the battery remains the owner's validation path).
