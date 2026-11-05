# track-render turn 26 — D0 identity gate: OWNER RUNBOOK (final form, execute verbatim)

One page. Every command copies as-is into the owner's PowerShell at the clone root of `instagramshashank8-bot/mysim-godot`. Scope note: the frame-hash manifest leverages the merged yml hook (lines ~298-299: `find outputs/cloud-game ... -exec sha256sum {} + | sort > frame-hashes.log`) and the public `game-runtime-*` artifact (uploads `*.log`). The container round-trip is byte-preserving (verified: `tools/cloud_encrypt_output.py` = uncompressed tar + AES-GCM stream; no transcode), so hashes of decrypted frames equal hashes the runner computed pre-encryption.

## STEP 0 — pre-flight

```powershell
git pull origin main
git rev-parse --short HEAD          # expect the current integration line (>= c677943); if older, STOP
Test-Path outputs/mistral-orchestration/agents/track-render-turn24-d0-manifest.csv   # must print True; if False, STOP
```

## STEP 1 — dispatch (the Q1 command, unchanged: no env, no extra inputs)

```powershell
gh workflow run game-review.yml --repo instagramshashank8-bot/mysim-godot --ref main -f depth=fence_miprepair_review
```

## STEP 2 — run id + wait to terminal state

```powershell
$RUN = (gh run list --repo instagramshashank8-bot/mysim-godot --workflow=game-review.yml -L 1 --json databaseId --jq '.[0].databaseId')
gh run watch $RUN --repo instagramshashank8-bot/mysim-godot --exit-status
```

Note: the fence-depth run historically terminates `failure` AFTER rendering (the archival yml-side assert behavior was since fixed); the hash check below is valid whenever the `game-runtime-0-attempt-1` artifact EXISTS, regardless of the job's conclusion line.

## STEP 3 — download the public runtime artifact

```powershell
if (Test-Path out/d0) { Remove-Item out/d0 -Recurse -Force }
gh run download $RUN --repo instagramshashank8-bot/mysim-godot -n game-runtime-0-attempt-1 -D out/d0
Get-Content out/d0/outputs/cloud-game/frame-hashes.log -ErrorAction Stop
```

The last command must print 5 lines (`<sha256>  outputs/cloud-game/<frame>.png`). If `frame-hashes.log` is missing, the dispatch predates the hook merge - STOP and re-dispatch from this head.

## STEP 4 — hash compare against the turn-24 manifest (the verdict)

```powershell
$man = Import-Csv outputs/mistral-orchestration/agents/track-render-turn24-d0-manifest.csv
$row = @{}
Get-Content out/d0/outputs/cloud-game/frame-hashes.log | ForEach-Object {
  $p = $_ -split '\s+', 2
  if ($p.Count -eq 2) { $row[(Split-Path $p[1].Trim() -Leaf)] = $p[0] }   # strip the outputs/cloud-game prefix
}
$miss = @($man | Where-Object { -not $row.ContainsKey($_.file) })
$diff = @($man | Where-Object { $row.ContainsKey($_.file) -and $row[$_.file] -ne $_.sha256 })
if ($miss.Count -eq 0 -and $diff.Count -eq 0 -and $row.Count -eq $man.Count) {
  'D0 HASH VERDICT: PASS (5/5 byte-identical to run 41843204651 baseline)'    # -> proceed to N0
} else {
  "D0 HASH VERDICT: FAIL  missing=$($miss.Count) diff=$($diff.Count) logRows=$($row.Count)"
  $miss | ForEach-Object { 'MISSING ' + $_.file }
  $diff | ForEach-Object { 'DIFF    ' + $_.file + ' baseline=' + $_.sha256.Substring(0,12) + ' candidate=' + $row[$_.file].Substring(0,12) }
}
```

## STEP 5 — verdict rules

- **PASS** (all 5 names present, all hashes equal, no extras): the matrix's identity gate holds; record `[run id] D0 PASS` in the matrix thread and start N0.
- **DIFF** (same frame name, different hash): a rendered-content change at default flags. Do NOT proceed. Do NOT locally interpret further unless continuing - the escalation is the full check below.
- **MISSING / row-count mismatch** (extra or absent frames): the mode/manifest set drifted from the 41843204651 baseline shape. Baseline re-cut decision belongs to the coordinator; the runbook does not waive.
- Either failure first gets the one-question triage: did anything in the toolchain change since the baseline run (runner image / binary / intentional merge)? No = the change is ours; treat as a regression signal.

## ESCALATION (only on DIFF): full pixel check (owner key path)

```powershell
gh run download $RUN --repo instagramshashank8-bot/mysim-godot -n encrypted-review-0-attempt-1 -D out/d0-enc
python tools/cloud_decrypt.py out/d0-enc/game-review-$RUN-1-0.enc out/d0-dec
$env:D0_CHECK_OUT = 'out/d0-verdict.txt'
powershell -File outputs/mistral-orchestration/agents/check-d0-identity.ps1 `
  -Baseline outputs/repair-20261029/mip3-41843204651/decrypted/review `
  -Candidate out/d0-dec/review
```

Exit 0 = PASS (pixel-identical + signature-clean); exit 1 = read `out/d0-verdict.txt`: the per-frame `bbox=` locates the damage, `SIGNATURE LEAK` lines mean the dispatch carried a flag (fix the dispatch, not the code). The failure taxonomy lives in `track-render-turn24.md` section 5.

## INVARIANTS (unchanged)

- One D0 = one run id; a replacement dispatch is a new id and a new verdict.
- PASS = byte equality on all five frames + a clean signature scan; no partial or lazy acceptance.
- Runner-image or engine-binary changes require re-baselining the manifest first (never waiving).
- No acceptance claims anywhere until this gate passes.
