$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$dataDir = Join-Path $repoRoot ".postgres-data"
$pgBin = "C:\Program Files\PostgreSQL\17\bin"
$postgres = Join-Path $pgBin "postgres.exe"
$initdb = Join-Path $pgBin "initdb.exe"
$psql = Join-Path $pgBin "psql.exe"

if (-not (Test-Path $postgres)) {
    throw "postgres.exe was not found at $postgres. Install PostgreSQL 17 or update this script."
}

if (-not (Test-Path $psql)) {
    throw "psql.exe was not found at $psql."
}

if (-not (Test-Path $dataDir)) {
    if (-not (Test-Path $initdb)) {
        throw "initdb.exe was not found at $initdb."
    }

    & $initdb -D $dataDir -U postgres --auth=trust --encoding=UTF8 --locale=C
}

$job = Start-Job -ScriptBlock {
    param($repoRoot, $postgres, $dataDir)
    Set-Location $repoRoot
    & $postgres -D $dataDir -p 5432
} -ArgumentList $repoRoot, $postgres, $dataDir

try {
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Seconds 1
        $tcp = Test-NetConnection localhost -Port 5432 -WarningAction SilentlyContinue
        if ($tcp.TcpTestSucceeded) {
            $ready = $true
            break
        }
    }

    if (-not $ready) {
        Receive-Job $job -Keep
        throw "PostgreSQL did not open localhost:5432."
    }

    Set-Location $repoRoot
    dotnet ef database update --project src\DineFlow.DataAccessObjects --startup-project src\DineFlow.Api --context AppDbContext
    & $psql -h localhost -p 5432 -U postgres -d DineFlowDb -f database\seed\SeedData.sql
}
finally {
    Stop-Job $job -ErrorAction SilentlyContinue
    Remove-Job $job -Force -ErrorAction SilentlyContinue
}
