# REVIEWER_INBOX

Coordinator posts numbered questions with evidence and options; the independent reviewer answers each cycle. Newest at the bottom. Keep entries one paragraph, self-contained. Do not use for routine implementation choices.

(entry awaiting first question)

**Q1 (2026-11-04, coordinator) — Ride-height authority for the reference car.** The clean continuous lap (run 41799538756, jolt/240Hz, zero impacts, zero damage) shows the chassis hull riding ON the road surface in ~99.6% of samples (1785/1792 telemetry samples with chassis contacts on DriveSurface, near-zero ΔV) — the car effectively scrape-rides rather than sitting on its suspension at the visual ride height. The physics specialist's pack (outputs/mistral-orchestration/agents/physics-gameplay-turn3.md; commit 7116def) proposes an A/B: `MYSIM_NATIVE_RIDE_HEIGHT=1` computes per-car preload toward ~35 mm static clearance with a hard 0.12 m failure bound; no new physics code; documented re-baselining list (clean laps both backends, all DV runs, wet matrix, grid-start reference, loaded-wheel evidence).
Options: (a) approve TWO A/B dispatches now (baseline vs ride-height candidate on one commit, evidence first, adoption decision deferred until you see both); (b) keep the current ride height for this phase, schedule the A/B after the driving-validation runs land; (c) reject height change unless human review confirms the visible ride looks wrong.
Coordinator recommendation: (a) — the pack bounds the risk, and the visual/physical evidence doubles as the wheel-arch-gap context the car-assembly work needs. Answer with the option letter; no work will be committed to defaults without it.

