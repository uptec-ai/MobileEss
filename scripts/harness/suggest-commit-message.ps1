. "$PSScriptRoot\Harness.Common.ps1"

try {
    $state = Assert-HarnessQualityGateContext
    $type = "chore"
    if ($state.title -match "fix|bug") { $type = "fix" }
    elseif ($state.title -match "feature|add") { $type = "feat" }
    elseif ($state.title -match "doc") { $type = "docs" }

    $subject = ($state.title -replace "\s+", " ").Trim()
    if ($state.status -eq "completed") {
        $planName = [IO.Path]::GetFileNameWithoutExtension($state.planPath)
        $subject = ($planName -replace "^\d{8}-\d{6}-", "" -replace "-", " ").Trim()
    }

    if ($subject.Length -gt 70) { $subject = $subject.Substring(0, 70).Trim() }
    $message = "${type}: $subject"

    Write-HarnessLogForState $state "Suggested commit message: $message"
    Write-Host $message
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
