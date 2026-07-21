param(
    [Parameter(Mandatory = $true)]
    [string] $OutputPck
)

$script = Join-Path $PSScriptRoot "export-pure-resource-pck.ps1"
& powershell -NoProfile -ExecutionPolicy Bypass -File $script -OutputPck $OutputPck
$exitCode = $LASTEXITCODE

if (Test-Path -LiteralPath $OutputPck -PathType Leaf) {
    try {
        if ((Get-Item -LiteralPath $OutputPck).Length -gt 0) {
            exit 0
        }
    } catch {
    }
}

if ($exitCode -ne 0) {
    exit $exitCode
}

exit 1
