# Recreates the 4 feature worktrees (gps/pcs/bms/history) for THIS clone.
# Worktrees are per-machine git metadata: a clone never includes them, so run
# this once after cloning (safe to re-run any time - every step is idempotent).
#
#   1. git worktree add ..\<repo>-<feature>  (skipped if already registered)
#   2. NTFS junctions in each worktree: packages\ and EMS_PJT_Hamburger\Maps\tiles\
#      -> both point into this clone (packages/tiles are not tracked by git)
#   3. Patches the absolute worktree paths in .claude/skills/multi-task/workflow.js
#      and .claude/CLAUDE.md to match THIS clone's location.
#
# Prerequisites on a fresh clone (warned about, not fatal):
#   - packages\  : restore NuGet packages once (VS build or `nuget restore`)
#   - Maps\tiles : copy the offline map tiles folder from an existing PC (~52MB)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (& git -C $PSScriptRoot rev-parse --show-toplevel).Replace('/', '\')
if (-not (Test-Path -LiteralPath (Join-Path $root 'EMS_PJT_Hamburger.sln'))) {
    throw "Repo root not resolved (no .sln next to it): $root"
}
$parent   = Split-Path -Parent $root
$repoName = Split-Path -Leaf $root
$features = @('gps', 'pcs', 'bms', 'history')

$existing = & git -C $root worktree list --porcelain |
    Where-Object { $_ -like 'worktree *' } |
    ForEach-Object { $_.Substring(9).Replace('/', '\') }

function New-JunctionIfMissing([string]$Path, [string]$Target, [string]$Hint) {
    if (Test-Path -LiteralPath $Path) { return }
    if (-not (Test-Path -LiteralPath $Target)) {
        Write-Warning "Junction target missing, skipped: $Target ($Hint)"
        return
    }
    New-Item -ItemType Junction -Path $Path -Target $Target | Out-Null
    Write-Host "  junction: $Path -> $Target"
}

foreach ($f in $features) {
    $wt = Join-Path $parent "$repoName-$f"
    Write-Host "== $f => $wt"

    if ($existing -contains $wt) {
        Write-Host '  worktree: already registered, skipped'
    } else {
        $branchExists = (& git -C $root branch --list "feature/$f") -ne $null
        if ($branchExists) {
            & git -C $root worktree add $wt "feature/$f"
        } elseif ((& git -C $root branch -r --list "origin/feature/$f") -ne $null) {
            & git -C $root worktree add $wt -b "feature/$f" "origin/feature/$f"
        } else {
            & git -C $root worktree add $wt -b "feature/$f" main
        }
        if ($LASTEXITCODE -ne 0) { throw "git worktree add failed for $f" }
    }

    New-JunctionIfMissing (Join-Path $wt 'packages') (Join-Path $root 'packages') 'restore NuGet packages in the main clone first'
    New-JunctionIfMissing (Join-Path $wt 'EMS_PJT_Hamburger\Maps\tiles') (Join-Path $root 'EMS_PJT_Hamburger\Maps\tiles') 'copy the offline tiles folder from an existing PC'
}

# --- Patch absolute paths recorded for multi-task / worktree routing ---------
# workflow.js uses forward slashes, CLAUDE.md uses backslashes. Any drive-
# letter path ending in "<something>-<feature>" is rewritten to this clone's
# actual worktree location; the repo folder name may differ between PCs.
$patchTargets = @(
    @{ file = Join-Path $root '.claude\skills\multi-task\workflow.js'; sep = '/'  },
    @{ file = Join-Path $root '.claude\CLAUDE.md';                     sep = '\' }
)
foreach ($t in $patchTargets) {
    if (-not (Test-Path -LiteralPath $t.file)) { continue }
    $text = [IO.File]::ReadAllText($t.file)
    $new  = $text
    foreach ($f in $features) {
        $wt = Join-Path $parent "$repoName-$f"
        if ($t.sep -eq '/') { $wt = $wt.Replace('\', '/') }
        # NOTE: backslashes are literal in .NET regex *replacement* strings —
        # do not escape them here or paths come out doubled (C:\\...).
        $new = [regex]::Replace($new, "[A-Za-z]:[^'``\|\r\n]*?-$f(?=['``\|\s])", $wt.Replace('$', '$$'))
    }
    if ($new -ne $text) {
        [IO.File]::WriteAllText($t.file, $new, (New-Object Text.UTF8Encoding $false))
        Write-Host "patched paths: $($t.file)"
        Write-Host '  -> commit this so the paths stay valid for this PC (re-run the script after pulling on another PC).'
    }
}

Write-Host ''
& git -C $root worktree list
Write-Host 'Done. Reminder: packages via NuGet restore, Maps\tiles copied manually (both untracked).'
