using System.Drawing;
using ILNumerics;
using ILNumerics.Community.TikzExport.Generator;
using ILNumerics.Community.TikzExport.Generator.Elements;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using Xunit;

public sealed partial class ExportTests
{
    [Fact]
    public void RebindingALineSnapshotClearsPreviousGeometry()
    {
        using var scope = Scope.Enter();
        using var scene = new Scene { new PlotCube { new LinePlot(new float[,] { { 1, 2 }, { 3, 4 } }) } };
        var plot = new TikzPlot();
        var globals = new TikzGlobals();
        plot.Bind(scene.First<LinePlot>(), globals);
        Assert.NotEmpty(plot.Content);
        plot.Bind(scene, globals);
        Assert.Empty(plot.Content);
        Assert.Empty(plot.PreTag);
        Assert.Empty(plot.PostTag);
    }

    [Theory]
    [InlineData(MarkerStyle.TriangleUp, 0)]
    [InlineData(MarkerStyle.TriangleDown, 180)]
    [InlineData(MarkerStyle.TriangleLeft, 90)]
    [InlineData(MarkerStyle.TriangleRight, 270)]
    public void TriangleMarkersKeepTheirOrientation(MarkerStyle style, int angle)
    {
        var text = TikzFormatUtility.FormatMarker(new TikzGlobals(), Color.Red, style, 3);
        Assert.Contains("mark=triangle*", text);
        Assert.Contains($"rotate={angle}", text);
    }

    [Fact]
    public void LinesAndMarkersPreserveTransparency()
    {
        var globals = new TikzGlobals();
        var color = Color.FromArgb(1, 17, 29, 43);
        Assert.Contains("opacity=0.00392156862745098", TikzFormatUtility.FormatLine(globals, color, DashStyle.Solid, 2));
        Assert.Contains("opacity=0.00392156862745098", TikzFormatUtility.FormatMarker(globals, color, MarkerStyle.Circle, 2));
    }
}
