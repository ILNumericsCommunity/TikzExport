namespace ILNumerics.Community.TikzExport.Generator.Global;

/// <summary>
/// Holds global export settings and shared resources.
/// </summary>
public sealed class TikzGlobals
{
    public TikzGlobals()
    {
        CanvasSize = new System.Drawing.Size(100, 100);
        PGFPlotOptions = new PGFPlotOptions(this);
        Colors = new TikzColors();
    }

    /// <summary>
    /// Gets or sets the canvas size used for the export.
    /// </summary>
    public System.Drawing.Size CanvasSize { get; set; }

    /// <summary>
    /// Gets the color registry used by the exporter.
    /// </summary>
    public TikzColors Colors { get; }

    /// <summary>
    /// Gets the collection of global PGFPlots options.
    /// </summary>
    public PGFPlotOptions PGFPlotOptions { get; }
}
