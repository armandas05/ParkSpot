$projectRoot = Split-Path $PSScriptRoot -Parent
& "$projectRoot/.local/mysql-8.4.13-winx64/bin/mysqladmin.exe" "--defaults-file=$projectRoot/.local/admin.ini" shutdown
if ($LASTEXITCODE -ne 0) { throw 'Could not stop the local database.' }
