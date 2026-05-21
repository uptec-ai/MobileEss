Set-StrictMode -Version Latest

function Get-HarnessRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

function Get-HarnessStatePath {
    return (Join-Path (Get-HarnessRoot) "current-task.json")
}

function Get-HarnessState {
    $path = Get-HarnessStatePath
    if (!(Test-Path -LiteralPath $path)) {
        throw "current-task.json not found. Run scripts/harness/start-task.ps1 first."
    }
    return Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
}

function Assert-HarnessTaskContext {
    $root = Get-HarnessRoot
    $state = Get-HarnessState

    if ([string]::IsNullOrWhiteSpace($state.planPath) -or [string]::IsNullOrWhiteSpace($state.logPath)) {
        throw "current-task.json is missing planPath or logPath."
    }

    $plan = Join-Path $root $state.planPath
    $log = Join-Path $root $state.logPath

    if (!(Test-Path -LiteralPath $plan)) { throw "Plan file not found: $($state.planPath)" }
    if (!(Test-Path -LiteralPath $log)) { throw "Log file not found: $($state.logPath)" }
    if ($state.status -ne "active") { throw "Current task is not active. status=$($state.status)" }

    return $state
}

function Get-LatestCompletedHarnessTaskContext {
    $root = Get-HarnessRoot
    $completedDir = Join-Path $root "plans/completed"
    $logDir = Join-Path $root "logs/harness"

    if (!(Test-Path -LiteralPath $completedDir)) {
        throw "No active task and no completed plan directory found. Run scripts/harness/start-task.ps1 first."
    }

    $plan = Get-ChildItem -LiteralPath $completedDir -Filter "*.md" -File |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (!$plan) {
        throw "No active task and no completed plan found. Run scripts/harness/start-task.ps1 first."
    }

    $taskId = [IO.Path]::GetFileNameWithoutExtension($plan.Name)
    $log = Join-Path $logDir "$taskId.log"
    if (!(Test-Path -LiteralPath $log)) {
        throw "Latest completed task log not found: logs/harness/$taskId.log"
    }

    return [pscustomobject]@{
        taskId = $taskId
        title = $taskId
        status = "completed"
        planPath = "plans/completed/$($plan.Name)"
        logPath = "logs/harness/$taskId.log"
    }
}

function Assert-HarnessQualityGateContext {
    try {
        return Assert-HarnessTaskContext
    }
    catch {
        return Get-LatestCompletedHarnessTaskContext
    }
}

function Write-HarnessLogForState {
    param(
        [Parameter(Mandatory = $true)]$State,
        [Parameter(Mandatory = $true)][string]$Message,
        [string]$Level = "INFO"
    )

    $root = Get-HarnessRoot
    $log = Join-Path $root $State.logPath
    if (!(Test-Path -LiteralPath $log)) {
        throw "Log file not found: $($State.logPath)"
    }

    $line = "[{0}] [{1}] {2}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Level.ToUpperInvariant(), $Message
    Add-Content -LiteralPath $log -Value $line -Encoding UTF8
    Write-Host $line
}

function Write-HarnessLog {
    param(
        [Parameter(Mandatory = $true)][string]$Message,
        [string]$Level = "INFO"
    )

    $state = if ($env:HARNESS_ALLOW_COMPLETED_TASK -eq "1") { Assert-HarnessQualityGateContext } else { Assert-HarnessTaskContext }
    Write-HarnessLogForState -State $state -Message $Message -Level $Level
}

function Find-MSBuild {
    if ($env:MSBUILD_EXE -and (Test-Path -LiteralPath $env:MSBUILD_EXE)) {
        return $env:MSBUILD_EXE
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path -LiteralPath $vswhere) {
        $path = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        if ($path -and (Test-Path -LiteralPath $path)) {
            return $path
        }
    }

    $cmd = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    throw "MSBuild.exe was not found. Install Visual Studio Build Tools or set MSBUILD_EXE."
}

function Get-TestProjects {
    param([string]$Pattern = "*Test*.csproj")
    $root = Get-HarnessRoot
    return Get-ChildItem -Path $root -Recurse -Filter $Pattern -File |
        Where-Object { $_.FullName -notmatch "\\packages\\|\\bin\\|\\obj\\" }
}

function Invoke-HarnessProcess {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$StepName,
        $State = $null
    )

    if ($null -ne $State) {
        Write-HarnessLogForState $State "$StepName started: $FilePath $($Arguments -join ' ')"
    }
    else {
        Write-HarnessLog "$StepName started: $FilePath $($Arguments -join ' ')"
    }

    & $FilePath @Arguments
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        if ($null -ne $State) {
            Write-HarnessLogForState $State "$StepName failed with exit code $exitCode" "ERROR"
        }
        else {
            Write-HarnessLog "$StepName failed with exit code $exitCode" "ERROR"
        }
        exit $exitCode
    }

    if ($null -ne $State) {
        Write-HarnessLogForState $State "$StepName passed"
    }
    else {
        Write-HarnessLog "$StepName passed"
    }
}

function Invoke-HarnessStepScript {
    param(
        [Parameter(Mandatory = $true)][string]$ScriptPath,
        [switch]$AllowCompletedTask
    )

    $args = @()
    if ($AllowCompletedTask) { $args += "-AllowCompletedTask" }
    & $ScriptPath @args
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
