using System;
using System.Collections.Generic;
using System.Linq;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator.Elements;

/// <summary>Converts supported scene geometry into an export snapshot without a UI.</summary>
internal sealed class TikzLiteral(IEnumerable<string> lines) : ITikzElement
{
    private readonly string[] lines = lines.ToArray();

    public string PreTag => "";

    public string PostTag => "";

    public IEnumerable<string> Content => lines;

    /// <summary>Dispatches visible supported nodes to geometry converters and reports omissions.</summary>
    public void Bind(Node node, TikzGlobals globals) => throw new NotSupportedException();
}
