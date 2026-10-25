using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Images;
using SkiaSharp;
using ZXing.Common;

namespace QRLab.Image.Generation;

public sealed class CodeImageRenderer
{
    public RasterImage Render(BitMatrix matrix, GenerationOptions settings)
    {
        var scale = Math.Min(settings.Width / matrix.Width, settings.Height / matrix.Height);
        if (scale < 1) throw new ArgumentException("Изображение слишком мало для кода.");
        
        var scaleY = settings.IsLinear ? settings.Height / matrix.Height : scale;
        var left = (settings.Width - matrix.Width * scale) / 2;
        var top = (settings.Height - matrix.Height * scaleY) / 2;
        
        using var surface = SKSurface.Create(new SKImageInfo(settings.Width, settings.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        using var paint = new SKPaint();
        paint.Color = SKColors.Black;
        paint.IsAntialias = false;
        surface.Canvas.Clear(SKColors.White);
        
        for (var y = 0; y < matrix.Height; y++)
            for (var x = 0; x < matrix.Width; x++)
                if (matrix[x, y]) surface.Canvas.DrawRect(left + x * scale, top + y * scaleY, scale, scaleY, paint);
        
        using var image = surface.Snapshot();
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}