namespace QRLab.Core.Models.Generation;

/// Допустимые границы числовых параметров генерации. Используются валидацией
/// GenerationOptions, ограничителями полей ввода UI и классом RasterImage.
public static class GenerationLimits
{
    /// Минимальное количество образцов в пакете.
    public const int MinimumCount = 1;
    /// Максимальное количество образцов в пакете.
    public const int MaximumCount = 1000;
    /// Максимальный первый номер (чтобы нумерация не переполняла int).
    public const int MaximumFirstNumber = 999999;
    /// Минимальный размер стороны изображения, px.
    public const int MinimumImageSize = 128;
    /// Максимальный размер стороны изображения, px.
    public const int MaximumImageSize = 8192;
    /// Минимальная тихая зона, модули.
    public const int MinimumQuietZone = 0;
    /// Максимальная тихая зона, модули.
    public const int MaximumQuietZone = 16;
    /// Максимальная версия QR-кода по стандарту.
    public const int MaximumQrVersion = 40;
}
