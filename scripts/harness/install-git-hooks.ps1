. "$PSScriptRoot\Harness.Common.ps1"

try {
    $root = Get-HarnessRoot
    New-Item -ItemType Directory -Force (Join-Path $root ".githooks") | Out-Null
    git -C $root config core.hooksPath .githooks
    Write-Host "Git hooks installed: core.hooksPath=.githooks"
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
