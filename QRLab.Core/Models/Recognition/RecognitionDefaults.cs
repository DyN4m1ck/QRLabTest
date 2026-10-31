namespace QRLab.Core.Models.Recognition;

/// <summary>Значения по умолчанию и границы для распознавания кодов.</summary>
public static class RecognitionDefaults
{
    /// <summary>Название декодера, попадает в отчёты/интерфейс.</summary>
    public const string DecoderName = "ZXing.Net";
    /// <summary>Тайм-аут декодирования одного изображения по умолчанию, мс.</summary>
    public const int TimeoutMilliseconds = 500;
    /// <summary>Минимально допустимый тайм-аут, мс.</summary>
    public const int MinimumTimeoutMilliseconds = 1;
    /// <summary>Максимально допустимый тайм-аут, мс.</summary>
    public const int MaximumTimeoutMilliseconds = 60000;
    /// <summary>«Улучшенный» режим (TryHarder) по умолчанию.</summary>
    public const bool Enhanced = true;
}
