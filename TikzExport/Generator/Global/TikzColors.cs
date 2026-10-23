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
    private readonly Dictionary<int, string> names = new();
    private int nextId;

    #region KnownColors

    private readonly Dictionary<Color, string> _knownColors = new() { { Color.FromArgb(Color.Black.ToArgb()), "black" }, { Color.FromArgb(Color.White.ToArgb()), "white" } };

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
                var colorName = names[_colors[i].ToArgb()];
                var color = FormatTikzColor(_colors[i]);

                yield return $"\\definecolor{{{colorName}}}{{rgb}}{{{color}}}";
            }
        }
    }

    /// <summary>
    /// Gets the closing tag for color definitions.
    /// </summary>
    public string PostTag => "";

    /// <inheritdoc />
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
        if (_knownColors.TryGetValue(Color.FromArgb(color.ToArgb()), out var name))
            return name;

        Add(color);
        return names[color.ToArgb()];
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
        if (_knownColors.ContainsKey(Color.FromArgb(item.ToArgb())))
            return;

        // Prevent duplicates (only add once)
        if (names.ContainsKey(item.ToArgb()))
            return;

        names.Add(item.ToArgb(), $"colorDef{nextId++:D2}");
        _colors.Add(item);
    }

    /// <summary>
    /// Removes all custom colors from the collection.
    /// </summary>
    public void Clear()
    {
        _colors.Clear();
        names.Clear();
        nextId = 0;
    }

    /// <summary>
    /// Determines whether the specified color is present.
    /// </summary>
    /// <param name="item">The color to locate.</param>
    /// <returns><c>true</c> when the color is present; otherwise, <c>false</c>.</returns>
    public bool Contains(Color item)
    {
        if (_knownColors.ContainsKey(Color.FromArgb(item.ToArgb())))
            return true;

        return names.ContainsKey(item.ToArgb());
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
    public bool Remove(Color item)
    {
        if (!names.Remove(item.ToArgb()))
            return false;
        _colors.RemoveAll(c => c.ToArgb() == item.ToArgb());
        return true;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the colors.
    /// </summary>
    public IEnumerator<Color> GetEnumerator() => _colors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _colors).GetEnumerator();

    #endregion
}
