using System;
using System.Collections.Generic;
using System.Drawing;
using ILNumerics.Drawing;
using static ILNumerics.Community.TikzExport.Generator.TikzColormapUtility;

namespace ILNumerics.Community.TikzExport.Generator.Global;

/// <summary>
/// Provides global PGFPlots options for the exported TikZ picture.
/// </summary>
public sealed class PGFPlotOptions : List<string>, ITikzElement
{
    private const int CompatibilityIndex = 0;
    private const int MajorGridStyleIndex = 2;
    private const int MinorGridStyleIndex = 3;
    private readonly TikzGlobals _globals;

    /// <summary>
    /// Initializes a new instance of the <see cref="PGFPlotOptions" /> class.
    /// </summary>
    /// <param name="globals">The global settings container.</param>
    public PGFPlotOptions(TikzGlobals globals)
    {
        _globals = globals;

        Add("compat=1.13"); // Minimum version
        Add("set layers"); // Sort layers
        Add("major grid style={solid,very thin,white!80!black}"); // Default major grid style
        Add("minor grid style={dashed,very thin,white!90!black}"); // Default minor grid style
    }

    #region Implementation of ITikzElement

    /// <summary>
    /// Gets the opening tag for PGFPlots options.
    /// </summary>
    public string PreTag => "";

    /// <summary>
    /// Gets the PGFPlots options content.
    /// </summary>
    public IEnumerable<string> Content
    {
        get
        {
            // PGFPlots Options
            foreach (var option in this)
            {
                if (String.IsNullOrEmpty(option)) // Skip empty options
                    continue;

                yield return $"\\pgfplotsset{{{option}}}";
            }
        }
    }

    /// <summary>
    /// Gets the closing tag for PGFPlots options.
    /// </summary>
    public string PostTag => "";

    public void Bind(Node node, TikzGlobals globals)
    {
        // NOTE: Don't bind automatically
        throw new NotSupportedException();
    }

    #endregion

    /// <summary>
    /// Sets the PGFPlots compatibility level.
    /// </summary>
    /// <param name="version">The compatibility version string.</param>
    public void SetCompatiblity(string version)
    {
        // Replace compat version
        this[CompatibilityIndex] = "compat=" + version;
    }

    /// <summary>
    /// Sets the major grid line style.
    /// </summary>
    /// <param name="gridColor">The grid line color.</param>
    /// <param name="gridStyle">The grid line style.</param>
    /// <param name="gridWidth">The grid line width.</param>
    public void SetMajorGridStyle(Color gridColor, DashStyle gridStyle, int gridWidth)
    {
        // Replace default major grid style
        this[MajorGridStyleIndex] = $"major grid style={{{TikzFormatUtility.FormatLine(_globals, gridColor, gridStyle, 0.5f * gridWidth)}}}";
    }

    /// <summary>
    /// Sets the minor grid line style.
    /// </summary>
    /// <param name="gridColor">The grid line color.</param>
    /// <param name="gridStyle">The grid line style.</param>
    /// <param name="gridWidth">The grid line width.</param>
    public void SetMinorGridStyle(Color gridColor, DashStyle gridStyle, int gridWidth)
    {
        // Replace default minor grid style
        this[MinorGridStyleIndex] = $"minor grid style={{{TikzFormatUtility.FormatLine(_globals, gridColor, gridStyle, 0.5f * gridWidth)}}}";
    }

    /// <summary>
    /// Adds a global colormap definition.
    /// </summary>
    /// <param name="colormap">The colormap to add.</param>
    public void AddColormap(Colormap colormap)
    {
        // Global colormap
        Add(FormatColormap(colormap));
    }
}
