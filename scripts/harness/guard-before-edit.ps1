. "$PSScriptRoot\Harness.Common.ps1"

try {
    $state = Assert-HarnessTaskContext
    Write-HarnessLog "Guard passed for task $($state.taskId). App code edits are allowed."
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
