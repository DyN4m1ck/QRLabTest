using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using SkiaSharp;

namespace QRLab.Image.Mutations;

public sealed class SaltAndPepperMutation : ImageMutation<SaltAndPepperOptions>
{
    public override MutationKind Kind => MutationKind.SaltAndPepper;

    protected override RasterImage Apply(RasterImage source, SaltAndPepperOptions options)
    {
        if (options.DensityPercent == 0) return source;
        using var image = SKImage.FromEncodedData(source.Data.Span);
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        surface.Canvas.DrawImage(image, 0, 0);
        surface.Canvas.Flush();
        using var pixels = surface.PeekPixels();
        var data = pixels.GetPixelSpan();
        var random = new Random(options.Seed);
        var density = options.DensityPercent / 100;
        var salt = options.SaltPercent / 100;
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            if (random.NextDouble() >= density) continue;
            var value = random.NextDouble() < salt ? (byte)255 : (byte)0;
            var offset = y * pixels.RowBytes + x * 4;
            data[offset] = value; data[offset + 1] = value; data[offset + 2] = value; data[offset + 3] = 255;
        }
        using var result = surface.Snapshot();
        using var png = result.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}