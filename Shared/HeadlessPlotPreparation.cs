using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using ILNumerics.Drawing;

namespace ILNumerics.Community.ExportPreparation;

// Source-shared between the two exporters. Never prepare a caller-owned node.
/// <summary>Owns an isolated renderer-independent geometry snapshot.</summary>
internal sealed class HeadlessPlotPreparation : IDisposable
{
    /// <summary>Samples per control point used when no viewport-specific resolution is available.</summary>
    internal const int DefaultSplineResolution = 32;

    /// <summary>Gets the prepared snapshot, or the original cube when no preparation was needed.</summary>
    internal PlotCube Cube { get; }

    private readonly bool ownsCube;
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly Lazy<MethodInfo> Tessellate = new(() => typeof(SplinePlot).GetMethod("ComputeAdditionalVertices", PrivateInstance)
                                                                    ?? throw new
                                                                        NotSupportedException("This ILNumerics version does not expose the expected spline geometry implementation."));

    /// <summary>Creates an isolated geometry snapshot when renderer-dependent plots are present. The source remains caller-owned.</summary>
    internal HeadlessPlotPreparation(PlotCube source)
    {
        ownsCube = source.Find<BarPlot>().Any() || source.Find<BarPlotEx>().Any() || source.Find<SplinePlot>().Any();
        Cube = ownsCube ? (PlotCube) source.Copy() : source;
        if (!ownsCube)
            return;
        try
        {
            using var scope = Scope.Enter();

            // Copy() shares buffers; Detach() is essential before any geometry computation.
            foreach (var shape in Cube.Find<Shape>())
                shape.Detach();
            Prepare(source.Plots, Cube.Plots);
            Cube.Plots.Reset();
        }
        catch
        {
            Cube.Dispose();
            throw;
        }
    }

    /// <summary>Recursively prepares supported plots on detached copies without invoking renderer visitors.</summary>
    private void Prepare(Group original, Group copy)
    {
        var originals = original.Children.ToArray();
        var copies = copy.Children.ToArray();
        for (var i = 0; i < originals.Length; i++)
        {
            if (!copies[i].Visible || (copies[i] is Group transformed && !transformed.Transform.Equals(Matrix4.Identity)))
                continue;
            switch (originals[i])
            {
                case BarPlot bar:
                    using (var detached = CloneBar(bar))
                    {
                        foreach (var shape in detached.Find<Shape>())
                            shape.Detach();

                        // ILNumerics 7.4 Copy omits padding and queued per-bar/per-group colors.

                        detached.Configure(false, false); // No parent => no axis/font creation.
                        RepairBarLayout(bar, detached);
                        ApplyBarColors(bar, detached);
                        var destination = (BarPlot) copies[i];
                        IsolateDefaultColors(destination);
                        Transfer(detached.Fill, destination.Fill);
                        Transfer(detached.Border, destination.Border);
                    }
                    break;
                case BarPlotEx bars:
                    using (var detached = (BarPlotEx) bars.Copy())
                    {
                        foreach (var shape in detached.Find<Shape>())
                            shape.Detach();
                        detached.Configure(false, false);
                        var destination = (BarPlotEx) copies[i];
                        Transfer(detached.Fill, destination.Fill);
                        Transfer(detached.Border, destination.Border);
                    }
                    break;
                case SplinePlot originalSpline:
                    var spline = (SplinePlot) copies[i];
                    CopyField(originalSpline, spline, "m_originalDimensions");

                    // Preserve native parametrization (including auto mode's chord-length path).
                    if (!spline.Resolution.HasValue)
                        Field(typeof(SplinePlot), "m_currentResolution").SetValue(spline, (int?) DefaultSplineResolution);
                    try
                    {
                        Tessellate.Value.Invoke(spline, null);
                    }
                    catch (TargetInvocationException e) when (e.InnerException != null)
                    {
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    }
                    break;
                case Group group when group.GetType() == typeof(Group) && copies[i] is Group copiedGroup:
                    Prepare(group, copiedGroup);
                    break;
            }
        }
    }

    /// <summary>Copies prepared shape buffers and converts data coordinates to the plot cube scale.</summary>
    private void Transfer(Shape source, Shape target)
    {
        target.Color = source.Color;
        target.Buffers = source.Buffers;
        target.Detach();

        // Bar generators return data coordinates; the export binders consume plot-cube coordinates.
        Array<float> positions = target.Positions.Storage.C;
        var modes = new[] { Cube.ScaleModes.XAxisScale, Cube.ScaleModes.YAxisScale, Cube.ScaleModes.ZAxisScale };
        for (var axis = 0; axis < 3; axis++)
        {
            if (modes[axis] == AxisScale.Logarithmic)
                for (var j = 0; j < positions.S[1]; j++)
                    positions.SetValue((float) Math.Log10(positions.GetValue(axis, j)), axis, j);
        }

        target.Positions.Update(positions);
    }

