# BATCH_REPORT — settings-persistence + reference-visual cycle

**Rolling report (latest block at top). Authoritative handoff: REVIEW_PACKET.md / REVIEW_REQUEST.json.**

## Block: visual cycle + fixture re-dispatch (live)
- `settings_persist_review` → run **41699253306** at `e1f6d21`: five isolated scenario boots (legacy/foreign/corrupt/malformed/failures), per-scenario `HOME`/`XDG_DATA_HOME` user-data isolation, JSON report gate (`passed>0`, `failed==0`, required case IDs, exit 0), `settings-persist-summary.json`. Fixture runs FIRST inside `_Ready` before any world/assets. **Awaiting cloud result.**
- `reference_evidence` → run **41700551316** at `91e6991`: car16/track0 seven fixed views (front/rear/left/right/top/front-wheel/cockpit) + two live drive frames (near chase + wide) through the existing CARVIEW/CAMPOS/SHOT harness; no material changes. **Awaiting cloud result.**
- Camera/code notes: current chase = len*1.5 back / hgt*~2.2-3.2 up / FOV→46° — "car small in captures" likely from the wide fence shots; decision deferred until the drive frames are actually viewed (no blind camera change).

## Block: persistence fixture rebuild (committed `e1f6d21`)
All SNAPSHOT_REVIEW findings addressed in code:
1. Node passes `WriteReport(path, report)` with the JSON string; exceptions exit 3/4 with diagnostics.
2. Audit branch keys on `SETTINGS_PERSIST_REVIEW` (workflow now exports it for the depth); stale `SETTINGS_PERSIST_SHARD` removed.
3. Duplicate private `run()` deleted; orchestrator uses its own named `scenario_boot` wrapper (home isolation + no screenshot requirements) around the SAME engine binary `g=GODOT_BIN`; no fake `/tmp/mysim/godot`, no `tools/SettingsPersistBoot.cs`.
4. No stray unconditional `print/sys.exit` outside the branch; other modes intact (F1_PAINT_REVIEW etc. preserved behind their own env gates).
5. Contract = structured JSON per scenario (`passed>0`, `failed==0`, required case-name list, process exit 0); missing/malformed/FAIL → orchestrator assert fails the job.
6. Real user-data isolation: per-scenario `HOME` + `XDG_DATA_HOME` (true Godot user-dir mechanism on Linux); Windows atomic-replace acceptance stays a separate later requirement (noted in REVIEW_PACKET; cannot be certified by Linux green).
7. Scenario = fresh PROCESS per legacy/foreign/corrupt/malformed/failures (no cross-scenario static-cache bleed).
8. Rename fault = env-gated seam swaps ONLY the rename target to an impossible path AFTER a successful tmp write (real failing syscall through the production `Set`), so tmp-exists and live-unchanged assertions are meaningful; temp-save fault = read-only directory BEFORE save (no tmp appears, live bytes captured before, compared after).
9. `Settings.Get<T>` and schema reads are type-safe now (VariantType-gated + try/catch → typed default; disk untouched by pure `Get`); foreign marker kept (`v>0` never rewritten to 2); `Set` returns `bool` so both failure stages are externally asserted.

## Prior blocks (condensed)
- A2C fence candidate rejected for adoption after image inspection; fence root cause remains unproven hypothesis; no more fence runs.
- Mesh 72/73 machine-screened (`machine_screened_only` labels in all ledgers); manual review pending.
- 4 fixture-compile failures preserved: `41675023687`/`41674946556`@`18f69a1`, `41688779865`@`9bed931`, `41645599180`@`1d5aed7` (each fixed with a distinct diagnosed change).
- Owner-credential dispatch authenticated via `gh api user` = `instagramshashank8-bot`, repo `admin`; earlier 403s were unset-`GH_TOKEN` shell ordering (self-inflicted, no permission changes).
