using System;
using System.Drawing;
using System.Text;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Provides formatting helpers for TikZ line, marker, and error bar styles.
/// </summary>
public static class TikzFormatUtility
{
    #region ErrorBar

    /// <summary>Formats explicit vertical error-bar styling.</summary>
    internal static string FormatErrorBars(TikzGlobals globals, Color color, DashStyle style, int width)
    {
        var errorBarDef = "error bars/.cd, y fixed,y dir=both, y explicit";
        var errorBarStyle = FormatLine(globals, color, style, width);

        return $"{errorBarDef},error bar style={{{errorBarStyle}}}";
    }

    #endregion

    #region Line

    /// <summary>Formats stroke color, opacity, dash pattern, and width for PGFPlots.</summary>
    internal static string FormatLine(TikzGlobals globals, Color color, DashStyle style, float width)
        => FormattableString.Invariant($"color={globals.Colors.GetColorName(color)},opacity={color.A / 255.0:R},{FormatDashStyle(style)},line width={width:F1}pt");

    /// <summary>Formats stroke color, opacity, dash pattern, and width for PGFPlots.</summary>
    internal static string FormatLine(TikzGlobals globals, Color color, DashStyle style, int width)
        => FormattableString.Invariant($"color={globals.Colors.GetColorName(color)},opacity={color.A / 255.0:R},{FormatDashStyle(style)},line width={width}pt");

    /// <summary>Maps the native dash pattern to a PGFPlots stroke option.</summary>
    internal static string FormatDashStyle(DashStyle style)
    {
        switch (style)
        {
            case DashStyle.Solid:
                return "solid";
            case DashStyle.Dashed:
                return "dashed";
            case DashStyle.PointDash:
                return "dash dot";
            case DashStyle.Dotted:
                return "dotted";
            default:
                return "densely dotted"; // Fallback style
        }
    }

    #endregion

    #region Marker

    /// <summary>Formats a uniformly colored marker using a pixel diameter.</summary>
    internal static string FormatMarker(TikzGlobals globals, Color color, MarkerStyle style, int size) => FormatMarker(globals, color, color, 1, style, size);

    /// <summary>Snapshots independent marker fill and outline appearance.</summary>
    internal static string FormatMarker(TikzGlobals globals, Marker marker)
        => FormatMarker(globals, marker.Fill.Visible ? marker.Fill.Color ?? Color.DarkGray : Color.Transparent,
                        marker.Border.Color ?? Color.Black, marker.Border.Visible ? marker.Border.Width : 0,
                        marker.Visible ? marker.Style : MarkerStyle.None, marker.Size);

    /// <summary>Formats marker diameter and outline width from 96-DPI pixels to TeX points.</summary>
    internal static string FormatMarker(TikzGlobals globals, Color fill, Color border, int width, MarkerStyle style, int diameter)
    {
        if (style == MarkerStyle.None || diameter <= 0 || (fill.A == 0 && (border.A == 0 || width <= 0)))
            return "mark=none";

        var rotation = style switch
        {
            MarkerStyle.TriangleDown => 180,
            MarkerStyle.TriangleLeft => 90,
            MarkerStyle.TriangleRight => 270,
            _ => 0
        };
        var fillName = fill.A == 0 ? "none" : globals.Colors.GetColorName(fill);
        var borderName = border.A == 0 || width <= 0 ? "none" : globals.Colors.GetColorName(border);
        var scale = style == MarkerStyle.Rectangle ? ",yscale=0.6" : "";

        // Reset inherited line opacity and dash settings before applying independent marker styles.
        return FormattableString.Invariant($"mark={FormatMarkerStyle(style)},mark size={diameter * 72.27 / 192:R}pt,mark options={{solid,opacity=1,fill={fillName},draw={borderName
        },fill opacity={fill.A / 255.0:R},draw opacity={(width <= 0 ? 0 : border.A / 255.0):R},line width={Math.Max(width, 0) * 72.27 / 96:R}pt,rotate={rotation}{scale}}}");
    }

    /// <summary>Defines filled plus and cross outlines absent from the standard PGF marker library.</summary>
    internal static string MarkerDefinition(MarkerStyle style)
    {
        (double X, double Y)[] points = style switch
        {
            MarkerStyle.Plus =>
            [
                (0.3, 0.3), (1, 0.3), (1, -0.3), (0.3, -0.3), (0.3, -1), (-0.3, -1), (-0.3, -0.3), (-1, -0.3), (-1, 0.3), (-0.3, 0.3),
                (-0.3, 1), (0.3, 1)
            ],
            MarkerStyle.Cross =>
            [
                (1.0 / 3, 0), (1, -2.0 / 3), (2.0 / 3, -1), (0, -1.0 / 3), (-2.0 / 3, -1), (-1, -2.0 / 3), (-1.0 / 3, 0), (-1, 2.0 / 3), (-2.0 / 3, 1), (0, 1.0 / 3),
                (2.0 / 3, 1), (1, 2.0 / 3)
            ],
            _ => []
        };
        if (points.Length == 0)
            return String.Empty;
        var result = new StringBuilder("\\pgfdeclareplotmark{" + FormatMarkerStyle(style) + "}{");
        for (var i = 0; i < points.Length; i++)
        {
            result.Append(i == 0 ? @"\pgfpathmoveto" : @"\pgfpathlineto");
            result.Append(FormattableString.Invariant($"{{\\pgfqpoint{{{points[i].X:R}\\pgfplotmarksize}}{{{points[i].Y:R}\\pgfplotmarksize}}}}"));
        }
        return result.Append(@"\pgfpathclose\pgfusepath{fill,stroke}}").ToString();
    }

    /// <summary>Maps a native marker shape to the corresponding PGFPlots symbol.</summary>
    internal static string FormatMarkerStyle(MarkerStyle style)
    {
        switch (style)
        {
            case MarkerStyle.Dot:
                return "*";
            case MarkerStyle.Circle:
                return "*";
            case MarkerStyle.Diamond:
                return "diamond*";
            case MarkerStyle.Square:
                return "square*";
            case MarkerStyle.Rectangle:
                return "square*";
            case MarkerStyle.TriangleUp:
                return "triangle*";
            case MarkerStyle.TriangleDown:
                return "triangle*";
            case MarkerStyle.TriangleLeft:
                return "triangle*";
            case MarkerStyle.TriangleRight:
                return "triangle*";
            case MarkerStyle.Plus:
                return "iln-plus";
            case MarkerStyle.Cross:
                return "iln-cross";
            case MarkerStyle.None:
                return "";
            default:
                return "+";
        }
    }

    #endregion
}
