using System;
using System.Drawing;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;

namespace ILNumerics.Community.ExportSamples;

// Keep this source byte-identical in WebExport and TikzExport. See Shared/README.md.
/// <summary>Creates deterministic standard ILNumerics scenes shared by both exporter demos and headless tests.</summary>
public static class ExportDemoScenes
{
    /// <summary>Gets the display names in the same order accepted by <see cref="Create" />.</summary>
    public static readonly string[] Names =
    {
        "Line 2D", "Line 3D", "Logarithmic line", "Vertical errors", "Horizontal errors", "Surface", "Fast surface", "Image", "Contour", "Box",
        "Candlestick", "Stacked area", "Indexed triangles", "Points", "Spline", "Bars", "Grouped bars", "Stacked horizontal bars", "Detached 3D bars", "Histogram bars",
        "Colormapped bars", "Spline 1D", "Spline 3D"
    };

    /// <summary>Creates a fresh, caller-owned scene without opening a window.</summary>
    /// <param name="index">Zero-based index in <see cref="Names" />.</param>
    /// <returns>A scene that the caller must dispose after use.</returns>
    public static Scene Create(int index)
    {
        using var scope = Scope.Enter();
        var cube = new PlotCube(twoDMode: index != 1 && index != 5 && index != 6 && index != 12 && index != 18 && index != 20 && index != 22);
        Array<float> values = new float[,] { { 1, 2, 3 }, { 4, 5, 6 } };
        switch (index)
        {
            case 0:
            case 1:
            case 2:
                var line = new LinePlot(new float[,] { { 1, 2, 3 }, { 2, 4, 3 }, { 1, 3, 2 } }, markerStyle: MarkerStyle.Circle);
                cube.Add(line);
                if (index == 2)
                {
                    cube.ScaleModes.XAxisScale = AxisScale.Logarithmic;
                    cube.ScaleModes.YAxisScale = AxisScale.Logarithmic;
                }
                cube.Add(new Legend(new[] { new LegendItem(line, 0, "Signal") }));
                break;
            case 3:
            case 4:
                cube.Add(new ErrorBarPlot((Array<float>) new float[,] { { 1, 2 } }, new float[,] { { 4, 6 } }, new float[,] { { 1, 2 } }, new float[,] { { 2, 3 } }, index == 3));
                break;
            case 5:
                cube.Add(new Surface((x, y) => x + y, 0, 2, 4, 0, 3, 3));
                break;
            case 6:
                var fast = new FastSurface();
                fast.Update(Z: values);
                cube.Add(fast);
                break;
            case 7:
                cube.Add(new ImageSCPlot(values));
                break;
            case 8:
                cube.Add(new ContourPlot(values));
                break;
            case 9:
                cube.Add(new BoxPlot((Array<float>) new float[,] { { 1 } }, new float[,] { { 1 }, { 3 }, { 5 }, { 7 }, { 9 } }));
                break;
            case 10:
                cube.Add(new Candlestick((Array<float>) new float[,] { { 1, 2 }, { 2, 3 }, { 8, 9 }, { 4, 7 }, { 6, 5 } }));
                break;
            case 11:
                cube.Add(new StackedAreaPlot((Array<float>) new float[,] { { 1, 2, 3 } }, new float[,] { { 1, 2, 3 }, { 2, 3, 4 } }));
                break;
            case 12:
                var mesh = new TrianglesStrip();
                mesh.Positions.Update(new float[,] { { 0, 1, 0, 1 }, { 0, 0, 1, 1 }, { 0, 1, 2, 3 } });
                mesh.Indices.Update(new[] { 0, 1, 2, 3 });
                cube.Add(mesh);
                break;
            case 13:
                var points = new Points();
                points.Positions.Update(new float[,] { { 0, 1, 2 }, { 1, 3, 2 }, { 0, 0, 0 } });
                cube.Add(points);
                break;
            case 14:
                cube.Add(new SplinePlot(new float[,] { { 1, 2, 3, 4 }, { 4, 2, 6, 3 }, { 0, 0, 0, 0 } }));
                break;
            case 15:
                cube.Add(new BarPlot((Array<float>) new float[,] { { 1, 2 } }));
                break;
            case 16:
            case 17:
            case 18:
            case 19:
                var bars = new BarPlot((Array<float>) new float[,] { { 2, 3 }, { 4, -1 }, { 1, 5 } },
                                       style: index == 17 ? BarPlotStyle.stacked :
                                       index == 18 ? BarPlotStyle.detached :
                                       index == 19 ? BarPlotStyle.hist : BarPlotStyle.grouped,
                                       drawHorizontal: index == 17, draw3DBars: index == 18, baseValue: 0.5f);
                bars.Padding = 0.35f;
                bars.SetBarColor(1, fillColor: Color.Crimson);
                cube.Add(bars);
                break;
            case 20:
                cube.Add(new BarPlotEx((Array<float>) new float[,] { { 1, 3 }, { 2, 4 } }));
                break;
            case 21:
                cube.Add(new SplinePlot((Array<float>) new float[,] { { 0, 1, 4, 9 } }, resolution: 16, useSplinePathFor1D: false, markerStyle: MarkerStyle.Circle));
                break;
            case 22:
                cube.Add(new SplinePlot((Array<float>) new float[,] { { 0, 1, 2, 3 }, { 0, 1, 0, 1 }, { 1, 0, 2, 3 } }, resolution: 16, markerStyle: MarkerStyle.Circle));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(index));
        }
        cube.Axes.XAxis.Label.Text = "x";
        cube.Axes.YAxis.Label.Text = "y";
        return new Scene { cube };
    }
}
