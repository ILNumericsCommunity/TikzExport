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

The following plot types are supported:

| Plot family | Export behavior |
| --- | --- |
| LinePlot | 2D/3D coordinates, markers, legend entries, logarithmic axes |
| ErrorBarPlot | Horizontal/vertical asymmetric bars and their caps |
| Surface | Structured grids, including non-square grids, plot-local sampled colormaps |
| FastSurface | Triangle meshes and face colors |
| ImageSCPlot | Scalar cells with actual colormap colors; linear 2D axes |
| ContourPlot | Contour lines; filled regions and labels are not exported |
| BoxPlot, Candlestick, StackedAreaPlot, FillArea | Fills and outlines |
| Lines, LineStrip, Points | Lines, line strips and point markers |
| Triangles, TrianglesStrip, TrianglesFan | Filled polygons and 3D meshes; vertex colors approximated per face |
| SplinePlot | Interpolated curves with original markers |
| BarPlot, BarPlotEx | Grouped and stacked bars, horizontal and vertical orientation, baselines and custom colors |

## Getting started

Install the NuGet package:

```powershell
dotnet add package ILNumerics.Community.TikzExport
```

## Basic usage examples

Two main entry points are provided:

- ExportString(scene) - returns the TikZ/PGFPlots code as a string
- ExportFile(scene, filePath) - writes the TikZ/PGFPlots code to a file (usually with .tikz extension)

Both methods accept an optional Size canvasSize parameter that specifies the output canvas in millimeters. If omitted, the default is 100 x 100 mm per plot cube.

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

3) Export with diagnostics

   ```csharp
   var result = TikzExport.ExportWithReport(scene);
   string tikz = result.Tikz;
   foreach (var issue in result.Issues)
       Console.WriteLine($"Plot {issue.PlotCubeIndex}: {issue.NodeType}: {issue.Message}");
   ```

4) Specify a custom canvas size (millimeters)

   ```csharp
   int mmWidth = 160;
   int mmHeight = 120;
   var canvasSize = new System.Drawing.Size(mmWidth, mmHeight);

   string tikz = TikzExport.ExportString(scene, canvasSize);
   // OR
   TikzExport.ExportFile(scene, filePath, canvasSize);
   ```

### Contributing

Contributions, bug reports and feature requests are welcome. Please open an issue or a pull request on the GitHub repository.

### License

ILNumerics.Community.TikzExport is licensed under the MIT license (http://opensource.org/licenses/MIT). See LICENSE.txt for details.
