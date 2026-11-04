# Track rendering — turn 14: battery checklist additions (forest A/B + track-8 overlay) + two integration fixes

Author: track-render specialist. Sources: the fork tip `3cd5aed` (fetched read-only; newer than `bb49b4`), `OWNER_MERGE_INSTRUCTIONS.md` (the six-command battery + command 0), and my committed code. Two real integration defects found and fixed on MY side this turn; the checklist additions below slot into the battery thread. No dispatches; no workflow edits.

## 1. Integration fix (mine, committed): the overlay would silently never fire in battery command 4

- The workflow sets env **`COLLISION_OVERLAY`** (bare name — `game-review.yml` L246 at 3cd5aed) for `depth=track_scene_audit`; the python's `run()` helper passes non-`MYSIM_` environment through but STRIPS any `MYSIM_*` from inheritance — so the yml can NEVER deliver my `MYSIM_COLLISION_OVERLAY` name, and my code checked only that name → the overlay would have silently no-op'd in the battery's overlay run (a green run with no overlay = the exact failure class the wrong-mode history warns about).
- Fixed in `WallBuilder.cs` (my file): `AttachCollisionOverlay` now accepts BOTH spellings (`MYSIM_COLLISION_OVERLAY` or `COLLISION_OVERLAY`); and when `MYSIM_COLLISION_OVERLAY_AUDIT` is absent but `MYSIM_TRACK_SCENE_AUDIT` is set (the battery case), the audit JSON defaults beside it (`<scene-audit-path>-collision-overlay.json`). Default behaviour (no env) unchanged. Battery command 4 now works AS WRITTEN, zero coordinator changes.

## 2. Track-8 coverage gap in battery item 4 (coordinator decision, two options documented)

The depth's track map (`cloud_game_audit.py` L598-599 at 3cd5aed): `depth=track_scene_audit` runs shards `[0,1]` → tracks `[(0,5)[shard]]` = **tracks 0 and 5 only** — the Hockenheim (track 8) corridor validation does NOT run under the current wiring. Options:

- **(a) recommended:** extend the map to `[(0,5,8)[shard]]` + the shard list `[0,1]`→`[0,1,2]` for this depth (two one-line edits, coordinator-owned) — then command 4 covers tracks 0/5/8 with the overlay + corridor in one dispatch.
- **(b) zero-code alternative:** `depth=track_scene_all` (12 shards; shard 8 covers tracks 8,20,32,44,56) — but note `COLLISION_OVERLAY` is bound to `track_scene_audit` ONLY (L246), so the all-depth runs WITHOUT the overlay; it serves as the all-tracks regression sweep, not the corridor check.

My corridor-drawing instrument resolves the route variant from the layer name (exact match, else first name CONTAINING the layer name — "hockenheim" → `HOCKENHEIM` (8) wins over HOCKENHEIM NATIONAL/SHORT by index order; unmatched layers skip the corridor draw with a log line — no wrong-corridor risk).

## 3. Forest A/B — exact capture/comparison checklist additions (riding command 0's fence depth)

The forest envs CANNOT ride the yml env-block (the `MYSIM_*` strip rule above) — the only path is the python's per-call values. Proposal (coordinator one-liner in the fence branch, after the five existing modes — no new workflow inputs, the 25-input limit untouched):

```python
# after the existing five-mode loop, same run() config as the fence modes:
text=run('day0-filtered-fence-forest_tone',{**base_fence_vals,'MYSIM_FOREST_TONE':'1.35'},['--headless'])
text=run('day0-filtered-fence-forest_tone_breakup',{**base_fence_vals,'MYSIM_FOREST_TONE':'1.35','MYSIM_TREE_BREAKUP':'1'},['--headless'])
```

Frames and comparisons (all same run, same commit, same camera = matched by construction):

|comparison|frames|accept (human visual)|reject|
|---|---|---|---|
|tone A/B|`day0-filtered-fence-baseline.png` vs `-forest_tone.png`|treeline+trees read foliage-green (measured expectation: RGB (51,53,45)→≈(69,71,61)); sky-line contrast reduced but present; EVERYTHING else pixel-consistent with the baseline|any non-vegetation pixel changed; pastel/washed vegetation; the wall still reads black|
|breakup add-on|`-forest_tone.png` vs `-forest_tone_breakup.png`|skyline height varies per segment; the hard top edge breaks; base lines stay planted (no floating/sunk strips)|strips displaced horizontally (would indicate a transform bug — none should exist: scale-only), base gaps|
|scope proof|the audit JSON `forest-tone-hockenheim.json`/`-hatch.json`|lists EXACTLY the `org_brandshatch_tree_line_*`/`org_tree_*`/`org_shrub_*`/`org_bush_*` materials — no road/fence/grass-ground/crowd entries|any non-backdrop material in the list|
|night interplay (optional)|one night pair (TOD 3) with/without the tone|night vegetation does not glow/ghost against the dark sky|glowing foliage at night|

## 4. No-global-change guarantee (rides the SAME fence run — the strongest check in the battery)

1. **Pixel-identity of the five unchanged modes:** the baseline/miprepair/isolation renders of this run must be PIXEL-IDENTICAL to run 41843204651's decrypted set (`outputs/repair-20261029/mip3-41843204651/decrypted/review/*.png`) — same track/camera/commit-content, the new passes env-gated off in those modes. Any difference = a global leak → the whole candidate set is rejected. (Byte-compare first; the PNG encoder is deterministic for identical frames.)
2. **Log-absence check:** the five unmodified modes' logs must NOT contain `FOREST_TONE`/`GRASS_BALANCE`/`ROAD_VARIATION`/`TREE_BREAKUP`/`COLLISION_OVERLAY` lines (every pass early-returns without its env).
3. **Corridor scoping (unconditional code, scoped by design):** track 0's drawn corridor edges = 6.0 m half (`CorridorWidth(0)=12`) — same position as any prior overlay render; track 8's = 8.2 m half (the intended change). A track-0 overlay frame vs any prior one must show identical cyan placement.
4. **The trim path stays inert:** no `MYSIM_COLLIDER_TRIM` in any battery command → `trims=0` in every audit JSON (the removal mechanism exists but stays unused until evidence justifies named entries).

## 5. Commit

`src/WallBuilder.cs` (+7/−1: the env alias + the audit-path default) and this file. Balance 71/71 + 296/296; no CJK; default behaviour unchanged without the envs. No dispatch; no workflow edits; `bestLap=0` remains physics-gameplay's (unchanged note).
