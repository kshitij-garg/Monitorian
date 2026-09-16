[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Version,

    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$binDirectory = Join-Path $repoRoot "Source\Monitorian\bin\$Configuration"
$appPath = Join-Path $binDirectory "Monitorian.exe"

if (-not (Test-Path $appPath -PathType Leaf)) {
    throw "Build Monitorian before packaging: $appPath"
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $fileVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($appPath).FileVersion
    $parsedVersion = [Version]$fileVersion
    $Version = "$($parsedVersion.Major).$($parsedVersion.Minor).$($parsedVersion.Build)"
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot "artifacts\release"
}
elseif (-not [IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot $OutputDirectory
}

$standardDirectory = Join-Path $OutputDirectory "Monitorian-$Version"
$portableDirectory = Join-Path $OutputDirectory "Monitorian-Portable-$Version"

if (Test-Path $OutputDirectory) {
    Remove-Item $OutputDirectory -Recurse -Force
}
New-Item $standardDirectory -ItemType Directory -Force | Out-Null

Copy-Item $appPath $standardDirectory
Copy-Item (Join-Path $binDirectory "Monitorian.exe.config") $standardDirectory
Copy-Item (Join-Path $repoRoot "LICENSE.txt") $standardDirectory
Copy-Item (Join-Path $repoRoot "README.md") $standardDirectory

$manifest = Join-Path $binDirectory "Monitorian.VisualElementsManifest.xml"
if (Test-Path $manifest) {
    Copy-Item $manifest $standardDirectory
}

Get-ChildItem $binDirectory -Directory | Where-Object {
    Test-Path (Join-Path $_.FullName "Monitorian.Core.resources.dll")
} | ForEach-Object {
    Copy-Item $_.FullName (Join-Path $standardDirectory $_.Name) -Recurse
}

Copy-Item $standardDirectory $portableDirectory -Recurse
New-Item (Join-Path $portableDirectory "portable.ini") -ItemType File -Force | Out-Null
Rename-Item (Join-Path $portableDirectory "Monitorian.exe") "Monitorian-Portable.exe"
Rename-Item (Join-Path $portableDirectory "Monitorian.exe.config") "Monitorian-Portable.exe.config"

$standardZip = Join-Path $OutputDirectory "Monitorian-$Version.zip"
$portableZip = Join-Path $OutputDirectory "Monitorian-Portable-$Version.zip"
Compress-Archive -Path (Join-Path $standardDirectory "*") -DestinationPath $standardZip
Compress-Archive -Path (Join-Path $portableDirectory "*") -DestinationPath $portableZip

Copy-Item $appPath (Join-Path $OutputDirectory "Monitorian.exe")
Copy-Item (Join-Path $portableDirectory "Monitorian-Portable.exe") $OutputDirectory

$releaseFiles = @(
    (Join-Path $OutputDirectory "Monitorian.exe"),
    (Join-Path $OutputDirectory "Monitorian-Portable.exe"),
    $standardZip,
    $portableZip
)

$checksums = $releaseFiles | ForEach-Object {
    $hash = Get-FileHash $_ -Algorithm SHA256
    "$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($_))"
}
$checksums | Set-Content (Join-Path $OutputDirectory "SHA256SUMS.txt") -Encoding ascii

Write-Host "Created release package v$Version in $OutputDirectory"
