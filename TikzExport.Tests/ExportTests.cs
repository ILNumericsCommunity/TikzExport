using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ILNumerics;
using ILNumerics.Community.ExportSamples;
using ILNumerics.Community.TikzExport.Generator;
using ILNumerics.Community.TikzExport.Generator.Elements;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using Xunit;
using Exporter = ILNumerics.Community.TikzExport.TikzExport;
using Group = ILNumerics.Drawing.Group;
using Size = System.Drawing.Size;

public sealed partial class ExportTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(23)]
    [InlineData(24)]
    [InlineData(25)]
    public void SharedCatalogExportsWithoutRendering(int index)
    {
        using var scene = ExportDemoScenes.Create(index);
        var result = Exporter.ExportWithReport(scene);
        Assert.Contains(@"\begin{tikzpicture}", result.Tikz);
        Assert.Contains(@"\addplot", result.Tikz);
        Assert.DoesNotContain("No exportable", result.Tikz);
        var dir = Environment.GetEnvironmentVariable("TIKZEXPORT_FIXTURES");
        if (dir != null)
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, $"plot-{index}.tikz"), result.Tikz);
        }
    }

    [Fact]
    public void LinesKeepZCoordinatesAndSnapshotSurvivesSceneDisposal()
    {
        var scene = ExportDemoScenes.Create(1);
        var picture = Exporter.Bind(scene);
        scene.Dispose();
        using var writer = new StringWriter();
        new TikzWriter(writer).Write(picture);
        Assert.Contains(@"\addplot3", writer.ToString());
        Assert.Contains("(2,4,3)", writer.ToString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ErrorBarsPreserveEveryEndpoint(bool vertical)
    {
        using var scope = Scope.Enter();
        var error = new ErrorBarPlot((Array<float>) new float[,] { { 1, 2 } }, new float[,] { { 4, 6 } }, new float[,] { { 1, 2 } }, new float[,] { { 2, 3 } }, vertical);
        using var scene = new Scene { new PlotCube { error } };
        var text = Exporter.ExportString(scene);
        Array<float> p = error.ErrorBar.Positions.Storage;
        for (var i = 0; i < p.S[1]; i++)
            Assert.Contains(FormattableString.Invariant($"({p.GetValue(0, i):R},{p.GetValue(1, i):R})"), text);
        Assert.Contains("(nan,nan)", text);
    }

    [Fact]
    public void MultipleCubesAndHiddenContent()
    {
        using var scope = Scope.Enter();
        using var scene = new Scene { new PlotCube { new LinePlot(new float[] { 1, 2 }) }, new PlotCube { new LinePlot(new float[] { 3, 4 }), new Label("missing") } };
        var result = Exporter.ExportWithReport(scene);
        Assert.Equal(2, Regex.Matches(result.Tikz, @"\\begin\{axis\}").Count);
        Assert.Contains("yshift=-125mm", result.Tikz);
        Assert.Contains(result.Issues, i => i.PlotCubeIndex == 1 && i.NodeType == nameof(Label));
        scene.Find<Label>().First(l => l.Text == "missing").Visible = false;
        Assert.Empty(Exporter.ExportWithReport(scene).Issues);
    }

    [Fact]
    public void InvalidIndicesPropagateAndEmptyScenesHavePlaceholder()
    {
        using var scope = Scope.Enter();
        using var empty = new Scene();
        Assert.Contains("No exportable", Exporter.ExportString(empty));
        var mesh = new Triangles();
        mesh.Positions.Update(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 0, 0, 0 } });
        using var scene = new Scene { new PlotCube { mesh } };
        mesh.Indices.Update(new[] { 0, 1, 9 });
        Assert.Throws<InvalidOperationException>(() => Exporter.ExportString(scene));
    }

    [Fact]
    public void ImageHasSixIndividuallyColoredCellsAndNonSquareSurfaceHasMeshDimensions()
    {
        using var image = ExportDemoScenes.Create(7);
        var text = Exporter.ExportString(image);
        Assert.Equal(6, Regex.Matches(text, "fill=colorDef").Count);
        Assert.Contains("(-0.5,-0.5)", text);
        Assert.Contains("(2.5,1.5)", text);
        using var surface = ExportDemoScenes.Create(5);
        Assert.Contains("mesh/cols=", Exporter.ExportString(surface));
    }

    [Fact]
    public void WriterRemainsOpenAndNumbersAreCultureIndependent()
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            using var scene = ExportDemoScenes.Create(7);
            using var writer = new StringWriter();
            Exporter.Export(scene, writer);
            writer.Write("still open");
            Assert.Contains("(-0.5,-0.5)", writer.ToString());
        }
        finally
        {
            CultureInfo.CurrentCulture = old;
        }
    }

    [Fact]
    public void TextEscapingPreservesMathAndEscapesReservedCharacters()
    {
        Assert.Equal(@"50\% \& \#1 \$5 \textasciitilde{} \textbackslash{}", TikzTextUtility.EscapeText("50% & #1 $5 ~ \\"));
        Assert.Equal(@"$\alpha+1$", TikzTextUtility.EscapeText(@"$\alpha+1$"));
        Assert.Contains("${}^{2}$", TikzTextUtility.EscapeText("m^2"));
        using var scene = ExportDemoScenes.Create(0);
        scene.First<PlotCube>().Axes.XAxis.Label.Text = "50% & #1 $5 ~ \\";
        var dir = Environment.GetEnvironmentVariable("TIKZEXPORT_FIXTURES");
        if (dir != null)
            File.WriteAllText(Path.Combine(dir, "text.tikz"), Exporter.ExportString(scene));
    }

    [Fact]
    public void CustomTicksUseBracesAndFullPrecision()
    {
        using var scene = ExportDemoScenes.Create(0);
        var axis = new TikzAxis();
        axis.Bind(scene.First<PlotCube>(), new TikzGlobals());
        axis.XTicks = new[] { 0.000123f, 1.234567f };
        var text = String.Join("\n", axis.Content);
        Assert.Contains("xtick={0.000123,1.234567}", text);
        Assert.DoesNotContain("xticks=", text);
    }

    [Fact]
    public void EquivalentColorsAreDeduplicatedAndRemovalKeepsNamesStable()
    {
        var colors = new TikzColors();
        Assert.Equal("black", colors.GetColorName(Color.FromArgb(255, 0, 0, 0)));
        var red = colors.GetColorName(Color.Red);
        Assert.Equal(red, colors.GetColorName(Color.FromArgb(255, 255, 0, 0)));
        var blue = colors.GetColorName(Color.Blue);
        Assert.True(colors.Remove(Color.Red));
        Assert.Equal(blue, colors.GetColorName(Color.Blue));
        Assert.Single(colors);
    }

    [Fact]
    public void LegacyElementsAlsoSnapshotThreeDimensionalAndErrorData()
    {
        var scene = ExportDemoScenes.Create(1);
        var line = new TikzPlot();
        line.Bind(scene.First<LinePlot>(), new TikzGlobals());
        scene.Dispose();
        Assert.Contains(@"\addplot3", line.PreTag);
        Assert.Contains("2 4 3", String.Join("\n", line.Content));
        using var errors = ExportDemoScenes.Create(4);
        var bars = new TikzPlotPlus();
        bars.Bind(errors.First<ErrorBarPlot>(), new TikzGlobals());
        Assert.Contains("(nan,nan)", String.Join("\n", bars.Content));
        using var surface = ExportDemoScenes.Create(5);
        var surf = new TikzPlot3();
        surf.Bind(surface.First<Surface>(), new TikzGlobals());
        Assert.Contains("mesh/cols=", String.Join("\n", surf.Content));
    }

    [Fact]
    public void RebindingDoesNotDuplicateAxesAndEmptyBoundsAreFinite()
    {
        using var scene = ExportDemoScenes.Create(0);
        var picture = Exporter.Bind(scene);
        picture.Bind(scene, new TikzGlobals());
        Assert.Single(picture);
        using var empty = ExportDemoScenes.Create(15);
        var output = Exporter.ExportString(empty);
        Assert.DoesNotContain("Infinity", output);
        Assert.DoesNotContain("NaN", output);
        Assert.Throws<ArgumentOutOfRangeException>(() => Exporter.Bind(scene, new Size(-1, 20)));
    }

    [Fact]
    public void IndexedTriangleStripsKeepTopologyAndSeparateNaNRuns()
    {
        using var scope = Scope.Enter();
        var mesh = new TrianglesStrip();
        mesh.Positions.Update(new[,] { { 0, 1, 0, 1, Single.NaN, 10, 11, 10 }, { 0, 0, 1, 1, Single.NaN, 0, 0, 1 }, { 2, 2, 2, 2, Single.NaN, 2, 2, 2 } });
        mesh.Indices.Update(new[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        using var scene = new Scene { new PlotCube(twoDMode: false) { mesh } };
        var text = Exporter.ExportString(scene).Replace("\r", "");
        Assert.Single(Regex.Matches(text, @"\\addplot3"));
        Assert.Contains("patch type=triangle", text);
        Assert.Contains("(0,1,2) [0]\n(1,0,2) [0]\n(1,1,2) [0]", text);
        Assert.Contains("(10,0,2) [0]\n(11,0,2) [0]\n(10,1,2) [0]", text);
    }

    [Fact]
    public void HiddenAndTransformedGroupsAreNotExported()
    {
        using var scope = Scope.Enter();
        var hidden = new Group { new LinePlot(new float[] { 100, 200 }) };
        hidden.Visible = false;
        var shifted = new Group(translate: new Vector3(10, 20, 0)) { new LinePlot(new float[] { 300, 400 }) };
        using var scene = new Scene { new PlotCube { hidden, shifted } };
        var result = Exporter.ExportWithReport(scene);
        Assert.DoesNotContain(@"\addplot", result.Tikz);
        Assert.Contains(result.Issues, i => i.Message.Contains("Transformed"));
        Assert.Contains("No exportable", result.Tikz);
    }

    [Fact]
    public void LogarithmicCoordinatesAreConvertedBackToDataValues()
    {
        using var scene = ExportDemoScenes.Create(2);
        var text = Exporter.ExportString(scene);
        Assert.Contains("xmode=log", text);
        Assert.Contains("ymode=log", text);
        var coordinates = Regex.Matches(text.Replace("\r", ""), @"^\(([^,]+),([^,)]+)\)$", RegexOptions.Multiline);
        Assert.Equal(3, coordinates.Count);
        var values = coordinates.Select(m => Double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)).ToArray();
        for (var i = 0; i < 3; i++)
            Assert.InRange(Math.Abs(values[i] - (i + 1)), 0, 0.00001);
    }

    [Fact]
    public void MultiCubeFixtureContainsLegendAndIsEmptyReflectsData()
    {
        using var scope = Scope.Enter();
        var line = new LinePlot(new float[] { 1, 2 });
        using var scene = new Scene { new PlotCube { line, new Legend(new[] { new LegendItem(line, 0, "one") }) }, new PlotCube { new LinePlot(new float[] { 3, 4 }) } };
        var result = Exporter.ExportWithReport(scene);
        Assert.Contains(@"\addlegendentry{one}", result.Tikz);
        Assert.False(Exporter.Bind(scene).IsEmpty);
        using var empty = new Scene();
        Assert.True(Exporter.Bind(empty).IsEmpty);
        var dir = Environment.GetEnvironmentVariable("TIKZEXPORT_FIXTURES");
        if (dir != null)
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "multiple.tikz"), result.Tikz);
        }
    }
}
