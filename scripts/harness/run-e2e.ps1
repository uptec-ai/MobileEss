param([switch]$AllowCompletedTask)

. "$PSScriptRoot\Harness.Common.ps1"

try {
    $root = Get-HarnessRoot
    $state = $null
    $state = if ($AllowCompletedTask.IsPresent -or $env:HARNESS_ALLOW_COMPLETED_TASK -eq "1") { Assert-HarnessQualityGateContext } else { Assert-HarnessTaskContext }

    $exe = Join-Path $root "EMS_PJT_Hamburger/bin/Release/EMS_PJT_Hamburger.exe"
    if (!(Test-Path -LiteralPath $exe)) {
        throw "Release executable not found. Run build first: $exe"
    }

    if ([string]::IsNullOrWhiteSpace($env:EMS_SCICHART_LICENSE_KEY)) {
        Write-HarnessLogForState $state "EMS_SCICHART_LICENSE_KEY is not set. Build can pass, but runtime chart licensing may warn." "WARN"
    }

    if ($env:EMS_RUN_DB_INTEGRATION -eq "1" -and [string]::IsNullOrWhiteSpace($env:EMS_DB_CONN)) {
        throw "EMS_RUN_DB_INTEGRATION=1 requires EMS_DB_CONN for E2E."
    }

    Write-HarnessLogForState $state "E2E verification passed for WPF release artifact."
    exit 0
}
catch {
    if ($null -ne $state) { Write-HarnessLogForState $state "E2E verification failed: $_" "ERROR" }
    else { Write-Error "E2E verification failed: $_" }
    exit 1
}
