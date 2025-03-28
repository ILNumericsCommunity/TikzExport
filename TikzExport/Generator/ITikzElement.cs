using System.Collections.Generic;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator;

public interface ITikzElement
{
    #region TikzTags

    string PreTag { get; }

    IEnumerable<string> Content { get; }

    string PostTag { get; }

    #endregion

    #region Binding
        
    void Bind(Node node, TikzGlobals globals);

    #endregion
}