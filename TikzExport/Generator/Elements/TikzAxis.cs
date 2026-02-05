using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

/// <summary>
/// Represents a PGFPlots axis bound to a plot cube.
/// </summary>
public class TikzAxis : TikzGroupElementBase, ITikzGroupElement
{
    #region TicksAlignEnum enum

    /// <summary>
    /// Defines the alignment of tick marks relative to the axis.
    /// </summary>
    public enum TicksAlignEnum
    {
        Inside,
        Center,
        Outside
    }

    #endregion

    #region TicksModeEnum enum

    /// <summary>
    /// Defines how ticks are generated for an axis.
    /// </summary>
    public enum TicksModeEnum
    {
        None,
        Coordinate,
        Custom,
        Auto
    }

    #endregion

    private TikzGlobals? globals;

    private float[]? xTicks;
    private float[]? yTicks;
    private float[]? zTicks;

    #region Implementation of ITikzGroupElement

    /// <summary>
    /// Gets the opening TikZ axis command.
    /// </summary>
    public override string PreTag => @"\begin{axis}[";

    /// <summary>
    /// Gets the content lines for the axis definition.
    /// </summary>
    public override IEnumerable<string> Content
    {
        get
        {
            if (globals == null)
                yield break;

            #region Global

            yield return $"  width={globals.CanvasSize.Width}mm,";
            yield return $"  height={globals.CanvasSize.Height}mm,";

            if (!String.IsNullOrEmpty(Title))
                yield return $"  title={{{TikzTextUtility.EscapeText(Title)}}},";

            yield return FormattableString.Invariant($"  view={{({ViewAzimuth})}}{{({ViewElevation})}},");

            #endregion

            #region XAxis

            if (!String.IsNullOrEmpty(XLabel))
                yield return $"  xlabel={{{TikzTextUtility.EscapeText(XLabel)}}},";
            switch (XScale)
            {
                case AxisScale.Linear:
                    yield return "  xmode=normal,";
                    break;
                case AxisScale.Logarithmic:
                    yield return "  xmode=log,";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return FormattableString.Invariant($"  xmin={XMin},");
            yield return FormattableString.Invariant($"  xmax={XMax},");
            switch (XTicksMode)
            {
                case TicksModeEnum.None:
                    //yield return @"  xticks=\empty,";
                    break;
                case TicksModeEnum.Coordinate:
                    yield return "  xticks=data,";
                    break;
                case TicksModeEnum.Custom:
                    if (XTicks != null)
                        yield return $"  xticks={String.Join(",", XTicks.Select(x => x.ToString("F", CultureInfo.InvariantCulture)))},";
                    break;
                case TicksModeEnum.Auto:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (XTicksAlign)
            {
                case TicksAlignEnum.Inside:
                    yield return "  xtick align=inside,";
                    break;
                case TicksAlignEnum.Center:
                    yield return "  xtick align=center,";
                    break;
                case TicksAlignEnum.Outside:
                    yield return "  xtick align=outside,";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return $"  xmajorticks={(XMajorTicks ? "true" : "false")},";
            yield return $"  xminorticks={(XMinorTicks ? "true" : "false")},";
            if (XMajorGrid)
                yield return "  xmajorgrids,";
            if (XMinorGrid)
                yield return "  xminorgrids,";

            #endregion

            #region YAxis

            if (!String.IsNullOrEmpty(YLabel))
                yield return $"  ylabel={{{TikzTextUtility.EscapeText(YLabel)}}},";
            switch (YScale)
            {
                case AxisScale.Linear:
                    yield return "  ymode=normal,";
                    break;
                case AxisScale.Logarithmic:
                    yield return "  ymode=log,";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return FormattableString.Invariant($"  ymin={YMin},");
            yield return FormattableString.Invariant($"  ymax={YMax},");
            switch (YTicksMode)
            {
                case TicksModeEnum.None:
                    //yield return @"  yticks=\empty,";
                    break;
                case TicksModeEnum.Coordinate:
                    yield return "  yticks=data,";
                    break;
                case TicksModeEnum.Custom:
                    if (YTicks != null)
                        yield return $"  yticks={String.Join(",", YTicks.Select(x => x.ToString("F", CultureInfo.InvariantCulture)))},";
                    break;
                case TicksModeEnum.Auto:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (YTicksAlign)
            {
                case TicksAlignEnum.Inside:
                    yield return "  ytick align=inside,";
                    break;
                case TicksAlignEnum.Center:
                    yield return "  ytick align=center,";
                    break;
                case TicksAlignEnum.Outside:
                    yield return "  ytick align=outside,";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return $"  ymajorticks={(YMajorTicks ? "true" : "false")},";
            yield return $"  yminorticks={(YMinorTicks ? "true" : "false")},";
            if (YMajorGrid)
                yield return "  ymajorgrids,";
            if (YMinorGrid)
                yield return "  yminorgrids,";

            #endregion

            #region ZAxis

            if (!TwoDMode)
            {
                if (!String.IsNullOrEmpty(ZLabel))
                    yield return $"  zlabel={{{TikzTextUtility.EscapeText(ZLabel)}}},";
                switch (ZScale)
                {
                    case AxisScale.Linear:
                        yield return "  zmode=normal,";
                        break;
                    case AxisScale.Logarithmic:
                        yield return "  zmode=log,";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return FormattableString.Invariant($"  zmin={ZMin},");
                yield return FormattableString.Invariant($"  zmax={ZMax},");
                switch (ZTicksMode)
                {
                    case TicksModeEnum.None:
                        //yield return @"  zticks=\empty,";
                        break;
                    case TicksModeEnum.Coordinate:
                        yield return "  zticks=data,";
                        break;
                    case TicksModeEnum.Custom:
                        if (ZTicks != null)
                            yield return $"  zticks={String.Join(",", ZTicks.Select(x => x.ToString("F", CultureInfo.InvariantCulture)))},";
                        break;
                    case TicksModeEnum.Auto:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (ZTicksAlign)
                {
                    case TicksAlignEnum.Inside:
                        yield return "  ztick align=inside,";
                        break;
                    case TicksAlignEnum.Center:
                        yield return "  ztick align=center,";
                        break;
                    case TicksAlignEnum.Outside:
                        yield return "  ztick align=outside,";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return $"  zmajorticks={(ZMajorTicks ? "true" : "false")},";
                yield return $"  zminorticks={(ZMinorTicks ? "true" : "false")},";
                if (ZMajorGrid)
                    yield return "  zmajorgrids,";
                if (ZMinorGrid)
                    yield return "  zminorgrids,";
            }

            #endregion

            #region Legend

            if (LegendVisible && globals != null)
                yield return FormatTikzLegendStyle;

            #endregion

            yield return "]";

            // Return child elements
            foreach (var element in base.Content)
                yield return element;
        }
    }

    /// <summary>
    /// Gets the closing TikZ axis command.
    /// </summary>
    public override string PostTag => @"\end{axis}";

    /// <summary>
    /// Binds the axis to a plot cube.
    /// </summary>
    /// <param name="group">The plot cube group.</param>
    /// <param name="globals">The shared globals.</param>
    public override void Bind(Group group, TikzGlobals globals)
    {
        this.globals = globals;

        if (!(group is PlotCube plotCube))
            return;

        // TODO: Font: Family, Size, Bold/Italic, Color

        // Global
        var title = group.First<Title>();
        Title = title?.Label?.Text ?? String.Empty;
        TwoDMode = plotCube.TwoDMode;
        if (!TwoDMode)
        {
            // Default orientation for 3D view
            ViewAzimuth = 60;
            ViewElevation = 60;
        }

        // XAxis
        XLabel = plotCube.Axes.XAxis.Label.Text;
        XScale = plotCube.ScaleModes.XAxisScale;
        XMin = plotCube.Axes.XAxis.Min ?? plotCube.Plots.Limits.XMin;
        XMax = plotCube.Axes.XAxis.Max ?? plotCube.Plots.Limits.XMax;
        if (plotCube.ScaleModes.XAxisScale == AxisScale.Logarithmic)
        {
            XMin = (float) Math.Pow(10.0, XMin);
            XMax = (float) Math.Pow(10.0, XMax);
        }
        XTicksAlign = TickLengthToTicksAlignEnum(plotCube.Axes.XAxis.Ticks.TickLength);
        XMajorTicks = plotCube.Axes.XAxis.Ticks.Visible;
        XMinorTicks = false;
        XMajorGrid = plotCube.Axes.XAxis.GridMajor.Visible;
        XMinorGrid = plotCube.Axes.XAxis.GridMinor.Visible;

        // YAxis
        YLabel = plotCube.Axes.YAxis.Label.Text;
        YScale = plotCube.ScaleModes.YAxisScale;
        YMin = plotCube.Axes.YAxis.Min ?? plotCube.Plots.Limits.YMin;
        YMax = plotCube.Axes.YAxis.Max ?? plotCube.Plots.Limits.YMax;
        if (plotCube.ScaleModes.YAxisScale == AxisScale.Logarithmic)
        {
            YMin = (float) Math.Pow(10.0, YMin);
            YMax = (float) Math.Pow(10.0, YMax);
        }
        YTicksAlign = TickLengthToTicksAlignEnum(plotCube.Axes.YAxis.Ticks.TickLength);
        YMajorTicks = plotCube.Axes.YAxis.Ticks.Visible;
        YMinorTicks = false;
        YMajorGrid = plotCube.Axes.YAxis.GridMajor.Visible;
        YMinorGrid = plotCube.Axes.YAxis.GridMinor.Visible;

        // ZAxis
        ZLabel = plotCube.Axes.ZAxis.Label.Text;
        ZScale = plotCube.ScaleModes.ZAxisScale;
        ZMin = plotCube.Axes.ZAxis.Min ?? plotCube.Plots.Limits.ZMin;
        ZMax = plotCube.Axes.ZAxis.Max ?? plotCube.Plots.Limits.ZMax;
        if (plotCube.ScaleModes.ZAxisScale == AxisScale.Logarithmic)
        {
            ZMin = (float) Math.Pow(10.0, ZMin);
            ZMax = (float) Math.Pow(10.0, ZMax);
        }
        ZTicksAlign = TickLengthToTicksAlignEnum(plotCube.Axes.ZAxis.Ticks.TickLength);
        ZMajorTicks = plotCube.Axes.ZAxis.Ticks.Visible;
        ZMinorTicks = false;
        ZMajorGrid = plotCube.Axes.ZAxis.GridMajor.Visible;
        ZMinorGrid = plotCube.Axes.ZAxis.GridMinor.Visible;

        // Major Grid (NOTE: Tikz doesn't support per-axis grid styles, use X axis as template)
        MajorGridColor = plotCube.Axes.XAxis.GridMajor.Color ?? Color.FromArgb(230, 230, 230);
        globals.Colors.Add(MajorGridColor);
        MajorGridStyle = plotCube.Axes.XAxis.GridMajor.DashStyle;
        MajorGridWidth = plotCube.Axes.XAxis.GridMajor.Width;

        // Push style to PGFPlotOptions (NOTE: grid style is set globally)
        globals.PGFPlotOptions.SetMajorGridStyle(MajorGridColor, MajorGridStyle, MajorGridWidth);

        // Minor Grid (NOTE: Tikz doesn't support per-axis grid styles, use X axis as template)
        MinorGridColor = plotCube.Axes.XAxis.GridMinor.Color ?? Color.FromArgb(230, 230, 230);
        globals.Colors.Add(MinorGridColor);
        MinorGridStyle = plotCube.Axes.XAxis.GridMinor.DashStyle;
        MinorGridWidth = plotCube.Axes.XAxis.GridMinor.Width;

        // Push style to PGFPlotOptions (NOTE: grid style is set globally)
        globals.PGFPlotOptions.SetMinorGridStyle(MinorGridColor, MinorGridStyle, MinorGridWidth);

        // Legend
        var legend = plotCube.First<Legend>();
        if (legend != null)
        {
            LegendVisible = legend.Visible;
            LegendLocation = legend.Location;
            LegendBorderColor = legend.Border.Color ?? Color.Black;
            globals.Colors.Add(LegendBorderColor);
            LegendBackgroundColor = legend.Background.Color ?? Color.White;
            globals.Colors.Add(LegendBackgroundColor);
        }

        // Map plots (LinePlot, Surface, etc.)
        this.BindPlots(plotCube, globals);
    }

    #endregion

    #region Properties

    #region Global

    /// <summary>
    /// Gets or sets the axis title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the azimuth view angle.
    /// </summary>
    public float ViewAzimuth { get; set; }

    /// <summary>
    /// Gets or sets the elevation view angle.
    /// </summary>
    public float ViewElevation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plot is 2D.
    /// </summary>
    public bool TwoDMode { get; set; }

    /// <summary>
    /// Sets the tick alignment for all axes.
    /// </summary>
    public TicksAlignEnum TicksAlign
    {
        set => XTicksAlign = YTicksAlign = ZTicksAlign = value;
    }

    /// <summary>
    /// Enables or disables major ticks for all axes.
    /// </summary>
    public bool MajorTicks
    {
        set => XMajorTicks = YMajorTicks = ZMajorTicks = value;
    }

    /// <summary>
    /// Enables or disables minor ticks for all axes.
    /// </summary>
    public bool MinorTicks
    {
        set => XMinorTicks = YMinorTicks = ZMinorTicks = value;
    }

    /// <summary>
    /// Enables or disables major and minor grids for all axes.
    /// </summary>
    public bool Grid
    {
        set
        {
            XMajorGrid = YMajorGrid = ZMajorGrid = value;
            XMinorGrid = YMinorGrid = ZMinorGrid = value;
        }
    }

    /// <summary>
    /// Enables or disables major grids for all axes.
    /// </summary>
    public bool MajorGrid
    {
        set => XMajorGrid = YMajorGrid = ZMajorGrid = value;
    }

    /// <summary>
    /// Enables or disables minor grids for all axes.
    /// </summary>
    public bool MinorGrid
    {
        set => XMinorGrid = YMinorGrid = ZMinorGrid = value;
    }

    #endregion

    #region XAxis

    /// <summary>
    /// Gets or sets the X-axis label.
    /// </summary>
    public string? XLabel { get; set; }

    /// <summary>
    /// Gets or sets the X-axis scale type.
    /// </summary>
    public AxisScale XScale { get; set; }

    /// <summary>
    /// Gets or sets the X-axis minimum value.
    /// </summary>
    public float XMin { get; set; }

    /// <summary>
    /// Gets or sets the X-axis maximum value.
    /// </summary>
    public float XMax { get; set; }

    /// <summary>
    /// Gets or sets the X-axis tick generation mode.
    /// </summary>
    public TicksModeEnum XTicksMode { get; set; }

    /// <summary>
    /// Gets or sets custom X-axis tick positions.
    /// </summary>
    public float[]? XTicks
    {
        get => xTicks;
        set
        {
            xTicks = value;
            XTicksMode = TicksModeEnum.Custom;
        }
    }

    /// <summary>
    /// Gets or sets the X-axis tick alignment.
    /// </summary>
    public TicksAlignEnum XTicksAlign { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether major X-axis ticks are visible.
    /// </summary>
    public bool XMajorTicks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether minor X-axis ticks are visible.
    /// </summary>
    public bool XMinorTicks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether major X-axis grid lines are visible.
    /// </summary>
    public bool XMajorGrid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether minor X-axis grid lines are visible.
    /// </summary>
    public bool XMinorGrid { get; set; }

    #endregion

    #region YAxis

    /// <summary>
    /// Gets or sets the Y-axis label.
    /// </summary>
    public string? YLabel { get; set; }

    /// <summary>
    /// Gets or sets the Y-axis scale type.
    /// </summary>
    public AxisScale YScale { get; set; }

    /// <summary>
    /// Gets or sets the Y-axis minimum value.
    /// </summary>
    public float YMin { get; set; }

    /// <summary>
    /// Gets or sets the Y-axis maximum value.
    /// </summary>
    public float YMax { get; set; }

    /// <summary>
    /// Gets or sets the Y-axis tick generation mode.
    /// </summary>
    public TicksModeEnum YTicksMode { get; set; }

    /// <summary>
    /// Gets or sets custom Y-axis tick positions.
    /// </summary>
    public float[]? YTicks
    {
        get => yTicks;
        set
        {
            yTicks = value;
            YTicksMode = TicksModeEnum.Custom;
        }
    }

    /// <summary>
    /// Gets or sets the Y-axis tick alignment.
    /// </summary>
    public TicksAlignEnum YTicksAlign { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether major Y-axis ticks are visible.
    /// </summary>
    public bool YMajorTicks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether minor Y-axis ticks are visible.
    /// </summary>
    public bool YMinorTicks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether major Y-axis grid lines are visible.
    /// </summary>
    public bool YMajorGrid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether minor Y-axis grid lines are visible.
    /// </summary>
    public bool YMinorGrid { get; set; }

    #endregion

    #region ZAxis

    /// <summary>
    /// Gets or sets the Z-axis label.
    /// </summary>
    public string? ZLabel { get; set; }

    /// <summary>
    /// Gets or sets the Z-axis scale type.
    /// </summary>
    public AxisScale ZScale { get; set; }

    /// <summary>
    /// Gets or sets the Z-axis minimum value.
    /// </summary>
    public float ZMin { get; set; }

    /// <summary>
    /// Gets or sets the Z-axis maximum value.
    /// </summary>
    public float ZMax { get; set; }

    /// <summary>
    /// Gets or sets the Z-axis tick generation mode.
    /// </summary>
    public TicksModeEnum ZTicksMode { get; set; }

    /// <summary>
    /// Gets or sets custom Z-axis tick positions.
    /// </summary>
    public float[]? ZTicks
    {
        get => zTicks;
        set
        {
            zTicks = value;
            ZTicksMode = TicksModeEnum.Custom;
        }
    }

    /// <summary>
    /// Gets or sets the Z-axis tick alignment.
    /// </summary>
    public TicksAlignEnum ZTicksAlign { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether major Z-axis ticks are visible.
    /// </summary>
    public bool ZMajorTicks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether minor Z-axis ticks are visible.
    /// </summary>
    public bool ZMinorTicks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether major Z-axis grid lines are visible.
    /// </summary>
    public bool ZMajorGrid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether minor Z-axis grid lines are visible.
    /// </summary>
    public bool ZMinorGrid { get; set; }

    #endregion

    #region MajorGrid

    /// <summary>
    /// Gets or sets the major grid line color.
    /// </summary>
    public Color MajorGridColor { get; set; }

    /// <summary>
    /// Gets or sets the major grid line style.
    /// </summary>
    public DashStyle MajorGridStyle { get; set; }

    /// <summary>
    /// Gets or sets the major grid line width.
    /// </summary>
    public int MajorGridWidth { get; set; }

    #endregion

    #region MinorGrid

    /// <summary>
    /// Gets or sets the minor grid line color.
    /// </summary>
    public Color MinorGridColor { get; set; }

    /// <summary>
    /// Gets or sets the minor grid line style.
    /// </summary>
    public DashStyle MinorGridStyle { get; set; }

    /// <summary>
    /// Gets or sets the minor grid line width.
    /// </summary>
    public int MinorGridWidth { get; set; }

    #endregion

    #region Legend

    /// <summary>
    /// Gets or sets a value indicating whether the legend is visible.
    /// </summary>
    public bool LegendVisible { get; set; }

    /// <summary>
    /// Gets or sets the legend location.
    /// </summary>
    public PointF LegendLocation { get; set; }

    /// <summary>
    /// Gets or sets the legend border color.
    /// </summary>
    public Color LegendBorderColor { get; set; }

    /// <summary>
    /// Gets or sets the legend background color.
    /// </summary>
    public Color LegendBackgroundColor { get; set; }

    #endregion

    #endregion

    #region Helpers

    #region Legend

    private string FormatTikzLegendStyle
        => FormattableString.Invariant($"  legend style={{legend cell align=left,align=left,{FormatTikzLegendColors},{FormatTikzLegendLocation}}},");

    private string FormatTikzLegendColors
        => globals == null
            ? String.Empty
            : FormattableString.Invariant($"fill={globals.Colors.GetColorName(LegendBackgroundColor)},draw={globals.Colors.GetColorName(LegendBorderColor)}");

    private string FormatTikzLegendLocation => globals == null ? String.Empty : FormattableString.Invariant($"at={{({LegendLocation.X},{1f - LegendLocation.Y})}}");

    #endregion

    private TicksAlignEnum TickLengthToTicksAlignEnum(float tickLength)
    {
        if (tickLength < 0)
            return TicksAlignEnum.Inside;

        if (tickLength > 0)
            return TicksAlignEnum.Outside;

        return TicksAlignEnum.Center;
    }

    #endregion
}
