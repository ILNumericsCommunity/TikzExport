using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using ILNumerics.Community.ExportPreparation;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

// Conversion happens while the scene is owned by the caller. Output contains no live IL arrays.
/// <summary>Converts supported scene geometry into an export snapshot without a UI.</summary>
internal sealed class PlotContentBinder(PlotCube cube, ITikzGroupElement target, TikzGlobals globals)
{
    private int count;

    /// <summary>Records an omitted or approximated node for the current plot cube.</summary>
    private void Issue(Node node, string message) => globals.Issues.Add(new ExportIssue(globals.PlotCubeIndex, node.GetType().Name, message));

    /// <summary>Converts the children of a plot container, emitting a placeholder if none produce data.</summary>
    public void BindChildren(Group group)
    {
        using var scope = Scope.Enter();
        foreach (var child in group.Children)
            Bind(child);
        if (count == 0)
        {
            Issue(group, "No supported visible data could be converted.");
            Add(@"\node[draw,fill=white,align=center] at (rel axis cs:0.5,0.5) {No exportable plot data};");
        }
    }

    /// <summary>Appends an immutable markup fragment and records that content was emitted.</summary>
    private void Add(params string[] lines)
    {
        target.Add(new TikzLiteral(lines));
        count++;
    }

    /// <summary>Dispatches visible supported nodes to geometry converters and reports omissions.</summary>
    internal void Bind(Node node)
    {
        if (!node.Visible)
            return;
        if (node is Group transformed && !transformed.Transform.Equals(Matrix4.Identity))
        {
            Issue(node, "Transformed plot groups are not supported and were omitted.");
            return;
        }
        switch (node)
        {
            case SplinePlot spline:
                if (spline.SmoothLine.Visible)
                    Shape(spline.SmoothLine);
                var lineVisible = spline.Line.Visible;
                spline.Line.Visible = false;
                try
                {
                    Line(spline);
                }
                finally
                {
                    spline.Line.Visible = lineVisible;
                }
                break;
            case ErrorBarPlot error:
                Bind(error.LinePlot);
                Bind(error.ErrorBar);
                break;
            case LinePlot line:
                Line(line);
                break;
            case Surface surface:
                Surface(surface);
                break;
            case FastSurface fast:
                Bind(fast.Fill);
                Issue(node, "FastSurface exports prepared triangle geometry; lighting is not reproduced.");
                break;
            case ImageSCPlot image:
                Image(image);
                break;
            case ContourPlot contour:
                foreach (var line in contour.Find<LineStrip>())
                {
                    var visible = true;
                    for (Node? n = line; n != null && n != contour; n = n.Parent)
                        visible &= n.Visible;
                    if (visible)
                        Bind(line);
                }
                Issue(node, "Contour line geometry is exported; filled regions and labels are omitted.");
                break;
            case BarPlot bar:
                if (bar.Fill.Visible)
                    BindTriangles(bar.Fill);
                foreach (var border in HeadlessPlotPreparation.Borders(bar.Border))
                    Shape(border);
                break;
            case BarPlotEx bar:
                if (bar.Fill.Visible)
                    BindTriangles(bar.Fill);
                foreach (var border in HeadlessPlotPreparation.Borders(bar.Border))
                    Shape(border);
                break;
            case BoxPlot or Candlestick or StackedAreaPlot or FillArea:
                Geometry((Group) node);
                Issue(node, "Rendered geometry is exported; custom labels and legend semantics are not retained.");
                break;
            case Triangles or TrianglesStrip or TrianglesFan:
                BindTriangles((Shape) node);
                break;
            case Lines or Points:
                Shape((Shape) node);
                break;
            case Group group when group.GetType() == typeof(Group):
                foreach (var child in group.Children)
                    Bind(child);
                break;
            default:
                Issue(node, "Unsupported content was omitted.");
                break;
        }
    }

    /// <summary>Visits existing primitive geometry belonging to a compound statistical plot.</summary>
    private void Geometry(Group group)
    {
        foreach (var child in group.Children)
        {
            if (child is Shape shape && isempty(shape.Positions.Storage))
                continue;
            Bind(child);
        }
    }

