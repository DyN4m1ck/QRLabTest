using System.Collections.Concurrent;
using System.Globalization;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform;

namespace QRLab.View.Controls;

/// Monochrome path SVG assets, rendered with Avalonia's inherited Foreground.
public sealed class SvgIcon : TemplatedControl
{
    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<SvgIcon, string?>(nameof(Source));
    private static readonly ConcurrentDictionary<string, IconDrawing> Cache = new();
    public string? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

    static SvgIcon() => AffectsRender<SvgIcon>(SourceProperty, ForegroundProperty);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (string.IsNullOrEmpty(Source) || Foreground is null) return;
        var drawing = Cache.GetOrAdd(Source, Load);
        var scale = Math.Min(Bounds.Width / drawing.ViewBox.Width, Bounds.Height / drawing.ViewBox.Height);
        var offsetX = (Bounds.Width - drawing.ViewBox.Width * scale) / 2;
        var offsetY = (Bounds.Height - drawing.ViewBox.Height * scale) / 2;
        using (context.PushTransform(Matrix.CreateTranslation(-drawing.ViewBox.X, -drawing.ViewBox.Y) *
            Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offsetX, offsetY)))
        {
            foreach (var path in drawing.Paths)
                context.DrawGeometry(path.Filled ? Foreground : null,
                    path.Stroked ? new Pen(Foreground, path.StrokeWidth, lineCap: path.LineCap) : null, path.Geometry);
        }
    }

    private static IconDrawing Load(string name)
    {
        using var stream = AssetLoader.Open(new Uri($"avares://QRLab/Assets/svg/{name}.svg"));
        var root = XDocument.Load(stream).Root!;
        var box = root.Attribute("viewBox")!.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        var paths = root.Elements().Select(path =>
        {
            // This asset set uses path-only SVGs. Original pigment is deliberately replaced by Foreground.
            var fill = (string?)path.Attribute("fill") ?? (string?)root.Attribute("fill") ?? "black";
            var stroke = (string?)path.Attribute("stroke") ?? (string?)root.Attribute("stroke") ?? "none";
            return new IconPath(Geometry.Parse(path.Attribute("d")!.Value), fill != "none", stroke != "none",
                double.Parse((string?)path.Attribute("stroke-width") ?? "1", CultureInfo.InvariantCulture),
                (string?)path.Attribute("stroke-linecap") == "round" ? PenLineCap.Round : PenLineCap.Flat);
        }).ToArray();
        return new(new Rect(box[0], box[1], box[2], box[3]), paths);
    }

    private sealed record IconPath(Geometry Geometry, bool Filled, bool Stroked, double StrokeWidth, PenLineCap LineCap);
    private sealed record IconDrawing(Rect ViewBox, IconPath[] Paths);
}
