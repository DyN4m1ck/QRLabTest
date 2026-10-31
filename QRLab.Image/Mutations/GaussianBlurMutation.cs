using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using SkiaSharp;

namespace QRLab.Image.Mutations;

// Гауссово размытие — имитирует расфокусировку/низкое качество съёмки.
// Реализовано одним вызовом SkiaSharp: фильтр SKImageFilter.CreateBlur применяет
// настоящее гауссово ядро на нативном уровне (быстрее любой ручной свёртки пикселей).
public sealed class GaussianBlurMutation : ImageMutation<GaussianBlurOptions>
{
    public override MutationKind Kind => MutationKind.GaussianBlur;

    protected override RasterImage Apply(RasterImage source, GaussianBlurOptions options)
    {
        // Нулевая сигма = «выключено»: возвращаем тот же экземпляр без пересчёта и перекодирования.
        if (options.Sigma == 0) return source;
        
        // Раскодирование PNG исходника в растр; если байты повреждены — понятная ошибка пользователю.
        using var image = SKImage.FromEncodedData(source.Data.Span)
            ?? throw new ArgumentException("Не удалось прочитать исходное изображение.");
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        // Одинаковая сигма по X и Y даёт круглое (изотропное) размытие; Clamp растягивает крайние
        // пиксели за границы, чтобы не появлялась тёмная/прозрачная рамка по краю кадра.
        using var filter = SKImageFilter.CreateBlur((float)options.Sigma, (float)options.Sigma, SKShaderTileMode.Clamp);
        using var paint = new SKPaint { ImageFilter = filter };
        // Белая подложка обязательна: код печатается на белом, и размытые края должны темнеть в белый,
        // как на реальной фотографии светлой этикетки.
        surface.Canvas.Clear(SKColors.White);
        // Само размытие происходит внутри DrawImage: paint с ImageFilter фильтрует каждый пиксель.
        surface.Canvas.DrawImage(image, 0, 0, paint);
        using var result = surface.Snapshot();
        using var png = result.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}