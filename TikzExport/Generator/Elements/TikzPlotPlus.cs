using System;
using System.Collections.Generic;
using System.Drawing;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;
using static ILNumerics.Community.TikzExport.Generator.TikzFormatUtility;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

/// <summary>
/// Represents a line plot with error bars.
/// </summary>
public class TikzPlotPlus : TikzPlot
{
    private ErrorBarPlot? errorBarPlot;

    /// <summary>
    /// Gets the data table content for the error bar plot.
    /// </summary>
    public override IEnumerable<string> Content
    {
        get
        {
            if (linePlot != null && errorBarPlot != null)
            {
                foreach (var tableEntry in FormatErrorDataTable(errorBarPlot))
                    yield return tableEntry;
            }
        }
    }

    /// <summary>
    /// Gets the opening TikZ command for the plot with error bars.
    /// </summary>
    public override string PreTag
    {
        get
        {
            if (globals == null)
                return String.Empty;

            var lineStyle = FormatLine(globals, LineColor, LineStyle, LineWidth);
            var errorBars = FormatErrorBars(globals, ErrorBarColor, ErrorBarStyle, ErrorBarWidth);

            if (MarkerStyle == MarkerStyle.None)
                return $"\\addplot+[{lineStyle},{errorBars}]";

            var markerStyle = FormatMarker(globals, MarkerColor, MarkerStyle, MarkerSize);

            return $"\\addplot+[{lineStyle},{markerStyle},{errorBars}]";
        }
    }

    /// <summary>
    /// Binds the plot to an error bar plot node.
    /// </summary>
    /// <param name="node">The node to bind.</param>
    /// <param name="globals">The shared globals.</param>
    public override void Bind(Node node, TikzGlobals globals)
    {
        this.globals = globals;

        if (!(node is ErrorBarPlot errorBarPlot))
            return;

        // ErrorBar
        ErrorBarColor = errorBarPlot.ErrorBar.Color ?? Color.Black;
        globals.Colors.Add(ErrorBarColor);
        ErrorBarStyle = errorBarPlot.ErrorBar.DashStyle;
        ErrorBarWidth = errorBarPlot.ErrorBar.Width;

        this.errorBarPlot = errorBarPlot; // Reference for data table

        // Delegate LinePlot to TikzPlot
        base.Bind(errorBarPlot.LinePlot, globals);
    }

    #region Helpers

    private static IEnumerable<string> FormatErrorDataTable(ErrorBarPlot errorBarPlot)
    {
        var scaleModes = errorBarPlot.FirstUp<PlotCubeDataGroup>().ScaleModes;

        yield return "  table[x=x, y=y, y error=ye, row sep=crcr]{";
        yield return "  x	y  ye\\\\"; // Header

        Array<float> linePlotPositions = errorBarPlot.LinePlot.Positions; // 3 x n
        Array<float> errorBarPositions = errorBarPlot.ErrorBar.Positions.Storage; // 3 x (3 * 2 * n)
        for (var i = 0; i < linePlotPositions.S[1]; i++)
        {
            // Line plot
            var x = linePlotPositions.GetValue(0, i);
            if (scaleModes.XAxisScale == AxisScale.Logarithmic)
                x = (float) Math.Pow(10.0, x);
            var y = linePlotPositions.GetValue(1, i);
            if (scaleModes.YAxisScale == AxisScale.Logarithmic)
                y = (float) Math.Pow(10.0, y);

            // Error bars
            var yeLower = 0f;
            var yeUpper = 0f;
            for (var j = 0; j < 3; j++) // Pairs of position = one line
            {
                if (j == 0) // Vertical line
                    continue;
                if (j == 1) // Lower error bar
                    yeLower = errorBarPositions.GetValue(1L, (6 * i) + (2 * j));
                if (j == 2) // Upper error bar
                    yeUpper = errorBarPositions.GetValue(1L, (6 * i) + (2 * j));
            }

            if (scaleModes.YAxisScale == AxisScale.Logarithmic)
                yeLower = (float) Math.Pow(10.0, yeLower);
            if (scaleModes.YAxisScale == AxisScale.Logarithmic)
                yeUpper = (float) Math.Pow(10.0, yeUpper);

            // Note: TIKZ does not support different upper/lower errors (-> use largest error margin in export)
            var ye = Math.Max(Math.Abs(yeLower - y), Math.Abs(yeUpper - y));

            yield return FormattableString.Invariant($"  {x}	{y}  {ye}\\\\");
        }

        yield return "};";
    }

    #endregion

    #region Properties

    #region ErrorBar

    /// <summary>
    /// Gets or sets the error bar color.
    /// </summary>
    public Color ErrorBarColor { get; set; }

    /// <summary>
    /// Gets or sets the error bar line style.
    /// </summary>
    public DashStyle ErrorBarStyle { get; set; }

    /// <summary>
    /// Gets or sets the error bar line width.
    /// </summary>
    public int ErrorBarWidth { get; set; }

    #endregion

    #endregion
}
