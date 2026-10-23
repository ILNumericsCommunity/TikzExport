using System;
using System.Drawing;
using System.Linq;
using ILNumerics;
using ILNumerics.Community.ExportPreparation;
using ILNumerics.Community.ExportSamples;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using Xunit;
#if WEB_EXPORT
using CSPlotlySlim.Chart.Traces;
using CSPlotlySlim.Chart.Traces.Properties;
using Exporter = ILNumerics.Community.WebExport.WebExport;

#else
using Exporter = ILNumerics.Community.TikzExport.TikzExport;
#endif

public partial class ExportTests
{
    [Theory]
    [InlineData(BarPlotStyle.grouped, false, false)]
    [InlineData(BarPlotStyle.stacked, false, false)]
    [InlineData(BarPlotStyle.grouped, true, false)]
    [InlineData(BarPlotStyle.detached, false, true)]
    [InlineData(BarPlotStyle.hist, false, false)]
    [InlineData(BarPlotStyle.histc, false, false)]
    [InlineData(BarPlotStyle.grouped, false, true)]
    [InlineData(BarPlotStyle.grouped, true, true)]
    [InlineData(BarPlotStyle.stacked, false, true)]
    [InlineData(BarPlotStyle.stacked, true, false)]
    public void PreparedBarsHaveCorrectLayoutWithoutMutatingSource(BarPlotStyle style, bool horizontal, bool threeD)
    {
        using var scope = Scope.Enter();
        Array<float> data = new float[,] { { 2, 3 }, { 4, -1 }, { 1, 5 } };
        using var expected = new BarPlot(data, style: style, drawHorizontal: horizontal, draw3DBars: threeD, baseValue: 0.5f);
        expected.Padding = 0.35f;
        expected.Configure(false, false);
        var source = new BarPlot(data, style: style, drawHorizontal: horizontal, draw3DBars: threeD, baseValue: 0.5f);
        source.Padding = 0.35f;
        using var scene = new Scene { new PlotCube(twoDMode: !threeD) { source } };
        var before = source.Fill.Positions.Storage.ToArray();
        var version = source.Version;
        using (var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>()))
        {
            var actual = prepared.Cube.First<BarPlot>();
            if (style == BarPlotStyle.detached && threeD)
                Assert.Equal(expected.Fill.Positions.Storage.ToArray(), Enumerable.ToArray<float>(actual.Fill.Positions.Storage));
            else if (style != BarPlotStyle.stacked)
            {
                Array<float> p = actual.Fill.Positions.Storage;
                var axis = horizontal ? threeD ? 2 : 1 : 0;
                var vertices = threeD ? 9 : 5;
                for (var bar = 0; bar < 6; bar++)
                {
                    var xs = Enumerable.Range(bar * vertices, vertices - 1).Select(i => p.GetValue(axis, i)).ToArray();
                    var center = (bar / 2) + 1 + (((bar % 2) + .5f) * .325f) - .325f + (style == BarPlotStyle.hist ? .1625f : 0);
                    Assert.True(Math.Abs(((xs.Min() + xs.Max()) / 2) - center) < 0.00001, $"bar {bar}: {xs.Min()}..{xs.Max()}, expected center {center}");
                }
            }
            else
            {
                Array<float> p = actual.Fill.Positions.Storage;
                Assert.Equal(.5f, p.GetValue(horizontal ? 0 :
                                             threeD ? 2 : 1, 0));
            }
            Assert.Equal(expected.Fill.Indices.Storage.ToArray(), Enumerable.ToArray<int>(actual.Fill.Indices.Storage));
        }
        string first = ExportText(scene), second = ExportText(scene);
#if TIKZ_EXPORT
        Assert.Equal(first, second);
#endif
        Assert.Equal(before, source.Fill.Positions.Storage.ToArray());
        Assert.Equal(version, source.Version);
        Assert.Equal(0.35f, source.Padding);
    }

    [Fact]
    public void PendingNullableBarColorsAndUpdatesArePreserved()
    {
        using var scope = Scope.Enter();
        var bar = new BarPlot((Array<float>) new float[,] { { 1, 2, 3 } });
        bar.SetBarColor(1, fillColor: Color.Crimson);
        using var scene = new Scene { new PlotCube { bar } };
        using (var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>()))
        {
            Array<float> colors = prepared.Cube.First<BarPlot>().Fill.Colors.Storage;
            Assert.Equal(Color.Crimson.R / 255f, colors.GetValue(0, 5));
            Assert.Equal(Color.Crimson.G / 255f, colors.GetValue(1, 5));
        }

        // Avoid the upstream nullable-color bug in Update by updating a fresh bar.
        var updated = new BarPlot((Array<float>) new float[,] { { 1, 2 } });
        using var changedScene = new Scene { new PlotCube { updated } };
        var before = ExportText(changedScene);
        updated.Update((Array<float>) new float[,] { { 7, 9 } });
        Assert.NotEqual(before, ExportText(changedScene));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplinePreparationPreservesEndpointsResolutionAndSource(bool path)
    {
        using var scope = Scope.Enter();
        var spline = new SplinePlot((Array<float>) new float[,] { { 0, 1, 4, 9 } }, resolution: 8, useSplinePathFor1D: path, markerStyle: MarkerStyle.Circle);
        using var scene = new Scene { new PlotCube { spline } };
        var original = spline.SmoothLine.Positions.Storage.ToArray();
        var version = spline.Version;
        using (var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>()))
        {
            Array<float> p = prepared.Cube.First<SplinePlot>().SmoothLine.Positions.Storage;
            Assert.Equal(32, p.S[1]);
            Assert.Equal(0, p.GetValue(1, 0));
            Assert.InRange(p.GetValue(1, 31), 8.999f, 9.001f);
            if (!path)
                for (var i = 0; i < 32; i++)
                    Assert.InRange(Math.Abs(p.GetValue(1, i) - Math.Pow(p.GetValue(0, i), 2)), 0, 0.0001);
        }
        ExportText(scene);
        Assert.Equal(original, spline.SmoothLine.Positions.Storage.ToArray());
        Assert.Equal(version, spline.Version);
        Assert.Equal(8, spline.Resolution);
    }

    [Fact]
    public void AutoSplineResolutionIsDeterministicAndNeverBecomesAControlPolygon()
    {
        using var scene = ExportDemoScenes.Create(14);
        using var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>());
        var source = scene.First<SplinePlot>();
        Array<float> p = prepared.Cube.First<SplinePlot>().SmoothLine.Positions.Storage;
        Assert.Equal(128, p.S[1]);
        Assert.Null(source.Resolution);
