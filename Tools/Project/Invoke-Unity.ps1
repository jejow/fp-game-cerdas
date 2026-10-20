param(
    [ValidateSet('Validate', 'Presentation', 'Smoke', 'Build')]
    [string]$Action = 'Validate',
    [string]$UnityPath = 'C:\Program Files\Unity 6000.0.37f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if (-not (Test-Path -LiteralPath $UnityPath)) { throw "Unity executable not found. Supply -UnityPath explicitly." }
$activeEditor = Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object {
    $_.CommandLine -and $_.CommandLine.Replace('/', '\').IndexOf($projectRoot, [StringComparison]::OrdinalIgnoreCase) -ge 0
}
if ($activeEditor) {
    throw 'Close this project in Unity before running a second Editor process.'
}
$lockPath = Join-Path $projectRoot 'Temp\UnityLockfile'
if (Test-Path -LiteralPath $lockPath) {
    # An interrupted batch Editor can leave its lock file after the process has gone.
    Remove-Item -LiteralPath $lockPath -ErrorAction Stop
}
$logFolder = Join-Path $projectRoot 'Logs\ProjectMaintenance'
[IO.Directory]::CreateDirectory($logFolder) | Out-Null
$method = switch ($Action) {
    'Validate' { 'RobotPresentationValidation.ValidateBatch' }
    'Presentation' { 'PresentationSmokeBatch.Run' }
    'Smoke' { 'OfflineSmokeBatch.Run' }
    'Build' { 'OfflineMatchValidation.BuildValidationPlayerBatch' }
}
$log = Join-Path $logFolder ($Action.ToLowerInvariant() + '.log')
$arguments = @('-batchmode', '-projectPath', ('"' + $projectRoot + '"'), '-executeMethod', $method, '-logFile', ('"' + $log + '"'))
if ($Action -in @('Smoke', 'Presentation')) { $arguments += @('-wwiseEnableWithNoGraphics', '-diag-job-temp-memory-leak-validation') }
if ($Action -eq 'Smoke') { $arguments += @('-job-worker-count', '2') }
if ($Action -notin @('Smoke', 'Presentation')) { $arguments += '-quit' }
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
Write-Output "Unity PID: $($process.Id). Log: $log"
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity exited with code $($process.ExitCode). Read $log" }
Write-Output "$Action completed. Check the report described in Docs/BUILD_AND_TEST.md."
