# Monitorian 2.0 - Build and Release Package Script
# Usage: .\build-release.ps1 [-Version "2.2.0"]

param(
    [string]$Version = "2.2.0"
)

$ErrorActionPreference = "Stop"
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
$root    = $PSScriptRoot
$sln     = "$root\Source\Monitorian.sln"
$binDir  = "$root\Source\Monitorian\bin\Release"
$outDir  = "$root\Release-Package"

Write-Host "=== Monitorian 2.0 Release Build v$Version ===" -ForegroundColor Cyan

# Remove stale portable.ini from bin (leftover from dev testing - affects settings path)
$stalePortable = Join-Path $binDir "portable.ini"
if (Test-Path $stalePortable) { Remove-Item $stalePortable -Force; Write-Host "  Removed stale portable.ini from bin" }

# 1. NuGet restore (required so Costura.Fody hooks into build)
Write-Host "`n[1/4] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore "$root\Source\Monitorian\Monitorian.csproj" --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed" }

# 2. Build Release - Costura.Fody will embed all DLLs into the single EXE
Write-Host "[2/4] Building solution (Release)..." -ForegroundColor Yellow
& $msbuild $sln /p:Configuration=Release "/p:Platform=Any CPU" /t:Rebuild /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# Verify the merged EXE is reasonably sized (> 500KB means DLLs were embedded)
$exeSize = (Get-Item "$binDir\Monitorian.exe").Length
Write-Host "  Monitorian.exe: $([math]::Round($exeSize/1KB, 0)) KB"
if ($exeSize -lt 512KB) {
    Write-Warning "EXE is too small - Costura may not have embedded DLLs. Aborting."
    exit 1
}

Write-Host "[3/4] Packaging release..." -ForegroundColor Yellow

# Clean and recreate output folder
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
New-Item $outDir -ItemType Directory | Out-Null

# Copy Costura-merged EXE + config (single-file distribution)
Copy-Item "$binDir\Monitorian.exe"        "$outDir\Monitorian.exe"
Copy-Item "$binDir\Monitorian.exe.config" "$outDir\Monitorian.exe.config"
Write-Host "  Monitorian.exe ($([math]::Round($exeSize/1KB,0)) KB, all DLLs embedded)"

# Copy satellite language DLL folders (still needed at runtime for locale string resolution)
$langFolders = Get-ChildItem $binDir -Directory | Where-Object {
    (Get-ChildItem $_.FullName -Filter "*.dll" -Recurse -ErrorAction SilentlyContinue).Count -gt 0
}
foreach ($lang in $langFolders) {
    Copy-Item $lang.FullName (Join-Path $outDir $lang.Name) -Recurse
    Write-Host "  Lang: $($lang.Name)"
}

# VisualElements manifest (optional, for Start menu tiles)
$manifest = Join-Path $binDir "Monitorian.VisualElementsManifest.xml"
if (Test-Path $manifest) { Copy-Item $manifest $outDir }

# License and readme
Copy-Item "$root\LICENSE.txt" $outDir
Copy-Item "$root\README.md"   $outDir

# === Portable version ===
# Same as standard but with portable.ini (signals app to use local settings) + renamed EXE
$portableDir = "$root\Release-Package-Portable"
if (Test-Path $portableDir) { Remove-Item $portableDir -Recurse -Force }
Copy-Item $outDir $portableDir -Recurse
[System.IO.File]::WriteAllText("$portableDir\portable.ini", "")
Rename-Item "$portableDir\Monitorian.exe"        "Monitorian-Portable.exe"
Rename-Item "$portableDir\Monitorian.exe.config" "Monitorian-Portable.exe.config"

# 4. ZIP archives
Write-Host "[4/4] Creating ZIP archives..." -ForegroundColor Yellow

$zipPath         = "$root\Monitorian-$Version.zip"
$zipPortablePath = "$root\Monitorian-Portable-$Version.zip"

if (Test-Path $zipPath)         { Remove-Item $zipPath }
if (Test-Path $zipPortablePath) { Remove-Item $zipPortablePath }

Compress-Archive -Path "$outDir\*"      -DestinationPath $zipPath
Compress-Archive -Path "$portableDir\*" -DestinationPath $zipPortablePath

$hash1 = (Get-FileHash $zipPath         -Algorithm SHA256).Hash
$hash2 = (Get-FileHash $zipPortablePath -Algorithm SHA256).Hash

Write-Host "`n=== Build Complete ===" -ForegroundColor Green
Write-Host "Standard ZIP : $(Split-Path $zipPath -Leaf)"
Write-Host "  SHA256     : $hash1"
Write-Host "Portable ZIP : $(Split-Path $zipPortablePath -Leaf)"
Write-Host "  SHA256     : $hash2"
Write-Host ""
Write-Host "Output folders:"
Write-Host "  Standard : $outDir"
Write-Host "  Portable : $portableDir"
