using System.Linq;
using ILNumerics.Community.TikzExport.Generator.Elements;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Provides helper methods to bind ILNumerics nodes to TikZ elements.
/// </summary>
public static class TikzBinderExtensions
{
    #region Plots

    /// <summary>
    /// Binds all supported plot types under the specified group.
    /// </summary>
    /// <param name="tikzGroup">The target TikZ group.</param>
    /// <param name="group">The ILNumerics group to search.</param>
    /// <param name="globals">The shared globals.</param>
    public static void BindPlots(this ITikzGroupElement tikzGroup, Group group, TikzGlobals globals)
    {
        // ErrorBarPlot
        var errorBarPlots = group.Find<ErrorBarPlot>().ToArray();
        foreach (var errorBarPlot in errorBarPlots)
            tikzGroup.BindElement<TikzPlotPlus>(errorBarPlot, globals);

        // LinePlot
        var linePlots = group.Find<LinePlot>().ToArray();
        linePlots = linePlots.Where(lp => lp.Parent is not ErrorBarPlot).ToArray(); // Exclude LinePlots which are a direct child of ErrorBarPlot
        foreach (var linePlot in linePlots)
            tikzGroup.BindElement<TikzPlot>(linePlot, globals);

        // Surface
        var surfaces = group.Find<Surface>().ToArray();
        foreach (var surface in surfaces)
            tikzGroup.BindElement<TikzPlot3>(surface, globals);

        // FastSurface
        var fastSurfaces = group.Find<FastSurface>().ToArray();
        foreach (var fastSurface in fastSurfaces)
            tikzGroup.BindElement<TikzPlot3>(fastSurface, globals);
    }

    #endregion

    #region Generic

    /// <summary>
    /// Creates and binds a TikZ element to the specified node, adding it to the group.
    /// </summary>
    /// <typeparam name="TTikz">The TikZ element type.</typeparam>
    /// <param name="tikzGroup">The target group element.</param>
    /// <param name="node">The node to bind.</param>
    /// <param name="globals">The shared globals.</param>
    public static void BindElement<TTikz>(this ITikzGroupElement tikzGroup, Node node, TikzGlobals globals)
        where TTikz : ITikzElement, new()
    {
        if (node == null)
            return;

        var tikz = new TTikz();
        tikz.Bind(node, globals);

        tikzGroup.Add(tikz);
    }

    /// <summary>
    /// Creates and binds a TikZ group to the specified ILNumerics group, adding it to the parent.
    /// </summary>
    /// <typeparam name="TTikzGroup">The TikZ group type.</typeparam>
    /// <param name="tikzGroup">The parent group element.</param>
    /// <param name="group">The group to bind.</param>
    /// <param name="globals">The shared globals.</param>
    public static void BindGroup<TTikzGroup>(this ITikzGroupElement tikzGroup, Group group, TikzGlobals globals)
        where TTikzGroup : ITikzGroupElement, new()
    {
        if (group == null)
            return;

        var tikz = new TTikzGroup();
        tikz.Bind(group, globals);

        tikzGroup.Add(tikz);
    }

    #endregion
}