    /// <summary>Formats finite coordinates invariantly without expanding exact single-precision values; missing data becomes nan.</summary>
    private static string Number(double value)
        => Double.IsFinite(value)
            ? value == (float) value ? ((float) value).ToString("R", CultureInfo.InvariantCulture) : value.ToString("R", CultureInfo.InvariantCulture)
            : "nan";

    /// <summary>Converts internal logarithmic plot coordinates back to data coordinates.</summary>
    private static double Unscale(float value, AxisScale scale) => scale == AxisScale.Logarithmic ? Math.Pow(10, value) : value;

    /// <summary>Formats a scaled vertex as a two-dimensional or three-dimensional PGFPlots coordinate.</summary>
    private string Coordinate(float x, float y, float z)
        => cube.TwoDMode
            ? $"({Number(Unscale(x, cube.ScaleModes.XAxisScale))},{Number(Unscale(y, cube.ScaleModes.YAxisScale))})"
            : $"({Number(Unscale(x, cube.ScaleModes.XAxisScale))},{Number(Unscale(y, cube.ScaleModes.YAxisScale))},{Number(Unscale(z, cube.ScaleModes.ZAxisScale))})";

    /// <summary>Formats a scaled vertex as a two-dimensional or three-dimensional PGFPlots coordinate.</summary>
    private string Coordinate(Array<float> p, int i) => Coordinate(p.GetValue(0, i), p.GetValue(1, i), p.GetValue(2, i));

    private string Command => cube.TwoDMode ? @"\addplot" : @"\addplot3";

    /// <summary>Snapshots coordinates and optional legend text into a PGFPlots command.</summary>
    private void Plot(string style, IEnumerable<string> coordinates, string? legend = null)
    {
        var data = coordinates.ToArray();
        if (data.Length == 0)
            return;
        Add($"{Command}[{style},unbounded coords=jump{(legend == null ? ",forget plot" : "")}] coordinates {{", String.Join("\n", data), "};",
            legend == null ? "" : $"\\addlegendentry{{{TikzTextUtility.EscapeText(legend)}}}");
    }

    /// <summary>Formats a registered color and its opacity using culture-independent numeric syntax.</summary>
    private string ColorStyle(Color color) => FormattableString.Invariant($"color={globals.Colors.GetColorName(color)},opacity={color.A / 255.0:R}");

    /// <summary>Snapshots line coordinates, marker appearance, and an available legend entry.</summary>
    private void Line(LinePlot line)
    {
        Array<float> positions = line.Positions;
        if (isempty(positions))
            return;
        var markerStyle = TikzFormatUtility.FormatMarker(globals, line.Marker);
        var markers = markerStyle != "mark=none";
        var lineVisible = line.Line.Visible && line.Line.Width > 0;
        if (!lineVisible && !markers)
            return;
        var style = lineVisible
            ? TikzFormatUtility.FormatLine(globals, line.Line.Color ?? Color.Black, line.Line.DashStyle, line.Line.Width)
            : "only marks";
        style += "," + markerStyle;
        if (markers)
        {
            var definition = TikzFormatUtility.MarkerDefinition(line.Marker.Style);
            if (definition.Length > 0)
                Add(definition);
            if (line.Marker.Style == MarkerStyle.Custom)
                Issue(line, "Custom marker geometry is approximated by a plus.");
        }
        var legend = cube.First<Legend>();
        var name = legend?.Visible == true ? legend.Find<LegendItem>().FirstOrDefault(i => i.ProviderID == line.ID)?.Text : null;
        if (name == null && legend?.Visible == true)
        {
            var index = cube.Find<LinePlot>().ToList().IndexOf(line);
            if (index >= 0)
                name = legend.Find<LegendItem>().ElementAtOrDefault(index)?.Text;
        }

        if (name == null && legend?.Visible == true)
            Issue(line, "Legend entries have not been prepared; the line is exported without a legend entry.");
        Plot(style, Enumerable.Range(0, (int) positions.S[1]).Select(i => Coordinate(positions, i)), name);
    }

