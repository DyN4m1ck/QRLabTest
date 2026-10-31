using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using SkiaSharp;

namespace QRLab.Image.Mutations;

// «Соль и перец» — импульсный шум: случайные пиксели становятся чисто белыми или чисто чёрными.
// Модель физики дефектов матрицы сенсора/битых пикселей. Работает напрямую по памяти пикселей,
// потому что фильтры Skia здесь были бы избыточны. Генератор случайных чисел инициализируется
// полем Seed, поэтому при одинаковых параметрах результат полностью детерминирован.
public sealed class SaltAndPepperMutation : ImageMutation<SaltAndPepperOptions>
{
    public override MutationKind Kind => MutationKind.SaltAndPepper;

    protected override RasterImage Apply(RasterImage source, SaltAndPepperOptions options)
    {
        // Нулевая плотность шума — быстрый выход без декодирования изображения.
        if (options.DensityPercent == 0) return source;
        using var image = SKImage.FromEncodedData(source.Data.Span);
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        surface.Canvas.DrawImage(image, 0, 0);
        // Flush гарантирует, что все команды отрисовки реально попали в память поверхности
        // до того, как мы начнём читать/менять пиксели через PeekPixels.
        surface.Canvas.Flush();
        // PeekPixels даёт прямой доступ к буферу Bgra8888 без копирования — единственный способ
        // точечно перезаписать отдельные пиксели.
        using var pixels = surface.PeekPixels();
        var data = pixels.GetPixelSpan();
        // Фиксированный seed => воспроизводимые эксперименты: один и тот же снимок набора
        // можно перегенерировать побайтово идентично (важно для тестов и сохранения наборов).
        var random = new Random(options.Seed);
        // Доли переводятся из процентов UI в вероятности [0..1]: density — шанс испортить пиксель,
        var density = options.DensityPercent / 100;
        // salt — доля «соли» (белых) среди испорченных; остальное — «перец» (чёрные).
        var salt = options.SaltPercent / 100;
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            // Ровно два вызова random.NextDouble() на затронутый пиксель сохраняют порядок
            // генерации независимо от ветвления — иначе детерминизм мог бы «плыть».
            if (random.NextDouble() >= density) continue;
            // Решаем, белый это шум или чёрный, и записываем одно значение во все три канала.
            var value = random.NextDouble() < salt ? (byte)255 : (byte)0;
            // RowBytes может быть больше Width*4 (выравнивание строки), поэтому смещение
            // считаем именно от RowBytes, а не от ширины. Канал A принудительно 255 — пиксель непрозрачен.
            var offset = y * pixels.RowBytes + x * 4;
            data[offset] = value; data[offset + 1] = value; data[offset + 2] = value; data[offset + 3] = 255;
        }
        using var result = surface.Snapshot();
        using var png = result.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}