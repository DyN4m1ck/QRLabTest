namespace QRLab.Core.Models.Recognition;

/// Результат распознавания одного образца: номер, итоговый статус,
/// длительность декодирования, считанный текст и сообщение об ошибке.
public sealed record RecognitionPreview(int Number, Status Status, double Milliseconds, string? DecodedContent = null, string? Error = null);
