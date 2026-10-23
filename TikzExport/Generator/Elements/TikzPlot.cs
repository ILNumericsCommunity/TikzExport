using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;
using static ILNumerics.Community.TikzExport.Generator.TikzFormatUtility;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

/// <summary>
/// Represents a 2D line plot in TikZ.
/// </summary>
public class TikzPlot : ITikzElement
{
    /// <summary>Shared color and axis configuration for the current binding.</summary>
    protected TikzGlobals? globals;

    /// <summary>Reserved for derived binders; base binding snapshots data without retaining the source plot.</summary>
    protected LinePlot? linePlot;

    private string[] table = Array.Empty<string>();

    /// <summary>Whether the snapshot uses two-dimensional coordinates.</summary>
    protected bool twoDMode = true;

    private bool lineVisible;

    #region Implementation of ITikzElement

    /// <summary>
    /// Gets the opening TikZ command for the plot.
    /// </summary>
    public virtual string PreTag
    {
        get
        {
            if (globals == null || table.Length == 0)
                return String.Empty;

            var lineStyle = lineVisible ? FormatLine(globals, LineColor, LineStyle, LineWidth) : "only marks";
            var command = twoDMode ? @"\addplot" : @"\addplot3";

            if (MarkerStyle == MarkerStyle.None)
                return $"{command}[{lineStyle},mark=none,unbounded coords=jump]";

            var markerStyle = FormatMarker(globals, MarkerColor, MarkerBorderColor, MarkerBorderWidth, MarkerStyle, MarkerSize);

            return MarkerDefinition(MarkerStyle) + $"{command}[{lineStyle},{markerStyle},unbounded coords=jump]";
        }
    }

    /// <summary>
    /// Gets the data table content for the plot.
    /// </summary>
    public virtual IEnumerable<string> Content => table;

    /// <summary>
    /// Gets the legend entry for the plot.
    /// </summary>
    public virtual string PostTag
    {
        get
        {
            if (String.IsNullOrEmpty(LegendItemText))
                return "";

            return $"\\addlegendentry{{{TikzTextUtility.EscapeText(LegendItemText)}}}";
        }
    }

    /// <summary>
    /// Binds the plot to a line plot node.
    /// </summary>
    /// <param name="node">The node to bind.</param>
    /// <param name="globals">The shared globals.</param>
    public virtual void Bind(Node node, TikzGlobals globals)
    {
        table = Array.Empty<string>();
        LegendItemText = null;
        this.linePlot = null;
        this.globals = globals;

        if (!(node is LinePlot linePlot))
            return;

        if (!linePlot.Visible || ((!linePlot.Line.Visible || linePlot.Line.Width <= 0) && FormatMarker(globals, linePlot.Marker) == "mark=none"))
            return;

        using var scope = Scope.Enter();
        this.linePlot = null;
        twoDMode = linePlot.FirstUp<PlotCube>()?.TwoDMode ?? true;
        lineVisible = linePlot.Line.Visible && linePlot.Line.Width > 0;
        table = FormatDataTable(linePlot).ToArray();
        LegendItemText = null;

        // Line
        LineColor = linePlot.Line.Color ?? Color.Black;
        globals.Colors.Add(LineColor);
        LineStyle = linePlot.Line.DashStyle;
        LineWidth = linePlot.Line.Width;

        // Marker
        MarkerColor = linePlot.Marker.Fill.Visible ? linePlot.Marker.Fill.Color ?? Color.DarkGray : Color.Transparent;
        MarkerBorderColor = linePlot.Marker.Border.Color ?? Color.Black;
        MarkerBorderWidth = linePlot.Marker.Border.Visible ? linePlot.Marker.Border.Width : 0;
        globals.Colors.Add(MarkerBorderColor);
        globals.Colors.Add(MarkerColor);
        MarkerStyle = linePlot.Marker.Visible ? linePlot.Marker.Style : MarkerStyle.None;
        MarkerSize = linePlot.Marker.Size;

        // LegendEntry
        var legend = linePlot.FirstUp<PlotCube>()?.First<Legend>();
        if (legend?.Visible == true)
            LegendItemText = legend.Find<LegendItem>().FirstOrDefault(legendItem => legendItem.ProviderID == linePlot.ID)?.Text;
    }

    #endregion

    #region Helpers

    /// <summary>Snapshots line positions as invariant data coordinates, undoing logarithmic storage scales.</summary>
    private static IEnumerable<string> FormatDataTable(LinePlot linePlot)
    {
        var cube = linePlot.FirstUp<PlotCube>() ?? throw new ArgumentException("Line plot must belong to a plot cube.");
        var modes = cube.ScaleModes;
        yield return cube.TwoDMode ? "table[x=x,y=y,row sep=crcr]{" : "table[x=x,y=y,z=z,row sep=crcr]{";
        yield return cube.TwoDMode ? @"x y\\" : @"x y z\\";
        Array<float> p = linePlot.Positions;

        string N(float n, AxisScale scale)
        {
            var value = scale == AxisScale.Logarithmic ? Math.Pow(10, n) : n;
            return Double.IsFinite(value) ? value.ToString("R", CultureInfo.InvariantCulture) : "nan";
        }

        for (var i = 0; i < p.S[1]; i++)
        {
            var xy = $"{N(p.GetValue(0, i), modes.XAxisScale)} {N(p.GetValue(1, i), modes.YAxisScale)}";
            yield return cube.TwoDMode ? xy + @"\\" : xy + " " + N(p.GetValue(2, i), modes.ZAxisScale) + @"\\";
        }
        yield return "};";
    }

    #endregion

    #region Properties

    #region Line

    /// <summary>
    /// Gets or sets the line color.
    /// </summary>
    public Color LineColor { get; set; }

    /// <summary>
    /// Gets or sets the line style.
    /// </summary>
    public DashStyle LineStyle { get; set; }

    /// <summary>
    /// Gets or sets the line width.
    /// </summary>
    public int LineWidth { get; set; }

    #endregion

    #region Marker

    /// <summary>
    /// Gets or sets the marker color.
    /// </summary>
    public Color MarkerColor { get; set; }

    /// <summary>Gets or sets the marker outline color independently of its fill.</summary>
    public Color MarkerBorderColor { get; set; } = Color.Black;

    /// <summary>Gets or sets the marker outline width in 96-DPI pixels; zero hides it.</summary>
    public int MarkerBorderWidth { get; set; } = 1;

    /// <summary>
    /// Gets or sets the marker style.
    /// </summary>
    public MarkerStyle MarkerStyle { get; set; }

    /// <summary>
    /// Gets or sets the marker diameter in 96-DPI pixels.
    /// </summary>
    public int MarkerSize { get; set; }

    #endregion

    #region Legend

    /// <summary>
    /// Gets or sets the legend text.
    /// </summary>
    public string? LegendItemText { get; set; }

    #endregion

    #endregion
}
