param([switch]$AllowCompletedTask)

. "$PSScriptRoot\Harness.Common.ps1"

try {
    $state = $null
    $state = if ($AllowCompletedTask.IsPresent -or $env:HARNESS_ALLOW_COMPLETED_TASK -eq "1") { Assert-HarnessQualityGateContext } else { Assert-HarnessTaskContext }

    if ($env:EMS_RUN_DB_INTEGRATION -eq "1" -and [string]::IsNullOrWhiteSpace($env:EMS_DB_CONN)) {
        throw "EMS_RUN_DB_INTEGRATION=1 requires EMS_DB_CONN."
    }

    $projects = Get-TestProjects -Pattern "*IntegrationTest*.csproj"
    if (!$projects -or $projects.Count -eq 0) {
        Write-HarnessLogForState $state "No integration test projects found. Passing with warning for current solution." "WARN"
        exit 0
    }

    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (!$dotnet) { throw "dotnet CLI not found for integration test execution." }

    foreach ($project in $projects) {
        Invoke-HarnessProcess -FilePath $dotnet.Source -Arguments @("test", $project.FullName, "--no-restore") -StepName "Integration tests $($project.Name)" -State $state
    }
    exit 0
}
catch {
    if ($null -ne $state) { Write-HarnessLogForState $state "Integration tests failed: $_" "ERROR" }
    else { Write-Error "Integration tests failed: $_" }
    exit 1
}
