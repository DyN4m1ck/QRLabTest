namespace QRLab.Core.Models;

/// <summary>
/// Устаревший результат декодирования одного изображения. Современный аналог —
/// RecognitionPreview; класс используется лишь тестом ModelTests.
/// </summary>
public sealed class DecodeResult
{
    /// <summary>Идентификатор результата.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>Идентификатор образца, к которому относится результат.</summary>
    public Guid SampleId { get; init; }
    /// <summary>Удалось ли считать код.</summary>
    public bool IsSuccess { get; init; }
    /// <summary>Считанный текст (если успех).</summary>
    public string? DecodedText { get; init; }
    /// <summary>Сообщение об ошибке (если провал).</summary>
    public string? ErrorMessage { get; init; }
    /// <summary>Длительность декодирования.</summary>
    public TimeSpan Duration { get; init; }
}
