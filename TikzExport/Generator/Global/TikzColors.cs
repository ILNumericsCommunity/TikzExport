using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator.Global;

/// <summary>
/// Maintains a collection of colors used in the TikZ output.
/// </summary>
public class TikzColors : ICollection<Color>, ITikzElement
{
    private readonly List<Color> _colors = new();

    #region KnownColors

    private readonly Dictionary<Color, string> _knownColors = new() { { Color.Black, "black" }, { Color.White, "white" } };

    #endregion

    #region Implementation of ITikzElement

    /// <summary>
    /// Gets the opening tag for color definitions.
    /// </summary>
    public string PreTag => "";

    /// <summary>
    /// Gets the color definition lines.
    /// </summary>
    public IEnumerable<string> Content
    {
        get
        {
            // Color Definitions
            for (var i = 0; i < Count; i++)
            {
                var colorName = $"colorDef{i:D2}";
                var color = FormatTikzColor(_colors[i]);

                yield return $"\\definecolor{{{colorName}}}{{rgb}}{{{color}}}";
            }
        }
    }

    /// <summary>
    /// Gets the closing tag for color definitions.
    /// </summary>
    public string PostTag => "";

    public void Bind(Node node, TikzGlobals globals)
    {
        // NOTE: Don't bind automatically
        throw new NotSupportedException();
    }

    #endregion

    /// <summary>
    /// Gets the TikZ name for the specified color, adding it if needed.
    /// </summary>
    /// <param name="color">The color to resolve.</param>
    /// <returns>The TikZ color name.</returns>
    public string GetColorName(Color color)
    {
        if (_knownColors.TryGetValue(color, out var name))
            return name;

        if (!_colors.Contains(color))
            _colors.Add(color);

        return $"colorDef{_colors.IndexOf(color):D2}";
    }

    /// <summary>
    /// Formats the specified color as a TikZ RGB definition.
    /// </summary>
    /// <param name="color">The color to format.</param>
    /// <returns>The formatted RGB color string.</returns>
    public string FormatTikzColor(Color color)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        return FormattableString.Invariant($"{r:F6},{g:F6},{b:F6}");
    }

    #region ICollection<Color>

    /// <summary>
    /// Gets the number of custom colors.
    /// </summary>
    public int Count => _colors.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds a color definition if it is not already present.
    /// </summary>
    /// <param name="item">The color to add.</param>
    public void Add(Color item)
    {
        if (_knownColors.ContainsKey(item))
            return;

        // Prevent duplicates (only add once)
        if (_colors.Contains(item))
            return;

        _colors.Add(item);
    }

    /// <summary>
    /// Removes all custom colors from the collection.
    /// </summary>
    public void Clear()
    {
        _colors.Clear();
    }

    /// <summary>
    /// Determines whether the specified color is present.
    /// </summary>
    /// <param name="item">The color to locate.</param>
    /// <returns><c>true</c> when the color is present; otherwise, <c>false</c>.</returns>
    public bool Contains(Color item)
    {
        if (_knownColors.ContainsKey(item))
            return true;

        return _colors.Contains(item);
    }

    /// <summary>
    /// Copies the collection to an array, starting at the specified index.
    /// </summary>
    /// <param name="array">The target array.</param>
    /// <param name="arrayIndex">The index at which to start copying.</param>
    public void CopyTo(Color[] array, int arrayIndex)
    {
        _colors.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the specified color from the collection.
    /// </summary>
    /// <param name="item">The color to remove.</param>
    /// <returns><c>true</c> when the color was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(Color item) => _colors.Remove(item);

    /// <summary>
    /// Returns an enumerator that iterates through the colors.
    /// </summary>
    public IEnumerator<Color> GetEnumerator() => _colors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _colors).GetEnumerator();

    #endregion
}
