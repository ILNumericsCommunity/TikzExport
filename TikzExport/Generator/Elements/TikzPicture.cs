using System;
using System.Collections.Generic;
using System.Linq;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

/// <summary>
/// Represents a TikZ picture containing global settings and child elements.
/// </summary>
public class TikzPicture : TikzGroupElementBase
{
    private TikzGlobals _globals;
    private bool _hasContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TikzPicture" /> class.
    /// </summary>
    /// <param name="canvasSize">Optional canvas size to use for the generated TikZ picture.</param>
    public TikzPicture(System.Drawing.Size canvasSize = default)
    {
        if (canvasSize.Width == 0 || canvasSize.Height == 0)
            canvasSize = new System.Drawing.Size(100, 100); // Default size

        CanvasSize = canvasSize;
    }

    /// <summary>
    /// Gets the canvas size for the picture.
    /// </summary>
    public System.Drawing.Size CanvasSize { get; }

    /// <summary>
    /// Gets the content lines for the picture.
    /// </summary>
    public override IEnumerable<string> Content
    {
        get
        {
            if (!_hasContent)
                yield break;

            // Colors
            foreach (var colorDef in _globals.Colors.Content)
                yield return colorDef;

            yield return "";

            yield return @"\begin{tikzpicture}";

            // PGFPlots Options
            foreach (var pgfPlotsOption in _globals.PGFPlotOptions.Content)
                yield return pgfPlotsOption;

            // Child Elements
            foreach (var contentLine in base.Content)
                yield return contentLine;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this picture has any exportable content.
    /// </summary>
    public bool IsEmpty => !_hasContent;

    /// <summary>
    /// Gets the closing TikZ picture command.
    /// </summary>
    public override string PostTag => _hasContent ? @"\end{tikzpicture}" : String.Empty;

    /// <summary>
    /// Gets the comment header for the picture.
    /// </summary>
    public override string PreTag => _hasContent ? "% Created via TikzExport (by the ILNumerics Community)" : String.Empty;

    /// <summary>
    /// Binds the picture to an ILNumerics scene.
    /// </summary>
    /// <param name="group">The scene group.</param>
    /// <param name="globals">The shared globals.</param>
    public override void Bind(Group group, TikzGlobals globals)
    {
        _globals = globals;
        globals.CanvasSize = CanvasSize;

        var scene = group as Scene;
        if (scene == null)
            return;

        var plotCube = scene.First<PlotCube>();
        if (plotCube == null)
        {
            _hasContent = false;
            return;
        }

        _hasContent = plotCube.Find<ErrorBarPlot>().Any() || plotCube.Find<LinePlot>().Any() || plotCube.Find<Surface>().Any() || plotCube.Find<FastSurface>().Any();

        if (!_hasContent)
            return;

        // NOTE: Only PlotCube is supported for now
        this.BindGroup<TikzAxis>(plotCube, globals);
    }
}
