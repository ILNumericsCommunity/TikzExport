using System;
using System.Collections.Generic;
using System.Linq;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

/// <summary>A snapshot of an ILNumerics surface converted to PGFPlots.</summary>
public class TikzPlot3 : ITikzElement
{
    private string[] content = Array.Empty<string>();

    /// <inheritdoc />
    public string PreTag => "";

    /// <inheritdoc />
    public string PostTag => "";

    /// <inheritdoc />
    public IEnumerable<string> Content => content;

    /// <inheritdoc />
    public void Bind(Node node, TikzGlobals globals)
    {
        content = Array.Empty<string>();
        if (node is not Surface && node is not FastSurface)
            return;
        var cube = ((Group) node).FirstUp<PlotCube>() ?? throw new ArgumentException("Surface must belong to a plot cube.", nameof(node));
        using var scope = Scope.Enter();
        var holder = new TikzAxis();
        new PlotContentBinder(cube, holder, globals).Bind(node);
        content = holder.SelectMany(e => new[] { e.PreTag }.Concat(e.Content).Append(e.PostTag)).ToArray();
    }
}
