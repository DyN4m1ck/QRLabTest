namespace QRLab.Core.Models.Recognition;

/// <summary>
/// Результат распознавания одного образца: номер, итоговый статус,
/// длительность декодирования, считанный текст и сообщение об ошибке.
/// </summary>
public sealed record RecognitionPreview(int Number, Status Status, double Milliseconds, string? DecodedContent = null, string? Error = null);
