using System.IO;
using ILNumerics.Community.TikzExport.Generator.Elements;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Writes a <see cref="TikzPicture" /> to a text stream.
/// </summary>
public class TikzWriter
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TikzWriter" /> class.
    /// </summary>
    /// <param name="writer">The destination writer.</param>
    public TikzWriter(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Writes the specified picture to the underlying writer.
    /// </summary>
    /// <param name="picture">The picture to write.</param>
    public void Write(TikzPicture picture)
    {
        _writer.WriteLine(picture.PreTag);

        // Render Content (line-by-line)
        foreach (var contentLine in picture.Content)
            _writer.WriteLine(contentLine);

        _writer.WriteLine(picture.PostTag);
    }
}
