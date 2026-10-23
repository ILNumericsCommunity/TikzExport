param([Parameter(Mandatory=$true)][string]$OtherRepository)
$ErrorActionPreference = 'Stop'
foreach ($name in @('ExportDemoScenes.cs', 'HeadlessPlotPreparation.cs', 'PreparationTests.cs')) {
    $local = Get-FileHash -LiteralPath (Join-Path $PSScriptRoot $name) -Algorithm SHA256
    $other = Get-FileHash -LiteralPath (Join-Path $OtherRepository "Shared/$name") -Algorithm SHA256
    if ($local.Hash -ne $other.Hash) { throw "Shared source differs: $name" }
}
Write-Output 'Shared samples, preparation, and tests are identical.'
