namespace QRLab.Core.Models.Recognition;

/// Одна строка CSV-отчёта о распознавании: ожидаемое и фактически считанное
/// содержимое, статус, время декодирования, цепочка мутаций и ошибка.
public sealed record RecognitionReportRow(int Number, string Name, string Format, string ExpectedContent,
    string? DecodedContent, Status? Status, double? Milliseconds, string MutationChain, string? Error);
