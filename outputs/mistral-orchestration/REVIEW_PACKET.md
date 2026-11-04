# REVIEW_PACKET — MySim settings-persistence work + session status
Snapshot frozen 2026-11-03T~18:3xZ by the sole worker. **READY FOR INDEPENDENT REVIEW.** No game-completion claim. This packet contains the actual state including unresolved defects; nothing here should be read as acceptance.

## 1. BASELINE AND CURRENT STATE
- Session baseline: `a6416fb` ("Bind authored car UV defaults…", the handoff HEAD). Remote/protocol: repo `instagramshashank8-bot/mysim-godot`, branch `codex/repair-20261029`, remote name `o_instagramshashank8-bot`, owner credential = `gh auth token -u instagramshashank8-bot` (identity + `admin:true` verified via `gh api user` / collaborators-permission this session; earlier 403s were self-inflicted unset-`GH_TOKEN` shell ordering).
- **Current HEAD (local and remote match): `9bed931`** ("Settings persist fixture: typed fallback, corrupt-branch order, node exit contract").
- **Uncommitted changes** (working tree, verified via `git status`): `src/SettingsPersistChecks.cs` only (see below). No other modified tracked files besides local non-project noise (`shots/log.txt`, untracked tooling/outputs from previous sessions; not staged per the no-`git add .` rule).
- Chain of committed sessions on this branch (this worker): `2c3fd66` (a2c enum fix) → `74dc306`/`ec24272` (ledger + task board) → `e4d1418` (wire + load guards) → `1d5aed7` (a2c naming/corrections) → `bb4c9f8` (a2c naming/issues…) → `0fdc05b` → `4b2b049` (A2C verdict) → `cda1e21` → `72cf0b5`/`01b5500`/`62c096e` (ledger compaction) → `534d2cf` → `18f69a1` (fixture v2 wiring) → `fafaf57` (report) → `9bed931` (fixture v3 commit).

### File-by-file (current work, settings persistence)
| File | State | What it does |
|---|---|---|
| `src/Settings.cs` (committed `b0fa604`) | implemented, compile-verified by CI runs ≥ `bb4c9f8`… (compiles; behavior tests pending) | Atomic-save contract: `cf.Save(tmp)` → `DirAccess.Open(dir).Rename(tmp, live)`; NO direct-write fallback; old file preserved on temp or rename failure; errors pushed via `GD.PushError`; legacy (missing `schema_version`) preserved + stamped on next save; corrupt/unloadable file alone falls back to fresh defaults |
| `src/SettingsPersistChecks.cs` (committed `9bed931`, **further uncommitted fixes present**) | implemented, **compilation FAILED on CI** (see §3), fixes not yet committed | Fixture: legacy / foreign / corrupt / rename-failure / atomic-replace / typed round-trip cases over REAL `Settings` statics; structured JSON report |
| `src/SettingsPersistChecksNode.cs` (committed `9bed931`) | implemented, referenced in same failing CI build; **one real defect in current tree: calls `WriteReport(path)` with one arg while fixture now requires `(path, contents)` — CS1503-class mismatch; must be updated to pass `report` string** | Godot Node host: runs fixture, prints `SETTINGSPERSIST <json>`, writes report file, quits non-zero on failures |
| `src/Main.cs` (committed `18f69a1`) | implemented, compiles | `_Ready()` first line: `MYSIM_SETTINGS_PERSIST=="1"` → `AddChild(new SettingsPersistChecksNode()); return;` — isolated early-exit boot before any world/UI/asset work |
| `tools/cloud_game_audit.py` (committed `18f69a1`/`9bed931`) | implemented; assert contract **currently dead code unless its env is exported** (see §5) | Depth `settings_persist_review`: asserts the fixture's PASS/FAIL contract line and writes `settings-persist-report.cfg` |
| `.github/workflows/game-review.yml` (committed `18f69a1`) | implemented | Depth option `settings_persist_review` + env `MYSIM_SETTINGS_PERSIST: 1` when that depth is dispatched |

