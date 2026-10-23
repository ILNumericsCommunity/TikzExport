using System;
using System.Text;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>Converts the actual ILNumerics palette to a plot-local PGFPlots palette.</summary>
public static class TikzColormapUtility
{
    /// <summary>Samples a native color map into a deterministic PGFPlots color-map definition.</summary>
    internal static string FormatColormap(Colormap colormap)
    {
        var builder = new StringBuilder("colormap={ilnumerics}{");
        for (var i = 0; i <= 64; i++)
        {
            var c = colormap.Map(i / 64f, Tuple.Create(0f, 1f));
            builder.Append(FormattableString.Invariant($"rgb({i}pt)=({c.X:R},{c.Y:R},{c.Z:R}); "));
        }

        return builder.Append('}').ToString();
    }
}
