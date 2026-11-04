# HUD/camera — turn-9 report: the 5-resolution container audit, the unified-hierarchy diffs, the cockpit plan

Base: 4566853 reconciled (my whole chain is merged up through the integration merges; 836de96 fetched into my branch). Turn commits: **5f54588** (the unified-hierarchy diffs) + **aa759b7** (the cockpit-contract gate asserts). No coordination-file edits, no runs, no workflow edits.

## 1. Container audit against the 5-resolution evidence (41798036351 stills + the HUD_RECTS gates)

Canvas space (the content scale is CanvasItems 1280x720 with Expand aspect): 16:9 resolutions → 1280x720; 16:10 → 1280x800; 21:9 → 1706x720. Wider canvases only add room; every anchored surface keeps its margins.

| Surface | 1280x720 (all 16:9 res) | 1280x800 (16:10) | 1706x720 (21:9) | Verdict |
|---|---|---|---|---|
| Strip, desktop (x12..1268, y8..72) | worst-case line ≈1115 px in 1256 px → fits | same width | more width | SAFE |
| Strip, touch BEFORE this turn (single row, x12..940) | worst-case ≈1100 px in ~904 px → **overflow ~200 at legal text extremes** (long lap time + service text) | same | same | FIX APPLIED |
| Strip, touch AFTER (two rows, x12..1268, y8..112) | row1 session ≤~430 px; row2 state ≤~660 px → fits | same | same | SAFE |
| Speed/gear chip, desktop (bottom-right, grow-inward) | ~200 px worst case; hugging panel | bottom-anchored (y≈680..780) | fits | SAFE |
| Speed/gear, touch BEFORE | bare labels y24..164 beside the strip's stop | same | same | FIX APPLIED (the chip below the strip) |
| Speed/gear, touch AFTER (the chip, y124..≈216) | below the strip, above the minimap | same | fits | SAFE |
| Standings (12,80, hugging ≤~260 px) | y80..≈330; below the strip bottom (112 touch / 72 desktop) → gap ≥8 | same | same | SAFE |
| Minimap, desktop (12, 550..700) | fits | y-anchored | fits | SAFE |
| Minimap, touch BEFORE (y170..340) | would sit under the new strip/speed stack | — | — | MOVED (276..446) |
| Minimap, touch AFTER (y276..446) | above the touch driving buttons (y≥538) → ≥92 px clear | same | same | SAFE |
| ERS (bottom-centre, y −66..−20 baseline) | x≈460..820 vs speed chip x≥~1000 → no overlap | bottom-anchored | fits | SAFE |

Audit conclusion: the desktop layout was already inside the safe edges at every supported canvas (matching 41798036351's accepted stills); the touch side had one real overflow path and one stacking ambiguity, both closed this turn.

## 2. The unified-hierarchy diffs (commit 5f54588)

1. **One chip surface for everything** (the reviewer's "disconnected debug labels"): `Hud.Chip()` is now public and is the resolved panel box of the strip, the standings, both speed/gear chips and the ERS (the ERS is styled at the composition root with an instance stylebox override, since the HybridControlsPanel file belongs to another agent). The minimap draws the same chip box instead of its old flat black rect, and marks the player with the theme accent (MySimTheme.Accent) instead of a bare green dot. The standings title is muted so the ranked rows are the visual entry point of the surface.
2. **Touch = one right column with three stacked stops** (the strip rows at 8..112, the speed chip at 124, the minimap at 276..446; the 12 px gap rule): closes the audit's touch overflow AND removes the bare-label look; the chip grows down (GrowVertical=End) below the strip exactly as the desktop chip grows up (Begin).
3. **Emission coverage**: `strip` joined the HUD_RECTS line (the runtime rect of the strip itself is now bounds-checked at all five resolutions by the cloud gate); `MiniMapPanel`/`StripPanel` accessors exist for the fixture.
4. **Product gate (aa759b7)**: the cohesion asserts (the standings/ERS resolved styleboxes are the chip: corner radius 6 + translucent fill), the desktop speed pair is chip-wrapped, the standings do not overlap the strip, and the touch stack (strip → speed → minimap) neither overlaps nor exceeds the canvas on a live second Hud built with screen controls on.

## 3. Cockpit-usability improvement plan (the reviewer's standing reject: halo dominance, wheel/dash unreadable)

Ground truth read this turn (no rendering): the mount profile puts the steering wheel at local (0 open-wheel / −0.35 closed, 0.66, 0.5); the authored eye (CockpitSeat) = (same x, 0.88, 0.05) → the eye rides 0.22 above the wheel centre and 0.45 behind it, i.e. ~3 px-equiv above the rim top; the camera contract (merged at 647a2bb) already pitches the eye-line 9° down (wheel/dash band mid-frame) and holds FOV 68. CarViewVisibility.Apply reveals the `grp=int` surfaces in cockpit mode (correcting my earlier "interior always hidden" reading), so the dash/cockpit geometry IS present in the frame.

Root causes left for the reject, in priority order, each with its owner and the gate that will judge it:

1. **Instrument/dash texture readability** — the dash surfaces' texture resolution/UV/mirroring belongs to the carbody-livery scope (the same mirrored-texture family as the exterior logos). Evidence pointer: the cockpit PNGs at 41798036351; ask: the dash faces get a UV-candidate pass after the exterior mirrored-logo root cause lands (the same mirror-detector tooling that agent built covers the int group).
2. **Halo band position** — with the 9° eye-line the halo should already be upper-band; the pre-registered rule: if the motion stills show the halo pillar crossing the central band (the lower 60% of the frame) for more than ~25% of the lap, raise the seat +0.04 in y and re-pitch to keep the 12-ish degree wheel/dash band — the constants AND the product-gate asserts move together (the contract is the single source of truth by design).
3. **Driver hands / helmet edges** — no such geometry exists in the int group today; that is an asset-scope item (car-assembly), listed so it is a decision rather than a surprise.
4. **Motion usability check** — the cockpit recording pair (the same low/high-speed gate extension as section 6 of the turn-8 material): the halo must stay anchored (no frame-to-frame jump beyond a bound) and the wheel must stay in its asserted band; recordings make the cockpit judgement quantitative.

What I committed for the cockpit this turn: the product gate now pins the cockpit contract constants (aa759b7): FOV, both seats, the 9° instrument-band pitch (asserted from the world-space basis Z), and the non-mirrored handedness — any future tuning step 2 has to move the contract and these asserts together, which is exactly the discipline the reviewer asked for (explicit target/FOV/pose).

## 4. Verification status

- All the landscape numbers above are computed from the committed code (the grow-directions, the anchor maths) and the measured route length (script: 121 points, 1926.59 m); the style-cohesion and the touch-stack asserts execute on the next product gate run (the same cloud gate that carries the unit-binding asserts).
- Not compiled locally (the standing cloud-only contract); the next product gate run compiles everything.
- No claim of visual acceptance anywhere: the chip unification and the touch stack are committed diffs for the reviewer's stills, not approved art.
