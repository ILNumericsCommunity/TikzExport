using System;
using System.Collections;
using System.Collections.Generic;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Base class for TikZ elements that contain child elements.
/// </summary>
public abstract class TikzGroupElementBase : ITikzElement, ICollection<ITikzElement>, ITikzGroupElement
{
    private readonly List<ITikzElement> _children = new();

    #region Implementation of ICollection<ITikzElement>

    public IEnumerator<ITikzElement> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _children).GetEnumerator();

    /// <summary>
    /// Adds a child element.
    /// </summary>
    /// <param name="item">The element to add.</param>
    public void Add(ITikzElement item)
    {
        _children.Add(item);
    }

    /// <summary>
    /// Removes all child elements.
    /// </summary>
    public void Clear()
    {
        _children.Clear();
    }

    /// <summary>
    /// Determines whether the specified element exists in the collection.
    /// </summary>
    /// <param name="item">The element to locate.</param>
    /// <returns><c>true</c> when the element exists; otherwise, <c>false</c>.</returns>
    public bool Contains(ITikzElement item) => _children.Contains(item);

    /// <summary>
    /// Copies the child elements to an array.
    /// </summary>
    /// <param name="array">The target array.</param>
    /// <param name="arrayIndex">The starting index.</param>
    public void CopyTo(ITikzElement[] array, int arrayIndex)
    {
        _children.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the specified element from the collection.
    /// </summary>
    /// <param name="item">The element to remove.</param>
    /// <returns><c>true</c> when the element was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(ITikzElement item) => _children.Remove(item);

    /// <summary>
    /// Gets the number of child elements.
    /// </summary>
    public int Count => _children.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    #endregion

    #region Implementation of ITikzElement

    public abstract string PreTag { get; }

    public virtual IEnumerable<string> Content
    {
        get
        {
            // Return elements (one-by-one)
            foreach (var element in _children)
            {
                // Return PreTag of the nth element
                if (!String.IsNullOrEmpty(element.PreTag))
                    yield return element.PreTag;

                // Return content of the nth element (line-by-line)
                foreach (var contentLine in element.Content)
                    yield return contentLine;

                // Return PostTag of the nth element
                if (!String.IsNullOrEmpty(element.PostTag))
                    yield return element.PostTag;
            }
        }
    }

    public abstract string PostTag { get; }

    public virtual void Bind(Node node, TikzGlobals globals)
    {
        // Not needed for a group element
    }

    #endregion

    #region Implementation of ITikzGroupElement

    public abstract void Bind(Group group, TikzGlobals globals);

    #endregion
}
