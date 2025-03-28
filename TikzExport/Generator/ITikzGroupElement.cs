using System.Collections.Generic;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator;

public interface ITikzGroupElement : ITikzElement, ICollection<ITikzElement>
{
    void Bind(Group group, TikzGlobals globals);
}