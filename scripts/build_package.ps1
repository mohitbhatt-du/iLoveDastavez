# Dastavez Studio - Local Packaging & Release Script
# Builds release binaries, generates portable ZIP, and runs Inno Setup

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$RootDir = Resolve-Path "$PSScriptRoot\.."
Set-Location $RootDir

Write-Host "=== Step 1: Running Automated Test Suite ===" -ForegroundColor Cyan
& dotnet test tests/DastavezStudio.UnitTests/DastavezStudio.UnitTests.csproj -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed! Aborting packaging."
}

Write-Host "=== Step 2: Publishing WinUI 3 Application ($Runtime) ===" -ForegroundColor Cyan
$PublishDir = "$RootDir\artifacts\publish\$Runtime"
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }

& dotnet publish src/DastavezStudio.App/DastavezStudio.App.csproj `
    -c $Configuration `
    -r $Runtime `
    --no-self-contained `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed!"
}

Write-Host "=== Step 3: Creating Portable ZIP Distribution ===" -ForegroundColor Cyan
$DistDir = "$RootDir\artifacts\dist"
if (!(Test-Path $DistDir)) { New-Item -ItemType Directory -Path $DistDir -Force | Out-Null }

$ZipPath = "$DistDir\DastavezStudio-v$Version-Portable-$Runtime.zip"
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }

Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath -Force
Write-Host "Portable ZIP created: $ZipPath" -ForegroundColor Green

Write-Host "=== Step 4: Building Inno Setup Installer ===" -ForegroundColor Cyan
$InnoCompiler = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (Test-Path $InnoCompiler) {
    & $InnoCompiler "$RootDir\packaging\installer.iss"
    Write-Host "Installer generated in $DistDir" -ForegroundColor Green
} else {
    Write-Warning "Inno Setup compiler (ISCC.exe) not found on local path. Skipping local installer compilation (automated in CI)."
}

Write-Host "=== Step 5: Generating SHA-256 Checksums ===" -ForegroundColor Cyan
$ChecksumFile = "$DistDir\SHA256SUMS.txt"
if (Test-Path $ChecksumFile) { Remove-Item $ChecksumFile -Force }

Get-ChildItem -Path "$DistDir\*" -Include *.zip, *.exe | ForEach-Object {
    $hash = (Get-FileHash -Path $_.FullName -Algorithm SHA256).Hash.ToLower()
    "$hash  $($_.Name)" | Out-File -FilePath $ChecksumFile -Append -Encoding ascii
}

Write-Host "Checksums written to ${ChecksumFile}:" -ForegroundColor Green
Get-Content $ChecksumFile

Write-Host "=== Packaging Complete! ===" -ForegroundColor Green
