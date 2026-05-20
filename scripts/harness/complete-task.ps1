. "$PSScriptRoot\Harness.Common.ps1"

try {
    $root = Get-HarnessRoot
    $state = Get-HarnessState
    $plan = Join-Path $root $state.planPath
    $completedDir = Join-Path $root "plans/completed"
    New-Item -ItemType Directory -Force $completedDir | Out-Null

    $completedRel = "plans/completed/$($state.taskId).md"
    $completedPath = Join-Path $root $completedRel
    if (Test-Path -LiteralPath $plan) {
        Move-Item -LiteralPath $plan -Destination $completedPath -Force
    }
    elseif (!(Test-Path -LiteralPath $completedPath)) {
        throw "Plan file not found: $($state.planPath)"
    }

    $log = Join-Path $root $state.logPath
    if (Test-Path -LiteralPath $log) {
        $line = "[{0}] [INFO] Task completed. Plan moved to {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $completedRel
        Add-Content -LiteralPath $log -Value $line -Encoding UTF8
        Write-Host $line
    }

    Remove-Item -LiteralPath (Get-HarnessStatePath)
    Write-Host "Completed task: $($state.taskId)"
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
