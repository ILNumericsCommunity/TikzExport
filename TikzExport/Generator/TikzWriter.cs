using System.IO;
using ILNumerics.Community.TikzExport.Generator.Elements;

namespace ILNumerics.Community.TikzExport.Generator;

public class TikzWriter
{
    private readonly TextWriter _writer;

    public TikzWriter(TextWriter writer)
    {
        _writer = writer;
    }

    public void Write(TikzPicture picture)
    {
        _writer.WriteLine(picture.PreTag);

        // Render Content (line-by-line)
        foreach (var contentLine in picture.Content)
            _writer.WriteLine(contentLine);

        _writer.WriteLine(picture.PostTag);
    }
}