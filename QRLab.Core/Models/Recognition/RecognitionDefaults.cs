namespace QRLab.Core.Models.Recognition;

/// Значения по умолчанию и границы для распознавания кодов.
public static class RecognitionDefaults
{
    /// Название декодера, попадает в отчёты/интерфейс.
    public const string DecoderName = "ZXing.Net";
    /// Тайм-аут декодирования одного изображения по умолчанию, мс.
    public const int TimeoutMilliseconds = 500;
    /// Минимально допустимый тайм-аут, мс.
    public const int MinimumTimeoutMilliseconds = 1;
    /// Максимально допустимый тайм-аут, мс.
    public const int MaximumTimeoutMilliseconds = 60000;
    /// «Улучшенный» режим (TryHarder) по умолчанию.
    public const bool Enhanced = true;
}