    /// <summary>Returns validated vertex indices, synthesizing sequential indices for non-indexed shapes.</summary>
    private static int[] Order(Shape shape, Array<float> positions)
    {
        Array<int> indices = shape.Indices.Storage;
        var order = isempty(indices) ? Enumerable.Range(0, (int) positions.S[1]).ToArray() : indices.ToArray();
        if (order.Any(i => i < 0 || i >= positions.S[1]))
            throw new InvalidOperationException("Plot geometry contains an out-of-range vertex index.");
        return order;
    }

    /// <summary>Snapshots indexed line or point primitives, preserving segment boundaries.</summary>
    private void Shape(Shape shape)
    {
        Array<float> p = shape.Positions.Storage;
        if (isempty(p))
            return;
        var order = Order(shape, p);
        var coordinates = new List<string>();
        for (var i = 0; i < order.Length; i++)
        {
            coordinates.Add(Coordinate(p, order[i]));
            if (shape is Lines && shape is not LineStrip && i % 2 == 1)
                coordinates.Add(cube.TwoDMode ? "(nan,nan)" : "(nan,nan,nan)");
        }

        var style = shape is Lines lines
            ? TikzFormatUtility.FormatLine(globals, shape.Color ?? Color.Black, lines.DashStyle, lines.Width) + ",mark=none"
            : FormattableString.Invariant($"only marks,mark=*,mark size={((Points) shape).Size / 2:R}pt,{ColorStyle(shape.Color ?? Color.Black)}");
        Plot(style, coordinates);
        if (!isempty(shape.Colors.Storage))
            Issue(shape, "Per-vertex colors are approximated by a single plot color.");
    }

