$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$localPath = Join-Path $projectRoot '.local'
$mysqlVersion = '8.4.13'
$mysqlRoot = Join-Path $localPath "mysql-$mysqlVersion-winx64"
New-Item -ItemType Directory -Force $localPath | Out-Null
if (-not (Test-Path "$mysqlRoot/bin/mysqld.exe")) {
    $archive = Join-Path $localPath "mysql-$mysqlVersion-winx64.zip"
    if (-not (Test-Path $archive)) {
        Invoke-WebRequest "https://cdn.mysql.com/Downloads/MySQL-8.4/mysql-$mysqlVersion-winx64.zip" -OutFile $archive
    }
    if ((Get-FileHash $archive -Algorithm MD5).Hash -ne '7b94489d5524dea9b27097068bea25a8') { throw 'MySQL archive checksum mismatch.' }
    Expand-Archive -LiteralPath $archive -DestinationPath $localPath
}
$dataPath = Join-Path $localPath 'mysql-data'
$configPath = Join-Path $projectRoot 'src/ParkSpot.Web/appsettings.Local.json'
if (Test-Path $dataPath) {
    if (Test-Path $configPath) { Write-Host 'Local database is already prepared.'; exit 0 }
    throw 'Database directory already exists but app configuration is missing. Restore local configuration; existing data was not changed.'
}
function New-LocalPassword {
    $bytes = New-Object byte[] 24
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [BitConverter]::ToString($bytes).Replace('-', '')
}
$rootPassword = New-LocalPassword
$appPassword = New-LocalPassword
& "$mysqlRoot/bin/mysqld.exe" --no-defaults --initialize-insecure "--basedir=$mysqlRoot" "--datadir=$dataPath" --console
if ($LASTEXITCODE -ne 0) { throw 'MySQL initialization failed.' }
$server = Start-Process "$mysqlRoot/bin/mysqld.exe" -ArgumentList @('--no-defaults', "--basedir=`"$mysqlRoot`"", "--datadir=`"$dataPath`"", '--port=3307','--bind-address=127.0.0.1','--mysqlx=0',"--pid-file=`"$localPath/mysql.pid`"") -WindowStyle Hidden -PassThru
for ($attempt=0; $attempt -lt 40; $attempt++) {
    $ErrorActionPreference = 'Continue'
    & "$mysqlRoot/bin/mysqladmin.exe" --no-defaults --host=127.0.0.1 --port=3307 --user=root ping --silent 2>$null
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -eq 0) { break }
    Start-Sleep -Milliseconds 500
}
$sql = @"
ALTER USER 'root'@'localhost' IDENTIFIED BY '$rootPassword';
CREATE DATABASE parkspot CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
CREATE USER 'parkspot'@'127.0.0.1' IDENTIFIED BY '$appPassword';
GRANT SELECT, INSERT, UPDATE, DELETE ON parkspot.* TO 'parkspot'@'127.0.0.1';
"@
$sql += "
USE parkspot;
" + (Get-Content (Join-Path $projectRoot 'Database/schema.sql') -Raw)
$sql | & "$mysqlRoot/bin/mysql.exe" --no-defaults --host=127.0.0.1 --port=3307 --user=root
if ($LASTEXITCODE -ne 0) { throw 'Database account setup failed. Check the local MySQL log.' }
@{ ConnectionStrings = @{ ParkSpot = "Server=127.0.0.1;Port=3307;Database=parkspot;User=parkspot;Password=$appPassword" } } | ConvertTo-Json -Depth 4 | Set-Content $configPath -Encoding utf8
@{ ConnectionString = "Server=127.0.0.1;Port=3307;User=root;Password=$rootPassword" } | ConvertTo-Json | Set-Content "$localPath/test-settings.json" -Encoding utf8
Set-Content "$localPath/admin.ini" "[client]`nuser=root`npassword=$rootPassword`nhost=127.0.0.1`nport=3307" -Encoding ascii
Push-Location $projectRoot
try {
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw 'Package restore failed.' }
} finally { Pop-Location }
Write-Host 'Ready. Run: powershell -ExecutionPolicy Bypass -File ./scripts/Start-Local.ps1'
