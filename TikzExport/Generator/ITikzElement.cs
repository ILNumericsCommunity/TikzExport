using System.Collections.Generic;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Defines a TikZ element that can be bound to ILNumerics nodes and rendered.
/// </summary>
public interface ITikzElement
{
    #region Binding

    /// <summary>
    /// Binds the element to the specified ILNumerics node.
    /// </summary>
    /// <param name="node">The node to bind.</param>
    /// <param name="globals">The shared globals.</param>
    void Bind(Node node, TikzGlobals globals);

    #endregion

    #region TikzTags

    /// <summary>
    /// Gets the opening tag for the element.
    /// </summary>
    string PreTag { get; }

    /// <summary>
    /// Gets the content lines for the element.
    /// </summary>
    IEnumerable<string> Content { get; }

    /// <summary>
    /// Gets the closing tag for the element.
    /// </summary>
    string PostTag { get; }

    #endregion
}
