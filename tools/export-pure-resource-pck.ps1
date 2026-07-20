param(
    [string] $GodotPath,
    [string] $BaseResourceRoot,
    [string] $StageRoot,
    [string] $OutputPck,
    [string] $PresetName = "Windows Desktop"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$ChaosModRoot = Split-Path -Parent (Split-Path -Parent $ProjectRoot)

function Resolve-FirstExistingFile([string] $label, [string[]] $candidates) {
    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "Could not locate $label. See export\build_tools\PATHS.md for configuration."
}

function Resolve-FirstExistingDirectory([string] $label, [string[]] $candidates) {
    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate -PathType Container)) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "Could not locate $label. See export\build_tools\PATHS.md for configuration."
}

if (-not $GodotPath) {
    $GodotPath = $env:CHAOS_GODOT_PATH
}
$godotCandidates = @(
    $GodotPath,
    (Join-Path $ChaosModRoot "Godot_v4.5.1\Godot_v4.5.1-stable_win64.exe"),
    (Join-Path $ChaosModRoot "Godot_v4.5.1\Godot_v4.5.1-stable_mono_win64.exe")
)
$godotCommand = Get-Command "godot" -CommandType Application -ErrorAction SilentlyContinue
if ($godotCommand) {
    $godotCandidates += $godotCommand.Source
}
$GodotPath = Resolve-FirstExistingFile "Godot 4.5.1 executable" $godotCandidates

if (-not $BaseResourceRoot) {
    $BaseResourceRoot = $env:CHAOS_STS2_RESOURCE_ROOT
}
$BaseResourceRoot = Resolve-FirstExistingDirectory "extracted Slay the Spire 2 resource root" @(
    $BaseResourceRoot,
    (Join-Path $ChaosModRoot "refer\Slay the Spire 2_pck_new6-10\pck")
)

if (-not $StageRoot) {
    $StageRoot = Join-Path $ProjectRoot "export\build_tools\.pck_stage"
}
$StageRoot = [System.IO.Path]::GetFullPath($StageRoot)
$allowedStageParent = [System.IO.Path]::GetFullPath((Join-Path $ProjectRoot "export\build_tools"))
if (-not $StageRoot.StartsWith($allowedStageParent + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Staging directory must stay inside export\build_tools: $StageRoot"
}

if (-not $OutputPck) {
    $OutputPck = Join-Path $ProjectRoot "export\Chaos_heidemarie\Chaos_heidemarie.pck"
}
$OutputPck = [System.IO.Path]::GetFullPath($OutputPck)

$stageProject = Join-Path $StageRoot "Slay the Spire 2"

if (Test-Path -LiteralPath $StageRoot) {
    $resolvedStage = (Resolve-Path -LiteralPath $StageRoot).Path
    if (-not $resolvedStage.StartsWith($allowedStageParent + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove unexpected staging directory: $resolvedStage"
    }
    Remove-Item -LiteralPath $resolvedStage -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $stageProject | Out-Null

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

function Copy-BaseFile([string] $relativePath) {
    $src = Join-Path $BaseResourceRoot $relativePath
    $dst = Join-Path $stageProject $relativePath
    if (-not (Test-Path -LiteralPath $src -PathType Leaf)) {
        throw "Missing base export dependency: $src"
    }

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dst) | Out-Null
    Copy-Item -LiteralPath $src -Destination $dst -Force
}

function Copy-BaseDirectory([string] $relativePath) {
    $src = Join-Path $BaseResourceRoot $relativePath
    $dst = Join-Path $stageProject $relativePath
    if (-not (Test-Path -LiteralPath $src -PathType Container)) {
        throw "Missing base export dependency directory: $src"
    }

    robocopy $src $dst /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -gt 7) {
        throw "robocopy failed for $src with exit code $LASTEXITCODE"
    }
}

foreach ($path in @(
    "addons\mega_text\MegaLabel.cs",
    "src\Core\Nodes\Combat\NEnergyCounter.cs",
    "src\Core\Nodes\Vfx\NCardTrail.cs",
    "src\Core\Nodes\Vfx\NCardTrailVfx.cs",
    "src\Core\Nodes\Vfx\Utilities\LocalizedTexture.cs",
    "src\Core\Nodes\Vfx\Utilities\NParticlesContainer.cs",
    "themes\canvas_item_material_additive_shared.tres",
    "themes\kreon_bold_shared.tres",
    "images\packed\vfx\small_card_silhouette.png",
    "images\packed\vfx\trail.png",
    "images\packed\vfx\trail2.png",
    "images\vfx\brush_particle_2.png",
    "images\vfx\vfx_ghostly_power_up\sparkle.png")) {
    Copy-BaseFile $path
}

foreach ($path in @(
    "fonts",
    "images\packed\vfx",
    "images\vfx",
    "materials\vfx",
    "shaders",
    "scenes\vfx\common",
    "scenes\vfx\fire_impact",
    "scenes\vfx\ribbon_flipbook")) {
    Copy-BaseDirectory $path
}

foreach ($path in @(
    "images\vfx\vfx_heal_osty")) {
    $dst = Join-Path $stageProject $path
    if (Test-Path -LiteralPath $dst) {
        Remove-Item -LiteralPath $dst -Recurse -Force
    }
}

foreach ($root in @(
    "images\vfx",
    "shaders")) {
    $dir = Join-Path $stageProject $root
    if (Test-Path -LiteralPath $dir) {
        Get-ChildItem -LiteralPath $dir -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match "^(necro_|osty_|regent_)|necrobinder|beam_necro" } |
            ForEach-Object { Remove-Item -LiteralPath $_.FullName -Force }
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
    Write-Warning "Godot import returned exit code $LASTEXITCODE; continuing to export-pack so the PCK existence check decides success."
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
    (Join-Path $stageProject ".godot\extension_list.cfg")
)

foreach ($path in $cacheFiles) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

# Keep the UID cache produced by this fresh import. Removing it makes export-pack
# reimport the whole staging tree; on this Mono build that process can crash before
# savepack. The only companion UID in the template belongs to the Spine extension
# and is removed together with that extension below.

# The Spine extension is required for the import pass, but loading it again in the
# export process crashes Godot 4.5.1 while hot-reloading the editor DLL.  The import
# artifacts above are self-contained for packing, so use the established two-phase
# flow: import with Spine present, then remove every editor-only extension file before
# export-pack.  The preset excludes the same paths as a second line of defence.
foreach ($path in @(
    (Join-Path $stageProject "spine_godot_extension.gdextension"),
    (Join-Path $stageProject "spine_godot_extension.gdextension.uid"),
    (Join-Path $stageProject "windows"),
    (Join-Path $stageProject "addons"),
    (Join-Path $stageProject ".godot\\extension_list.cfg")
)) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
    }
}

