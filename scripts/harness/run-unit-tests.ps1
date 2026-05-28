param([switch]$AllowCompletedTask)

. "$PSScriptRoot\Harness.Common.ps1"

try {
    $state = $null
    $state = if ($AllowCompletedTask.IsPresent -or $env:HARNESS_ALLOW_COMPLETED_TASK -eq "1") { Assert-HarnessQualityGateContext } else { Assert-HarnessTaskContext }
    $projects = Get-TestProjects | Where-Object { $_.Name -notmatch "Integration" }
    if (!$projects -or $projects.Count -eq 0) {
        Write-HarnessLogForState $state "No unit test projects found. Current solution has no test structure; passing with warning." "WARN"
        exit 0
    }

    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (!$dotnet) { throw "dotnet CLI not found for test execution." }

    foreach ($project in $projects) {
        Invoke-HarnessProcess -FilePath $dotnet.Source -Arguments @("test", $project.FullName, "--no-restore") -StepName "Unit tests $($project.Name)" -State $state
    }
    exit 0
}
catch {
    if ($null -ne $state) { Write-HarnessLogForState $state "Unit tests failed: $_" "ERROR" }
    else { Write-Error "Unit tests failed: $_" }
    exit 1
}
