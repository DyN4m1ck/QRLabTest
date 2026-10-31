namespace QRLab.Core.Models.Generation;

/// <summary>
/// Допустимые границы числовых параметров генерации. Используются валидацией
/// GenerationOptions, ограничителями полей ввода UI и классом RasterImage.
/// </summary>
public static class GenerationLimits
{
    /// <summary>Минимальное количество образцов в пакете.</summary>
    public const int MinimumCount = 1;
    /// <summary>Максимальное количество образцов в пакете.</summary>
    public const int MaximumCount = 1000;
    /// <summary>Максимальный первый номер (чтобы нумерация не переполняла int).</summary>
    public const int MaximumFirstNumber = 999999;
    /// <summary>Минимальный размер стороны изображения, px.</summary>
    public const int MinimumImageSize = 128;
    /// <summary>Максимальный размер стороны изображения, px.</summary>
    public const int MaximumImageSize = 8192;
    /// <summary>Минимальная тихая зона, модули.</summary>
    public const int MinimumQuietZone = 0;
    /// <summary>Максимальная тихая зона, модули.</summary>
    public const int MaximumQuietZone = 16;
    /// <summary>Максимальная версия QR-кода по стандарту.</summary>
    public const int MaximumQrVersion = 40;
}
