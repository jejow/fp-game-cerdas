param(
    [ValidateSet('Validate', 'Smoke', 'Build')]
    [string]$Action = 'Validate',
    [string]$UnityPath = 'C:\Program Files\Unity 6000.0.37f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if (-not (Test-Path -LiteralPath $UnityPath)) { throw "Unity executable not found. Supply -UnityPath explicitly." }
if (Test-Path -LiteralPath (Join-Path $projectRoot 'Temp\UnityLockfile')) {
    throw 'Close this project in Unity before running a second Editor process.'
}
$logFolder = Join-Path $projectRoot 'Logs\ProjectMaintenance'
[IO.Directory]::CreateDirectory($logFolder) | Out-Null
$method = switch ($Action) {
    'Validate' { 'ProjectStructureValidation.ValidateBatch' }
    'Smoke' { 'OfflineSmokeBatch.Run' }
    'Build' { 'OfflineMatchValidation.BuildValidationPlayerBatch' }
}
$log = Join-Path $logFolder ($Action.ToLowerInvariant() + '.log')
$arguments = @('-batchmode', '-projectPath', ('"' + $projectRoot + '"'), '-executeMethod', $method, '-logFile', ('"' + $log + '"'))
if ($Action -eq 'Smoke') { $arguments += @('-wwiseEnableWithNoGraphics', '-diag-job-temp-memory-leak-validation') }
if ($Action -ne 'Smoke') { $arguments += '-quit' }
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
Write-Output "Unity PID: $($process.Id). Log: $log"
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity exited with code $($process.ExitCode). Read $log" }
Write-Output "$Action completed. Check the report described in Docs/BUILD_AND_TEST.md."
