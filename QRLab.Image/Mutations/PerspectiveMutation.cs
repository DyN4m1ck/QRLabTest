using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using SkiaSharp;

namespace QRLab.Image.Mutations;

public sealed class PerspectiveMutation : ImageMutation<PerspectiveMutationOptions>
{
    public override MutationKind Kind => MutationKind.Perspective;

    protected override RasterImage Apply(RasterImage source, PerspectiveMutationOptions options)
    {
        if (options.AngleX == 0 && options.AngleY == 0) return source;
        var ax = options.AngleX * Math.PI / 180;
        var ay = options.AngleY * Math.PI / 180;
        var cx = source.Width / 2d; var cy = source.Height / 2d;
        var focal = Math.Max(source.Width, source.Height) * options.Distance;
        var a = Math.Cos(ay); var b = Math.Sin(ay) * Math.Sin(ax); var d = Math.Cos(ax);
        var px = -Math.Sin(ay) / focal; var py = Math.Cos(ay) * Math.Sin(ax) / focal;
        var p0 = 1 - px * cx - py * cy;
        var matrix = new SKMatrix
        {
            ScaleX = (float)(a + cx * px), SkewX = (float)(b + cx * py),
            TransX = (float)(-a * cx - b * cy + cx * p0),
            SkewY = (float)(cy * px), ScaleY = (float)(d + cy * py),
            TransY = (float)(-d * cy + cy * p0),
            Persp0 = (float)px, Persp1 = (float)py, Persp2 = (float)p0
        };
        var sampling = options.Interpolation switch
        {
            ImageInterpolation.Nearest => new SKSamplingOptions(SKFilterMode.Nearest),
            ImageInterpolation.Bilinear => new SKSamplingOptions(SKFilterMode.Linear),
            ImageInterpolation.Bicubic => new SKSamplingOptions(SKCubicResampler.Mitchell),
            _ => throw new ArgumentOutOfRangeException(nameof(options))
        };
        using var image = SKImage.FromEncodedData(source.Data.Span);
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        surface.Canvas.Clear(SKColors.White);
        surface.Canvas.SetMatrix(matrix);
        surface.Canvas.DrawImage(image, 0, 0, sampling);
        using var result = surface.Snapshot();
        using var png = result.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}