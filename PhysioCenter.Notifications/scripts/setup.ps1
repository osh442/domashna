param(
    [string]$Container = 'physio-sql',
    [string]$Password  = 'Physio_Strong123!',
    [switch]$Reset
)

$sqlDir = (Resolve-Path (Join-Path $PSScriptRoot '..\sql')).Path
$sqlcmd = '/opt/mssql-tools18/bin/sqlcmd'

$running = docker ps --filter "name=^/$Container$" --format '{{.Names}}'
if (-not $running) { throw "Container '$Container' is not running. Start it first with the main scripts\setup.ps1." }

docker cp $sqlDir "${Container}:/tmp/notifications-sql"
if ($LASTEXITCODE -ne 0) { throw "docker cp failed" }

$scripts = @('01_create_database.sql', '02_create_tables.sql')
if ($Reset) { $scripts = @('00_reset.sql') + $scripts }

foreach ($s in $scripts) {
    Write-Host "=== $s ===" -ForegroundColor Cyan
    docker exec $Container $sqlcmd -S localhost -U sa -P $Password -C -b -I -i "/tmp/notifications-sql/$s"
    if ($LASTEXITCODE -ne 0) { throw "$s failed (exit code $LASTEXITCODE)" }
}

Write-Host "Database PhysioNotifications is ready in container '$Container'." -ForegroundColor Green
