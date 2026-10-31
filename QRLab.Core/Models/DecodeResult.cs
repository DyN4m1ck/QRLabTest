namespace QRLab.Core.Models;

/// Устаревший результат декодирования одного изображения. Современный аналог —
/// RecognitionPreview; класс используется лишь тестом ModelTests.
public sealed class DecodeResult
{
    /// Идентификатор результата.
    public Guid Id { get; init; } = Guid.NewGuid();
    /// Идентификатор образца, к которому относится результат.
    public Guid SampleId { get; init; }
    /// Удалось ли считать код.
    public bool IsSuccess { get; init; }
    /// Считанный текст (если успех).
    public string? DecodedText { get; init; }
    /// Сообщение об ошибке (если провал).
    public string? ErrorMessage { get; init; }
    /// Длительность декодирования.
    public TimeSpan Duration { get; init; }
}