#if WEB_EXPORT
        var chart = Exporter.GetChart(scene)!;
        Assert.Contains(chart.Traces, t => t is ScatterTrace trace && trace.X.Count == 128);
#else
        string text = ExportText(scene);
        Assert.DoesNotContain("control polygon", text);
        Assert.Contains(@"\addplot", text);
#endif
    }

    [Fact]
    public void ColormappedBarsHaveGeometryAndSourceSurvivesDisposal()
    {
        using var scene = ExportDemoScenes.Create(20);
        var bar = scene.First<BarPlotEx>();
        var before = bar.Fill.Positions.Storage.ToArray();
        using (var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>()))
        {
            Assert.True(prepared.Cube.First<BarPlotEx>().Fill.Indices.Storage.Length > 0);
        }
        ExportText(scene);
        Assert.Equal(before, bar.Fill.Positions.Storage.ToArray());
    }

    [Fact]
    public void SplineErrorsPropagateWithoutAControlPolygonFallback()
    {
        using var scope = Scope.Enter();
        using var scene = new Scene { new PlotCube { new SplinePlot((Array<float>) new float[,] { { 1, 1, 1, 1 }, { 2, 2, 2, 2 }, { 0, 0, 0, 0 } }, resolution: 8) } };
        Assert.ThrowsAny<Exception>(() => Exporter.ExportString(scene));
    }

    [Fact]
    public void BarBorderColorsAndSplineMarkersAreRetainedInExport()
    {
        using var scope = Scope.Enter();
        var bar = new BarPlot((Array<float>) new float[,] { { 1, 2, 3 } });
        bar.SetBarColor(1, Color.Red);
        using var scene = new Scene { new PlotCube { bar } };
        using (var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>()))
        {
            Assert.Contains(Enumerable.Select<Lines, Color?>(HeadlessPlotPreparation.Borders(prepared.Cube.First<BarPlot>().Border), l => l.Color).ToArray(),
                            c => c == Color.FromArgb(Color.Red.ToArgb()));
        }
        using var spline = ExportDemoScenes.Create(21);
#if WEB_EXPORT
        var traces = Exporter.GetChart(spline)!.Traces.OfType<ScatterTrace>().ToArray();
        Assert.Contains(traces, t => t.X.Count == 64);
        Assert.Contains(traces, t => t.X.Count == 4 && t.Mode == ScatterMode.Markers);
#else
        string text = ExportText(spline);
        Assert.Contains("only marks", text);
        Assert.Contains("mark=*", text);
        Assert.Contains("fill=none", text);
