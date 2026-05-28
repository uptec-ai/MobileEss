param(
    [Parameter(Mandatory = $true)][string]$Title
)

. "$PSScriptRoot\Harness.Common.ps1"
$ErrorActionPreference = "Stop"

$root = Get-HarnessRoot
$slug = ($Title.ToLowerInvariant() -replace "[^a-z0-9]+", "-" -replace "^-|-$", "")
if ([string]::IsNullOrWhiteSpace($slug)) { $slug = "task" }
$taskId = "{0}-{1}" -f (Get-Date -Format "yyyyMMdd-HHmmss"), $slug

$planRel = "plans/active/$taskId.md"
$logRel = "logs/harness/$taskId.log"
$planPath = Join-Path $root $planRel
$logPath = Join-Path $root $logRel

New-Item -ItemType Directory -Force (Split-Path $planPath), (Split-Path $logPath) | Out-Null

$plan = @"
# $Title

- Task ID: $taskId
- Status: active
- Created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Goal

Describe the goal before editing app code.

## Scope

Describe included and excluded work.

## Impacted Files

- TBD

## Test Strategy

- Unit:
- Integration:
- Static analysis:
- Build:
- E2E:

## Rollback

Describe how to revert this task safely.

## Notes

- Harness guard must pass before app code edits.
"@

Set-Content -LiteralPath $planPath -Value $plan -Encoding UTF8
$initialLogLine = "[{0}] [INFO] Task started: {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Title
Set-Content -LiteralPath $logPath -Value $initialLogLine -Encoding UTF8

$state = [ordered]@{
    taskId = $taskId
    title = $Title
    status = "active"
    createdAt = (Get-Date).ToString("o")
    planPath = $planRel
    logPath = $logRel
}

$state | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Get-HarnessStatePath) -Encoding UTF8

Write-Host "Created task: $taskId"
Write-Host "Plan: $planRel"
Write-Host "Log: $logRel"
