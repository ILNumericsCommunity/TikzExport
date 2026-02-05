TikzExport
==========

[![Nuget](https://img.shields.io/nuget/v/ILNumerics.Community.TikzExport?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/ILNumerics.Community.TikzExport)

Export functionality for ILNumerics (http://ilnumerics.net/) scene graphs and plot cubes to TikZ/PGFPlots (LaTeX graphics package, see [Wikipedia](https://en.wikipedia.org/wiki/PGF/TikZ)).

## About TikZ/PGFPlots

PGF/TikZ is a pair of languages for producing vector graphics from a geometric or algebraic description. PGF is a lower-level language that provides the primitives for drawing graphics, while TikZ is a collection of higher-level macros built on top of PGF that make it easier to describe complex figures. Together they are commonly used in LaTeX documents to produce technical illustrations, plots, and publication-quality figures. PGFPlots is a TeX package that builds on PGF/TikZ and provides a high-level interface specifically geared toward creating 2D and 3D plots (line plots, surface plots, error bars, etc.). Exporting ILNumerics scenes to PGF/TikZ/PGFPlots allows embedding figures directly in LaTeX documents while retaining sharp vector graphics and fine control over styling.

## Features

- Export ILNumerics scene graphs or individual plot cubes to TikZ/PGFPlots
- Produces TikZ code suitable for inclusion in LaTeX documents (usually saved as *.tikz files)
- Supports the most common plot types used in ILNumerics (see supported plot types)

**Supported plot types**

As of February 2026 (only) the following plot types are supported:

- LinePlot
- ErrorBarPlot (y-error only)
- Surface
- FastSurface

## Getting started

Install the NuGet package:

```powershell
dotnet add package ILNumerics.Community.TikzExport
```

## Basic usage examples

Two main entry points are provided:

- ExportString(scene) - returns the TikZ/PGFPlots code as a string
- ExportFile(scene, filePath) - writes the TikZ/PGFPlots code to a file (usually with .tikz extension)

Both methods accept an optional Size canvasSize parameter that specifies the output canvas in millimeters. If omitted, the default is 120 x 100 mm.

### Examples

1) Export scene to a string
   
   ```csharp
   // Create or obtain an ILNumerics scene (pseudocode)
   // var scene = new Scene() { ... };
   
   string tikz = TikzExport.ExportString(scene);
   ```

2) Export scene to a file
   
   ```csharp
   string filePath = "figure.tikz";
   TikzExport.ExportFile(scene, filePath);
   // The file 'figure.tikz' now contains the TikZ/PGFPlots markup
   ```

3) Convert Scene to a Chart object (CSPlotlySlim Chart) for programmatic inspection
   
   ```csharp
   CSPlotlySlim.Chart.Chart? chart = ILNumerics.Community.WebExport.WebExport.GetChart(scene);
   // (optional) Manipluate chart object as needed
   string? html = chart?.Render();
   ```

4) Specify a custom canvas size (millimeters)
   
   ```csharp
   int mmWidth = 160;
   int mmHeight = 120;
   var canvasSize = Size(mmWidth, mmHeight);
   
   string tikz = TikzExport.ExportString(scene, canvasSize);
   // OR
   TikzExport.ExportFile(scene, filePath, canvasSize);
   ```

## Notes and tips

- The exported TikZ code is intended to be included in LaTeX documents. To compile the output you will typically need to include the pgfplots package in your LaTeX preamble (for example: \usepackage{pgfplots} and \pgfplotsset{compat=1.18}).
- Some complex ILNumerics features or custom shaders are not translated and may be omitted or approximated in the export.
- If you need additional plot types or custom styling, consider post-processing the generated TikZ code or extending the exporter.

### Contributing

Contributions, bug reports and feature requests are welcome. Please open an issue or a pull request on the GitHub repository.

### License

ILNumerics.Community.TikzExport is licensed under the MIT license (http://opensource.org/licenses/MIT). See LICENSE.txt for details.