#endif
    }

    [Fact]
    public void GroupColorsApplyAcrossAllRowsAndDoNotConsumeTheSourcePalette()
    {
        using var scope = Scope.Enter();
        var bar = new BarPlot((Array<float>) new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } });
        bar.SetGroupColor(1, fillColor: Color.Lime);
        using var scene = new Scene { new PlotCube { bar } };
        using (var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>()))
        {
            Array<float> colors = prepared.Cube.First<BarPlot>().Fill.Colors.Storage;
            foreach (var vertex in new[] { 5, 15, 25 })
            {
                Assert.Equal(0, colors.GetValue(0, vertex));
                Assert.Equal(1, colors.GetValue(1, vertex));
            }
        }
        using var other = new BarPlot((Array<float>) new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } });
        Assert.Equal(other.NextColors.NextColor(), bar.NextColors.NextColor());
    }

    [Fact]
    public void LogarithmicBarsAreScaledExactlyOnce()
    {
        using var scope = Scope.Enter();
        var bar = new BarPlot((Array<float>) new float[,] { { 10, 100 } }, baseValue: 1);
        var cube = new PlotCube { bar };
        cube.ScaleModes.YAxisScale = AxisScale.Logarithmic;
        using var scene = new Scene { cube };
        using (var prepared = new HeadlessPlotPreparation(cube))
        {
            Array<float> p = prepared.Cube.First<BarPlot>().Fill.Positions.Storage;
            var ys = Enumerable.Range(0, (int) p.S[1]).Select(i => p.GetValue(1, i)).Where(Single.IsFinite).ToArray();
            Assert.Equal(0, ys.Min());
            Assert.Equal(2, ys.Max());
        }
        ExportText(scene);
    }

    [Fact]
    public void UpdatedSplineDoesNotReuseStaleTessellation()
    {
        using var scope = Scope.Enter();
        var spline = new SplinePlot((Array<float>) new float[,] { { 0, 1, 2, 3 }, { 0, 1, 0, 1 }, { 0, 0, 0, 0 } }, resolution: 8);
        using var scene = new Scene { new PlotCube { spline } };
        ExportText(scene);
        spline.Update((Array<float>) new float[,] { { 0, 1, 2, 3 }, { 10, 11, 10, 12 }, { 0, 0, 0, 0 } });
        using var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>());
        Array<float> p = prepared.Cube.First<SplinePlot>().SmoothLine.Positions.Storage;
        Assert.InRange(p.GetValue(1, 0), 9.999f, 10.001f);
        Assert.InRange(p.GetValue(1, 31), 11.999f, 12.001f);
    }

    [Fact]
    public void GroupedBarsRespectExplicitGroupPositions()
    {
        using var scope = Scope.Enter();
        var bar = new BarPlot((Array<float>) new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } }, (Array<float>) new float[,] { { 10, 20, 30 } });
        using var scene = new Scene { new PlotCube { bar } };
        using var prepared = new HeadlessPlotPreparation(scene.First<PlotCube>());
        Array<float> p = prepared.Cube.First<BarPlot>().Fill.Positions.Storage;
        for (var row = 0; row < 3; row++)
        {
            var xs = Enumerable.Range(row * 10, 9).Select(i => p.GetValue(0, i)).Where(Single.IsFinite).ToArray();
            Assert.InRange(Math.Abs(((xs.Min() + xs.Max()) / 2) - ((row + 1) * 10)), 0, 0.00001);
        }
    }

    [Theory]
    [InlineData(23)]
    [InlineData(24)]
    public void SharedMarkerCatalogContainsEveryStandardStyle(int index)
    {
        using var scene = ExportDemoScenes.Create(index);
        var styles = scene.Find<LinePlot>().Select(line => line.Marker.Style).OrderBy(style => style).ToArray();
        Assert.Equal(Enum.GetValues<MarkerStyle>().Where(style => style != MarkerStyle.Custom).OrderBy(style => style), styles);
        ExportText(scene);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(22)]
    [InlineData(24)]
    public void ThreeDimensionalDemosStartAngledAndAllowRotation(int index)
    {
        using var scene = ExportDemoScenes.Create(index);
        var cube = scene.First<PlotCube>();
        Assert.False(cube.TwoDMode);
        Assert.True(cube.AllowRotation);
        Assert.True(cube.Axes.ZAxis.Visible);
        Assert.NotEqual(Matrix4.Identity, cube.Rotation);
        using var copy = (Group) scene.Copy();
        Assert.False(copy.First<PlotCube>().TwoDMode);
        Assert.True(copy.First<PlotCube>().AllowRotation);
    }

    private static string ExportText(Scene scene)
    {
        var text = Exporter.ExportString(scene);
#if WEB_EXPORT
        Assert.DoesNotContain("ilnumerics-export-placeholder", text);
#else
        Assert.DoesNotContain("No exportable", text);
#endif
        return text;
    }
}
