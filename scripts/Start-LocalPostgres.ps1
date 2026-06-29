$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$dataDir = Join-Path $repoRoot ".postgres-data"
$pgBin = "C:\Program Files\PostgreSQL\17\bin"
$postgres = Join-Path $pgBin "postgres.exe"
$initdb = Join-Path $pgBin "initdb.exe"

if (-not (Test-Path $postgres)) {
    throw "postgres.exe was not found at $postgres. Install PostgreSQL 17 or update this script."
}

if (-not (Test-Path $dataDir)) {
    if (-not (Test-Path $initdb)) {
        throw "initdb.exe was not found at $initdb."
    }

    & $initdb -D $dataDir -U postgres --auth=trust --encoding=UTF8 --locale=C
}

Write-Host "Starting DineFlow PostgreSQL on localhost:5432"
Write-Host "Keep this window open while using the WPF app. Press Ctrl+C to stop."
& $postgres -D $dataDir -p 5432
