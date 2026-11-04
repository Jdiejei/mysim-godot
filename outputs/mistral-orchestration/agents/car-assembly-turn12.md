# Car-assembly turn 12 - rear tub band source-check - 2026-11-06

Document only. No code changes. No dispatches.

## 1. Measured bands (car16, from retained run data and turn-1 assembly capture)

| Object | z-span | y-span | x-band |
|---|---|---|---|
| CHASSIS_mm_misc (the rear tub layer) | -2.3635 .. +1.2675 | 0.0525 .. 0.4935 | -0.7875 .. +0.7875 |
| RL/RR tyre (raw pivot line, hub z -1.5888, r 0.2996) | -1.8884 .. -1.2892 | 0.0404 .. 0.6396 | RL: -1.0227 .. -0.6127 |
| RL/RR tyre (fitted line, shift about -0.4765) | -2.3649 .. -1.7657 | unchanged | unchanged |

## 2. Findings

1. The tub's z-band fully contains the rear tyre band on the raw pivot line; the y ranges overlap; the x overlap covers the tyre's inner half only: the tub edge (-0.7875) sits inside the rear tyre band (-1.0227 .. -0.6127), so only the tyre's inboard portion can meet the tub. The measured -45 mm penetration at rest is real geometry on the raw line.
2. After the axle fit the tyre's rear edge (-2.3649) and the tub's rear edge (-2.3635) coincide to about 1.4 mm. The tub's rear boundary runs along the tyre's rear shoulder instead of under it.
3. Therefore the rear band outcome depends on the tub's surface detail near that shared edge, not on clear z separation like the front wing case.

## 3. Pre-registered expectations for the battery (car16; apply per F1 car with its own numbers)

| Quantity | axleShift=0 (measured) | axleShift=fitted (pre-registered) |
|---|---|---|
| rear bodywork worst, all poses | -0.0452..-0.0453 | -0.005..+0.03 expected; ACCEPT if >= -0.002; investigate 0..-0.005 as within-tolerance edge sampling |
| rest-state rear bodywork | -0.0452 | same as above |
| rear suspension (upper strut) | -0.0203 | clears (tyre returns to the strut's outer end) |
| stance no-hub | +0.0099 | unchanged |

The rear bodywork row gets the SAME hard floor as the front (>= -0.002), with one allowance: a result in (-0.005, 0) is read as the shared-edge coincidence above (1.4 mm class), i.e. WATCH not FAIL, and it requires one pinned re-run with a 0.5 mm-class denser grid before either verdict is recorded.

## 4. If the rear row stays below -0.005 after the fit

Then the tub's rear edge itself extends into the fitted tyre band beyond the coincidence value. Ladder: (1) re-check the CHASSIS mesh's own source extent against the axle target (the tub may be part of the same misplaced-lineage as the pivots); (2) mount/transform correction only with source correspondence; no tyre change, no threshold change, no hiding.