    // Preserve per-bar border colors without creating a trace for every edge.
    /// <summary>Groups indexed bar edges by color, yielding temporary line shapes that are disposed as enumeration advances.</summary>
    internal static IEnumerable<Lines> Borders(Lines border)
    {
        if (!border.Visible)
            yield break;
        using var scope = Scope.Enter();
        Array<float> positions = border.Positions.Storage, colors = border.Colors.Storage;
        Array<int> indices = border.Indices.Storage;
        var order = indices.IsEmpty ? Enumerable.Range(0, (int) positions.S[1]).ToArray() : indices.ToArray();
        var groups = new Dictionary<Color, List<float[]>>();
        for (var i = 1; i < order.Length; i += border is LineStrip ? 1 : 2)
        {
            int a = order[i - 1], c = order[i];
            if (a < 0 || c < 0 || a >= positions.S[1] || c >= positions.S[1])
                throw new InvalidOperationException("Bar border contains an invalid vertex index.");
            if (Enumerable.Range(0, 3).Any(d => !Single.IsFinite(positions.GetValue(d, a)) || !Single.IsFinite(positions.GetValue(d, c))))
                continue;
            var color = border.Color ?? Color.Black;
            if (!border.Color.HasValue && !colors.IsEmpty)
            {
                var column = colors.S[1] == 1 ? 0 : a;

                int Channel(int row) => (int) Math.Round(Math.Clamp(colors.GetValue(row, column), 0, 1) * 255);

                color = Color.FromArgb(Channel(3), Channel(0), Channel(1), Channel(2));
            }

            if (!groups.TryGetValue(color, out var vertices))
                groups.Add(color, vertices = new List<float[]>());
            foreach (var vertex in new[] { a, c })
                vertices.Add(new[] { positions.GetValue(0, vertex), positions.GetValue(1, vertex), positions.GetValue(2, vertex) });
        }

        foreach (var pair in groups)
        {
            using var lines = new Lines { Color = pair.Key, Width = border.Width, DashStyle = border.DashStyle };
            var data = new float[3, pair.Value.Count];
            for (var i = 0; i < pair.Value.Count; i++)
                for (var d = 0; d < 3; d++)
                    data[d, i] = pair.Value[i][d];
            lines.Positions.Update(data);
            yield return lines;
        }
    }

    /// <summary>Copies a bar plot while isolating palette state omitted or shared by the native copy operation.</summary>
    private static BarPlot CloneBar(BarPlot source)
    {
        var result = (BarPlot) source.Copy();
        IsolateDefaultColors(result);
        result.Padding = source.Padding;

        // Clone enumerator state rather than consuming the caller's palette.
        result.NextColors = (ColorEnumerator) typeof(object).GetMethod("MemberwiseClone", PrivateInstance)!.Invoke(source.NextColors, null)!;
        return result;
    }

    /// <summary>Detaches the private native palette array so configuring a copy cannot consume or dispose source state.</summary>
    private static void IsolateDefaultColors(BarPlot bar)
    {
        var field = Field(typeof(BarPlot), "m_defaultColors");
        Array<float> original = (Array<float>) field.GetValue(bar)!;
        Array<float> colors = localMember<float>();
        colors.a = original.C;
        field.SetValue(bar, colors);
    }

