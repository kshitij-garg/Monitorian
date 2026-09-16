[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$RunApplicationSmoke
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$appOutput = Join-Path $repoRoot "Source\Monitorian\bin\$Configuration"
$coreOutput = Join-Path $repoRoot "Source\Monitorian.Core\bin\$Configuration"
$resourceSource = Join-Path $repoRoot "Source\Monitorian.Core\Properties"

function Assert-File {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if (-not (Test-Path $Path -PathType Leaf)) {
        throw "Required build output is missing: $Path"
    }
}

$appPath = Join-Path $appOutput "Monitorian.exe"
$portablePath = Join-Path $appOutput "Monitorian-Portable.exe"

Assert-File $appPath
Assert-File (Join-Path $appOutput "Monitorian.exe.config")
Assert-File $portablePath
Assert-File (Join-Path $appOutput "Monitorian-Portable.exe.config")
Assert-File (Join-Path $coreOutput "Monitorian.Core.dll")

if ((Get-Item $appPath).Length -lt 512KB) {
    throw "Monitorian.exe is smaller than 512 KB; Costura dependency embedding likely failed."
}

$baseResourcePath = Join-Path $resourceSource "Resources.resx"
[xml]$baseResource = Get-Content $baseResourcePath -Raw
$baseKeys = @($baseResource.root.data | ForEach-Object { $_.name })

if ($baseKeys.Count -eq 0) {
    throw "The invariant resource file contains no strings."
}

$cultureFiles = Get-ChildItem $resourceSource -Filter "Resources.*.resx" -File
if ($cultureFiles.Count -eq 0) {
    throw "No localized resource files were found."
}

foreach ($cultureFile in $cultureFiles) {
    $culture = $cultureFile.BaseName.Substring("Resources.".Length)

    try {
        [void][Globalization.CultureInfo]::GetCultureInfo($culture)
    }
    catch {
        throw "Resource file '$($cultureFile.Name)' has invalid culture code '$culture'."
    }

    [xml]$localizedResource = Get-Content $cultureFile.FullName -Raw
    $localizedKeys = @($localizedResource.root.data | ForEach-Object { $_.name })

    if ($localizedKeys.Count -eq 0) {
        throw "Resource file '$($cultureFile.Name)' contains no strings."
    }

    $unknownKeys = @($localizedKeys | Where-Object { $_ -notin $baseKeys })
    if ($unknownKeys.Count -gt 0) {
        throw "Resource file '$($cultureFile.Name)' contains unknown keys: $($unknownKeys -join ', ')"
    }

    Assert-File (Join-Path $coreOutput "$culture\Monitorian.Core.resources.dll")
    Assert-File (Join-Path $appOutput "$culture\Monitorian.Core.resources.dll")
}

Write-Host "Validated $($cultureFiles.Count) cultures and required $Configuration outputs."

if (-not $RunApplicationSmoke) {
    return
}

$primary = $null
$secondary = $null

try {
    $primary = Start-Process $appPath -ArgumentList "/lang", "hi" -PassThru

    if ($primary.WaitForExit(8000)) {
        throw "Monitorian exited unexpectedly during localized startup with code $($primary.ExitCode)."
    }

    $secondary = Start-Process $appPath -ArgumentList "/get" -PassThru
    if (-not $secondary.WaitForExit(30000)) {
        throw "The CLI forwarding smoke test timed out."
    }
    if ($secondary.ExitCode -ne 0) {
        throw "The CLI forwarding smoke test exited with code $($secondary.ExitCode)."
    }

    Write-Host "Localized startup and CLI forwarding smoke tests passed."
}
finally {
    if ($secondary -and -not $secondary.HasExited) {
        Stop-Process -Id $secondary.Id -Force -ErrorAction SilentlyContinue
    }
    if ($primary -and -not $primary.HasExited) {
        Stop-Process -Id $primary.Id -Force -ErrorAction SilentlyContinue
    }
}