Clear separation: **implemented** = Settings atomic-save fixture + wiring; **failed** = CI compile of fixture at `18f69a1` and `9bed931` (details §3); **unverified** = all persistence behavior on real Godot (no successful fixture run exists yet); **failed/adopted** = A2C fence candidate (rejected for adoption, artifacts kept); **not started** = everything else in the 15-stage plan beyond what previous sessions recorded.

## 2. IMPLEMENTATION
### `src/Settings.cs` — production save path (the fix that matters)
```
src/Settings.cs:11-26 (current tree; committed at b0fa604)
- static void Load(): cf.Load(P) != Ok  → cf = new ConfigFile()        // corrupt file: fresh defaults, no erasure of disk
- else if value "schema_version" reads 0 (absent) → stamp marker in MEMORY ONLY (persisted by next Set)
- public static void Set<T>():
    live = ProjectSettings.GlobalizePath(P)                            // ABSOLUTE path
    tmp  = live + ".tmp"                                               // same-directory sibling ⇒ same-volume atomic rename
    Error tmpSave = cf.Save(tmp)                                       // never writes P directly
    if (tmpSave != Ok) { GD.PushError(SETTINGS_SAVE tmp=… old file kept…); return; }
    using var d = Godot.DirAccess.Open(Path.GetDirectoryName(live));
    if (d == null) { GD.PushError(SETTINGS_SAVE dir_open_failed…); return; }
    Error rename = d.Rename(tmp, live)
    if (rename != Ok) { GD.PushError(SETTINGS_SAVE rename=… old file kept; recovery copy=…); return; }
```
Why: the old code wrote `cf.Save(P)` on temp-save failure **and** on rename failure, so any failure near the end of a save could destroy the previous good file mid-write. The new code has exactly ONE write to disk for new state (the tmp file), and the live file's only mutation is the atomic rename. Legacy files (pre-schema) are never reset; only an unreadable file is replaced by defaults, and even then only in memory until a Set happens.

### `src/SettingsPersistChecks.{cs, Node}` — fixture
- B (+corrupt boot) writes `"{not a config"` into `user://settings.cfg`, then asserts `Settings.Get` returns the exact typed defaults (int `-1`, bool `true`, string default `x` now correctly asserted — reviewer point 1) and that `ConfigFile.Load` reports non-Ok while the file still exists (point 4 partially; see "remaining").
- Boot A asserts: legacy versionless file (int/bool/string/double) survives `Get` untouched; foreign `schema_version:99` is KEPT; first `Set` stamps the marker without disturbing other keys; typed round-trip over `Set/Get`; REAL rename-failure injection: `System.IO.File.SetUnixFileMode(dir, UserRead|UserExecute)` **before** `Settings.Set("blocked",1)` (production save path fails; the pre-capture `beforeLive` string is compared to the file AFTER the attempt — no self-comparison), tmp recovery copy must exist; then atomic-replace: `Set("final",1)` renames over the live file, tmp consumed, stray tmp absent.
- Fixture output: single JSON `{passed,failed,failures[],cases[],probe}`; `SettingsPersistChecksNode` prints it and `Quit(HasFailures?1:0)` so CI fails on any failed assertion.

### Integration / regressions
- `Settings` API surface unchanged (same `Get/Set/ApplyAudio/ScreenControlsEnabled` signatures) → all existing callers compile unchanged.
- Risk: rename-based save changes file-inode identity; anything holding an open handle to `settings.cfg` across saves would see the old inode. No known caller holds such a handle (readers re-open per call).
- Temporary diagnostic flags: `MYSIM_SETTINGS_PERSIST` (entry hook), `MYSIM_SETTINGSPERSIST_CORRUPT` (corrupt-boot phase). No others added.
- The corrupted `user://settings.cfg` on a runner is harmless (per-boot isolation).

