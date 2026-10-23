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

The standard plot families match WebExport's catalog:

| Plot family | Export behavior |
| --- | --- |
| LinePlot | 2D/3D coordinates, markers, legend entries, logarithmic axes |
| ErrorBarPlot | Horizontal/vertical asymmetric bars and their caps |
| Surface | Structured grids, including non-square grids, plot-local sampled colormaps |
| FastSurface | Existing indexed triangle geometry and face colors |
| ImageSCPlot | Scalar cells with actual colormap colors; linear 2D axes |
| ContourPlot | Existing contour lines; fills and contour labels omitted |
| BoxPlot, Candlestick, StackedAreaPlot, FillArea | Existing filled geometry and outlines |
| Lines, LineStrip, Points | Existing indexed geometry |
| Triangles, TrianglesStrip, TrianglesFan | Indexed faces with NaN run separators; vertex colors approximated per face |
| SplinePlot | Native not-a-knot interpolation, computed headlessly; original markers retained |
| BarPlot, BarPlotEx | Headless geometry preparation, including grouping/stacking, orientation, baseline, padding and color overrides |

MPI-specific plots such as WavePlot do not belong in this library.

## Getting started

Install the NuGet package:

```powershell
dotnet add package ILNumerics.Community.TikzExport
```

## Basic usage examples

Two main entry points are provided:

- ExportString(scene) - returns the TikZ/PGFPlots code as a string
- ExportFile(scene, filePath) - writes the TikZ/PGFPlots code to a file (usually with .tikz extension)

Both methods accept an optional Size canvasSize parameter that specifies the output canvas in millimeters. If omitted, the default is 100 x 100 mm per plot cube. Multiple cubes are stacked vertically with 25 mm spacing; original viewport placement is not retained.

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

## Export contract and limitations

- Call export/bind on the scene-owning thread, without concurrent mutation. Export snapshots geometry, prepares bars/splines on isolated copies, and does not call `Scene.Configure()` or require a window. Returned pictures contain snapshots and can be written after the scene is disposed.
- Empty/unsupported plots produce visible placeholders. `ExportWithReport` and `TikzPicture.Issues` expose conversion limitations; unexpected exceptions propagate. Cube indices are zero based (`-1` for a scene-level issue). `IsEmpty` still identifies pictures with no converted plot data.
- Visibility is respected. Nonidentity transforms on plot groups are reported and omitted. Custom shaders, text decorations, statistical legend semantics, lighting, and the original 3D camera rotation are not reconstructed. 3D views use a fixed angle; patch meshes use depth sorting within bounded batches rather than the original renderer. WebExport and TikzExport share plot families, not pixel-identical rendering.
- Large image/triangle scenes generate proportionally large vector output. Color lookup is cached and numeric data is captured once. For very dense datasets, decimate upstream or use a bitmap export.
- Plain labels escape TeX special characters; explicit `$...$` math is retained as trusted TeX. Unicode Greek symbols and simple sub/superscripts are supported. Do not treat this as a sandbox for untrusted TeX input.

## LaTeX and validation

Include `\usepackage{pgfplots}` in the document preamble, then `\input{figure.tikz}` in the document. The fragment requires PGFPlots 1.13 or newer.

```powershell
dotnet build TikzExport.slnx -c Release
dotnet run --project TikzExport.Tests -c Release -p:SolutionDir="$PWD/"
./Shared/VerifySamples.ps1 -OtherRepository ../WebExport
```

`Shared/ExportDemoScenes.cs` is byte-identical in both repositories and compiled into both demos and test suites. The catalog contains 23 deterministic scenes, including grouped/stacked/horizontal/3D/histogram/colormapped bars and 1D/3D splines. Set `TIKZEXPORT_FIXTURES` to an output directory when running tests to emit the catalog and regression fragments. `TikzExport.Tests/LatexSmoke.ps1` generates and compiles the fixtures with a local `pdflatex` installation containing PGFPlots. No Avalonia, ILNumerics window, or display control is needed.

### Contributing

Contributions, bug reports and feature requests are welcome. Please open an issue or a pull request on the GitHub repository.

### License

ILNumerics.Community.TikzExport is licensed under the MIT license (http://opensource.org/licenses/MIT). See LICENSE.txt for details.

## Headless bar and spline preparation

Both exporters compile the identical `Shared/HeadlessPlotPreparation.cs` adapter and run identical `Shared/PreparationTests.cs` tests. Source buffers are explicitly detached, so preparation, disposal, and repeated exports do not change the caller's plot. Bounds include the prepared geometry. Bar border colors are grouped into independent line batches; TikzExport batches 3D faces to avoid excessive TeX memory consumption.

Explicit `SplinePlot.Resolution` is honored. When it is null, export uses **32** as the native sampling factor (ILNumerics 7.4.62 produces `controlPointCount * resolution` samples across the curve). This is deterministic rather than dependent on viewport pixels. Native chord-length and 1D interpolation modes are retained; original data markers remain at the control points. Invalid interpolation data propagates an exception, never a control-polygon fallback.

Compatibility: ILNumerics 7.4.62 keeps spline tessellation and some bar settings private. The shared adapter uses a small reflection bridge to invoke its native math and preserve state omitted by native copies, with explicit errors if the required members disappear. The adapter also repairs native grouped-row offsets and the ignored stacked baseline; tests check expected coordinates independently of the native implementation. Bar calculation runs only on detached nodes, bypassing axis/font creation. Nullable per-bar/per-group color overrides are applied independently because the native implementation dereferences both optional colors. This bridge requires review when upgrading ILNumerics and is not intended for trimming/AOT without preserving these members. A public upstream `PrepareGeometry` API would remove that dependency.

Automatic ticks/legends and other general export limitations still apply; preparation reconstructs geometry, not native UI layout. Desktop UI rendering is a separate validation path.

## Release validation

Build the solution in Release and run the headless test executable:

```powershell
dotnet build TikzExport.slnx -c Release
dotnet run --project TikzExport.Tests -c Release
dotnet list TikzExport.slnx package --outdated
./Shared/VerifySamples.ps1 -OtherRepository ../WebExport
```

Public API documentation warnings fail the build. Apply JetBrains `Built-in: Careful Cleanup` using the solution settings, then review the diff and rerun both exporter suites. Shared conditional sources must retain imports required by both exporters; keep their three shared files byte-identical after cleanup.

Release tags use `vMAJOR.MINOR.PATCH`. The existing package convention appends a date-based build number, so tag `v7.2.0` produces package version `7.2.0.<build-number>`. Build and pack after tagging to embed the release version and source revision. Build-time analyzers and MinVer are private dependencies.
