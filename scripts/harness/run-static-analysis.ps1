param([switch]$AllowCompletedTask)

. "$PSScriptRoot\Harness.Common.ps1"

try {
    $root = Get-HarnessRoot
    $state = $null
    $state = if ($AllowCompletedTask.IsPresent -or $env:HARNESS_ALLOW_COMPLETED_TASK -eq "1") { Assert-HarnessQualityGateContext } else { Assert-HarnessTaskContext }

    $required = @(
        "docs/harness/analysis.md",
        "docs/harness/rules.md",
        "docs/harness/workflow.md",
        "docs/harness/qc.md",
        "docs/harness/wpf-rules.md",
        "docs/harness/database.md",
        "AGENTS.md",
        ".githooks/pre-commit",
        ".githooks/commit-msg",
        ".githooks/pre-push"
    )

    foreach ($item in $required) {
        if (!(Test-Path -LiteralPath (Join-Path $root $item))) {
            throw "Required artifact missing: $item"
        }
    }

    $agentLines = (Get-Content -LiteralPath (Join-Path $root "AGENTS.md")).Count
    if ($agentLines -gt 200) {
        throw "AGENTS.md must be 200 lines or fewer. current=$agentLines"
    }

    $secretHits = @()
    foreach ($file in @("EMS_PJT_Hamburger/App.config", "EMS_PJT_Hamburger/NLog.config")) {
        $path = Join-Path $root $file
        if (Test-Path -LiteralPath $path) {
            $matches = Select-String -Path $path -Pattern "Password\s*=|SciChartLicenseKey\s*=\s*['""][^'""]+|EMS_SCICHART_LICENSE_KEY\s*=\s*['""][^'""]+" -AllMatches
            if ($matches) { $secretHits += $matches }
        }
    }
    if ($secretHits.Count -gt 0) {
        throw "Potential secret found in config files."
    }

    Write-HarnessLogForState $state "Static analysis passed."
    exit 0
}
catch {
    if ($null -ne $state) { Write-HarnessLogForState $state "Static analysis failed: $_" "ERROR" }
    else { Write-Error "Static analysis failed: $_" }
    exit 1
}
