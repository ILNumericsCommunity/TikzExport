using System;
using System.Collections.Generic;
using System.Drawing;
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
    protected TikzGlobals? globals;
    protected LinePlot? linePlot;

    #region Implementation of ITikzElement

    /// <summary>
    /// Gets the opening TikZ command for the plot.
    /// </summary>
    public virtual string PreTag
    {
        get
        {
            if (globals == null)
                return String.Empty;

            var lineStyle = FormatLine(globals, LineColor, LineStyle, LineWidth);

            if (MarkerStyle == MarkerStyle.None)
                return $"\\addplot[{lineStyle}]";

            var markerStyle = FormatMarker(globals, MarkerColor, MarkerStyle, MarkerSize);

            return $"\\addplot[{lineStyle},{markerStyle}]";
        }
    }

    /// <summary>
    /// Gets the data table content for the plot.
    /// </summary>
    public virtual IEnumerable<string> Content
    {
        get
        {
            if (linePlot != null)
            {
                foreach (var tableEntry in FormatDataTable(linePlot))
                    yield return tableEntry;
            }
        }
    }

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
        this.globals = globals;

        if (!(node is LinePlot linePlot))
            return;

        this.linePlot = linePlot; // Reference for data table

        // Line
        LineColor = linePlot.Line.Color ?? Color.Black;
        globals.Colors.Add(LineColor);
        LineStyle = linePlot.Line.DashStyle;
        LineWidth = linePlot.Line.Width;

        // Marker
        MarkerColor = linePlot.Marker.Fill.Color ?? LineColor;
        globals.Colors.Add(MarkerColor);
        MarkerStyle = linePlot.Marker.Style;
        MarkerSize = Math.Max(linePlot.Marker.Size / 2, 1);

        // LegendEntry
        var legend = linePlot.FirstUp<PlotCube>().First<Legend>();
        if (legend != null)
            LegendItemText = legend.Find<LegendItem>().FirstOrDefault(legendItem => legendItem.ProviderID == linePlot.ID)?.Text;
    }

    #endregion

    #region Helpers

    private static IEnumerable<string> FormatDataTable(LinePlot linePlot)
    {
        var scaleModes = linePlot.FirstUp<PlotCubeDataGroup>().ScaleModes;

        yield return "  table[x=x, y=y, row sep=crcr]{";
        yield return "  x	y\\\\"; // Header

        Array<float> positions = linePlot.Positions; // 3 x n
        for (var i = 0; i < positions.S[1]; i++)
        {
            var x = positions.GetValue(0, i);
            if (scaleModes.XAxisScale == AxisScale.Logarithmic)
                x = (float) Math.Pow(10.0, x);
            var y = positions.GetValue(1, i);
            if (scaleModes.YAxisScale == AxisScale.Logarithmic)
                y = (float) Math.Pow(10.0, y);

            yield return FormattableString.Invariant($"  {x}	{y}\\\\");
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

    /// <summary>
    /// Gets or sets the marker style.
    /// </summary>
    public MarkerStyle MarkerStyle { get; set; }

    /// <summary>
    /// Gets or sets the marker size.
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
