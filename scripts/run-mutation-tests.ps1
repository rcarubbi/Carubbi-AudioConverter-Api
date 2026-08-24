[CmdletBinding()]
param(
    [string]$Project = 'Carubbi.AudioConverter.Api\Carubbi.AudioConverter.Api.csproj',
    [string]$TestProject = 'Carubbi.AudioConverter.Api.Tests\Carubbi.AudioConverter.Api.Tests.csproj',
    [switch]$Open
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not (Get-Command dotnet-stryker -ErrorAction SilentlyContinue)) {
    Write-Host 'Installing dotnet-stryker global tool...' -ForegroundColor Cyan
    dotnet tool install -g dotnet-stryker
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$projectPath = Join-Path $repoRoot $Project
$testProjectPath = Join-Path $repoRoot $TestProject

if (-not (Test-Path $projectPath)) {
    throw "Project under test not found: $projectPath"
}
if (-not (Test-Path $testProjectPath)) {
    throw "Test project not found: $testProjectPath"
}

$libName = [System.IO.Path]::GetFileNameWithoutExtension($Project)
$outputDir = "TestResults/mutation/$libName"

Write-Host "Running mutation tests: $libName (Stryker MTP runner)..." -ForegroundColor Cyan
Write-Host 'This can take a while: every mutant runs the test suite.' -ForegroundColor Yellow

Push-Location $repoRoot
try {
    $strykerArgs = @(
        '--test-runner', 'mtp',
        '--project', $Project,
        '--test-project', $TestProject,
        '--output', $outputDir
    )

    if ($Open) {
        $strykerArgs += '--open-report'
    }

    & dotnet-stryker @strykerArgs
    $exitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

Write-Host "Report: $(Join-Path $repoRoot "$outputDir\reports\mutation-report.html")" -ForegroundColor Green

if ($exitCode -ne 0) {
    Write-Warning "Mutation testing failed or below threshold for $libName."
    exit $exitCode
}
