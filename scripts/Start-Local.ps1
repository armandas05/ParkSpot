$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$localPath = Join-Path $projectRoot '.local'
$mysqlRoot = Join-Path $localPath 'mysql-8.4.13-winx64'
if (-not (Test-Path "$localPath/admin.ini")) { throw 'Run scripts/Setup-Local.ps1 first.' }
$ErrorActionPreference = 'Continue'
& "$mysqlRoot/bin/mysqladmin.exe" "--defaults-file=$localPath/admin.ini" ping --silent 2>$null
$ErrorActionPreference = 'Stop'
if ($LASTEXITCODE -ne 0) {
    Start-Process "$mysqlRoot/bin/mysqld.exe" -ArgumentList @('--no-defaults', "--basedir=`"$mysqlRoot`"", "--datadir=`"$localPath/mysql-data`"", '--port=3307','--bind-address=127.0.0.1','--mysqlx=0',"--pid-file=`"$localPath/mysql.pid`"") -WindowStyle Hidden
    $ready = $false
    for ($attempt=0; $attempt -lt 40; $attempt++) {
        Start-Sleep -Milliseconds 500
        $ErrorActionPreference = 'Continue'
        & "$mysqlRoot/bin/mysqladmin.exe" "--defaults-file=$localPath/admin.ini" ping --silent 2>$null
        $ErrorActionPreference = 'Stop'
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    }
    if (-not $ready) { throw 'MySQL did not start. Check .local/mysql-data/*.err.' }
}
Push-Location $projectRoot
try { dotnet run --project src/ParkSpot.Web --launch-profile http } finally { Pop-Location }
