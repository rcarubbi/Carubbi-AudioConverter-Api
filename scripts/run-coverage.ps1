[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string[]]$TestProjects = @(),
    [switch]$Report,
    [switch]$Open
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$resultsDir = Join-Path $repoRoot 'TestResults\coverage'

if ($TestProjects.Count -eq 0) {
    $TestProjects = Get-ChildItem -Path $repoRoot -Recurse -Filter '*.csproj' |
        Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\' -and $_.Name -match '\.(Unit)?Tests\.csproj$' } |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1) }
}

if ($TestProjects.Count -eq 0) {
    throw 'No *.Tests.csproj or *.UnitTests.csproj projects found.'
}

New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null
Get-ChildItem -Path $resultsDir -Recurse -Filter '*.cobertura.xml' | Remove-Item -Force

foreach ($testProject in $TestProjects) {
    $testProjectPath = Join-Path $repoRoot $testProject
    if (-not (Test-Path $testProjectPath)) {
        throw "Test project not found: $testProjectPath"
    }

    $name = [System.IO.Path]::GetFileNameWithoutExtension($testProject)
    Write-Host "Running tests with coverage: $name ($Configuration)..." -ForegroundColor Cyan

    dotnet run --project $testProjectPath -c $Configuration -- `
        --coverage `
        --coverage-output-format cobertura `
        --coverage-output "$name.cobertura.xml" `
        --results-directory $resultsDir

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Test run failed for $name."
        exit $LASTEXITCODE
    }
}

$coverageFiles = Get-ChildItem -Path $resultsDir -Recurse -Filter '*.cobertura.xml' |
    Sort-Object LastWriteTime -Descending

Write-Host ""
Write-Host "Coverage files:" -ForegroundColor Green
$coverageFiles | ForEach-Object { Write-Host "  $($_.FullName)" }

if ($Report) {
    $reportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue
    if (-not $reportGenerator) {
        Write-Host 'Installing dotnet-reportgenerator-globaltool...' -ForegroundColor Cyan
        dotnet tool install -g dotnet-reportgenerator-globaltool
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    $htmlDir = Join-Path $resultsDir 'html'
    $reports = ($coverageFiles | ForEach-Object { $_.FullName }) -join ';'
    reportgenerator "-reports:$reports" "-targetdir:$htmlDir" -reporttypes:Html

    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "Merged HTML report: $(Join-Path $htmlDir 'index.html')" -ForegroundColor Green

    if ($Open) {
        Start-Process (Join-Path $htmlDir 'index.html')
    }
}