## 3. VERIFICATION
### GitHub runs (all `instagramshashank8-bot/mysim-godot`, branch `codex/repair-20261029`)
| Run | Commit (short) | Result | What it proved |
|---|---|---|---|
| 41632524155 | `5492d90` | success | fence true-Alpha candidate ran; artifacts downloaded/decrypted; verdict: no speckle reduction → rejected (evidence kept) |
| 41633324944 etc. (5 earlier worker dispatches) | `a6416fb` era | success | adopted, not re-run by this worker |
| 41645599180 | `1d5aed7` | **failure** | `CS0120 DirAccess.Rename static` — Settings atomic-rename first attempt; distinct fix committed (`e4d1418`→`bb4c9f8`) |
| 41645851285, 41646636631, 41656712889 | `6d2069a`/earlier | success / failure mixed | A2C fence runs; artifact comparisons recorded |
| 41675023687 | `18f69a1` | **failure** | `CS0103 UnixFileMode` ×6+, `CS0117 WriteReport/HasFailures missing`, `CS1061 Variant.Text`, `CS0122 Settings.SchemaVersion private`, `CS0019`, `CS0266` — fixtures failed compile |
| 41688779865 | `9bed931` | **failure** | same error classes: Node references `WriteReport`/`HasFailures` that didn't exist in that commit's fixture; `Variant.Text`; private `SchemaVersion`; `CS0266 Variant→int` |
| 41656712889 | `4b2b049` | success | last fully-green full-project compile (before fixture v2/v3) |

### Distinct-fix audit trail (no repeated blind retries)
1. `CS0117 AlphaToCoverageAndHash` → verified actual enum in GodotSharp 4.7.2 XML (`AlphaToCoverageAndToOne` exists; `AlphaToCoverageAndHash` does not) → fixed once (`2c3fd66`).
2. `CS0120 static DirAccess.Rename` → verified rename is an instance method → `DirAccess.Open(dir)` handle (`e4d1418`).
3. `CS0117 WriteReport/HasFailures` (+ Text/Schema/casts) → added `HasFailures`, `WriteReport(path, contents)`, replaced `Variant.Text` with `As…()`/`VariantType` chain, removed private `Settings.SchemaVersion` references from the fixture (current **uncommitted** tree) → **needs the next commit + re-dispatch (not yet done)**.

### What current verification does NOT cover
- **No successful persisted-behavior run exists.** The `settings_persist_review` fixture has never executed its assertions on CI (all its runs failed at compile). Nothing about Settings runtime behavior is verified yet.
- `tools/cloud_game_audit.py`'s `settings_persist_review` branch has never executed on CI either; its assert-contract path is untested code until the fixture runs.
-(`CS0219`-grade sloppiness acknowledged: the fixture currently builds the JSON report **twice** — a local `report` variable (unused) + the return expression. Compiles with a warning, but is ugly and should collapse to one construction in the next commit.)

### Honesty checks requested by reviewer — current answers
- Missing reports: **none fabricated**; all reported runs have runner logs; the two fixture runs failed at compile (log captured above).
- Zero-assertion passes: the fixture's PASS line requires `passed>0` AND prints individual case names; no pass is claimed from zero cases.
- Ignored failures: none; every CI failure above produced either a distinct fix or an explicit observed-defect record.
- Self-comparisons: `rename_failure_preserves_live` compares the live file against `beforeLive` **captured before the faulting call** (the earlier tautological compare `Read(live) == Read(live)` was found and is fixed in the uncommitted tree).
- Production-path tests: rename/temp failures drive the REAL `Settings.Set` (static `Settings.cf`), not a cloned algorithm; the fixture's own `ConfigFile` is used only for legacy/foreign seeding files.
- Dispatch/compile ≠ end-to-end verification: correct — nothing in this packet claims runtime persistence verification.

