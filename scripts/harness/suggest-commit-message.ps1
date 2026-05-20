. "$PSScriptRoot\Harness.Common.ps1"

try {
    $state = Assert-HarnessTaskContext
    $type = "chore"
    if ($state.title -match "fix|bug") { $type = "fix" }
    elseif ($state.title -match "feature|add") { $type = "feat" }
    elseif ($state.title -match "doc") { $type = "docs" }

    $subject = ($state.title -replace "\s+", " ").Trim()
    if ($subject.Length -gt 70) { $subject = $subject.Substring(0, 70).Trim() }
    $message = "${type}: $subject"

    Write-HarnessLog "Suggested commit message: $message"
    Write-Host $message
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
