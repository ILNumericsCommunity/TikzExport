using System;
using System.Drawing;
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

    /// <summary>Formats marker shape, size, orientation, and opacity.</summary>
    internal static string FormatMarker(TikzGlobals globals, Color color, MarkerStyle style, int size)
    {
        if (style == MarkerStyle.None)
            return "";

        var rotation = style switch
        {
            MarkerStyle.TriangleDown => 180,
            MarkerStyle.TriangleLeft => 90,
            MarkerStyle.TriangleRight => 270,
            _ => 0
        };
        return FormattableString.Invariant($"mark={FormatMarkerStyle(style)},mark size={size}pt,mark options={{{globals.Colors.GetColorName(color)},rotate={rotation},opacity={
            color.A / 255.0:R}}}");
    }

    /// <summary>Maps a native marker shape to the corresponding PGFPlots symbol.</summary>
    internal static string FormatMarkerStyle(MarkerStyle style)
    {
        switch (style)
        {
            case MarkerStyle.Dot:
                return "*";
            case MarkerStyle.Circle:
                return "o";
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
                return "+";
            case MarkerStyle.Cross:
                return "x";
            case MarkerStyle.None:
                return "";
            default:
                return "plus";
        }
    }

    #endregion
}
