$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$settings = Get-Content "$projectRoot/.local/test-settings.json" -Raw | ConvertFrom-Json
$env:PARKSPOT_TEST_MYSQL = $settings.ConnectionString
Push-Location $projectRoot
try { dotnet test; $testExit = $LASTEXITCODE } finally { Pop-Location; Remove-Item Env:PARKSPOT_TEST_MYSQL }
exit $testExit