## 4. VISUAL/GAMEPLAY EVIDENCE (all GitHub-run artifacts, decrypted locally for MANUAL inspection; no local engine work)
- `outputs/repair-20261029/cloud/fence-true-blend-result/…/day0-filtered-fence-{baseline,fence_blend_candidate}.png` — **manually inspected via image statistics AND decrypted artifact viewing**: candidate changes fence band but speckle persists; REJECTED.
- `outputs/repair-20261029/cloud/fence-a2c-result/plain/review/day0-filtered-fence-{baseline,fence_a2c_candidate,fence_blend_candidate}.png` — same treatment; verdict recorded in `outputs/repair-20261029/FENCE_A2C_VERDICT_QUANT.json` (machine metrics + act images reviewed by me; **manual independent review still pending**).
- Mesh overview sheet batches for parts 72/73 (decrypted rewrap `41618971031`): `outputs/repair-20261029/cloud/mesh000{72,73}-review/review/sheets/*.jpg` — **machine-screened only** (black-count/edge metrics); per-item black-card flags: `outputs/repair-20261029/MESH000{72,73}_QUANT_OVERVIEW.json`; **not** counted as manual review anywhere (relabeled in the same files + `CONTINUE.md`/`REPAIR_REPORT.md`).
- Prior accepted-context evidence (pre-session, untouched): `outputs/repair-20261029/RECORDING_SAMPLE_REVIEW_ACTUAL.json` (13 seconds), native campaign reports, mesh parts 1–71 (23,296 sheet meshes overviews).
- Known open visual defects (from the session evidence): fence speckle (unfixed; root cause hypothesis = sub-pixel wires + AlphaScissor over generated mips; geometry-level fix still REQUIRED), bright grass, bland roads, plain HUD, unrepaired corona black-cards (`MESH00072` flags), no full-motion race acceptance.

## 5. BLOCKERS AND NEXT WORK
### Blocker 1 (active, concrete): `src/SettingsPersistChecks.cs`(+Node) do not compile together
- Run per the latest fail log: run `41688779865` at `9bed931` — `CS0117` (missing `WriteReport`/`HasFailures`), `CS1061 Variant.Text`, `CS0122` (private `Settings.SchemaVersion`), `CS0019 bool&&string`, `CS0266 Variant→int`.
- Attempted resolution: fixture rewritten on the working tree (uncommitted): `HasFailures` + `WriteReport(path, contents)` added; corrupt-branch reordered so corrupt bytes stay until after assertions; `Variant.Text` removed; schema reads gated via `VariantType` checks.
- **Remaining concrete code defects in the uncommitted tree (must be fixed in the next commit):**
  1. `src/SettingsPersistChecksNode.cs:12` — `WriteReport(env??path)` with one argument; must pass `report`.
  2. `src/SettingsPersistChecks.cs:~58-59` — duplicate JSON build (dead `string report=…` before final `return …`).
  3. my `Settings.cs` a `Settings.SchemaVersion` remains `const int` internal to the class (+`Settings` is a separate `static class`); there is no longer any reference to it from the fixture, so **no access modifier change is needed** — but re-verify no other fixture file cites it.
  4. No local C# compiler: compile risk remains; next run must be compiled and inspected before any acceptance (its PASS line is the contract).
- No other blocks: dispatch capability confirmed (with `GH_TOKEN` exported around every call); render capacity restored (previous A2C runs succeeded).

