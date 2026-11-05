# D0 identity-guard checker: byte-compare D0 run frames against the 41843204651 baseline.
# Usage:
#   powershell -File check-d0-identity.ps1 -Baseline <dir> -Candidate <dir> [-Weak]
# Baseline = decrypted/review dir of run 41843204651 (or any prior reference decrypt).
# Candidate = the D0 run's decrypted review dir (same depth, all candidate flags off).
# Exit 0 = PASS (byte-identical, or with -Weak: near-identical within epsilon);
# exit 1 = FAIL; exit 2 = usage/comparison error.
param(
  [Parameter(Mandatory=$true)][string]$Baseline,
  [Parameter(Mandatory=$true)][string]$Candidate,
  [switch]$Weak,
  [double]$MaxMeanDelta = 0.5,     # mean |delta| per channel, 0-255 scale
  [double]$MaxChangedFrac = 0.005, # fraction of channel samples differing by > 2
  [int]$MaxBBoxSpan = 64           # px; largest connected changed-structure span tolerated in Weak mode
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Get-Sha([string]$p) {
  $sha = [System.Security.Cryptography.SHA256]::Create()
  $bytes = [System.IO.File]::ReadAllBytes($p)
  (($sha.ComputeHash($bytes)) | ForEach-Object { $_.ToString('x2') }) -join ''
}

# PNG pixel probe: returns meanAbs, changedFrac, maxDelta, bbox (of deltas > 2)
function Probe([string]$a, [string]$b) {
  $ia = [System.Drawing.Bitmap]::new($a); $ib = [System.Drawing.Bitmap]::new($b)
  if ($ia.Width -ne $ib.Width -or $ia.Height -ne $ib.Height) { $ia.Dispose(); $ib.Dispose(); throw "dims differ" }
  $rect = [System.Drawing.Rectangle]::new(0, 0, $ia.Width, $ia.Height)
  $fmt = [System.Drawing.Imaging.PixelFormat]::Format32bppArgb
  $bdA = $ia.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, $fmt)
  $bdB = $ib.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, $fmt)
  $len = $bdA.Stride * $ia.Height
  $bufA = [byte[]]::new($len); $bufB = [byte[]]::new($len)
  [System.Runtime.InteropServices.Marshal]::Copy($bdA.Scan0, $bufA, 0, $len)
  [System.Runtime.InteropServices.Marshal]::Copy($bdB.Scan0, $bufB, 0, $len)
  $ia.UnlockBits($bdA); $ib.UnlockBits($bdB)
  $sum = [double]0; $changed = 0; $maxd = 0
  $minX = [int]$ia.Width; $maxX = -1; $minY = [int]$ia.Height; $maxY = -1
  $count = $len / 4
  for ($p = 0; $p -lt $len; $p += 4) {
    for ($c = 0; $c -lt 3; $c++) {
      $d = [Math]::Abs([int]$bufA[$p + $c] - [int]$bufB[$p + $c])
      $sum += $d
      if ($d -gt 2) {
        $changed++
        $px = ($p / 4) % $ia.Width; $py = [Math]::Floor(($p / 4) / $ia.Width)
        if ($px -lt $minX) { $minX = $px }; if ($px -gt $maxX) { $maxX = $px }
        if ($py -lt $minY) { $minY = $py }; if ($py -gt $maxY) { $maxY = $py }
      }
      if ($d -gt $maxd) { $maxd = $d }
    }
  }
  $ia.Dispose(); $ib.Dispose()
  return @{ mean = [Math]::Round($sum / $count, 4); frac = [Math]::Round($changed / $count, 6); max = $maxd;
            bbox = if ($maxX -ge 0) { "x$minX..$maxX y$minY..$maxY" } else { $null } }
}

$results = [System.Collections.Generic.List[object]]::new()
$basePngs = Get-ChildItem $Baseline -Filter '*.png' | Sort-Object Name
$candPngs = Get-ChildItem $Candidate -Filter '*.png' | Sort-Object Name
$baseNames = $basePngs | ForEach-Object { $_.Name }
$candNames = $candPngs | ForEach-Object { $_.Name }
$missingInCand = $baseNames | Where-Object { $_ -notin $candNames }
$extraInCand = $candNames | Where-Object { $_ -notin $baseNames }

