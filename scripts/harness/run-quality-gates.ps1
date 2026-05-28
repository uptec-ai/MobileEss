param([switch]$AllowCompletedTask)

. "$PSScriptRoot\Harness.Common.ps1"

$previousAllowCompletedTask = $env:HARNESS_ALLOW_COMPLETED_TASK

try {
    $state = $null
    $useCompletedTask = $AllowCompletedTask.IsPresent -or $env:HARNESS_ALLOW_COMPLETED_TASK -eq "1"
    $state = if ($useCompletedTask) { Assert-HarnessQualityGateContext } else { Assert-HarnessTaskContext }
    Write-HarnessLogForState $state "Quality gates started."

    $stepArgs = @()
    if ($useCompletedTask) { $env:HARNESS_ALLOW_COMPLETED_TASK = "1" }

    & "$PSScriptRoot\run-unit-tests.ps1" @stepArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & "$PSScriptRoot\run-integration-tests.ps1" @stepArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & "$PSScriptRoot\run-static-analysis.ps1" @stepArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & "$PSScriptRoot\run-build.ps1" @stepArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & "$PSScriptRoot\run-e2e.ps1" @stepArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-HarnessLogForState $state "Quality gates passed."
    $env:HARNESS_ALLOW_COMPLETED_TASK = $previousAllowCompletedTask
    exit 0
}
catch {
    $env:HARNESS_ALLOW_COMPLETED_TASK = $previousAllowCompletedTask
    if ($null -ne $state) { Write-HarnessLogForState $state "Quality gates failed: $_" "ERROR" }
    else { Write-Error "Quality gates failed: $_" }
    exit 1
}
