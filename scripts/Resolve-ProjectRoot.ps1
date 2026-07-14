[CmdletBinding()]
param([string]$StartPath = $PSScriptRoot, [string]$Anchor = '*.sln')
# 저장소가 어느 폴더로 clone되든 앵커(기본: *.sln)를 가진 상위 폴더를 찾아 프로젝트 루트를 반환.
# 빌드/실행 명령에서 절대경로 대신 이 리졸버를 사용한다.
if ([string]::IsNullOrWhiteSpace($StartPath)) { $StartPath = (Get-Location).Path }
$dir = Get-Item -LiteralPath $StartPath
if (-not $dir.PSIsContainer) { $dir = $dir.Directory }
while ($null -ne $dir) {
    if (Get-ChildItem -LiteralPath $dir.FullName -Filter $Anchor -File -ErrorAction SilentlyContinue | Select-Object -First 1) {
        return $dir.FullName
    }
    $dir = $dir.Parent
}
throw "Anchor '$Anchor' not found at or above '$StartPath'."
