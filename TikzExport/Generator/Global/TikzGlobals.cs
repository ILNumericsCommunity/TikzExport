namespace ILNumerics.Community.TikzExport.Generator.Global;

public sealed class TikzGlobals
{
    public TikzGlobals()
    {
        CanvasSize = new System.Drawing.Size(100, 100);
        PGFPlotOptions = new PGFPlotOptions(this);
        Colors = new TikzColors();
    }

    public System.Drawing.Size CanvasSize { get; set; }
        
    public PGFPlotOptions PGFPlotOptions { get; }

    public TikzColors Colors { get; }
}