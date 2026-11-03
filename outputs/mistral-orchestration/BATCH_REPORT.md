# Batch report — 2026-11-03 (session 2, Mistral Vibe, sole worker)

## 1. Auth diagnosis (supervisor point 1) — RESOLVED
- `gh api user` under owner GH_TOKEN = `instagramshashank8-bot`; repo collaborator permission `admin` (admin:true, maintain/push/pull true). Owner credential is valid and sufficient.
- Inherited GH_TOKEN/GH_HOST: none at time of failure (GH_TOKEN len 0 in a fresh shell; no GH_HOST set). Exact failed command: `gh workflow run game-review.yml -R instagramshashank8-bot/mysim-godot --ref codex/repair-20261029 -f depth=fence_hash_review ...` run AFTER `unset GH_TOKEN` in the same command line, so gh fell back to the active default keyring account (Jdiejei) which lacks repo admin. Root cause: shell ordering, not missing permissions; no permission change requested.
- Earlier owner dispatches (41633324944/41632524155 era) succeeded with the same command shape when GH_TOKEN stayed exported across the whole line.

## 2. Naming correction (supervisor point 2) — CORRECTED
- `AlphaToCoverageAndToOne` is alpha-to-coverage with alpha forced to one over 4x MSAA — NOT alpha hashing. Claims corrected: depth renamed `fence_a2c_review`, candidate `fence_a2c_candidate`, local `a2cFence`. API semantics verified against GodotSharp 4.7.2 XML: `AlphaToCoverage` passes alpha to the AntiAliasing sample mask; `AlphaToCoverageAndToOne` additionally forces non-zero alpha to one. `anti_aliasing/quality/msaa_3d=3` (4x MSAA) already set in project.godot, so the sample mask is real. No alpha hashing is claimed or used.

## 3. Review-class correction (supervisor points 3+3) — CORRECTED
- `MESH000{72,73}_QUANT_OVERVIEW.json`: `review_class` set to `machine_screened_only` with an explicit note: quantitative tile metrics only, NO manual image inspection, does not count toward manual review coverage. `itemsFlagged` wording corrected to machine flags. Prior inflated wording removed; history and originals preserved. Parts 72/73 remain opt-in candidates pending true manual image inspection.
- Corrections were applied without deleting duplicate-prose history from earlier entries; counts were not re-inflated.

## 4. Substantive fixes retained
- 41632524155 rejection stands with metrics retained; fence root-cause description stands; 403 root cause corrected to shell ordering, not a permissions gap; no permission change requested.

## 5. Review request
- Independent review requested on: auth diagnosis (token identity/permission/admin), A2C naming correction, machine-screen relabel, Settings migration.

## 6. Follow-up queue (needs admin or D:)
- Part 74+ rewrap runs (还得 admin) — hold.
- Enhanced-candidate provenance verification vs unchanged-copy history — hold.

## Fence A2C outcome
- Run 41647350933 SUCCESS at 6d2069a: A2C candidate applied (780 surfaces, fence band 8-26pct/cell) but fence-crop edge density unchanged in 8/9 changed cells -> did NOT reduce speckle; REJECTED for adoption, kept recorded opt-in. Speckle is geometry-level (sub-pixel wires + scissor over mips): next fix must be mesh/texture-level (coverage-preserving mip rebuild / wire thickening), cloud-only. Details: FENCE_A2C_VERDICT_QUANT.json.

## Priority1 outcome (b0fa604)
- Settings.cs: BOTH unsafe direct cf.Save(Path) fallbacks removed (temp-write failure and rename failure now report and return with the old file intact); live path is GlobalizePath (absolute); tmp is an absolute same-directory sibling (same volume), kept on failure. Valid legacy file without schema_version preserved in place; marker stamped on next save only. Verified: single cf.Save call (tmp only), no cf.Save(P) anywhere, single Set/ApplyAudio, balanced braces.
- SettingsPersistChecks.cs landed: aligned to Variant.As<T>() (no AsInt32/AsBool/AsDouble per 4.7.2 API).
- cloud persistence tests: legacy typed keys (int/bool/string/double), foreign schema kept, first Set stamps version, corrupt boot typed defaults + file present, typed round-trip, REAL rename failure via read-only dir on Linux (old file byte-identical, tmp recovery kept), atomic success replace + no stray tmp. Pending cloud verify.
