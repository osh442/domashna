# setup.ps1
# One command that gives you a working database in a Docker container:
#   1. starts (or creates) the SQL Server container
#   2. waits until SQL Server accepts connections
#   3. copies the sql\ folder into the container and runs the scripts in order
#
# Usage (from the task folder):
#   powershell -ExecutionPolicy Bypass -File scripts\setup.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\setup.ps1 -Reset     # drop and rebuild the database

param(
    [string]$Container = 'physio-sql',
    [string]$Password  = 'Physio_Strong123!',
    [int]$Port         = 1433,
    [switch]$Reset
)

$sqlDir = (Resolve-Path (Join-Path $PSScriptRoot '..\sql')).Path

# --- 1. Container -----------------------------------------------------------
$nameFilter = 'name=^/' + $Container + '$'
$running = docker ps --filter $nameFilter --format '{{.Names}}'
if (-not $running) {
    $exists = docker ps -a --filter $nameFilter --format '{{.Names}}'
    if ($exists) {
        Write-Host "Starting existing container '$Container'..."
        docker start $Container | Out-Null
    } else {
        Write-Host "Creating container '$Container' (downloads the image on first run)..."
        docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$Password" -p "${Port}:1433" `
            -v physio_data:/var/opt/mssql --name $Container -d mcr.microsoft.com/mssql/server:2022-latest | Out-Null
    }
    if ($LASTEXITCODE -ne 0) { throw "Could not start the container. Is Docker Desktop running?" }
}

# --- 2. sqlcmd inside the container ----------------------------------------
$null = docker exec $Container test -x /opt/mssql-tools18/bin/sqlcmd 2>&1
if ($LASTEXITCODE -eq 0) { $sqlcmd = '/opt/mssql-tools18/bin/sqlcmd' } else { $sqlcmd = '/opt/mssql-tools/bin/sqlcmd' }

Write-Host "Waiting for SQL Server to accept connections..."
$ready = $false
for ($i = 1; $i -le 20; $i++) {
    $null = docker exec $Container $sqlcmd -S localhost -U sa -P $Password -C -b -Q "SELECT 1" 2>&1
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 3
}
if (-not $ready) { throw "SQL Server is not ready. Check: docker logs $Container" }

# --- 3. Run the scripts ------------------------------------------------------
docker cp $sqlDir "${Container}:/tmp"
if ($LASTEXITCODE -ne 0) { throw "docker cp failed" }

$scripts = @('01_create_database.sql', '02_create_tables.sql', '03_create_indexes.sql',
             '04_seed_data.sql', '05_verify.sql')
if ($Reset) { $scripts = @('00_reset.sql') + $scripts }

foreach ($s in $scripts) {
    Write-Host ""
    Write-Host "=== $s ===" -ForegroundColor Cyan
    docker exec $Container $sqlcmd -S localhost -U sa -P $Password -C -b -I -i "/tmp/sql/$s"
    if ($LASTEXITCODE -ne 0) { throw "$s failed (exit code $LASTEXITCODE)" }
}

Write-Host ""
Write-Host "Done. Database PhysioCenter is running in container '$Container' on localhost,$Port (user sa)." -ForegroundColor Green
