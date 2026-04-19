<#
Short runner script for the Colegio solution.

Usage:
  powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run.ps1 -action run
Actions:
  restore  - dotnet restore (solution)
  build    - dotnet build (no restore)
  migrate  - apply EF migrations (update database)
  run      - run the API (dotnet run --project src/Colegio.Api)
  test     - run tests
  docker   - docker-compose up --build (requires Docker)
  all      - restore, build, migrate, run
#>

param(
    [ValidateSet('restore','build','migrate','run','test','docker','all')]
    [string]$action = 'run'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $repoRoot

function Check-Command($cmd) {
    $null -ne (& where.exe $cmd 2>$null)
}

switch ($action) {
    'restore' {
        dotnet restore
        break
    }
    'build' {
        dotnet build --no-restore
        break
    }
    'migrate' {
        dotnet ef database update --project src/Colegio.Infrastructure --startup-project src/Colegio.Api
        break
    }
    'run' {
        dotnet run --project src/Colegio.Api
        break
    }
    'test' {
        dotnet test
        break
    }
    'docker' {
        if (-not (Check-Command 'docker')) {
            Write-Error "Docker CLI not found in PATH."
            exit 1
        }
        docker-compose -f docker/docker-compose.yml up --build
        break
    }
    'all' {
        dotnet restore
        dotnet build --no-restore
        dotnet ef database update --project src/Colegio.Infrastructure --startup-project src/Colegio.Api
        dotnet run --project src/Colegio.Api
        break
    }
}

Pop-Location