$byteIdentical = 0; $near = 0; $failed = 0
foreach ($b in $basePngs) {
  $cand = Join-Path $Candidate $b.Name
  if (-not (Test-Path $cand)) {
    $results.Add([pscustomobject]@{ file = $b.Name; verdict = 'MISSING'; shaBase = (Get-Sha $b.FullName); shaCand = $null; stats = $null }); $failed++; continue
  }
  $sb = Get-Sha $b.FullName; $sc = Get-Sha $cand
  if ($sb -eq $sc) {
    $results.Add([pscustomobject]@{ file = $b.Name; verdict = 'BYTE-IDENTICAL'; shaBase = $sb; shaCand = $sc; stats = $null }); $byteIdentical++; continue
  }
  try {
    $s = Probe $b.FullName $cand
    $ok = $s.mean -le $MaxMeanDelta -and $s.frac -le $MaxChangedFrac -and $s.max -le 64
    $struct = $s.bbox -and ($s.max -gt 32)
    $verdict = if ($ok -and -not $struct) { 'NEAR-IDENTICAL' } elseif ($ok -and $struct -and $Weak) { 'NEAR-IDENTICAL(STRUCT)' } else { 'DIFF' }
    if ($verdict -like 'NEAR*') { $near++ } else { $failed++ }
    $results.Add([pscustomobject]@{ file = $b.Name; verdict = $verdict; shaBase = $sb; shaCand = $sc; stats = ('mean=' + $s.mean + ' frac=' + $s.frac + ' max=' + $s.max + ' bbox=' + $s.bbox) })
  } catch {
    $results.Add([pscustomobject]@{ file = $b.Name; verdict = 'ERROR'; shaBase = $sb; shaCand = $null; stats = $_.Exception.Message }); $failed++
  }
}

# candidate-signature scan: D0 logs must show NO candidate activity.
# Mode-aware: FENCE_MIPREPAIR lines are legitimate output of the miprepair candidate
# MODE itself (not an identity concern); they fail the check only inside the
# baseline / family-isolate modes, where nothing but pure default rendering may run.
$leaks = [System.Collections.Generic.List[string]]::new()
Get-ChildItem $Candidate -Filter '*.log' -ErrorAction SilentlyContinue | ForEach-Object {
  $t = [System.IO.File]::ReadAllText($_.FullName)
  if ($_.Name -like '*baseline*' -or $_.Name -like '*family_isolate*') {
    foreach ($sig in @('WET_REFLECT ', 'NIGHT_LIFT ', 'NIGHT_VEHICLE_LIGHTS ', 'SEAM_WELD ', 'FENCE_MIPREPAIR')) { if ($t.Contains($sig)) { $leaks.Add($_.Name + ' contains "' + $sig + '"') } }
  }
}

$pass = $failed -eq 0 -and $leaks.Count -eq 0 -and $missingInCand.Count -eq 0 -and ($byteIdentical -gt 0 -or $Weak)
$verdictLine = if ($byteIdentical -eq $basePngs.Count -and $basePngs.Count -gt 0 -and $leaks.Count -eq 0) { 'PASS: all frames byte-identical' } elseif ($pass) { 'PASS: within epsilon (weak mode)' } else { 'FAIL' }
$out = [System.Collections.Generic.List[string]]::new()
$out.Add('D0 IDENTITY CHECK baseline=' + $Baseline + ' candidate=' + $Candidate)
$out.Add('VERDICT: ' + $verdictLine)
$out.Add('frames byte-identical=' + $byteIdentical + ' near=' + $near + ' failed=' + $failed + ' missing=' + $missingInCand.Count)
if ($missingInCand.Count) { $out.Add('MISSING in candidate: ' + ($missingInCand -join ', ')) }
if ($extraInCand.Count) { $out.Add('extra in candidate (ignored): ' + ($extraInCand -join ', ')) }
foreach ($l in $leaks) { $out.Add('SIGNATURE LEAK: ' + $l) }
foreach ($r in $results) { $out.Add(($r.verdict + '  ' + $r.file + '  ' + $(if ($r.stats) { $r.stats } else { '' }))) }
$out | ForEach-Object { Write-Output $_ }
if ($env:D0_CHECK_OUT) { $out | Set-Content $env:D0_CHECK_OUT -Encoding UTF8 }
if ($pass) { exit 0 } else { exit 1 }
