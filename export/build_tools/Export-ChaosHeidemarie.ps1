[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Dll", "Pck", "All")]
    [string] $Mode
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ExportRoot = Split-Path -Parent $PSScriptRoot
$ProjectRoot = Split-Path -Parent $ExportRoot
$ArtifactName = "Chaos_heidemarie"
$ArtifactDir = Join-Path $ExportRoot $ArtifactName
$ManifestSource = Join-Path $ProjectRoot "mod_manifest.json"
$DllSource = Join-Path $ProjectRoot ".godot\mono\temp\bin\Release\$ArtifactName.dll"
$PckOutput = Join-Path $ArtifactDir "$ArtifactName.pck"

function Ensure-ArtifactDirectory {
    New-Item -ItemType Directory -Force -Path $ArtifactDir | Out-Null

    $allowedFiles = @(
        "$ArtifactName.dll",
        "$ArtifactName.json",
        "$ArtifactName.pck"
    )
    $unexpected = @(
        Get-ChildItem -LiteralPath $ArtifactDir -Force |
            Where-Object { $_.Name -notin $allowedFiles }
    )

    if ($unexpected.Count -gt 0) {
        $names = $unexpected.Name -join ", "
        throw "Export folder contains files that are not part of the three-piece mod artifact: $names"
    }
}

function Copy-Manifest {
    if (-not (Test-Path -LiteralPath $ManifestSource -PathType Leaf)) {
        throw "Missing mod manifest: $ManifestSource"
    }

    Copy-Item -LiteralPath $ManifestSource -Destination (Join-Path $ArtifactDir "$ArtifactName.json") -Force
}

function Build-Dll {
    Push-Location $ProjectRoot
    try {
        & dotnet build ".\ChaosHeidemarie.csproj" -c Release -p:CopyToGame=false --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "DLL compilation failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }

    if (-not (Test-Path -LiteralPath $DllSource -PathType Leaf)) {
        throw "Expected Release DLL was not produced: $DllSource"
    }

    Copy-Item -LiteralPath $DllSource -Destination (Join-Path $ArtifactDir "$ArtifactName.dll") -Force
}

function Build-Pck {
    $exportScript = Join-Path $ProjectRoot "tools\export-pure-resource-pck.ps1"
    if (-not (Test-Path -LiteralPath $exportScript -PathType Leaf)) {
        throw "Missing PCK export script: $exportScript"
    }

    # The shared exporter performs the established staging/import/export-pack flow
    # and writes a PCK that is safe for the game's mod loader.
    & $exportScript -OutputPck $PckOutput
}

Ensure-ArtifactDirectory

switch ($Mode) {
    "Dll" {
        Build-Dll
        Copy-Manifest
        Write-Host "DLL export completed: $ArtifactDir"
    }
    "Pck" {
        Copy-Manifest
        Build-Pck
    }
    "All" {
        Build-Dll
        Copy-Manifest
        Build-Pck
    }
}
