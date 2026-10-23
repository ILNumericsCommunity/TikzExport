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
    private TikzGlobals? _globals;
    private bool _hasContent;
    private bool _hasData;

    /// <summary>
    /// Initializes a new instance of the <see cref="TikzPicture" /> class.
    /// </summary>
    /// <param name="canvasSize">Optional canvas size to use for the generated TikZ picture.</param>
    public TikzPicture(System.Drawing.Size canvasSize = default)
    {
        if (canvasSize.Width < 0 || canvasSize.Height < 0)
            throw new ArgumentOutOfRangeException(nameof(canvasSize));
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
            if (!_hasContent || _globals == null)
                yield break;

            var globals = _globals;

            // Colors
            foreach (var colorDef in globals.Colors.Content)
                yield return colorDef;

            yield return "";

            yield return @"\begin{tikzpicture}";

            // Child Elements
            foreach (var contentLine in base.Content)
                yield return contentLine;
        }
    }

    /// <summary>
    /// Gets conversion limitations encountered while binding.
    /// </summary>
    public IReadOnlyList<ExportIssue> Issues => _globals?.Issues.AsReadOnly() ?? (IReadOnlyList<ExportIssue>) Array.Empty<ExportIssue>();

    /// <summary>True when no plot data was converted; the output still contains a placeholder.</summary>
    public bool IsEmpty => !_hasData;

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
        Clear();
        _globals = globals;
        globals.Issues.Clear();
        globals.CanvasSize = CanvasSize;
        _hasData = false;
        _hasContent = true; // Even empty scenes produce a visible placeholder.
        var cubes = group.Find<PlotCube>().Where(c => Visible(c)).ToArray();
        for (var i = 0; i < cubes.Length; i++)
        {
            globals.PlotCubeIndex = i;
            var axis = new TikzAxis();
            axis.Bind(cubes[i], globals);

            // Snapshot each axis before the next cube changes its grid/colormap settings.
            var lines = new List<string> { $"\\begin{{scope}}[yshift={-i * (CanvasSize.Height + 25)}mm]" };
            lines.AddRange(globals.PGFPlotOptions.Content);
            lines.Add(axis.PreTag);
            lines.AddRange(axis.Content);
            lines.Add(axis.PostTag);
            _hasData |= lines.Any(line => line.StartsWith(@"\addplot", StringComparison.Ordinal));
            lines.Add(@"\end{scope}");
            Add(new TikzLiteral(lines));
        }

        if (cubes.Length == 0)
        {
            globals.Issues.Add(new ExportIssue(-1, group.GetType().Name, "No visible plot cube could be exported."));
            Add(new TikzLiteral(new[] { @"\node[draw,align=center] {No exportable plot data};" }));
        }
    }

    /// <summary>Checks visibility through all ancestor nodes.</summary>
    private static bool Visible(Node node)
    {
        for (var n = node; n != null; n = n.Parent)
        {
            if (!n.Visible)
                return false;
        }
        return true;
    }
}
