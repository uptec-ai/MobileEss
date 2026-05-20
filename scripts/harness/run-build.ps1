param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU",
    [switch]$AllowCompletedTask
)

. "$PSScriptRoot\Harness.Common.ps1"

try {
    $root = Get-HarnessRoot
    $state = $null
    $state = if ($AllowCompletedTask.IsPresent -or $env:HARNESS_ALLOW_COMPLETED_TASK -eq "1") { Assert-HarnessQualityGateContext } else { Assert-HarnessTaskContext }
    $solution = Join-Path $root "EMS_PJT_Hamburger.sln"
    if (!(Test-Path -LiteralPath $solution)) { throw "Solution not found: $solution" }

    $msbuild = Find-MSBuild
    Invoke-HarnessProcess -FilePath $msbuild -Arguments @(
        $solution,
        "/m",
        "/p:Configuration=$Configuration",
        "/p:Platform=$Platform",
        "/v:minimal"
    ) -StepName "MSBuild $Configuration|$Platform"
    exit 0
}
catch {
    if ($null -ne $state) { Write-HarnessLogForState $state "Build failed: $_" "ERROR" }
    else { Write-Error "Build failed: $_" }
    exit 1
}
