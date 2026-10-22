using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using SkiaSharp;

namespace QRLab.Image.Mutations;

public sealed class GaussianBlurMutation : ImageMutation<GaussianBlurOptions>
{
    public override MutationKind Kind => MutationKind.GaussianBlur;

    protected override RasterImage Apply(RasterImage source, GaussianBlurOptions options)
    {
        if (options.Sigma == 0) return source;
        using var image = SKImage.FromEncodedData(source.Data.Span)
            ?? throw new ArgumentException("Не удалось прочитать исходное изображение.");
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        using var filter = SKImageFilter.CreateBlur((float)options.Sigma, (float)options.Sigma, SKShaderTileMode.Clamp);
        using var paint = new SKPaint { ImageFilter = filter };
        surface.Canvas.Clear(SKColors.White);
        surface.Canvas.DrawImage(image, 0, 0, paint);
        using var result = surface.Snapshot();
        using var png = result.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}