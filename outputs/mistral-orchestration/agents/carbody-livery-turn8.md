# carbody-livery — turn 8 (merge confirmation + the car16 INT group)

Author: carbody-livery specialist. Branch `mistral/carbody-livery`. **No dispatches.** Context honoured: fork pushes parse-clean since the 25-input fix (the last parse failure = 41905184212); the six-command owner battery (now seven with the axle command) remains the only validation path; no acceptance claims — parser-clean ≠ game verification.

## 1. 8b01292 merge confirmation (mechanical, `git merge-tree` vs `654d7eb`)

- **The inventory TOOL merges clean** — the fork's copy is an older revision; my deltas (manifest mode, six-field schema, annotations) apply without conflict.
- **The six inventory DATA files conflict add/add** — the fork carries the OLD five-column `dc8e061` canonicals (verified: zero `orientationClass`/`defectTaxonomy` content in its `car16.md`; its tool has no `orientation_class`/`DOCUMENTED_FINDINGS`). **Resolution: take this branch's versions** — they are a strict superset (every old column survives: px/bytes/import/bindings/repairs all present in the new rows). Exact commands for the coordinator:

```bash
git merge mistral/carbody-livery          # conflicts only on the six inventory files
git checkout --ours outputs/mistral-orchestration/inventory/car16.md outputs/mistral-orchestration/inventory/car16.json \
                     outputs/mistral-orchestration/inventory/car6.md outputs/mistral-orchestration/inventory/car6.json \
                     outputs/mistral-orchestration/inventory/track0_brands_hatch.md outputs/mistral-orchestration/inventory/track0_brands_hatch.json
git add -A outputs/mistral-orchestration/inventory && git commit
```

- No other conflicts exist in the tree (verified: the merge-tree conflict list = the six inventory files only).

## 2. The car16 INT group (the cockpit-usability delegation — texture side)

The inventory now carries a **groups.int rollup — "cockpit-readable surfaces"** — membership rule: a texture joins only if ALL of its `_BaseMap` bindings are interior-family renderers (`_mm_cab`/`_mm_int`/`_mm_sw`/`LOD_A_INT_`/`LOD_A_STEERING`):

| Texture | px | Binding | orientationClass | defect | correction | Readability note |
|---|---|---|---|---|---|---|
| `cab.png` | 512×512 | `INTERIOR_mm_cab` + `STEERING_WHEEL_mm_cab` | mirrored-X (default transform) | **mirrored-UV** + `low-res-source` (secondary) | uv-remap-per-surface / source-art-fix | the dash + wheel-face text renders MIRRORED at the seated-eye view; instrument glyphs land at a few px at 512² — matches the reviewer's "instrument display not readable" |
| `int.png` | 512×512 | the `LOD_A_INT_*` family | mirrored-X | mirrored-UV + low-res-source | same | the interior cage; visible only in cockpit mode (pending the cockpit-audit visibility extension) |
| `sw.png` | 1024×1024 | the `SHIFT_LIT_01..11` LED segments + `LED_EMISSIVE` | mirrored-X | mirrored-UV (low impact: LEDs non-directional; wheel-face labels directional) | uv-remap-per-surface | the shift-light strip = the brightest texture; labels small |

Three statements of limit, stated plainly:
1. **The INT-family UV candidates are NOT staged** — the same H1 fix pattern (V-only per surface) applies mechanically, but the EVIDENCE for each interior surface does not exist: every assembly audit to date covers **visible-only surfaces**, and the `LOD_A_INT_*` meshes are hidden in those runs. The cockpit-usability owner's audit extension (emit INT-surface UV summaries in the cockpit state) is the gate; the cab-family candidates then stage exactly like the badges did.
2. **The visibility dependency is upstream of me:** `CarLoader` hides ALL `LOD_A_INT_*` unconditionally — whether the cockpit should show the interior (and which parts) is the cockpit-usability owner's call; my table gives them the texture facts to decide with.
3. The A/B comparison machinery from P1 (the leg pairs + the local decryption) applies unchanged once the candidates exist — the turn-4 verdict-table pattern generalises to the interior renderings.

## 3. Bookkeeping

- `car16.md/json` regenerated with the INT-group section + the new cab/int/sw findings (15 rows; all six fields; the group rollup).
- `FLEET_MANIFEST.md` §3.0: the merge-status row + the INT-group row added.
- No dispatches; no data-file changes; the only network operations = read-only (`git fetch`, `merge-tree` is local).

## 4. Open

- The merge itself (the coordinator; the resolution commands above).
- The cockpit-audit extension (the cockpit-usability owner) → then the cab/int/sw candidates stage.
- The owner battery (7 commands incl. the axle A/B riding `wheel_fit_review` per the fork's `56b40d9`/`654d7eb`) — the only validation path; nothing here counts as verified until those run.