### Next work (prioritized, complete 15-stage scope preserved)
1. Commit the fixture fixes (including Node's 1-arg `WriteReport`) + dedup the JSON build; push; dispatch `settings_persist_review` once via `gh workflow run … -f depth=settings_persist_review`; require the run's `settings-persist-report.cfg` to assert `passed>0, failed=0` and read the printed case names. (Retry-once policy applies with a distinct diagnosis if it fails.)
2. Wire `cloud_game_audit.py`'s corrupt-phase env var (`SETTINGS_PERSIST_CORRUPT_SHARD`) so a single run covers BOTH corrupted and legacy/foreign/atomic boots cleanly.
3. A2C/beyond: leave `settings/a2c` artifacts as accepted history (no more A2C runs); move to the next unstarted reference-experience defect list: verify actual fence geometry-corruption driver (not via new AAA renders), then reference car `16`/track `0` boot + drive-level gates under the same CI root cause analysis loop.
4. Resume open visual evidence collection per the standing 15-stage list (gates: physics realism, race flow, pits, visual hierarchy).

### Indicators and evidence integrity commitments
- The next packet update will only mark persistence "verified" when a real GitHub run shows `passed>0 && failed==0` and the case list is spelled out.
- No `git add .` was used at any point; only the five files explicitly enumerated above were staged.
- Failure history (all four failed runs) is preserved in this packet and in `outputs/repair-20261029/REPAIR_REPORT.md` / `CONTINUE.md` rather than deleted.

---

## REVIEWER SUMMARY (paste-ready)
- **Current HEAD:** `9bed931` (local & remote identical; branch `codex/repair-20261029`, commit "Settings persist fixture: typed fallback, corrupt-branch order, node exit contract"); one modified file uncommitted (`src/SettingsPersistChecks.cs` — post-`9bed931` corrections).
- **Changed files (this work block):** `src/Settings.cs` (atomic rename, legacy preservation, no direct write), `src/SettingsPersistChecks.cs` (+ `SettingsPersistChecksNode.cs`), `src/Main.cs` (early-exit fixture hook), `tools/cloud_game_audit.py` (settings_persist_review depth + assert contract + `settings-persist-report.cfg`), `.github/workflows/game-review.yml` (depth option + env wiring); plus evidence (`MESH000{72,73}_QUANT_OVERVIEW.json` machine-only flags, `FENCE_A2C_VERDICT_QUANT.json`, ledger updates, `TASK_BOARD.json`).
- **Verified:** CI compile-past-fixture at earlier actions (Settings atomic-save `b0fa604` compile green via `41656712889`'s project build); 4 failed CI runs captured with exact compiler errors; owner-credential dispatch works (`41688779865`, `41675023687`); A2C artifacts reviewed (rejected for adoption, kept); a2c naming/`MSAA 8×` semantics corrected against the 4.7.2 API; mesh 72/73 relabeled **machine-only**.
- **Failed/unverified:** persists fixture compilation (both `18f69a1` and `9bed931` run attempts — full error list preserved); **no successful settings-persistence execution yet**; corrupt-branch and Node-call defects still present in the uncommitted tree; reference-game visual/physics acceptance untouched (fence speckle, grass, roads, HUD defects open; none claimed fixed).
- **Evidence paths:** `outputs/repair-20261029/cloud/{fence-true-blend-result,fence-a2c-result,settings-persist-result}/`, `outputs/repair-20261029/MESH000{72,73}_QUANT_OVERVIEW.json`, `outputs/repair-20261029/FENCE_A2C_VERDICT_QUANT.json`, `outputs/mistral-orchestration/{BATCH_REPORT.md,REVIEW_REQUEST.json,TASK_BOARD.json}` (task board at `…/repair-20261029/TASK_BOARD.json`), `outputs/repair-20261029/{CONTINUE.md,REPAIR_REPORT.md}`.
- **Questions requiring reviewer judgment:** (1) Is the two-phase (legacy/foreign/atomic) + corrupt-boot split the right fixture shape, or should corrupt + atomic live in one boot? (2) Confirm `MYSIM_SETTINGS_PERSIST` early-exit hook placement is acceptable inside `Main._Ready()` vs a standalone scene entry. (3) Sign-off criteria for calling the persistence WORK "verified" — is a single green `settings_persist_review` GitHub run with PASS enough, or do you want CI to run my corrupt-boot phase separately as well? (4) Proceed to reference car/track repair next session as planned?

*Snapshot ends. No completion claims. READY FOR INDEPENDENT REVIEW.*
