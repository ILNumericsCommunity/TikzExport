using System.Drawing;
using System.Globalization;
using ILNumerics;
using ILNumerics.Community.TikzExport.Generator.Elements;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using Xunit;
using Exporter = ILNumerics.Community.TikzExport.TikzExport;

public sealed partial class ExportTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void MarkersPreserveIndependentFillAndBorderInBothBinders(bool fill, bool border)
    {
        using var scope = Scope.Enter();
        var line = new LinePlot(new float[,] { { 0, 1 }, { 1, 2 } }, markerStyle: MarkerStyle.Circle);
        line.Line.Color = Color.Transparent;
        line.Marker.Fill.Color = Color.FromArgb(128, Color.Gold);
        line.Marker.Fill.Visible = fill;
        line.Marker.Border.Color = Color.Crimson;
        line.Marker.Border.Width = 3;
        line.Marker.Border.Visible = border;
        line.Marker.Size = 17;
        using var scene = new Scene { new PlotCube { line } };
        var globals = new TikzGlobals();
        var snapshot = new TikzPlot();
        snapshot.Bind(line, globals);
        var text = Exporter.ExportString(scene);
        foreach (var output in new[] { snapshot.PreTag, text })
        {
            if (!fill && !border)
            {
                Assert.Contains("mark=none", output);
                continue;
            }
            Assert.Contains("mark size=" + (17 * 72.27 / 192).ToString("R", CultureInfo.InvariantCulture) + "pt", output);
            Assert.Contains("opacity=1,fill=", output);
            Assert.Contains(fill ? "fill opacity=0.5019607843137255" : "fill=none", output);
            Assert.Contains(border ? "draw opacity=1" : "draw opacity=0", output);
            Assert.Contains(border ? "line width=2.2584375pt" : "line width=0pt", output);
        }
    }

    [Fact]
    public void DefaultHollowCircleRetainsVisibleOutline()
    {
        using var scope = Scope.Enter();
        var line = new LinePlot(new float[,] { { 0, 1 }, { 1, 2 } }, markerStyle: MarkerStyle.Circle);
        using var scene = new Scene { new PlotCube { line } };
        var text = Exporter.ExportString(scene);
        Assert.Contains("fill=none", text);
        Assert.Contains("draw opacity=1", text);
        Assert.DoesNotContain("mark size=4pt", text);
        line.Marker.Size = 0;
        Assert.Contains("mark=none", Exporter.ExportString(scene));
    }

    [Theory]
    [InlineData(MarkerStyle.Plus, "iln-plus")]
    [InlineData(MarkerStyle.Cross, "iln-cross")]
    public void FilledCrossMarkersDeclareTheirOutlines(MarkerStyle style, string name)
    {
        using var scope = Scope.Enter();
        using var scene = new Scene { new PlotCube { new LinePlot(new float[,] { { 0, 1 }, { 1, 2 } }, markerStyle: style) } };
        var text = Exporter.ExportString(scene);
        Assert.Contains("\\pgfdeclareplotmark{" + name + "}", text);
        Assert.Contains("mark=" + name, text);
        Assert.Contains(@"\pgfpathclose\pgfusepath{fill,stroke}", text);
    }
}