    /// <summary>Converts indexed triangle lists, strips, or fans while preserving separator runs and face colors.</summary>
    private void BindTriangles(Shape shape)
    {
        Array<float> p = shape.Positions.Storage;
        if (isempty(p))
            return;
        Array<float> colors = shape.Colors.Storage;
        var run = new List<int>();
        var approximated = false;
        var faces = new List<(int A, int B, int C, Color Color)>();
        foreach (var vertex in Order(shape, p))
        {
            if (Enumerable.Range(0, 3).Any(i => !Single.IsFinite(p.GetValue(i, vertex))))
            {
                EmitRun();
                run.Clear();
            }
            else
                run.Add(vertex);
        }

        EmitRun();

        // One PGFPlots patch mesh per opacity, instead of thousands of addplot commands.
        foreach (var opacityGroup in faces.GroupBy(face => face.Color.A))
        {
            foreach (var batch in opacityGroup.Chunk(128))
            {
                var palette = batch.Select(face => face.Color).Distinct().ToArray();
                var colorIndices = palette.Select((color, index) => (color, index)).ToDictionary(item => item.color, item => item.index);
                var map =
                    String.Join(" ", (palette.Length == 1 ? new[] { palette[0], palette[0] } : palette).Select((color, index) => $"rgb255({index}pt)=({color.R},{color.G},{color.B
                    });"));
                var rows = new List<string>
                {
                    FormattableString.Invariant($"\\addplot3[patch,patch type=triangle,shader=flat,draw=none,z buffer=sort,forget plot,opacity={opacityGroup.Key / 255.0
                        :R},point meta=explicit,colormap access=direct,colormap={{faces}}{{{map}}}] coordinates {{")
                };
                foreach (var face in batch)
                {
                    foreach (var vertex in new[] { face.A, face.B, face.C })
                        rows.Add(Coordinate(p, vertex) + " [" + colorIndices[face.Color] + "]");
                }
                rows.Add("};");
                target.Add(new TikzLiteral(rows));
                count++;
            }
        }

        if (approximated)
            Issue(shape, "Interpolated vertex colors are approximated by one color per triangle.");

        void EmitRun()
        {
            if (shape is TrianglesStrip)
            {
                for (var i = 2; i < run.Count; i++)
                    Emit(run[i % 2 == 0 ? i - 2 : i - 1], run[i % 2 == 0 ? i - 1 : i - 2], run[i]);
            }
            else if (shape is TrianglesFan)
            {
                for (var i = 2; i < run.Count; i++)
                    Emit(run[0], run[i - 1], run[i]);
            }
            else
            {
                for (var i = 2; i < run.Count; i += 3)
                    Emit(run[i - 2], run[i - 1], run[i]);
            }
        }

        Color VertexColor(int i)
        {
            if (shape.Color.HasValue || isempty(colors))
                return shape.Color ?? Color.Gray;
            var col = colors.S[1] == 1 ? 0 : i;
            if (col >= colors.S[1])
                throw new InvalidOperationException("Plot geometry has too few vertex colors.");

            int Channel(int row) => (int) Math.Round(Math.Clamp(colors.GetValue(row, col), 0, 1) * 255);

            return Color.FromArgb(Channel(3), Channel(0), Channel(1), Channel(2));
        }

        void Emit(int a, int b, int c)
        {
            if (a == b || a == c || b == c)
                return;
            var color = VertexColor(a);
            approximated |= color != VertexColor(b) || color != VertexColor(c);
            if (!cube.TwoDMode)
                faces.Add((a, b, c, color));
            else
            {
                Plot("mark=none,draw=none,fill=" + globals.Colors.GetColorName(color) + "," + ColorStyle(color),
                     new[] { Coordinate(p, a), Coordinate(p, b), Coordinate(p, c), Coordinate(p, a) });
            }
        }
    }

    /// <summary>Converts finite scalar-image pixels to colored rectangles on linear two-dimensional axes.</summary>
    private void Image(ImageSCPlot image)
    {
        if (!cube.TwoDMode || cube.ScaleModes.XAxisScale != AxisScale.Linear || cube.ScaleModes.YAxisScale != AxisScale.Linear)
        {
            Issue(image, "Scalar images currently require linear 2D axes.");
            return;
        }

        Array<float> values = image.Values;
        for (var y = 0; y < image.NumberRows; y++)
        {
            for (var x = 0; x < image.NumberColumns; x++)
            {
                var value = values.GetValue(y, x);
                if (!Single.IsFinite(value))
                    continue;
                var v = image.Colormap.Map(value, image.MinMaxDataRange);

                int C(float c) => (int) Math.Round(Math.Clamp(c, 0, 1) * 255);

                var color = Color.FromArgb(C(v.W), C(v.X), C(v.Y), C(v.Z));
                Plot("mark=none,draw=none,fill=" + globals.Colors.GetColorName(color) + "," + ColorStyle(color),
                     new[]
                     {
                         Coordinate(x - .5f, y - .5f, 0), Coordinate(x + .5f, y - .5f, 0), Coordinate(x + .5f, y + .5f, 0), Coordinate(x - .5f, y + .5f, 0),
                         Coordinate(x - .5f, y - .5f, 0)
                     });
            }
        }
    }

    /// <summary>Snapshots surface heights and coordinates into a PGFPlots mesh table.</summary>
    private void Surface(Surface surface)
    {
        Array<float> p = surface.Positions;
        if (isempty(p))
            return;
        var lines = new List<string>
        {
            $"\\addplot3[surf,shader=flat,z buffer=sort,mesh/cols={p.S[1]},forget plot,{TikzColormapUtility.FormatColormap(surface.Colormap)}] table[row sep=crcr] {{",
            @"x y z\\"
        };
        for (var y = 0; y < p.S[0]; y++)
        {
            for (var x = 0; x < p.S[1]; x++)
            {
                lines.Add($"{Number(Unscale(p.GetValue(y, x, 1), cube.ScaleModes.XAxisScale))} {Number(Unscale(p.GetValue(y, x, 2), cube.ScaleModes.YAxisScale))} {
                    Number(Unscale(p.GetValue(y, x, 0), cube.ScaleModes.ZAxisScale))}\\\\");
            }
        }
        lines.Add("};");
        target.Add(new TikzLiteral(lines));
        count++;
    }
}
