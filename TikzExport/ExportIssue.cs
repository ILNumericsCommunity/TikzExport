using System.Collections.Generic;

namespace ILNumerics.Community.TikzExport;

/// <summary>A limitation encountered while exporting; cube indices are zero based (-1 for the scene).</summary>
/// <param name="PlotCubeIndex">Zero-based cube index, or -1 for a scene-level issue.</param>
/// <param name="NodeType">Native scene-node type associated with the issue.</param>
/// <param name="Message">Human-readable explanation of omitted or approximated content.</param>
public sealed record ExportIssue(int PlotCubeIndex, string NodeType, string Message);

/// <summary>TikZ markup and its conversion diagnostics.</summary>
/// <param name="Tikz">Generated markup, including placeholders when conversion is unavailable.</param>
/// <param name="Issues">Conversion limitations associated with this snapshot.</param>
public sealed record ExportResult(string Tikz, IReadOnlyList<ExportIssue> Issues);
