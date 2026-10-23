# Shared headless export sources

`ExportDemoScenes.cs` defines 23 deterministic scenes for both exporters. `HeadlessPlotPreparation.cs` is the production adapter for bar/spline preparation without a render driver. `PreparationTests.cs` is compiled into both test suites and checks native geometry parity, source immutability, updates, colors, markers, log axes, sampling, and exception propagation.

Keep these three files byte-identical in WebExport and TikzExport. Both repositories build independently. After editing, synchronize the files, run both test suites, and run `./Shared/VerifySamples.ps1 -OtherRepository <sibling-path>`.

The compatibility adapter is validated against ILNumerics 7.4.62. See the repository README for its private API dependency and sampling policy.
