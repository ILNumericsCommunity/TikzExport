param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'bin/latex-smoke'), [string]$LatexCommand = 'pdflatex')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $output -Force | Out-Null
$previous = $env:TIKZEXPORT_FIXTURES
try {
    $env:TIKZEXPORT_FIXTURES = $output
    & dotnet run --project $PSScriptRoot -c Release "-p:SolutionDir=$repo/"
    if ($LASTEXITCODE -ne 0) { throw 'Headless export tests failed.' }
} finally { $env:TIKZEXPORT_FIXTURES = $previous }
$lines = @('\documentclass{article}', '\usepackage[a4paper,margin=10mm]{geometry}', '\usepackage{pgfplots}', '\pgfplotsset{compat=1.18}', '\begin{document}')
foreach ($fixture in (Get-ChildItem -LiteralPath $output -Filter '*.tikz' | Sort-Object Name)) {
    $lines += '\input{' + $fixture.Name + '}\clearpage'
}
$lines += '\end{document}'
[IO.File]::WriteAllLines((Join-Path $output 'catalog.tex'), $lines)
Push-Location $output
try {
    & $LatexCommand -interaction=nonstopmode -halt-on-error catalog.tex
    if ($LASTEXITCODE -ne 0) { throw 'LaTeX compilation failed. See catalog.log.' }
} finally { Pop-Location }
