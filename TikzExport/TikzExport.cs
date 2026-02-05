using System;
using System.IO;
using ILNumerics.Community.TikzExport.Generator;
using ILNumerics.Community.TikzExport.Generator.Elements;
using ILNumerics.Community.TikzExport.Generator.Global;
using ILNumerics.Drawing;

namespace ILNumerics.Community.TikzExport;

/// <summary>
/// Provides methods to export an ILNumerics <see cref="Scene" /> to PGF/TikZ (https://en.wikipedia.org/wiki/PGF/TikZ).
/// </summary>
/// <remarks>
/// PGF/TikZ is a pair of languages for producing vector graphics (for example, technical illustrations and drawings)
/// from a geometric or algebraic description. PGF is a lower-level language, while TikZ is a set of higher-level
/// macros built on PGF. (Wikipedia)
/// The exported output is a standalone PGF/TikZ fragment (string) or file that contains the necessary
/// PGF/TikZ markup and data. Use <see cref="ExportString(Scene, System.Drawing.Size)" /> to get a string or
/// <see cref="ExportFile(Scene, string, System.Drawing.Size)" /> to write to a TikZ file.
/// </remarks>
public static class TikzExport
{
    /// <summary>
    /// Exports the given ILNumerics <see cref="Scene" /> to an PGF/TikZ string.
    /// </summary>
    /// <param name="scene">ILNumerics <see cref="Scene" /> to convert. Must not be <c>null</c>.</param>
    /// <param name="canvasSize">Optional canvas size to use for the generated TikZ picture.</param>
    /// <remarks>
    /// When the scene contains no supported plots, the file is created but left empty.
    /// </remarks>
    /// <returns>
    /// A string containing the PGF/TikZ fragment. Returns an empty string when the scene
    /// contains no supported plots.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="scene" /> is <c>null</c>.</exception>
    public static string ExportString(Scene scene, System.Drawing.Size canvasSize = default)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        using (var stringWriter = new StringWriter())
        {
            Export(scene, stringWriter, canvasSize);

            return stringWriter.ToString();
        }
    }

    /// <summary>
    /// Exports the given ILNumerics <see cref="Scene" /> to a PGF/TikZ file on disk.
    /// </summary>
    /// <param name="scene">ILNumerics <see cref="Scene" /> to convert. Must not be <c>null</c>.</param>
    /// <param name="filePath">The path of the file to write the exported TikZ markup to.</param>
    /// <param name="canvasSize">Optional canvas size to use for the generated TikZ picture.</param>
    /// <remarks>
    /// When the scene contains no supported plots, no output is written.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="scene" /> is <c>null</c>.</exception>
    public static void ExportFile(Scene scene, string filePath, System.Drawing.Size canvasSize = default)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));
        if (String.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        using (var streamWriter = new StreamWriter(filePath))
        {
            Export(scene, streamWriter, canvasSize);

            streamWriter.Flush();
        }
    }

    /// <summary>
    /// Exports the given ILNumerics <see cref="Scene" /> to PGF/TikZ via a <see cref="TextWriter" />.
    /// </summary>
    /// <param name="scene">ILNumerics <see cref="Scene" /> to convert. Must not be <c>null</c>.</param>
    /// <param name="writer">Text writer to which the TikZ markup will be written. Must not be <c>null</c>.</param>
    /// <param name="canvasSize">Optional canvas size to use for the generated TikZ picture.</param>
    /// <exception cref="ArgumentNullException"><paramref name="scene" /> is <c>null</c> or <paramref name="writer" /> is <c>null</c>.</exception>
    public static void Export(Scene scene, TextWriter writer, System.Drawing.Size canvasSize = default)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        // Obtain TIKZ picture from Scene
        var tikzPicture = Bind(scene, canvasSize);

        // Write to TextWriter
        var tikzWriter = new TikzWriter(writer);
        tikzWriter.Write(tikzPicture);
    }

    /// <summary>
    /// Converts an ILNumerics <see cref="Scene" /> into a <see cref="TikzPicture" /> instance.
    /// </summary>
    /// <param name="scene">ILNumerics <see cref="Scene" /> to convert. Must not be <c>null</c>.</param>
    /// <param name="canvasSize">Optional canvas size to use for the generated TikZ picture.</param>
    /// <returns>
    /// A <see cref="TikzPicture" /> instance that is bound to <paramref name="scene" />. The
    /// returned picture is empty when the scene contains no supported plots.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="scene" /> is <c>null</c>.</exception>
    public static TikzPicture Bind(Scene scene, System.Drawing.Size canvasSize = default)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        scene.Configure();

        var tikzPicture = new TikzPicture(canvasSize);
        tikzPicture.Bind(scene, new TikzGlobals());

        return tikzPicture;
    }
}
