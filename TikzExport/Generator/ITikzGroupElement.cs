using System.Collections.Generic;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Defines a TikZ element that contains child elements and binds to groups.
/// </summary>
public interface ITikzGroupElement : ITikzElement, ICollection<ITikzElement>
{
    /// <summary>
    /// Binds the group element to the specified ILNumerics group.
    /// </summary>
    /// <param name="group">The group to bind.</param>
    /// <param name="globals">The shared globals.</param>
    void Bind(Group group, TikzGlobals globals);
}
