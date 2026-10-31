using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using SkiaSharp;

namespace QRLab.Image.Mutations;


// Перспективная деформация — имитация съёмки кода под углом (фото этикетки «в реальном мире»).
// Изображение мысленно поворачивается в 3D вокруг центра на AngleX/AngleY градусов и
// проецируется обратно на плоскость; всё сведено к одной гомографии — перспективной
// SKMatrix (9 элементов), которую Skia применяет на канве. Параметр Distance управляет
// силой перспективы (чем меньше, тем сильнее искажение).
public sealed class PerspectiveMutation : ImageMutation<PerspectiveMutationOptions>
{
    public override MutationKind Kind => MutationKind.Perspective;

    protected override RasterImage Apply(RasterImage source, PerspectiveMutationOptions options)
    {
        // Нулевые углы — тождественное преобразование: возвращаем исходный кадр как есть.
        if (options is { AngleX: 0, AngleY: 0 }) 
            return source;
        
        // Углы переводятся в радианы для тригонометрии поворота вокруг осей X и Y.
        var ax = options.AngleX * Math.PI / 180;
        var ay = options.AngleY * Math.PI / 180;
        // Центр поворота — середина кадра, иначе изображение уехало бы за границу.
        var cx = source.Width / 2d; var cy = source.Height / 2d;
        // Эффективное «фокусное расстояние» камеры: задаёт глубину и силу схождения линий.
        var focal = Math.Max(source.Width, source.Height) * options.Distance;
        // Композиция двух поворотов (сначала вокруг Y, затем вокруг X) в компактных множителях
        // affine-части матрицы проекции.
        var a = Math.Cos(ay); var b = Math.Sin(ay) * Math.Sin(ax); var d = Math.Cos(ax);
        // Элементы Persp0/Persp1 — знаменатели перспективного деления: они отвечают за то,
        // что дальний край кода сжимается ближе к центру.
        var px = -Math.Sin(ay) / focal; var py = Math.Cos(ay) * Math.Sin(ax) / focal;
        // Persp2 подбирается так, чтобы центр кадра остался неподвижной точкой преобразования.
        var p0 = 1 - px * cx - py * cy;
        // Итоговая 3x3 гомография: верхняя левая часть — поворот+масштаб, Translation —
        // компенсация смещения, последняя строка — перспективные коэффициенты.
        var matrix = new SKMatrix
        {
            ScaleX = (float)(a + cx * px), SkewX = (float)(b + cx * py),
            TransX = (float)(-a * cx - b * cy + cx * p0),
            SkewY = (float)(cy * px), ScaleY = (float)(d + cy * py),
            TransY = (float)(-d * cy + cy * p0),
            Persp0 = (float)px, Persp1 = (float)py, Persp2 = (float)p0
        };
        
        // Режим выборки пикселей при обратной проекции:
        // Nearest — резкие «зубчатые» края (точно, быстро), Linear — стандартный билинейный,
        // Mitchell (bicubic) — самый гладкий вариант для печати/демо.
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
        // Применяем гомографию к канве: дальше DrawImage сам растеризует изображение
        // уже «повёрнутым» внутри того же кадра фиксированного размера.
        surface.Canvas.SetMatrix(matrix);
        surface.Canvas.DrawImage(image, 0, 0, sampling);
        
        using var result = surface.Snapshot();
        using var png = result.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}