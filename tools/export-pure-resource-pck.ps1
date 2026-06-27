param(
    [string] $GodotPath = "C:\Users\admin\Documents\Chaos_heidemarie\.tools\godot-4.5.1\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe",
    [string] $SpineTemplateProject = "H:\Trae_home\ChaosMod\git\Chaos_XC_regent02\Chaos_XC_regent02_pck",
    [string] $StageRoot = "H:\Trae_home\ChaosMod\pck_build\heidemarie_pure",
    [string] $OutputPck = "C:\Users\admin\Documents\Chaos_heidemarie\ChaosHeidemarie.pck",
    [string] $PresetName = "Windows Desktop"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$stageProject = Join-Path $StageRoot "Slay the Spire 2"

if (Test-Path -LiteralPath $StageRoot) {
    $resolvedStage = (Resolve-Path -LiteralPath $StageRoot).Path
    if (-not $resolvedStage.StartsWith("H:\Trae_home\ChaosMod\pck_build", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove unexpected staging directory: $resolvedStage"
    }
    Remove-Item -LiteralPath $resolvedStage -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $stageProject | Out-Null

foreach ($name in @("project.godot", "export_presets.cfg", "spine_godot_extension.gdextension", "spine_godot_extension.gdextension.uid", "windows")) {
    $src = Join-Path $SpineTemplateProject $name
    $dst = Join-Path $stageProject $name
    if (Test-Path -LiteralPath $src -PathType Container) {
        robocopy $src $dst /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -gt 7) {
            throw "robocopy failed for $src with exit code $LASTEXITCODE"
        }
    } elseif (Test-Path -LiteralPath $src -PathType Leaf) {
        Copy-Item -LiteralPath $src -Destination $dst -Force
    }
}

Copy-Item -LiteralPath (Join-Path $ProjectRoot "project.godot") -Destination (Join-Path $stageProject "project.godot") -Force
Copy-Item -LiteralPath (Join-Path $ProjectRoot "export_presets.cfg") -Destination (Join-Path $stageProject "export_presets.cfg") -Force

foreach ($name in @("ArtWorks", "src", "mod_manifest.json")) {
    $src = Join-Path $ProjectRoot $name
    $dst = Join-Path $stageProject $name
    if (Test-Path -LiteralPath $src -PathType Container) {
        robocopy $src $dst /MIR /XD bin obj .godot .tools /XF *.cs *.csproj *.sln *.log /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -gt 7) {
            throw "robocopy failed for $src with exit code $LASTEXITCODE"
        }
    } elseif (Test-Path -LiteralPath $src -PathType Leaf) {
        Copy-Item -LiteralPath $src -Destination $dst -Force
    }
}

$locSourceRoot = Join-Path $ProjectRoot "src\Localization"
if (Test-Path -LiteralPath $locSourceRoot -PathType Container) {
    Get-ChildItem -LiteralPath $locSourceRoot -Directory | ForEach-Object {
        $tableName = $_.Name
        Get-ChildItem -LiteralPath $_.FullName -File -Filter "*.json" | ForEach-Object {
            $language = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
            $destinationDir = Join-Path $stageProject ("ChaosHeidemarie\Localization\" + $tableName)
            New-Item -ItemType Directory -Force -Path $destinationDir | Out-Null
            Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $destinationDir ($language + ".json")) -Force
        }
    }
}

& $GodotPath --headless --path $stageProject --import --quit
if ($LASTEXITCODE -ne 0) {
    throw "Godot import failed with exit code $LASTEXITCODE"
}

function Get-ImportDestPath([string] $ImportPath) {
    $content = Get-Content -LiteralPath $ImportPath -Raw
    if ($content -notmatch 'dest_files=\["([^"]+)"\]') {
        return $null
    }

    $resPath = $Matches[1]
    if (-not $resPath.StartsWith("res://", [System.StringComparison]::OrdinalIgnoreCase)) {
        return $null
    }

    return Join-Path $stageProject ($resPath.Substring(6).Replace("/", "\"))
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

Get-ChildItem -LiteralPath $stageProject -Recurse -File -Filter "*.atlas.import" | ForEach-Object {
    $dest = Get-ImportDestPath $_.FullName
    if (-not $dest) {
        return
    }

    $source = $_.FullName.Substring(0, $_.FullName.Length - ".import".Length)
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Missing atlas source for import file: $($_.FullName)"
    }

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    $atlasData = [System.IO.File]::ReadAllText($source, [System.Text.Encoding]::UTF8)
    $sourceResPath = "res://" + $source.Substring($stageProject.Length + 1).Replace("\", "/")
    $json = [ordered]@{
        atlas_data = $atlasData
        normal_texture_prefix = "n"
        source_path = $sourceResPath
        specular_texture_prefix = "s"
    } | ConvertTo-Json -Compress
    [System.IO.File]::WriteAllText($dest, $json, $utf8NoBom)
}

Get-ChildItem -LiteralPath $stageProject -Recurse -File -Filter "*.skel.import" | ForEach-Object {
    $dest = Get-ImportDestPath $_.FullName
    if (-not $dest) {
        return
    }

    $source = $_.FullName.Substring(0, $_.FullName.Length - ".import".Length)
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Missing skeleton source for import file: $($_.FullName)"
    }

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    Copy-Item -LiteralPath $source -Destination $dest -Force
}

$cacheFiles = @(
    (Join-Path $stageProject ".godot\uid_cache.bin"),
    (Join-Path $stageProject ".godot\extension_list.cfg")
)

foreach ($path in $cacheFiles) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

& $GodotPath --headless --path $stageProject --export-pack $PresetName $OutputPck --quit
if ($LASTEXITCODE -ne 0) {
    throw "Godot export failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath $OutputPck)) {
    throw "PCK export did not produce output: $OutputPck"
}

$bytes = [System.IO.File]::ReadAllBytes($OutputPck)
$text = [System.Text.Encoding]::UTF8.GetString($bytes)

if ($text -match "windows/libspine_godot") {
    throw "Exported PCK still contains a Spine native library."
}

if ($text -match "spine_godot_extension") {
    Write-Warning "Exported PCK still contains the Spine extension project setting; keeping it to avoid corrupting the PCK index."
}

Write-Host "Pure resource PCK export completed: $OutputPck"