    /// <summary>Repairs native grouped offsets and stacked baselines in the detached geometry.</summary>
    private static void RepairBarLayout(BarPlot original, BarPlot target)
    {
        Array<float> data = (Array<float>) Field(typeof(BarPlot), "m_data").GetValue(original)!;
        Array<float> x = (Array<float>) Field(typeof(BarPlot), "m_dataX").GetValue(original)!;
        var customX = (bool) Field(typeof(BarPlot), "m_userDefinedX").GetValue(original)!;
        var grouped = !data.IsVector && original.Style != BarPlotStyle.stacked && !(original.Style == BarPlotStyle.detached && original.Draw3D);
        if ((!grouped || (customX && !x.IsVector)) && original.Style != BarPlotStyle.stacked)
            return;
        var vertices = original.Draw3D ? 9 : 5;
        var category = original.Horizontal ? original.Draw3D && original.Style != BarPlotStyle.detached ? 2 : 1 : 0;
        var height = original.Horizontal ? 0 :
            original.Draw3D ? 2 : 1;

        // Native grouped layout overwrites later row offsets instead of adding them;
        // native stacked layout also ignores BaseValue. Repair only those coordinates.
        Array<float> p = target.Border.Positions.Storage.C;
        for (var bar = 0; bar < data.S.NumberOfElements; bar++)
        {
            var start = bar * vertices;
            if (original.Style == BarPlotStyle.stacked)
            {
                for (var i = 0; i < vertices - 1; i++)
                    p.SetValue(p.GetValue(height, start + i) + original.BaseValue, height, start + i);
                continue;
            }
            int row = bar / (int) data.S[1], col = bar % (int) data.S[1];
            var width = (1 - original.Padding) / (int) data.S[1];
            var desired = (customX ? x.GetValue(row) : row + 1) + ((col + .5f) * width) - ((1 - original.Padding) / 2);
            if (original.Style == BarPlotStyle.hist)
                desired += width / 2;
            float min = Single.PositiveInfinity, max = Single.NegativeInfinity;
            for (var i = 0; i < vertices - 1; i++)
            {
                var value = p.GetValue(category, start + i);
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }

            var shift = desired - ((min + max) / 2);
            for (var i = 0; i < vertices - 1; i++)
                p.SetValue(p.GetValue(category, start + i) + shift, category, start + i);
        }

        target.Border.Positions.Update(p);
        target.Fill.Positions.Update(p);
    }

    /// <summary>Applies queued group and individual overrides, preserving channels whose optional colors are absent.</summary>
    private static void ApplyBarColors(BarPlot source, BarPlot target)
    {
        Array<float> data = (Array<float>) Field(typeof(BarPlot), "m_data").GetValue(source)!;
        var vertices = source.Draw3D ? 9 : 5;
        Array<float> border = target.Border.Colors.Storage.C, fill = target.Fill.Colors.Storage.C;
        var groups = (List<Tuple<int, Color?, Color?>>) Field(typeof(BarPlot), "m_groupColorsStore").GetValue(source)!;
        var bars = (List<Tuple<int, Color?, Color?>>) Field(typeof(BarPlot), "m_barColorsStore").GetValue(source)!;
        foreach (var entry in groups)
        {
            for (var bar = 0; bar < data.S.NumberOfElements; bar++)
                if (data.IsVector || bar % data.S[1] == entry.Item1)
                    Apply(entry, bar);
        }

        foreach (var entry in bars)
            Apply(entry, entry.Item1);
        if (groups.Concat(bars).Any(e => e.Item2.HasValue))
            target.Border.Color = null;
        if (groups.Concat(bars).Any(e => e.Item3.HasValue))
            target.Fill.Color = null;
        target.Border.Colors.Update(border);
        target.Fill.Colors.Update(fill);

        void Apply(Tuple<int, Color?, Color?> entry, int bar)
        {
            for (var j = bar * vertices; j < (bar + 1) * vertices; j++)
            {
                if (entry.Item2 is Color c)
                    Write(border, j, c);
                if (entry.Item3 is Color f)
                    Write(fill, j, f);
            }
        }

        static void Write(Array<float> colors, int vertex, Color color)
        {
            if (vertex < 0 || vertex >= colors.S[1])
                throw new InvalidOperationException("Bar color refers to an invalid vertex.");
            colors.SetValue(color.R / 255f, 0, vertex);
            colors.SetValue(color.G / 255f, 1, vertex);
            colors.SetValue(color.B / 255f, 2, vertex);
            colors.SetValue(color.A / 255f, 3, vertex);
        }
    }

    /// <summary>Resolves a required native geometry field or fails explicitly when the dependency contract changes.</summary>
    private static FieldInfo Field(Type type, string name)
        => type.GetField(name, PrivateInstance)
           ?? throw new NotSupportedException($"ILNumerics {type.Name} no longer exposes the expected geometry state '{name}'.");

    /// <summary>Copies native state that the dependency omits from its public copy operation.</summary>
    private static void CopyField(object source, object target, string name) => Field(source.GetType(), name).SetValue(target, Field(source.GetType(), name).GetValue(source));

    /// <summary>Releases the owned snapshot; never disposes the caller-owned plot cube.</summary>
    public void Dispose()
    {
        if (ownsCube)
            Cube.Dispose();
    }
}
