param(
    [Parameter(Mandatory = $true)][string]$Message,
    [string]$Level = "INFO"
)

. "$PSScriptRoot\Harness.Common.ps1"
Write-HarnessLog -Message $Message -Level $Level