$outputDir = Split-Path -Parent $OutputPck
if ($outputDir) {
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
}

$exportStartedAt = Get-Date
$exportLog = Join-Path $StageRoot "export-pack.log"
$exportOutputPck = Join-Path (Split-Path -Parent $StageRoot) ([System.IO.Path]::GetFileName($OutputPck))
Remove-Item -LiteralPath $exportLog -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $exportOutputPck -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $OutputPck -Force -ErrorAction SilentlyContinue
# --export-pack exits on its own.  Adding --quit races its pack writer on this
# Godot Mono build, leaving a zero-byte temporary file instead of the PCK.
& $GodotPath --headless --log-file $exportLog --path $stageProject --export-pack $PresetName $exportOutputPck
if ($LASTEXITCODE -ne 0) {
    # Godot 4.5.1 occasionally returns before the asynchronous pack writer has
    # renamed its completed output.  Treat the actual fresh PCK as the source
    # of truth, but wait briefly for that writer before reporting a failure.
    for ($i = 0; $i -lt 300 -and -not (Test-Path -LiteralPath $exportOutputPck); $i++) {
        Start-Sleep -Milliseconds 100
    }

    if (-not (Test-Path -LiteralPath $exportOutputPck)) {
        throw "Godot export failed with exit code $LASTEXITCODE"
    }

    Write-Warning "Godot export returned exit code $LASTEXITCODE after producing the PCK; continuing."
}

if (-not (Test-Path -LiteralPath $exportOutputPck)) {
    throw "PCK export did not produce output: $exportOutputPck"
}

$outputInfo = Get-Item -LiteralPath $exportOutputPck
if ($outputInfo.LastWriteTime -lt $exportStartedAt.AddSeconds(-2)) {
    throw "PCK export did not refresh output: $exportOutputPck"
}

function Test-FileContainsAscii([string] $path, [string] $needle) {
    $needleBytes = [System.Text.Encoding]::ASCII.GetBytes($needle)
    $bufferSize = 1024 * 1024
    $buffer = New-Object byte[] $bufferSize
    $carry = New-Object byte[] ([Math]::Max($needleBytes.Length - 1, 0))

    $stream = [System.IO.File]::OpenRead($path)
    try {
        $carryLength = 0
        while (($read = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $window = New-Object byte[] ($carryLength + $read)
            if ($carryLength -gt 0) {
                [Array]::Copy($carry, 0, $window, 0, $carryLength)
            }

            [Array]::Copy($buffer, 0, $window, $carryLength, $read)

            $maxIndex = $window.Length - $needleBytes.Length
            $i = 0
            while ($i -le $maxIndex) {
                $i = [Array]::IndexOf($window, $needleBytes[0], $i)
                if ($i -lt 0 -or $i -gt $maxIndex) {
                    break
                }

                $matched = $true
                for ($j = 1; $j -lt $needleBytes.Length; $j++) {
                    if ($window[$i + $j] -ne $needleBytes[$j]) {
                        $matched = $false
                        break
                    }
                }

                if ($matched) {
                    return $true
                }

                $i++
            }

            $carryLength = [Math]::Min($carry.Length, $window.Length)
            if ($carryLength -gt 0) {
                [Array]::Copy($window, $window.Length - $carryLength, $carry, 0, $carryLength)
            }
        }
    } finally {
        $stream.Dispose()
    }

    return $false
}

if (Test-FileContainsAscii $exportOutputPck "windows/libspine_godot") {
    throw "Exported PCK still contains a Spine native library."
}

if (Test-FileContainsAscii $exportOutputPck "spine_godot_extension") {
    throw "Exported PCK still contains the Spine native extension project setting."
}

Copy-Item -LiteralPath $exportOutputPck -Destination $OutputPck -Force

# The packer must write beside the staging directory first, but that PCK is
# only an intermediate file.  Keep the requested output and remove the
# intermediate so it cannot be mistaken for a second distributable artifact.
if (-not [string]::Equals(
        [System.IO.Path]::GetFullPath($exportOutputPck),
        [System.IO.Path]::GetFullPath($OutputPck),
        [System.StringComparison]::OrdinalIgnoreCase)) {
    Remove-Item -LiteralPath $exportOutputPck -Force
}

Write-Host "Pure resource PCK export completed: $OutputPck"
exit 0
