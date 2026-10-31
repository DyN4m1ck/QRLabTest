using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Images;
using SkiaSharp;
using ZXing.Common;

namespace QRLab.Image.Generation;

// Растеризатор логической битовой матрицы кода в PNG-изображение заданного размера.
// Работает через SkiaSharp: каждый «чёрный» модуль матрицы превращается в квадрат
// из scale×scale пикселей (для линейных штрихкодов высота масштабируется отдельно).
// Антиалиасинг выключен намеренно — код должен состоять из резких чёрно-белых границ.
public sealed class CodeImageRenderer
{
    // Главный метод: принимает матрицу модулей и параметры генерации, возвращает готовый PNG.
    // Результат оборачивается в неизменяемый RasterImage — дальше по приложению ходят только байты.
    public RasterImage Render(BitMatrix matrix, GenerationOptions settings)
    {
        // Целочисленный масштаб: подбирается минимальный из двух, чтобы код гарантированно
        // поместился в кадр и сохранил пропорции модулей (квадраты, а не прямоугольники).
        var scale = Math.Min(settings.Width / matrix.Width, settings.Height / matrix.Height);
        // Если целый масштаб меньше единицы — физически невозможно нарисовать код без усечения.
        if (scale < 1) throw new ArgumentException("Изображение слишком мало для кода.");
        
        // У линейных штрихкодов (EAN-13, Code128) матрица низкая и широкая,
        // поэтому по высоте её растягивают на весь кадр отдельным масштабом.
        var scaleY = settings.IsLinear ? settings.Height / matrix.Height : scale;
        // Отступы слева и сверху центрируют код внутри изображения.
        var left = (settings.Width - matrix.Width * scale) / 2;
        var top = (settings.Height - matrix.Height * scaleY) / 2;
        
        // Создаём растровую поверхность нужного размера; Bgra8888 + Opaque — быстрый формат
        // без альфа-канала, потому что код всегда непрозрачный (фон — белая подложка).
        using var surface = SKSurface.Create(new SKImageInfo(settings.Width, settings.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        using var paint = new SKPaint();
        paint.Color = SKColors.Black;
        // Отключаем сглаживание: пиксельная точность критична для распознаваемости кода.
        paint.IsAntialias = false;
        // Белый фон заполняет весь кадр, включая поля (quiet zone) вокруг кода.
        surface.Canvas.Clear(SKColors.White);
        
        // Проходим по всем модулям матрицы и рисуем только «чёрные» (true) клетки.
        // Индексация matrix[x, y] соответствует координатам модуля, а не пикселя.
        for (var y = 0; y < matrix.Height; y++)
            for (var x = 0; x < matrix.Width; x++)
                if (matrix[x, y]) surface.Canvas.DrawRect(left + x * scale, top + y * scaleY, scale, scaleY, paint);
        
        // Снимок поверхности -> изображение -> PNG в памяти -> неизменяемый RasterImage.
        // Качество 100 и отсутствие фильтра дают детерминированный результат для одинаковых входных данных.
        using var image = surface.Snapshot();
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}
