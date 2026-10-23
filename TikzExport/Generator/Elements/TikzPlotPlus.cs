using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;
using static ILNumerics.Community.TikzExport.Generator.TikzFormatUtility;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

/// <summary>
/// Represents a line plot with error bars.
/// </summary>
public class TikzPlotPlus : TikzPlot
{
    private string[] errorCoordinates = Array.Empty<string>();

    /// <inheritdoc />
    public override string PreTag => base.PreTag;

    /// <inheritdoc />
    public override string PostTag => "";

    /// <inheritdoc />
    public override IEnumerable<string> Content
    {
        get
        {
            foreach (var entry in base.Content)
                yield return entry;
            yield return base.PostTag;
            if (globals == null || errorCoordinates.Length == 0)
                yield break;
            var command = twoDMode ? @"\addplot" : @"\addplot3";
            yield return $"{command}[{FormatLine(globals, ErrorBarColor, ErrorBarStyle, ErrorBarWidth)},mark=none,forget plot,unbounded coords=jump] coordinates {{";
            foreach (var entry in errorCoordinates)
                yield return entry;
            yield return "};";
        }
    }

    /// <inheritdoc />
    public override void Bind(Node node, TikzGlobals globals)
    {
        errorCoordinates = Array.Empty<string>();
        if (node is not ErrorBarPlot error)
        {
            base.Bind(node, globals);
            return;
        }
        using var scope = Scope.Enter();
        base.Bind(error.LinePlot, globals);
        ErrorBarColor = error.ErrorBar.Color ?? Color.Black;
        globals.Colors.Add(ErrorBarColor);
        ErrorBarStyle = error.ErrorBar.DashStyle;
        ErrorBarWidth = error.ErrorBar.Width;
        if (!error.ErrorBar.Visible)
            return;
        var modes = error.FirstUp<PlotCube>().ScaleModes;
        Array<float> p = error.ErrorBar.Positions.Storage;
        var result = new List<string>();

        string N(float n, AxisScale scale) => Double.IsFinite(n) ? (scale == AxisScale.Logarithmic ? Math.Pow(10, n) : n).ToString("R", CultureInfo.InvariantCulture) : "nan";

        for (var i = 0; i < p.S[1]; i++)
        {
            var xy = $"{N(p.GetValue(0, i), modes.XAxisScale)},{N(p.GetValue(1, i), modes.YAxisScale)}";
            result.Add(twoDMode ? $"({xy})" : $"({xy},{N(p.GetValue(2, i), modes.ZAxisScale)})");
            if (i % 2 == 1)
                result.Add(twoDMode ? "(nan,nan)" : "(nan,nan,nan)");
        }
        errorCoordinates = result.ToArray();
    }

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
